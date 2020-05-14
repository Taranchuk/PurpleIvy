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
    public class Plant_Nest : AlienPlant, IAttackTarget
    {
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
                return 0.8f;
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
            if (this.Growth >= 0.25f)
            {
                base.ThrowGasOrAdjustGasSize(4f);
            }
        }

        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            if (dinfo.Instigator is Pawn)
            {
                this.Map.GetComponent<MapComponent_MapEvents>().LastAttacked = Find.TickManager.TicksGame;
            }
            PurpleIvyMoteMaker.ThrowToxicGas(this.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(1f), this.Map, 1f);
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

        public void SpreadPlants()
        {
            //Pick a random direction cell
            IntVec3 dir = new IntVec3();
            //dir = GenAdj.RandomAdjacentCellCardinal(Position);
            dir = GenRadial.RadialCellsAround(this.Position, Convert.ToInt32(this.Growth * 20), true)
                .RandomElement();
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

        public void SpawnFruit()
        {
            var fruit = ThingMaker.MakeThing(PurpleIvyDefOf.PI_NestFruit);
            GenSpawn.Spawn(fruit, GenRadial.RadialCellsAround(this.Position, 1, 1).RandomElement(), this.Map);
            fruit.SetForbidden(true);
        }
        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % 2000 == 0)
            {
                base.TickLong();
                if (this.Growth >= 0.25f)
                {
                    base.ThrowGasOrAdjustGasSize(4f);
                    base.DoDamageToBuildings();
                    this.nectarAmount++;
                }
                if (this.Growth >= 0.75f)
                {
                    this.SpawnFruit();
                }
                UpdateGlower();
            }
            if (Find.TickManager.TicksGame % 350 == 0)
            {
                if (this.Growth >= 0.1f)
                {
                    this.SpreadPlants();
                }
                if (this.Growth >= 0.25f)
                {
                    base.DoDamageToThings();
                }

            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.nectarAmount, "nectarAmount", 0, true);
        }
    }
}

