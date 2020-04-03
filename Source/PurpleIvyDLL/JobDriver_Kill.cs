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
                        Pawn victim = (Pawn)thing;
                        if (actor.meleeVerbs.TryMeleeAttack(thing, null, true))
                        {
                            this.numMeleeAttacksLanded++;
                            if (this.numMeleeAttacksLanded >= curJob.maxNumMeleeAttacks)
                            {
                                this.EndJobWith(JobCondition.Succeeded);
                            }
                            if (10f >= Rand.Range(0f, 100f))
                            {
                                if (thing.TryGetComp<AlienInfection>() == null)
                                {
                                    ThingDef dummyCorpse = PurpleIvyDefOf.InfectedCorpseDummy;
                                    AlienInfection comp = new AlienInfection();
                                    comp.Initialize(dummyCorpse.GetCompProperties<CompProperties_AlienInfection>());
                                    comp.parent = victim;
                                    comp.Props.typesOfCreatures = new List<string>()
                                    {
                                        actor.kindDef.defName
                                    };
                                    if (actor.kindDef.defName == PurpleIvyDefOf.Genny_ParasiteAlpha.defName)
                                    {
                                        IntRange range = new IntRange(1, 1);
                                        comp.totalNumberOfCreatures = range.RandomInRange;
                                        comp.Props.maxNumberOfCreatures = range;
                                    }
                                    else if (actor.kindDef.defName == PurpleIvyDefOf.Genny_ParasiteBeta.defName)
                                    {
                                        IntRange range = new IntRange(1, 3);
                                        comp.totalNumberOfCreatures = range.RandomInRange;
                                        comp.Props.maxNumberOfCreatures = range;
                                    }
                                    else if (actor.kindDef.defName == PurpleIvyDefOf.Genny_ParasiteOmega.defName)
                                    {
                                        IntRange range = new IntRange(1, 10);
                                        comp.totalNumberOfCreatures = range.RandomInRange;
                                        comp.Props.maxNumberOfCreatures = range;
                                    }
                                    Log.Message("comp.Props.maxNumberOfCreatures - " +
                                            comp.Props.maxNumberOfCreatures.ToString());                                    comp.Props.incubationPeriod = new IntRange(10000, 40000);
                                    comp.Props.IncubationData = new IncubationData();
                                    comp.Props.IncubationData.tickStartHediff = new IntRange(2000, 4000);
                                    comp.Props.IncubationData.deathChance = 90f;
                                    comp.Props.IncubationData.hediff = HediffDefOf.Pregnant.defName;
                                    victim.AllComps.Add(comp);
                                    Log.Message("Adding infected comp to living creature " + victim.Label);
                                }
                            }
                        }
                        try
                        {
                            if (victim.Dead)
                            {
                                var comp = victim.TryGetComp<AlienInfection>();
                                Corpse corpse = (Corpse)victim.ParentHolder;
                                if (comp != null)
                                {
                                    Log.Message("Moving infected comp from living creature to corpse " + corpse.Label);
                                    Log.Message("comp.Props.maxNumberOfCreatures - " +
                                    comp.Props.maxNumberOfCreatures.ToString());
                                    corpse.AllComps.Add(comp);
                                }
                                if (corpse.TryGetComp<AlienInfection>() == null)
                                {
                                    if (10f >= Rand.Range(0f, 100f))
                                    {
                                        Log.Message(thing.Label + " killed! Now trying to attach an infected comp");
                                        if (corpse.TryGetComp<AlienInfection>() == null)
                                        {
                                            ThingDef dummyCorpse = PurpleIvyDefOf.InfectedCorpseDummy;
                                            comp = new AlienInfection();
                                            comp.Initialize(dummyCorpse.GetCompProperties<CompProperties_AlienInfection>());
                                            comp.parent = corpse;
                                            comp.Props.typesOfCreatures = new List<string>()
                                            {
                                                actor.kindDef.defName
                                            };
                                            if (actor.kindDef.defName == PurpleIvyDefOf.Genny_ParasiteAlpha.defName)
                                            {
                                                IntRange range = new IntRange(1, 1);
                                                comp.totalNumberOfCreatures = range.RandomInRange;
                                                comp.Props.maxNumberOfCreatures = range;
                                            }
                                            else if (actor.kindDef.defName == PurpleIvyDefOf.Genny_ParasiteBeta.defName)
                                            {
                                                IntRange range = new IntRange(1, 3);
                                                comp.totalNumberOfCreatures = range.RandomInRange;
                                                comp.Props.maxNumberOfCreatures = range;
                                            }
                                            else if (actor.kindDef.defName == PurpleIvyDefOf.Genny_ParasiteOmega.defName)
                                            {
                                                IntRange range = new IntRange(1, 10);
                                                comp.totalNumberOfCreatures = range.RandomInRange;
                                                comp.Props.maxNumberOfCreatures = range;
                                            }
                                            corpse.AllComps.Add(comp);
                                            Log.Message("Adding infected comp to " + corpse.Label);
                                        }
                                    }
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

