using System;
using System.Collections.Generic;
using RimWorld;
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
            var followAndAttack = new Toil();
            followAndAttack.tickAction = delegate ()
            {
                var actor = followAndAttack.actor;
                var curJob = actor.jobs.curJob;
                var thing = curJob.GetTarget(this.a).Thing;
                var pawn1 = thing as Pawn;
                if (thing != actor.pather.Destination.Thing || (!this.pawn.pather.Moving && !this.pawn.Position.AdjacentTo8WayOrInside(thing)))
                {
                    actor.pather.StartPath(thing, PathEndMode.Touch);
                }
                else
                {
                    if (!this.pawn.Position.AdjacentTo8WayOrInside(thing)) return;
                    if (thing is Pawn && pawn1.Downed && !curJob.killIncappedTarget)
                    {
                        this.EndJobWith(JobCondition.Succeeded);
                    }
                    var victim = (Pawn)thing;
                    if (actor.meleeVerbs.TryMeleeAttack(thing, null, true))
                    {
                        this.numMeleeAttacksLanded++;
                        if (this.numMeleeAttacksLanded >= curJob.maxNumMeleeAttacks)
                        {
                            this.EndJobWith(JobCondition.Succeeded);
                        }
                        if (10f >= Rand.Range(0f, 100f))
                        {
                            if (!victim.RaceProps.IsMechanoid && PurpleIvyData.maxNumberOfCreatures.ContainsKey(actor.def.defName) &&
                            thing.TryGetComp<AlienInfection>() == null)
                            {
                                AlienInfectionHediff hediff = (AlienInfectionHediff)HediffMaker.MakeHediff
                                (PurpleIvyDefOf.PI_AlienInfection, victim);
                                hediff.instigator = this.pawn.kindDef;
                                victim.health.AddHediff(hediff);
                            }
                        }
                    }
                }
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

