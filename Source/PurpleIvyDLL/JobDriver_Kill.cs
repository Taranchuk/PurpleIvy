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
                        if (actor.meleeVerbs.TryMeleeAttack(thing, null, true))
                        {
                            this.numMeleeAttacksLanded++;
                            if (this.numMeleeAttacksLanded >= curJob.maxNumMeleeAttacks)
                            {
                                this.EndJobWith(JobCondition.Succeeded);
                            }
                        }
                        try
                        {
                            Pawn victim = (Pawn)thing;
                            if (victim.Dead && 10f >= Rand.Range(0f, 100f))
                            {
                                Log.Message(thing.Label + " killed! Now trying to attach an infected comp");
                                Corpse corpse = (Corpse)victim.ParentHolder;
                                if (corpse.TryGetComp<AlienInfection>() == null)
                                {
                                    CompProperties_AlienInfection compProperties = new CompProperties_AlienInfection();
                                    compProperties.compClass = typeof(AlienInfection);
                                    compProperties.numberOfCreaturesPerSpawn = 1;
                                    compProperties.typesOfCreatures = new List<string>()
                                    {
                                        actor.kindDef.defName
                                    };
                                    compProperties.maxNumberOfCreatures = 20;
                                    compProperties.ageTick = 0;
                                    compProperties.ticksPerSpawn = 10000;
                                    AlienInfection infected = new AlienInfection
                                    {
                                        parent = corpse
                                    };
                                    corpse.AllComps.Add(infected);
                                    infected.Initialize(compProperties);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Message("it seems there is a error" +
                                " with the pawn or it is not killed " + thing.Label);
                            Log.Message(ex.Message);
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