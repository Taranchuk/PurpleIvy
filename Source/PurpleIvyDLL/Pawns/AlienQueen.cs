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
        private LocalTargetInfo focus = null;
        public override void PostMake()
        {
            base.PostMake();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.SetFaction(PurpleIvyData.AlienFaction);

            int nestCount = 0;
            List<IntVec3> freeTiles = new List<IntVec3>();
            foreach (IntVec3 dir in GenRadial.RadialCellsAround(this.Position, 50, true))
            {
                if (GenGrid.InBounds(dir, this.Map) && this.Map.fertilityGrid.FertilityAt(dir) >= 0.5)
                {
                    var plant = dir.GetPlant(this.Map);
                    if (plant?.def == PurpleIvyDefOf.PI_Nest)
                    {
                        nestCount++;
                    }
                    if (plant?.Faction != PurpleIvyData.AlienFaction && !GenCollection.Any<Thing>
                        (GridsUtility.GetThingList(dir, this.Map), (Thing t) => (t.def.IsBuildingArtificial
                        || t.def.IsNonResourceNaturalRock)))
                    {
                        if (freeTiles.Count < 50)
                        {
                            freeTiles.Add(dir);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (nestCount == 4) break;
            }
            if (nestCount < 4)
            {
                var rnd = new System.Random();
                foreach (IntVec3 current in freeTiles.OrderBy(x => rnd.Next()).Take(4 - nestCount))
                {
                    var plant = current.GetPlant(this.Map);
                    if (plant == null)
                    {
                        Plant newNest = (Plant)ThingMaker.MakeThing(ThingDef.Named("PI_Nest"));
                        GenSpawn.Spawn(newNest, current, this.Map);
                        if (first == true)
                        {
                            PawnDuty duty = new PawnDuty(DutyDefOf.DefendHiveAggressively);
                            this.mindState.duty = duty;
                            this.mindState.duty.focus = new LocalTargetInfo(newNest.Position);
                            Log.Message("Set focus to " + newNest);
                            focus = this.mindState.duty.focus;
                            first = false;
                        }
                    }
                    else
                    {
                        plant.Destroy();
                        Plant newNest = (Plant)ThingMaker.MakeThing(ThingDef.Named("PI_Nest"));
                        GenSpawn.Spawn(newNest, current, this.Map);
                        if (first == true)
                        {
                            PawnDuty duty = new PawnDuty(DutyDefOf.DefendHiveAggressively);
                            this.mindState.duty = duty;
                            this.mindState.duty.focus = new LocalTargetInfo(newNest.Position);
                            first = false;
                            focus = newNest;
                            Log.Message("Set focus to " + newNest);
                        }
                    }
                }
            }
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
        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (base.Dead)
            {
                Log.Message("QUEEN DEEAD");
                AlienInfectionHediff hediff = (AlienInfectionHediff)HediffMaker.MakeHediff
                (PurpleIvyDefOf.PI_AlienInfection, this);
                hediff.instigator = PawnKindDef.Named("Genny_ParasiteOmega");
                var range = new IntRange(30, 50);
                hediff.maxNumberOfCreatures = range.RandomInRange;
                hediff.ageTicks = new IntRange(40000, 50000).RandomInRange;
                this.health.AddHediff(hediff);
                var corpse = (Corpse)this.ParentHolder;
                int nestCount = 0;
                List<IntVec3> freeTiles = new List<IntVec3>();
                foreach (IntVec3 dir in GenRadial.RadialCellsAround(this.Position, 50, true))
                {
                    if (GenGrid.InBounds(dir, corpse.Map) && corpse.Map.fertilityGrid.FertilityAt(dir) >= 0.5)
                    {
                        var plant = dir.GetPlant(corpse.Map);
                        if (plant?.def == PurpleIvyDefOf.PI_Nest)
                        {
                            nestCount++;
                        }
                        if (plant?.Faction != PurpleIvyData.AlienFaction && !GenCollection.Any<Thing>
                            (GridsUtility.GetThingList(dir, corpse.Map), (Thing t) => 
                            (t.def.IsBuildingArtificial || t.def.IsNonResourceNaturalRock)))
                        {
                            if (freeTiles.Count < 50)
                            {
                                freeTiles.Add(dir);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    if (nestCount == 4) break;
                }
                if (nestCount < 4)
                {
                    var rnd = new System.Random();
                    foreach (IntVec3 current in freeTiles.OrderBy(x => rnd.Next()).Take(4 - nestCount))
                    {
                        var plant = current.GetPlant(corpse.Map);
                        if (plant == null)
                        {
                            Plant newNest = (Plant)ThingMaker.MakeThing(ThingDef.Named("PI_Nest"));
                            GenSpawn.Spawn(newNest, current, corpse.Map);
                        }
                        else
                        {
                            plant.Destroy();
                            Plant newNest = (Plant)ThingMaker.MakeThing(ThingDef.Named("PI_Nest"));
                            GenSpawn.Spawn(newNest, current, corpse.Map);
                        }
                    }
                }
            }
            else
            {
                var hediff = this.health.hediffSet.hediffs
.FirstOrDefault((Hediff h) => h.def == PurpleIvyDefOf.PI_CrashlandedDowned);
                if (hediff != null)
                {
                    this.health.hediffSet.hediffs.Remove(hediff);
                    RestUtility.Awake(this);
                }
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
                }
            }
            spawnticks--;
            if (spawnticks <= 0)
            {
                int nestCount = 0;
                List<IntVec3> freeTiles = new List<IntVec3>();
                foreach (IntVec3 dir in GenRadial.RadialCellsAround(this.Position, 50, true))
                {
                    if (GenGrid.InBounds(dir, this.Map) && this.Map.fertilityGrid.FertilityAt(dir) >= 0.5)
                    {
                        var plant = dir.GetPlant(this.Map);
                        if (plant?.def == PurpleIvyDefOf.PI_Nest)
                        {
                            nestCount++;
                        }
                        if (plant?.Faction != PurpleIvyData.AlienFaction && !GenCollection.Any<Thing>
                            (GridsUtility.GetThingList(dir, this.Map), (Thing t) =>
                            (t.def.IsBuildingArtificial || t.def.IsNonResourceNaturalRock)))
                        {
                            if (freeTiles.Count < 50)
                            {
                                freeTiles.Add(dir);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    if (nestCount == 4) break;
                }
                if (nestCount < 4)
                {
                    var rnd = new System.Random();
                    foreach (IntVec3 current in freeTiles.OrderBy(x => rnd.Next()).Take(4 - nestCount))
                    {
                        var plant = current.GetPlant(this.Map);
                        if (plant == null)
                        {
                            Plant newNest = (Plant)ThingMaker.MakeThing(ThingDef.Named("PI_Nest"));
                            GenSpawn.Spawn(newNest, current, this.Map);
                        }
                        else
                        {
                            plant.Destroy();
                            Plant newNest = (Plant)ThingMaker.MakeThing(ThingDef.Named("PI_Nest"));
                            GenSpawn.Spawn(newNest, current, this.Map);
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
            Scribe_TargetInfo.Look(ref this.focus, "focus");
            PawnDuty duty = new PawnDuty(DutyDefOf.DefendHiveAggressively);
            this.mindState.duty = duty;
            this.mindState.duty.focus = focus;
        }

        public int recoveryTick = 0;
    }
}

