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
            //if (Rand.Value < 0.5f)
            //{
            //    Job job = JobMaker.MakeJob(JobDefOf.Wait_Combat);
            //    job.expiryInterval = 90;
            //    return job;
            //}
            if (pawn.TryGetAttackVerb(null, false) == null)
            {
                Log.Message(pawn + " JobGiver_GennyBerserk : ThinkNode_JobGiver - return null; - 5", true);
                return null;
            }
            Pawn pawn2 = this.FindPawnTarget(pawn);
            if (pawn2 != null)
            {
                Log.Message(pawn + " choose target: " + pawn2.Label);
                if (!pawn2.Downed)
                {
                    Job job2 = JobMaker.MakeJob(JobDefOf.AttackMelee, pawn2);
                    job2.maxNumMeleeAttacks = 1;
                    job2.expiryInterval = Rand.Range(420, 900);
                    job2.canBash = true;
                    Log.Message(pawn + " return attack " + pawn2.Label);
                    return job2;
                }
                else if (pawn2.Downed)
                {
                    Job job2 = JobMaker.MakeJob(PurpleIvyDefOf.PI_Kill, pawn2);
                    job2.maxNumMeleeAttacks = 1;
                    job2.expiryInterval = Rand.Range(420, 900);
                    job2.canBash = true;
                    job2.killIncappedTarget = true;
                    Log.Message(pawn + " return kill " + pawn2.Label);
                    return job2;
                }
                Log.Error(pawn2.Label + " not downed or downed, " +
                    "idk what’s going on here, but just in case I return null");
                return null;
            }
            Log.Message(pawn + " return null");
            return null;
        }

        private Pawn FindPawnTarget(Pawn pawn)
        {
            Pawn victim = null;
            Predicate<Thing> predicate = (Thing p) => p != null && p != pawn 
            && p.Faction != pawn.Faction;
            List<Pawn> allPawns = pawn.Map.mapPawns.AllPawns;
            victim = (Pawn)GenClosest.ClosestThing_Global_Reachable(pawn.Position,
                pawn.Map, allPawns, PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false)
                , 50f, predicate);

            //
            //victim = (Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable, delegate (Thing x)
            //{
            //    Pawn pawn2 = x as Pawn;
            //    return pawn2 != null && pawn2.Spawned && pawn.CanReach(pawn2, PathEndMode.ClosestTouch, Danger.Deadly,
            //        false, TraverseMode.ByPawn);
            //}, 0f, 50f);
            //if (victim == null)
            //{
            //    List<Pawn> tmpPredatorCandidates = new List<Pawn>();
            //    TraverseParms traverseParms = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
            //    RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region to)
            //        => to.Allows(traverseParms, true), delegate (Region x)
            //        {
            //            List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
            //            for (int j = 0; j < list.Count; j++)
            //            {
            //                tmpPredatorCandidates.Add((Pawn)list[j]);
            //            }
            //            return false;
            //        }, 999999, RegionType.Set_Passable);
            //    Predicate<Thing> predicate = (Thing p) => p != null && p != pawn 
            //    && p.Faction != pawn.Faction;
            //    victim = (Pawn)GenClosest.ClosestThing_Global(pawn.Position,
            //        tmpPredatorCandidates, 50f, predicate);
            //}
            return victim;
        }

        private const float MaxAttackDistance = 40f;

        private const float WaitChance = 0.5f;

        private const int WaitTicks = 90;

        private const int MinMeleeChaseTicks = 420;

        private const int MaxMeleeChaseTicks = 900;
    }
}