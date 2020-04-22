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
                            if (thing.TryGetComp<AlienInfection>() == null)
                            {
                                var dummyCorpse = PurpleIvyDefOf.InfectedCorpseDummy;
                                var comp = new AlienInfection();
                                comp.Initialize(dummyCorpse.GetCompProperties<CompProperties_AlienInfection>());
                                comp.parent = victim;
                                comp.Props.typesOfCreatures = new List<string>()
                                {
                                    actor.kindDef.defName
                                };
                                if (actor.def.defName == PurpleIvyDefOf.Genny_ParasiteAlpha.defName)
                                {
                                    var range = new IntRange(1, 1);
                                    comp.maxNumberOfCreatures = range.RandomInRange;
                                    comp.Props.maxNumberOfCreatures = range;
                                }
                                else if (actor.def.defName == PurpleIvyDefOf.Genny_ParasiteBeta.defName)
                                {
                                    var range = new IntRange(1, 3);
                                    comp.maxNumberOfCreatures = range.RandomInRange;
                                    comp.Props.maxNumberOfCreatures = range;
                                }
                                else if (actor.def.defName == PurpleIvyDefOf.Genny_ParasiteGamma.defName)
                                {
                                    var range = new IntRange(1, 3);
                                    comp.maxNumberOfCreatures = range.RandomInRange;
                                    comp.Props.maxNumberOfCreatures = range;
                                }
                                else if (actor.def.defName == PurpleIvyDefOf.Genny_ParasiteOmega.defName)
                                {
                                    var range = new IntRange(1, 5);
                                    comp.maxNumberOfCreatures = range.RandomInRange;
                                    comp.Props.maxNumberOfCreatures = range;
                                }
                                else
                                {
                                    Log.Error("2 Something went wrong while adding infected comp: " + comp.parent + " - " + actor);
                                }
                                comp.Props.incubationPeriod = new IntRange(10000, 40000);
                                comp.Props.IncubationData = new IncubationData();
                                comp.Props.IncubationData.tickStartHediff = new IntRange(2000, 4000);
                                comp.Props.IncubationData.deathChance = 90f;
                                comp.Props.IncubationData.hediff = HediffDefOf.Pregnant.defName;
                                victim.AllComps.Add(comp);
                                Log.Message("Adding infected comp to living creature " + victim);
                            }
                        }
                    }
                    try
                    {
                        if (!victim.Dead) return;
                        var comp = victim.TryGetComp<AlienInfection>();
                        var corpse = (Corpse)victim.ParentHolder;
                        if (comp != null)
                        {
                            Log.Message("Moving infected comp from living creature to corpse " + corpse);
                            corpse.AllComps.Add(comp);
                        }

                        if (corpse.TryGetComp<AlienInfection>() != null) return;
                        if (!(10f >= Rand.Range(0f, 100f))) return;
                        Log.Message(thing + " killed! Now trying to attach an infected comp");
                        if (corpse.TryGetComp<AlienInfection>() != null) return;
                        var dummyCorpse = PurpleIvyDefOf.InfectedCorpseDummy;
                        comp = new AlienInfection();
                        comp.Initialize(dummyCorpse.GetCompProperties<CompProperties_AlienInfection>());
                        comp.parent = corpse;
                        comp.Props.typesOfCreatures = new List<string>()
                        {
                            actor.kindDef.defName
                        };
                        if (actor.def.defName == PurpleIvyDefOf.Genny_ParasiteAlpha.defName)
                        {
                            var range = new IntRange(1, 1);
                            comp.maxNumberOfCreatures = range.RandomInRange;
                            comp.Props.maxNumberOfCreatures = range;
                        }
                        else if (actor.def.defName == PurpleIvyDefOf.Genny_ParasiteBeta.defName)
                        {
                            var range = new IntRange(1, 3);
                            comp.maxNumberOfCreatures = range.RandomInRange;
                            comp.Props.maxNumberOfCreatures = range;
                        }
                        else if (actor.def.defName == PurpleIvyDefOf.Genny_ParasiteGamma.defName)
                        {
                            var range = new IntRange(1, 3);
                            comp.maxNumberOfCreatures = range.RandomInRange;
                            comp.Props.maxNumberOfCreatures = range;
                        }
                        else if (actor.def.defName == PurpleIvyDefOf.Genny_ParasiteOmega.defName)
                        {
                            var range = new IntRange(1, 5);
                            comp.maxNumberOfCreatures = range.RandomInRange;
                            comp.Props.maxNumberOfCreatures = range;
                        }
                        else
                        {
                            Log.Error("3 Something went wrong while adding infected comp: " + comp.parent + " - " + actor);
                        }
                        corpse.AllComps.Add(comp);
                        Log.Message("Adding infected comp to " + corpse);
                    }
                    catch (Exception ex)
                    {
                        Log.Message("it seems there is a error" +
                                    " with the pawn or it is not killed " + thing);
                        Log.Message(ex.Message);
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

