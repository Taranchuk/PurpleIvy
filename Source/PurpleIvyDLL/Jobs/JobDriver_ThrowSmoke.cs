using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace PurpleIvy
{
    public class JobDriver_ThrowSmoke : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var followAndAttack = new Toil();
            followAndAttack.initAction = delegate ()
            {
                var actor = followAndAttack.actor;
                var curJob = actor.jobs.curJob;
                var thing = curJob.GetTarget(this.a).Thing;
                var pawn1 = thing as Pawn;

                pawn1.pather.StopDead();
                pawn1.TakeDamage(new DamageInfo(PurpleIvyDefOf.PI_ToxicExplosion, 3, 0f, -1f, actor, null, null));
                PurpleIvyUtils.DoExplosion(pawn1.Position, actor.Map, 3f, PurpleIvyDefOf.PI_ToxicExplosion, actor);
                if (actor.jobs.curJob != null)
                {
                    actor.jobs.curDriver.Notify_PatherArrived();
                }
                actor.jobs.TryTakeOrderedJob(PurpleIvyUtils.MeleeAttackJob(actor, pawn1));
            };
            followAndAttack.defaultCompleteMode = ToilCompleteMode.Never;
            followAndAttack.EndOnDespawnedOrNull<Toil>(this.a, JobCondition.Succeeded);
            followAndAttack.FailOn<Toil>(new Func<bool>(this.hunterIsKilled));
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

