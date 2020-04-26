using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using System.Linq;
using Verse.AI;

namespace PurpleIvy
{
    public class AlienQueen : Pawn
    {
        private int spawnticks = new IntRange(15000, 30000).RandomInRange;
        private bool first = true;
        private Thing focus = null;
        public override void PostMake()
        {
            base.PostMake();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
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
                            if (first == true)
                            {
                                PawnDuty duty = new PawnDuty(DutyDefOf.DefendHiveAggressively);
                                this.mindState.duty = duty;
                                this.mindState.duty.focus = newNest;
                                Log.Message("Set focus to " + newNest);
                                focus = newNest;
                                first = false;
                            }
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
                                if (first == true)
                                {
                                    PawnDuty duty = new PawnDuty(DutyDefOf.DefendHiveAggressively);
                                    this.mindState.duty = duty;
                                    this.mindState.duty.focus = newNest;
                                    first = false;
                                    focus = newNest;
                                    Log.Message("Set focus to " + newNest);
                                }
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
                PawnDuty duty = new PawnDuty(DutyDefOf.DefendHiveAggressively);
                this.mindState.duty = duty;
                this.mindState.duty.focus = focus;
                Log.Message("Set focus to " + focus);
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
                    PawnDuty duty = new PawnDuty(DutyDefOf.DefendHiveAggressively);
                    this.mindState.duty = duty;
                    this.mindState.duty.focus = focus;
                    Log.Message("Set focus to " + focus);
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
            Scribe_Values.Look<bool>(ref this.first, "first", true);
            Scribe_References.Look<Thing>(ref this.focus, "focus");
        }

        public int recoveryTick = 0;
    }
}

