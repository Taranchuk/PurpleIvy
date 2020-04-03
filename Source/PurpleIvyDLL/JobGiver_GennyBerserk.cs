using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace PurpleIvy
{
    public class JobGiver_GennyBerserk : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.TryGetAttackVerb(null, false) == null)
            {
                return null;
            }
            Pawn pawn2 = this.FindPawnTarget(pawn);
            if (pawn2 != null)
            {
                if (!pawn2.Downed)
                {
                    return MeleeAttackJob(pawn, pawn2);
                }
                else if (pawn2.Downed)
                {
                    Job job2 = JobMaker.MakeJob(PurpleIvyDefOf.PI_Kill, pawn2);
                    job2.maxNumMeleeAttacks = 1;
                    job2.expiryInterval = Rand.Range(420, 900);
                    job2.canBash = true;
                    job2.killIncappedTarget = true;
                    return job2;
                }
            }
            Building building = this.FindTurretTarget(pawn);
            if (building != null)
            {
                return MeleeAttackJob(pawn, building);
            }
            if (pawn2 != null)
            {
                using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, pawn2.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors, false), PathEndMode.OnCell))
                {
                    if (!pawnPath.Found)
                    {
                        return null;
                    }
                    IntVec3 loc;
                    if (!pawnPath.TryFindLastCellBeforeBlockingDoor(pawn, out loc))
                    {
                        Log.Error(pawn + " did TryFindLastCellBeforeDoor but found none when it should have been one. Target: " + pawn2.LabelCap, false);
                        return null;
                    }
                    IntVec3 randomCell = CellFinder.RandomRegionNear(loc.GetRegion(pawn.Map, RegionType.Set_Passable), 9, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), null, null, RegionType.Set_Passable).RandomCell;
                    if (randomCell == pawn.Position)
                    {
                        return JobMaker.MakeJob(JobDefOf.Wait, 30, false);
                    }
                    return JobMaker.MakeJob(JobDefOf.Goto, randomCell);
                }
            }
            return null;
        }

        private Job MeleeAttackJob(Pawn pawn, Thing target)
        {
            Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
            job.maxNumMeleeAttacks = 1;
            job.expiryInterval = Rand.Range(420, 900);
            job.attackDoorIfTargetLost = true;
            return job;
        }
        private Building FindTurretTarget(Pawn pawn)
        {
            return (Building)AttackTargetFinder.BestAttackTarget(pawn, 
                TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | 
                TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat | 
                TargetScanFlags.NeedAutoTargetable, (Thing t) => t is Building, 0f, 70f, 
                default(IntVec3), float.MaxValue, false, true);
        }

        private Pawn FindPawnTarget(Pawn pawn)
        {
            Pawn victim = null;
            Predicate<Thing> predicate = (Thing p) => p != null && p != pawn 
            && p.Faction != pawn.Faction;
            List<Pawn> allPawns = pawn.Map.mapPawns.AllPawns;
            victim = (Pawn)GenClosest.ClosestThing_Global_Reachable(pawn.Position,
                pawn.Map, allPawns, PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, 
                TraverseMode.PassDoors, false)
                , 50f, predicate);
            return victim;
        }

        private const float MaxAttackDistance = 40f;

        private const float WaitChance = 0.5f;

        private const int WaitTicks = 90;

        private const int MinMeleeChaseTicks = 420;

        private const int MaxMeleeChaseTicks = 900;
    }
}

