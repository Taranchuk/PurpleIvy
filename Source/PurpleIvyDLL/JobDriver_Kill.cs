using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    public class JobDriver_Kill : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil followAndAttack = new Toil();
            followAndAttack.tickAction = delegate ()
            {
                Pawn actor = followAndAttack.actor;
                Job curJob = actor.jobs.curJob;
                Thing thing = curJob.GetTarget(this.a).Thing;
                Pawn pawn = thing as Pawn;
                if (thing != actor.pather.Destination.Thing || (!this.pawn.pather.Moving && !GenAdj.AdjacentTo8WayOrInside(this.pawn.Position, thing)))
                {
                    actor.pather.StartPath(thing, PathEndMode.Touch);
                }
                else
                {
                    if (GenAdj.AdjacentTo8WayOrInside(this.pawn.Position, thing))
                    {
                        if (thing is Pawn && pawn.Downed && !curJob.killIncappedTarget)
                        {
                            this.EndJobWith(JobCondition.Succeeded);
                        }
                        if (actor.meleeVerbs.TryMeleeAttack(thing, null, false))
                        {
                            this.numMeleeAttacksLanded++;
                            if (this.numMeleeAttacksLanded >= curJob.maxNumMeleeAttacks)
                            {
                                this.EndJobWith(JobCondition.Succeeded);
                            }
                        }
                    }
                }
            };
            followAndAttack.defaultCompleteMode = ToilCompleteMode.Never;
            ToilFailConditions.EndOnDespawnedOrNull<Toil>(followAndAttack, this.a, JobCondition.Succeeded);
            ToilFailConditions.FailOn<Toil>(followAndAttack, new Func<bool>(this.hunterIsKilled));
            yield return followAndAttack;
            yield break;
        }

        private bool hunterIsKilled()
        {
            return this.pawn.Dead || this.pawn.HitPoints == 0;
        }

        private int numMeleeAttacksLanded;

        public TargetIndex a = TargetIndex.A;
    }
}