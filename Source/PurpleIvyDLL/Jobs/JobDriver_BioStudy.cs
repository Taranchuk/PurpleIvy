using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    public class JobDriver_BioStudy : JobDriver_DoBill
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
            var tableThing = this.job.GetTarget(TargetIndex.A).Thing as Building_BioLab;
            CompRefuelable refuelableComp = tableThing.GetComp<CompRefuelable>();
            Toil toil = new Toil();
            toil.initAction = delegate ()
            {
                this.job.bill.Notify_DoBillStarted(this.pawn);
                this.workCycleProgress = this.job.bill.recipe.workAmount;
            };
            toil.tickAction = delegate ()
            {
                this.workCycleProgress -= StatExtension.GetStatValue(this.pawn, StatDefOf.WorkToMake, true);
                tableThing.UsedThisTick();
                if (!tableThing.CurrentlyUsableForBills() || (refuelableComp != null && !refuelableComp.HasFuel))
                {
                    this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true, true);
                }
                if (this.workCycleProgress <= 0f)
                {
                    SkillDef workSkill = this.job.bill.recipe.workSkill;
                    if (workSkill != null)
                    {
                        SkillRecord skill = this.pawn.skills.GetSkill(workSkill);
                        if (skill != null)
                        {
                            skill.Learn(0.11f * this.job.bill.recipe.workSkillLearnFactor, false);
                        }
                    }
                    GenSpawn.Spawn(this.job.targetB.Thing, GetActor().Position, GetActor().Map);
                    Toils_Reserve.Release(TargetIndex.A);
                    PawnUtility.GainComfortFromCellIfPossible(this.pawn, false);
                    this.ReadyForNextToil();
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            ToilEffects.WithEffect(toil, () => this.job.bill.recipe.effectWorking, TargetIndex.A);
            ToilEffects.PlaySustainerOrSound(toil, () => toil.actor.CurJob.bill.recipe.soundWorking);
            ToilEffects.WithProgressBar(toil, TargetIndex.A, delegate ()
            {
                Log.Message((PurpleIvyUtils.GetPercentageFromPartWhole
                (this.job.bill.recipe.workAmount - this.workCycleProgress,
                (int)this.job.bill.recipe.workAmount) / 100f).ToString());
                return PurpleIvyUtils.GetPercentageFromPartWhole
                (this.job.bill.recipe.workAmount - this.workCycleProgress,
                (int)this.job.bill.recipe.workAmount) / 100f;
            }, false, 0.5f);
            ToilFailConditions.FailOn<Toil>(toil, delegate ()
            {
                IBillGiver billGiver = this.job.GetTarget(TargetIndex.A).Thing as IBillGiver;
                return this.job.bill.suspended || this.job.bill.DeletedOrDereferenced || (billGiver != null && !billGiver.CurrentlyUsableForBills());
            });
            yield return toil;
            yield break;
        }

        private float workCycleProgress;
    }
}
