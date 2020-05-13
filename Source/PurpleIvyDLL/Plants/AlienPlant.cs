using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace PurpleIvy
{
    public class AlienPlant : Plant
    {
        public Gas Spores = null;
        public void ThrowGasOrAdjustGasSize(float size)
        {
            if (this.Spores != null)
            {
                this.Spores.Graphic.drawSize.x = (this.Growth * size) - 1f;
                this.Spores.Graphic.drawSize.y = (this.Growth * size) - 1f;
                this.Spores.Graphic.color.a = this.Growth;
                this.Spores.destroyTick = Find.TickManager.TicksGame + 3000;
            }
            else if (this.Position.InBounds(this.Map))
            {
                ThingDef thingDef = new ThingDef
                {
                    defName = "Spores" + this.ThingID.GetHashCode(),
                    thingClass = typeof(Gas),
                    category = ThingCategory.Gas,
                    altitudeLayer = AltitudeLayer.Gas,
                    useHitPoints = false,
                    tickerType = TickerType.Normal,
                    graphicData = new GraphicData
                    {
                        texPath = PurpleIvyDefOf.PI_Spores.graphicData.texPath,
                        graphicClass = typeof(Graphic_Gas),
                        shaderType = ShaderTypeDefOf.Transparent,
                        drawSize = new Vector2((this.Growth * size) - 1f, (this.Growth * size) - 1f),
                        color = new ColorInt(PurpleIvyDefOf.PI_Spores.graphicData.color).ToColor
                    },
                    gas = new GasProperties
                    {
                        expireSeconds = new FloatRange(60f, 100f),
                        blockTurretTracking = true,
                        accuracyPenalty = 0.7f,
                        rotationSpeed = 20f
                    }
                };
                Thing thing = ThingMaker.MakeThing(thingDef, null);
                GenSpawn.Spawn(thing, this.Position, this.Map, 0);
                this.Spores = (Gas)thing;
                this.Spores.destroyTick = Find.TickManager.TicksGame + 3000;
            }
        }

        public void DoDamageToBuildings()
        {
            List<Thing> list = new List<Thing>();
            foreach (var pos2 in GenAdj.CellsAdjacent8Way(this))
            {
                try
                {
                    if (GenGrid.InBounds(pos2, this.Map))
                    {
                        list = this.Map.thingGrid.ThingsListAt(pos2);
                    }
                }
                catch
                {
                    continue;
                }
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Faction != PurpleIvyData.AlienFaction)
                    {
                        if (list[i] is Pawn || list[i] is Gas || list[i] is Mote || list[i] is Filth) continue;
                        var comp = this.Map.GetComponent<MapComponent_MapEvents>();
                        if (comp != null)
                        {
                            int oldDamage = 0;
                            if (!comp.ToxicDamages.ContainsKey(list[i]))
                            {
        
                                if (PurpleIvyUtils.IsChunk(list[i]))
                                {
                                    comp.ToxicDamagesChunksDeep[list[i]] = list[i].MaxHitPoints - 1;
                                }
                                else
                                {
                                    comp.ToxicDamagesThings[list[i]] = list[i].MaxHitPoints - 1;
                                }
                                oldDamage = list[i].MaxHitPoints;
                                comp.ToxicDamages[list[i]] = list[i].MaxHitPoints - 1;
                            }
                            else
                            {
                                oldDamage = comp.ToxicDamages[list[i]];
                                comp.ToxicDamages[list[i]] -= 1;
                            }
                            //Log.Message("Damaging " + list[i], true);
                            ThingsToxicDamageSectionLayerUtility.Notify_ThingHitPointsChanged(comp, list[i], oldDamage);
                            if (list[i] is Building b && comp.ToxicDamages[b] / 2 < b.MaxHitPoints)
                            {
        
                                if (b.GetComp<CompBreakdownable>() != null)
                                {
                                    b.GetComp<CompBreakdownable>().DoBreakdown();
                                }
                                if (b.GetComp<CompPowerPlantWind>() != null)
                                {
                                    b.GetComp<CompPowerPlantWind>().PowerOutput /= 2f;
                                }
                                if (b.GetComp<CompPowerTrader>() != null)
                                {
                                    b.GetComp<CompPowerTrader>().PowerOn = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void DoDamageToCorpse(Corpse corpse)
        {
            var compRottable = corpse.TryGetComp<CompRottable>();
            if (compRottable.Stage == RotStage.Dessicated)
            {
                corpse.TakeDamage(new DamageInfo(DamageDefOf.Scratch, 1));
            }
            else
            {
                this.Growth += 0.001f;
                if (corpse.TryGetComp<CompRottable>().Stage < RotStage.Dessicated &&
                    corpse.TryGetComp<AlienInfection>() == null)
                {
                    var hediff = (AlienInfectionHediff)HediffMaker.MakeHediff
                        (PurpleIvyDefOf.PI_AlienInfection, corpse.InnerPawn);
                    hediff.instigator = PawnKindDef.Named("Genny_ParasiteOmega");
                    corpse.InnerPawn.health.AddHediff(hediff);
                }
            }
        }

        public void DoDamageToPawn(Pawn pawn)
        {
            if (!pawn.RaceProps.IsMechanoid)
            {
                var damageInfo = new DamageInfo(DamageDefOf.Scratch, 1, 0f, -1f, this, null, null);
                pawn.TakeDamage(damageInfo);
                var hediff = HediffMaker.MakeHediff(PurpleIvyDefOf.PoisonousPurpleHediff,
                    pawn, null);
                hediff.Severity = 0.1f;
                (pawn).health.AddHediff(hediff, null, null, null);
                var hediff2 = HediffMaker.MakeHediff(PurpleIvyDefOf.HarmfulBacteriaHediff,
                    pawn, null);
                hediff2.Severity = 0.1f;
                pawn.health.AddHediff(hediff2, null, null, null);
                if (Rand.Chance(0.1f) && pawn.health.hediffSet.GetFirstHediffOfDef(PurpleIvyDefOf.PI_AlienMutation) == null)
                {
                    var hediff3 = HediffMaker.MakeHediff(PurpleIvyDefOf.PI_AlienMutation, pawn, null);
                    pawn.health.AddHediff(hediff3, null, null, null);
                }
            }
        }

        public void DoDamageToThings()
        {
            List<Thing> list = new List<Thing>();
            try
            {
                if (GenGrid.InBounds(this.Position, this.Map))
                {
                    list = this.Map.thingGrid.ThingsListAt(this.Position);
                }
            }
            catch
            {
                return;
            }

            if (list == null || list.Count <= 0) return;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null || list[i].Faction == PurpleIvyData.AlienFaction) continue;
                if (list[i].def.IsCorpse)
                {
                    this.DoDamageToCorpse((Corpse)list[i]);
                }
                else switch (list[i])
                    {
                        case Pawn _:
                            {
                                this.DoDamageToPawn((Pawn)list[i]);
                                break;
                            }
                        case StickySlugs stickySlugs:
                            {
                                if (stickySlugs.HasAnyContents)
                                {
                                    var thing = stickySlugs.ContainedThing;
                                    if (thing is Pawn pawn)
                                    {
                                        var damageInfo = new DamageInfo(DamageDefOf.Scratch, 1, 0f, -1f, this, null, null);
                                        pawn.TakeDamage(damageInfo);
                                        this.Growth += 0.001f;
                                        if (pawn.TryGetComp<AlienInfection>() == null)
                                        {
                                            var hediff = (AlienInfectionHediff)HediffMaker.MakeHediff
                                                (PurpleIvyDefOf.PI_AlienInfection, pawn);
                                            hediff.instigator = PawnKindDef.Named("Genny_ParasiteOmega");
                                            pawn.health.AddHediff(hediff);
                                        }
                                        if (Rand.Chance(0.1f) && pawn.health.hediffSet.GetFirstHediffOfDef(PurpleIvyDefOf.PI_AlienMutation) == null)
                                        {
                                            var hediff3 = HediffMaker.MakeHediff(PurpleIvyDefOf.PI_AlienMutation, pawn, null);
                                            pawn.health.AddHediff(hediff3, null, null, null);
                                        }
                                    }
                                    else if (thing is Corpse corpse)
                                    {
                                        this.DoDamageToCorpse(corpse);
                                        stickySlugs.EjectContents();
                                    }
                                }
                                break;
                            }
                        case Plant _:
                            {
                                if (list[i].def != PurpleIvyDefOf.PurpleIvy
                                    && list[i].def != PurpleIvyDefOf.PI_Nest
                                    && list[i].def != PurpleIvyDefOf.PlantVenomousToothwort
                                    && list[i].def != PurpleIvyDefOf.PI_CorruptedTree)
                                {
                                    PurpleIvyMoteMaker.ThrowToxicSmoke(this.Position.ToVector3Shifted(), this.Map);
                                    //FilthMaker.TryMakeFilth(this.Position, this.Map, PurpleIvyDefOf.PI_ToxicFilth);
                                    list[i].TakeDamage(new DamageInfo(PurpleIvyDefOf.PI_ToxicBurn, 1));
                                }
                                break;
                            }
                    }
            }
        }
    }
}

