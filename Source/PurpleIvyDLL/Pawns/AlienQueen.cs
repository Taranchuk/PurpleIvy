using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using System.Linq;

namespace PurpleIvy
{
    public class AlienQueen : Pawn
    {
        private int spawnticks = new IntRange(15000, 30000).RandomInRange;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.SetFactionDirect(PurpleIvyData.AlienFaction);
            foreach (IntVec3 current in GenAdj.CellsAdjacentCardinal(this))
            {
                if (GenGrid.InBounds(current, this.Map))
                {
                    if (current.GetPlant(this.Map) == null)
                    {
                        if (!GenCollection.Any<Thing>(GridsUtility.GetThingList(current, this.Map), (Thing t) => (t.def.IsBuildingArtificial || t.def.IsNonResourceNaturalRock)))
                        {
                            Plant newNest = (Plant)ThingMaker.MakeThing(ThingDef.Named("PI_Nest"));
                            GenSpawn.Spawn(newNest, current, this.Map);
                        }
                    }
                    else
                    {
                        Plant plant = current.GetPlant(this.Map);
                        if (plant.def.defName != "PI_Nest")
                        {
                            if (!GenCollection.Any<Thing>(GridsUtility.GetThingList(current, this.Map), (Thing t) => (t.def.IsBuildingArtificial || t.def.IsNonResourceNaturalRock)))
                            {
                                plant.Destroy();
                                Plant newNest = (Plant)ThingMaker.MakeThing(ThingDef.Named("PI_Nest"));
                                GenSpawn.Spawn(newNest, current, this.Map);
                            }
                        }
                    }
                }
            }
        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            var hediff = this.health.hediffSet.hediffs
.FirstOrDefault((Hediff h) => h.def == PurpleIvyDefOf.PI_CrashlandedDowned);
            if (hediff != null)
            {
                this.health.hediffSet.hediffs.Remove(hediff);
                RestUtility.Awake(this);
                this.health.Reset();
            }
            base.PostApplyDamage(dinfo, totalDamageDealt);
        }
        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame > this.recoveryTick)
            {
                var hediff = this.health.hediffSet.hediffs
                .FirstOrDefault((Hediff h) => h.def == PurpleIvyDefOf.PI_CrashlandedDowned);
                if (hediff != null)
                {
                    this.health.hediffSet.hediffs.Remove(hediff);
                    RestUtility.Awake(this);
                    this.health.Reset();
                }
            }
            spawnticks--;
            if (spawnticks == 0)
            {
                if (GenGrid.InBounds(this.Position, this.Map))
                {
                    if (this.Position.GetPlant(this.Map) == null)
                    {
                        if (!GenCollection.Any<Thing>(GridsUtility.GetThingList(this.Position, this.Map), (Thing t) => (t.def.IsBuildingArtificial || t.def.IsNonResourceNaturalRock)))
                        {
                            Plant newNest = (Plant)ThingMaker.MakeThing(ThingDef.Named("PI_Nest"));
                            GenSpawn.Spawn(newNest, this.Position, this.Map);
                        }
                    }
                    else
                    {
                        Plant plant = this.Position.GetPlant(this.Map);
                        if (plant.def.defName != "PI_Nest")
                        {
                            if (!GenCollection.Any<Thing>(GridsUtility.GetThingList(this.Position, this.Map), (Thing t) => (t.def.IsBuildingArtificial || t.def.IsNonResourceNaturalRock)))
                            {
                                plant.Destroy();
                                Plant newNest = (Plant)ThingMaker.MakeThing(ThingDef.Named("PI_Nest"));
                                GenSpawn.Spawn(newNest, this.Position, this.Map);
                            }
                        }
                    }
                }
                spawnticks = new IntRange(15000, 30000).RandomInRange;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.recoveryTick, "recoveryTick", 0);
        }

        public int recoveryTick = 0;
    }
}

