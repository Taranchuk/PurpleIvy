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
            return true;
        }
        protected override Toil DoBill()
        {
            var tableThing = this.job.GetTarget(TargetIndex.A).Thing as Building_СontainmentBreach;
            CompRefuelable refuelableComp = tableThing.GetComp<CompRefuelable>();
            Toil toil = new Toil();
            toil.initAction = delegate ()
            {
                this.job.bill.Notify_DoBillStarted(this.pawn);
                this.workCycleProgress = this.job.bill.recipe.workAmount;
            };
            toil.tickAction = delegate ()
            {
                Thing thing = this.job.GetTarget(TargetIndex.B).Thing;
                if (thing == null || thing.Destroyed)
                {
                    this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true, true);
                }
                this.workCycleProgress -= StatExtension.GetStatValue(this.pawn, StatDefOf.WorkToMake, true);
                tableThing.UsedThisTick();
                if (!tableThing.CurrentlyUsableForBills() || (refuelableComp != null && !refuelableComp.HasFuel))
                {
                    this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true, true);
                }
                if (this.workCycleProgress <= 0f)
                {
                    SkillDef workSkill = this.job.RecipeDef.workSkill;
                    if (workSkill != null)
                    {
                        SkillRecord skill = this.pawn.skills.GetSkill(workSkill);
                        if (skill != null)
                        {
                            skill.Learn(0.11f * this.job.RecipeDef.workSkillLearnFactor, false);
                        }
                    }
                    Log.Message("TEST1");
                    //Pawn donor = null;
                    //if (tableThing.RecoveryBloodData != null)
                    //{
                    //    Log.Message("TEST2");
                    //
                    //    foreach (var alien in tableThing.innerContainer)
                    //    {
                    //        Log.Message("TEST3");
                    //
                    //        if (tableThing.RecoveryBloodData.ContainsKey((Pawn)alien))
                    //        {
                    //            Log.Message("TEST4");
                    //
                    //            if (tableThing.RecoveryBloodData[(Pawn)alien] <= 0)
                    //            {
                    //                Log.Message("TEST5");
                    //
                    //                donor = (Pawn)alien;
                    //                break;
                    //            }
                    //        }
                    //        else
                    //        {
                    //            Log.Message("TEST6");
                    //
                    //            donor = (Pawn)alien;
                    //            break;
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    Log.Message("TEST7");
                    //    Log.Message(tableThing.Aliens.RandomElement().Label);
                    //    Log.Message("TEST7.5");
                    //    donor = (Pawn)tableThing.innerContainer[0];
                    //}
                    //Log.Message("TEST8");
                    //
                    //Log.Message("Donor: " + donor.Label);
                    string aliens = "TESTALIENS\n";
                    if (tableThing.Aliens.Count > 0)
                    {
                        foreach (var alien in tableThing.Aliens)
                        {
                            aliens += alien + "\n";
                        }
                    }
                    Log.Message(aliens);
                    GenSpawn.Spawn(PurpleIvyDefOf.PI_AlphaBlood, tableThing.InteractionCell, thing.Map, 0);
                    Toils_Reserve.Release(TargetIndex.B);
                    Toils_Reserve.Release(TargetIndex.C);
                    Toils_Reserve.Release(TargetIndex.A);
                    PawnUtility.GainComfortFromCellIfPossible(this.pawn, false);
                    List<Thing> list = new List<Thing>();
                    list.Add(thing);
                    this.job.bill.Notify_IterationCompleted(this.pawn, list);
                    this.ReadyForNextToil();
                    Log.Message(thing.Label);
                    thing.Destroy(DestroyMode.Vanish);
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            ToilEffects.WithEffect(toil, () => this.job.bill.recipe.effectWorking, TargetIndex.A);
            ToilEffects.PlaySustainerOrSound(toil, () => toil.actor.CurJob.bill.recipe.soundWorking);
            ToilEffects.WithProgressBar(toil, TargetIndex.A, delegate ()
            {
                Thing thing = this.job.GetTarget(TargetIndex.B).Thing;
                return (float)thing.HitPoints / (float)thing.MaxHitPoints;
            }, false, 0.5f);
            ToilFailConditions.FailOn<Toil>(toil, delegate ()
            {
                IBillGiver billGiver = this.job.GetTarget(TargetIndex.A).Thing as IBillGiver;
                return this.job.bill.suspended || this.job.bill.DeletedOrDereferenced || (billGiver != null && !billGiver.CurrentlyUsableForBills());
            });
            return toil;
        }


        private float workCycleProgress;
    }
}

