using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace PurpleIvy
{
    public class JobDriver_JumpToTarget : JobDriver
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
                if (thing != actor.pather.Destination.Thing || (!this.pawn.pather.Moving && !this.pawn.Position.AdjacentTo8WayOrInside(thing)))
                {
                    RoofDef roofDef = actor.Map.roofGrid.RoofAt(actor.Position);
                    if (roofDef != null)
                    {
                        if (roofDef != RoofDefOf.RoofConstructed)
                        {
                            return;
                        }
                        if (!SoundDefHelper.NullOrUndefined(roofDef.soundPunchThrough))
                        {
                            SoundStarter.PlayOneShot(roofDef.soundPunchThrough, new TargetInfo(actor.Position, actor.Map, false));
                            CellRect.CellRectIterator iterator = CellRect.CenteredOn(actor.Position, 1).GetIterator();
                            while (!iterator.Done())
                            {
                                Find.CurrentMap.roofGrid.SetRoof(iterator.Current, null);
                                iterator.MoveNext();
                            }
                        }
                    }
                    RoofDef roofDef2 = actor.Map.roofGrid.RoofAt(thing.Position);
                    if (roofDef2 != null)
                    {
                        if (roofDef2 != RoofDefOf.RoofConstructed)
                        {
                            return;
                        }
                        if (!SoundDefHelper.NullOrUndefined(roofDef2.soundPunchThrough))
                        {
                            SoundStarter.PlayOneShot(roofDef2.soundPunchThrough, new TargetInfo(actor.Position, actor.Map, false));
                            CellRect.CellRectIterator iterator2 = CellRect.CenteredOn(thing.Position, 1).GetIterator();
                            while (!iterator2.Done())
                            {
                                Find.CurrentMap.roofGrid.SetRoof(iterator2.Current, null);
                                iterator2.MoveNext();
                            }
                        }
                    }
                }
                actor.pather.StartPath(thing, PathEndMode.Touch);
                actor.Position = thing.Position;
                actor.pather.StopDead();
                pawn1.pather.StopDead();
                pawn1.TakeDamage(new DamageInfo(PurpleIvyDefOf.AlienToxicSting, 10, 0f, -1f, actor, null, null));
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

