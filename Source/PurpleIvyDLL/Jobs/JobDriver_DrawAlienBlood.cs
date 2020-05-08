using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    public class JobDriver_DrawAlienBlood : JobDriver_DoBill
    {
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.workCycleProgress, "workCycleProgress", 1f, false);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            base.AddEndCondition(delegate ()
            {
                var thing = base.GetActor().jobs.curJob.GetTarget(TargetIndex.A).Thing;
                if (thing is Building && !thing.Spawned)
                {
                    return JobCondition.Incompletable;
                }
                return JobCondition.Ongoing;
            });
            this.FailOnBurningImmobile<JobDriver_DoBill>(TargetIndex.A);
            this.FailOn<JobDriver_DoBill>(delegate ()
            {
                if (!(this.job.GetTarget(TargetIndex.A).Thing is IBillGiver billGiver)) return false;
                if (this.job.bill.DeletedOrDereferenced)
                {
                    return true;
                }
                if (!billGiver.CurrentlyUsableForBills())
                {
                    return true;
                }
                return false;
            });
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            var tableThing = this.job.GetTarget(TargetIndex.A).Thing as Building_СontainmentBreach;
            var refuelableComp = tableThing.GetComp<CompRefuelable>();  //I think you should check this for Null, but i'm not sure where are you using it.
            var toil = new Toil
            {
                initAction = delegate ()
                {
                    this.job.bill.Notify_DoBillStarted(this.pawn);
                    this.workCycleProgress = this.job.bill.recipe.workAmount;
                },
                tickAction = delegate ()
                {
                    this.workCycleProgress -= this.pawn.GetStatValue(StatDefOf.WorkToMake, true);
                    tableThing.UsedThisTick();
                    if (!tableThing.CurrentlyUsableForBills() || (refuelableComp != null && !refuelableComp.HasFuel))
                    {
                        this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true, true);
                    }

                    if (!(this.workCycleProgress <= 0f)) return;
                    var workSkill = this.job.bill.recipe.workSkill;
                    if (workSkill != null)
                    {
                        SkillRecord skill = this.pawn.skills.GetSkill(workSkill);
                        skill?.Learn(0.11f * this.job.bill.recipe.workSkillLearnFactor, false);
                    }
                    GenSpawn.Spawn(tableThing.GetAlienBloodByRecipe(this.job.bill.recipe),
                        tableThing.InteractionCell, tableThing.Map, 0);
                    Toils_Reserve.Release(TargetIndex.A);
                    this.pawn.GainComfortFromCellIfPossible(false);
                    this.job.bill.Notify_IterationCompleted(this.pawn, null);
                    this.ReadyForNextToil();
                },
                defaultCompleteMode = ToilCompleteMode.Never
            };
            toil.WithEffect(() => this.job.bill.recipe.effectWorking, TargetIndex.A);
            toil.PlaySustainerOrSound(() => toil.actor.CurJob.bill.recipe.soundWorking);
            toil.WithProgressBar(TargetIndex.A, delegate ()
            {
                return PurpleIvyUtils.GetPercentageFromPartWhole
                (this.job.bill.recipe.workAmount - this.workCycleProgress,
                (int)this.job.bill.recipe.workAmount) / 100f;
            }, false, 0.5f);
            toil.FailOn<Toil>(delegate ()
            {
                return this.job.bill.suspended || this.job.bill.DeletedOrDereferenced || (this.job.GetTarget(TargetIndex.A).Thing is IBillGiver billGiver && !billGiver.CurrentlyUsableForBills());
            });
            yield return toil;
            yield break;
        }

        private float workCycleProgress;
    }
}
