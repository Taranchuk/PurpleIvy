﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;

namespace PurpleIvy
{
    public class Plant_Ivy : Plant
    {
        public bool CanMutate = false;
        ThingDef sporesThingDef = ThingDef.Named("Spores");
        private Gas Spores = null;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (this.Growth >= 0.25f)
            {
                this.ThrowGasOrAdjustGasSize();
            }
        }

        public override void PostMake()
        {
            base.PostMake();
            if (Rand.Chance(0.15f))
            {
                CanMutate = true;
            }
            else
            {
                CanMutate = false;
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
        }

        public bool HasNoBuildings(IntVec3 dir)
        {
            return GenAdj.CellsAdjacent8Way(new TargetInfo(dir, this.Map, false)).All(current => current.Standable(this.Map));
        }

        public void DoDamageToBuildings(IntVec3 pos)
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
                    if (list[i] is Building && list[i].Faction != PurpleIvyData.AlienFaction)
                    {
                        Building b = (Building)list[i];
                        var comp = this.Map.GetComponent<MapComponent_MapEvents>();
                        if (comp != null)
                        {
                            int oldDamage = 0;
                            if (comp.ToxicDamages == null)
                            {
                                comp.ToxicDamages = new Dictionary<Building, int>();
                                comp.ToxicDamages[b] = b.MaxHitPoints;
                            }
                            Log.Message("Taking damage to " + b);
                            if (!comp.ToxicDamages.ContainsKey(b))
                            {
                                oldDamage = b.MaxHitPoints;
                                comp.ToxicDamages[b] = b.MaxHitPoints - 1;
                            }
                            else
                            {
                                oldDamage = comp.ToxicDamages[b];
                                comp.ToxicDamages[b] -= 1;
                            }
                            BuildingsToxicDamageSectionLayerUtility.Notify_BuildingHitPointsChanged((Building)list[i], oldDamage);
                            if (comp.ToxicDamages[b] / 2 < b.MaxHitPoints)
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
                            break;
                        }
                    }
                }
            }
        }

        public void DoDamageToThings(IntVec3 pos)
        {
            List<Thing> list = new List<Thing>();
            try
            {
                if (GenGrid.InBounds(pos, this.Map))
                {
                    list = this.Map.thingGrid.ThingsListAt(pos);
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
                    var corpse = (Corpse)list[i];
                    var compRottable = list[i].TryGetComp<CompRottable>();
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
                            var dummyCorpse = ThingMaker.MakeThing(PurpleIvyDefOf.InfectedCorpseDummy);
                            var comp = dummyCorpse.TryGetComp<AlienInfection>();
                            comp.parent = corpse;
                            var range = PurpleIvyData.maxNumberOfCreatures["Genny_ParasiteOmega"];
                            comp.Props.maxNumberOfCreatures = range;
                            comp.maxNumberOfCreatures = range.RandomInRange;
                            comp.Props.typesOfCreatures = new List<string>()
                            {
                                "Genny_ParasiteOmega"
                            };
                            corpse.AllComps.Add(comp);
                            Log.Message("5 Adding infected comp to " + corpse);
                        }
                    }
                }
                else switch (list[i])
                    {
                        //If we find a pawn and its not a hatchling
                        case Pawn _:
                            {
                                var pawn = (Pawn)list[i];
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
                                    (pawn).health.AddHediff(hediff2, null, null, null);
                                }               
                                break;
                            }
                        //If we find a plant
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

        public bool NoNestsNearby(int radius)
        {
            List<Thing> list = new List<Thing>();
            foreach (var dir in GenRadial.RadialCellsAround(this.Position, radius, true))
            {
                try
                {
                    if (GenGrid.InBounds(dir, this.Map))
                    {
                        list = this.Map.thingGrid.ThingsListAt(dir);
                    }
                }
                catch
                {
                    return false;
                }
                foreach (var t in list)
                {
                    if (t.def == PurpleIvyDefOf.PI_Nest)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public void TryMutate()
        {
            if (this.Map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSacNestGuard).Count == 0)
            {
                Building_EggSac EggSac = (Building_EggSac)ThingMaker.MakeThing(PurpleIvyDefOf.EggSacNestGuard);
                EggSac.SetFactionDirect(PurpleIvyData.AlienFaction);
                GenSpawn.Spawn(EggSac, Position, this.Map);
                Log.Message("No other guard eggs - " + this + " mutate into EggSac NestGuard");
            }
            else if (HasNoBuildings(Position))
            {

                float randChance = Rand.Range(0f, 1f);
                Log.Message(this + " - rand chance: " + randChance.ToString());
                if (randChance >= 0f && randChance <= 0.3f)
                {
                    if (NoNestsNearby(5))
                    {
                        var nest = ThingMaker.MakeThing(PurpleIvyDefOf.PI_Nest);
                        GenSpawn.Spawn(nest, Position, this.Map);
                        Log.Message(this + " mutate into Nest");
                        this.Destroy(DestroyMode.Vanish);
                    }
                }
                else if (randChance >= 0.35f && randChance <= 0.359f)
                {
                    var GenMortar = (Building_TurretGun)ThingMaker.MakeThing(PurpleIvyDefOf.Turret_GenMortarSeed);
                    GenMortar.SetFactionDirect(PurpleIvyData.AlienFaction);
                    GenSpawn.Spawn(GenMortar, Position, this.Map);
                    Log.Message("Rand chance: " + randChance.ToString() + " - " + this + " mutate into GenMortar");

                }
                else if (randChance >= 0.36f && randChance <= 0.369f)
                {
                    Thing thing = ThingMaker.MakeThing(PurpleIvyDefOf.GenTurretBase, null);
                    thing.SetFactionDirect(PurpleIvyData.AlienFaction);
                    GenSpawn.Spawn(thing, this.Position, this.Map, thing.Rotation, WipeMode.Vanish, false);
                    Log.Message("Rand chance: " + randChance.ToString() + " - " + this + " mutate into GenTurret");
                }
                else if (randChance >= 0.44f && randChance <= 0.4459f) // 0.005 - 0.5%
                {
                    var EggSac = ThingMaker.MakeThing(PurpleIvyDefOf.EggSac);
                    EggSac.SetFactionDirect(PurpleIvyData.AlienFaction);
                    GenSpawn.Spawn(EggSac, Position, this.Map);
                    if (EggSac.Map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteAlpha).Count
                        > PurpleIvySettings.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteAlpha.defName])
                    {
                        EggSac.TryGetComp<AlienInfection>().stopSpawning = true;
                    }
                    Log.Message("Rand chance: " + randChance.ToString() + " - " + this + " mutate into EggSac");
                }
                else if (randChance >= 0.446f && randChance <= 0.4549f)
                {
                    var EggSac = ThingMaker.MakeThing(PurpleIvyDefOf.EggSacBeta);
                    EggSac.SetFactionDirect(PurpleIvyData.AlienFaction);
                    GenSpawn.Spawn(EggSac, Position, this.Map);
                    if (EggSac.Map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSacBeta).Count
                        > PurpleIvySettings.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteBeta.defName])
                    {
                        EggSac.TryGetComp<AlienInfection>().stopSpawning = true;
                    }
                    Log.Message("Rand chance: " + randChance.ToString() + " - " + this + " mutate into EggSac beta");
                }
                else if (randChance >= 0.455f && randChance <= 0.469f)
                {
                    var EggSac = ThingMaker.MakeThing(PurpleIvyDefOf.EggSacGamma);
                    EggSac.SetFactionDirect(PurpleIvyData.AlienFaction);
                    GenSpawn.Spawn(EggSac, Position, this.Map);
                    if (EggSac.Map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSacGamma).Count 
                        > PurpleIvySettings.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteGamma.defName])
                    {
                        EggSac.TryGetComp<AlienInfection>().stopSpawning = true;
                    }
                    Log.Message("Rand chance: " + randChance.ToString() + " - " + this + " mutate into EggSac gamma");
                }
                else if (randChance >= 0.47f && randChance <= 0.499f)
                {
                    if (this.Map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSacNestGuard).Count < 4)
                    {
                        var EggSac = ThingMaker.MakeThing(PurpleIvyDefOf.EggSacNestGuard);
                        EggSac.SetFactionDirect(PurpleIvyData.AlienFaction);
                        GenSpawn.Spawn(EggSac, Position, this.Map);
                        Log.Message("Rand chance: " + randChance.ToString() + " - " + this + " mutate into EggSac NestGuard");
                    }
                }
                else if (randChance >= 0.50f && randChance <= 0.519f)
                {
                    var EggSac = ThingMaker.MakeThing(PurpleIvyDefOf.ParasiteEgg);
                    EggSac.SetFactionDirect(PurpleIvyData.AlienFaction);
                    GenSpawn.Spawn(EggSac, Position, this.Map);
                    if (EggSac.Map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteOmega).Count
                        > PurpleIvySettings.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteOmega.defName])
                    {
                        EggSac.TryGetComp<AlienInfection>().stopSpawning = true;
                    }
                    Log.Message(this + " mutate into ParasiteEgg");
                }
                else if (randChance >= 0.55f && randChance <= 0.59f)
                {
                    var PlantVenomousToothwort = ThingMaker.MakeThing(PurpleIvyDefOf.PlantVenomousToothwort);
                    GenSpawn.Spawn(PlantVenomousToothwort, Position, this.Map);
                    Log.Message("Rand chance: " + randChance.ToString() + " - " + this + " mutate into PlantVenomousToothwort");
                    this.Destroy(DestroyMode.Vanish);
                }
                else if (randChance >= 0.30f && randChance <= 0.329f)
                {
                    var GasPump = ThingMaker.MakeThing(PurpleIvyDefOf.GasPump);
                    GasPump.SetFactionDirect(PurpleIvyData.AlienFaction);
                    GenSpawn.Spawn(GasPump, Position, this.Map);
                    Log.Message("Rand chance: " + randChance.ToString() + " - " + this + " mutate into GasPump");
                }
            }
        }

        public void ThrowGasOrAdjustGasSize()
        {
            if (this.Spores != null)
            {
                this.Spores.Graphic.drawSize.x = (this.Growth * 4f) - 1f;
                this.Spores.Graphic.drawSize.y = (this.Growth * 4f) - 1f;
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
                        texPath = sporesThingDef.graphicData.texPath,
                        graphicClass = typeof(Graphic_Gas),
                        shaderType = ShaderTypeDefOf.Transparent,
                        drawSize = new Vector2((this.Growth) * 2f - 1f, (this.Growth) * 2f - 1f),
                        color = new ColorInt(sporesThingDef.graphicData.color).ToColor
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

        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % 2000 == 0)
            {
                base.TickLong();
                if (this.Growth >= 0.25f)
                {
                    this.ThrowGasOrAdjustGasSize();
                    this.DoDamageToBuildings(Position);
                    if (this.CanMutate == true)
                    {
                        this.TryMutate();
                    }
                }
            }
            if (Find.TickManager.TicksGame % 350 == 0)
            {
                if (this.Growth >= 0.25f)
                {
                    this.DoDamageToThings(Position);
                }
            }
            if (this.Growth >= 0.75f && Rand.Chance(0.3f) && Find.TickManager.TicksGame 
                % Rand.RangeInclusive(60, 100) == 0)
            {
                PurpleIvyMoteMaker.ThrowEMPLightningGlow(this.Position.ToVector3Shifted(), this.Map, 0.3f);
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.CanMutate, "MutateTry", true, true);
        }
    }
}

