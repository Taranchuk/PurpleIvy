using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    public class JobDriver_DrawAlienBlood : JobDriver_DoBillPlus
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
        protected override Toil DoBill()
        {
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
                    var thing = this.job.GetTarget(TargetIndex.B).Thing;
                    if (thing == null || thing.Destroyed)
                    {
                        this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true, true);
                    }
                    this.workCycleProgress -= this.pawn.GetStatValue(StatDefOf.WorkToMake, true);
                    tableThing.UsedThisTick();
                    if (!tableThing.CurrentlyUsableForBills() || (refuelableComp != null && !refuelableComp.HasFuel))
                    {
                        this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true, true);
                    }

                    if (!(this.workCycleProgress <= 0f)) return;
                    var workSkill = this.job.RecipeDef.workSkill;
                    if (workSkill != null)
                    {
                        SkillRecord skill = this.pawn.skills.GetSkill(workSkill);
                        skill?.Learn(0.11f * this.job.RecipeDef.workSkillLearnFactor, false);
                    }
                    GenSpawn.Spawn(tableThing.GetAlienBloodByRecipe(this.job.bill.recipe),
                        tableThing.InteractionCell, thing.Map, 0);
                    Toils_Reserve.Release(TargetIndex.B);
                    Toils_Reserve.Release(TargetIndex.C);
                    Toils_Reserve.Release(TargetIndex.A);
                    this.pawn.GainComfortFromCellIfPossible(false);
                    var list = new List<Thing> { thing };
                    this.job.bill.Notify_IterationCompleted(this.pawn, list);
                    this.ReadyForNextToil();
                    Log.Message(thing.Label);
                    thing.Destroy(DestroyMode.Vanish);
                },
                defaultCompleteMode = ToilCompleteMode.Never
            };
            toil.WithEffect(() => this.job.bill.recipe.effectWorking, TargetIndex.A);
            toil.PlaySustainerOrSound(() => toil.actor.CurJob.bill.recipe.soundWorking);
            toil.WithProgressBar(TargetIndex.A, delegate ()
            {
                Thing thing = this.job.GetTarget(TargetIndex.B).Thing;
                return (float)thing.HitPoints / (float)thing.MaxHitPoints;
            }, false, 0.5f);
            toil.FailOn<Toil>(delegate ()
            {
                return this.job.bill.suspended || this.job.bill.DeletedOrDereferenced || (this.job.GetTarget(TargetIndex.A).Thing is IBillGiver billGiver && !billGiver.CurrentlyUsableForBills());
            });
            return toil;
        }

        private float workCycleProgress;
    }
}

