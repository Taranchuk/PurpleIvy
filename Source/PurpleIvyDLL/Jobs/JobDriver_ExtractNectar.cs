using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    public class JobDriver_ExtractNectar : JobDriver
    {
        protected float WorkTotal
        {
            get
            {
                return 400f;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.gatherProgress, "gatherProgress", 0f, false);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil wait = new Toil();
            wait.initAction = delegate ()
            {
                Pawn actor = wait.actor;
                actor.pather.StopDead();
            };
            wait.tickAction = delegate ()
            {
                Pawn actor = wait.actor;
                Plant_Nest nest = (Plant_Nest)this.job.targetA.Thing;
                this.gatherProgress += actor.GetStatValue(StatDefOf.PlantWorkSpeed, true);
                if (this.gatherProgress >= this.WorkTotal)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded, true, true);
                    if (!Rand.Chance(actor.GetStatValue(StatDefOf.PlantHarvestYield, true)))
                    {
                        nest.nectarAmount /= 2;
                        MoteMaker.ThrowText((actor.DrawPos + nest.DrawPos) / 2f, actor.Map, "TextMote_ProductWasted".Translate(), 3.65f);
                    }
                    else
                    {
                        int i = GenMath.RoundRandom((float)1 * (float)nest.nectarAmount);
                        int totalExtracted = 0;
                        while (i > 0)
                        {
                            int num = Mathf.Clamp(i, 1, PurpleIvyDefOf.PI_Nectar.stackLimit);
                            i -= num;
                            totalExtracted += num;
                            Thing thing = ThingMaker.MakeThing(PurpleIvyDefOf.PI_Nectar, null);
                            thing.stackCount = num;
                            GenPlace.TryPlaceThing(thing, actor.Position, actor.Map, ThingPlaceMode.Near, null, null, default(Rot4));
                        }
                        nest.nectarAmount -= totalExtracted;
                        if (nest.nectarAmount < 0)
                        {
                            nest.nectarAmount = 0;
                        }
                    }
                }
            };
            wait.FailOnDespawnedOrNull(TargetIndex.A);
            wait.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            wait.AddEndCondition(delegate
            {
                return JobCondition.Ongoing;
            });
            wait.defaultCompleteMode = ToilCompleteMode.Never;
            wait.WithProgressBar(TargetIndex.A, () => this.gatherProgress / this.WorkTotal, false, -0.5f);
            wait.activeSkill = (() => SkillDefOf.Plants);
            yield return wait;
            yield break;
        }

        private float gatherProgress;

    }
}

