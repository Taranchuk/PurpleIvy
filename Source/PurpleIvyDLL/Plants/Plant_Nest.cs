﻿using System;
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
    public class Plant_Nest : Plant, IAttackTarget
    {
        private int SpreadTick;
        private int OrigSpreadTick;

        public int nectarAmount;
        private Gas Spores = null;

        Thing IAttackTarget.Thing
        {
            get
            {
                return this;
            }
        }
        public LocalTargetInfo TargetCurrentlyAimingAt
        {
            get
            {
                return LocalTargetInfo.Invalid;
            }
        }

        public float TargetPriorityFactor
        {
            get
            {
                return 0.3f;
            }
        }

        public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
        {
            return false;
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.SetFaction(PurpleIvyData.AlienFaction);
            UpdateGlower();
            if (!(this.Growth < 1)) return;
            var random = new System.Random();
            SpreadTick = random.Next(1, 3);
            OrigSpreadTick = SpreadTick;
        }

        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            PurpleIvyMoteMaker.ThrowToxicGas(this.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(1f), this.Map);
            base.PreApplyDamage(ref dinfo, out absorbed);
        }

        public override void SetFaction(Faction newFaction, Pawn recruiter = null)
        {
            this.factionInt = newFaction;
            IAttackTarget attackTarget = this as IAttackTarget;
            if (attackTarget != null)
            {
                this.Map.attackTargetsCache.UpdateTarget(attackTarget);
            }
        }
        public override void PostMapInit()
        {
            base.PostMapInit();
            if (this.Growth >= 0.25f)
            {
                this.ThrowGasOrAdjustGasSize();
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
        }

        public void SpawnIvy(IntVec3 dir)
        {
            if (GenCollection.Any<Thing>(GridsUtility.GetThingList(dir, Map),
                (Thing t) => (t.def.IsBuildingArtificial || t.def.IsNonResourceNaturalRock))) return;
            Plant newivy = new Plant();
            newivy = (Plant)ThingMaker.MakeThing(PurpleIvyDefOf.PurpleIvy);
            GenSpawn.Spawn(newivy, dir, this.Map);
        }

        public bool IvyInCell(IntVec3 dir)
        {
            //List all things in that random direction cell
            List<Thing> list = this.Map.thingGrid.ThingsListAt(dir);
            return list.Count > 0 && list.OfType<Plant>().Any(t =>
            t.def == PurpleIvyDefOf.PurpleIvy || t.def == PurpleIvyDefOf.PI_Nest
            || t.def == PurpleIvyDefOf.PlantVenomousToothwort);
        }

        public bool IsSurroundedByIvy(IntVec3 dir)
        {
            return GenAdj.CellsAdjacent8Way(new TargetInfo(dir, this.Map, false)).All(IvyInCell);
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
            //Loop over things if there are things
            if (list == null || list.Count <= 0) return;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null || list[i].Faction == PurpleIvyData.AlienFaction) continue;
                //If we find a corpse
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
                        SpreadTick--;
                        SpreadTick--;
                        SpreadTick--;
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

        public void SpreadPlants()
        {
            this.SpreadTick--;
            if (this.SpreadTick <= 0)
            {
                //Pick a random direction cell
                IntVec3 dir = new IntVec3();
                //dir = GenAdj.RandomAdjacentCellCardinal(Position);
                dir = GenRadial.RadialCellsAround(this.Position, Convert.ToInt32(this.Growth * 20), true).RandomElement();
                //If in bounds
                try
                {
                    if (dir.InBounds(this.Map))
                    {
                        TerrainDef terrain = dir.GetTerrain(this.Map);
                        if (terrain != null)
                        {
                            if (terrain.defName != "WaterDeep" &&
                                     terrain.defName != "WaterShallow" &&
                                     terrain.defName != "MarshyTerrain")
                            {
                                //if theres no ivy here
                                if (!IvyInCell(dir))
                                {
                                    SpawnIvy(dir);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR:" + ex.Message);
                } 
                SpreadTick = OrigSpreadTick;
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
                        texPath = PurpleIvyDefOf.PI_Spores.graphicData.texPath,
                        graphicClass = typeof(Graphic_Gas),
                        shaderType = ShaderTypeDefOf.Transparent,
                        drawSize = new Vector2((this.Growth * 4f) - 1f, (this.Growth * 4f) - 1f),
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
            }
        }

        public override string GetInspectString()
        {
            return "NectarAmount".Translate() + this.nectarAmount + "\n" + base.GetInspectString();
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var option in base.GetFloatMenuOptions(selPawn)) 
            {
                yield return option;
            }
            if (this.nectarAmount > 0)
            {
                yield return new FloatMenuOption(Translator.Translate("ExtractNectar"), delegate ()
                {
                    if (selPawn != null)
                    {
                        Job job = JobMaker.MakeJob(PurpleIvyDefOf.PI_ExtractNectar, this);
                        selPawn.jobs.TryTakeOrderedJob(job, 0);
                    }
                }, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else
            {
                yield return new FloatMenuOption("NestsDoesNotHaveNectar".Translate(), null, 
                    MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            yield break;
        }

        public CompGlower Glower
        {
            get
            {
                return (CompGlower)this.AllComps.Where(x => x is CompGlower).FirstOrDefault();
            }
        }
        public void UpdateGlower()
        {
            if (this.Glower != null)
            {
                base.Map.glowGrid.DeRegisterGlower(this.Glower);
                this.Glower.Initialize(new CompProperties_Glower
                {
                    compClass = typeof(CompGlower),
                    glowColor = new ColorInt(96, 172, 204),
                    //glowColor = new ColorInt(183, 168, 186),
                    //glowColor = new ColorInt(148, 127, 153),
                    glowRadius = this.Growth * 20,
                    overlightRadius = 0

                });
                base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
                base.Map.glowGrid.RegisterGlower(this.Glower);
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
                    this.nectarAmount++;
                }
                UpdateGlower();
                //ChangeGlower(new ColorInt(148, 127, 153), this.Growth * 20);
                //ChangeGlower(, this.Growth * 20);
            }
            if (Find.TickManager.TicksGame % 350 == 0)
            {
                if (this.Growth >= 0.1f)
                {
                    this.SpreadPlants();
                }
                if (this.Growth >= 0.25f)
                {
                    this.DoDamageToThings(Position);
                }

            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.SpreadTick, "SpreadTick", 0, true);
            Scribe_Values.Look<int>(ref this.OrigSpreadTick, "OrigSpreadTick", 0, true);
            Scribe_Values.Look<int>(ref this.nectarAmount, "nectarAmount", 0, true);
        }
    }
}

