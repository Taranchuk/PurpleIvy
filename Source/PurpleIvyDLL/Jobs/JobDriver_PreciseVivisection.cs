using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    public class JobDriver_PreciseVivisection : JobDriver_DoBill
    {
        public override string GetReport()
        {
            if (this.job.bill.recipe != null)
            {
                return base.ReportStringProcessed(this.job.bill.recipe.jobString);
            }
            return base.GetReport();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.workLeft, "workLeft", 0f, false);
            Scribe_Values.Look<int>(ref this.billStartTick, "billStartTick", 0, false);
            Scribe_Values.Look<int>(ref this.ticksSpentDoingRecipeWork, "ticksSpentDoingRecipeWork", 0, false);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, -1, null, false);
            ////if (!this.pawn.Reserve(this.job.GetTarget(TargetIndex.B), this.job, 1, -1, null, false))
            ////{
            ////    return false;
            ////}
            ////this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.B), this.job, 1, -1, null);
            //return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            base.AddEndCondition(delegate
            {
                Thing thing = base.GetActor().jobs.curJob.GetTarget(TargetIndex.A).Thing;
                if (thing is Building && !thing.Spawned)
                {
                    return JobCondition.Incompletable;
                }
                return JobCondition.Ongoing;
            });
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOn(delegate ()
            {
                IBillGiver billGiver = this.job.GetTarget(TargetIndex.A).Thing as IBillGiver;
                if (billGiver != null)
                {
                    if (this.job.bill.DeletedOrDereferenced)
                    {
                        return true;
                    }
                    if (!billGiver.CurrentlyUsableForBills())
                    {
                        return true;
                    }
                }
                return false;
            });
            Toil gotoBillGiver = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    if (this.job.targetQueueB != null && this.job.targetQueueB.Count == 1)
                    {
                        UnfinishedThing unfinishedThing = this.job.targetQueueB[0].Thing as UnfinishedThing;
                        if (unfinishedThing != null)
                        {
                            unfinishedThing.BoundBill = (Bill_ProductionWithUft)this.job.bill;
                        }
                    }
                }
            };
            //yield return Toils_Jump.JumpIf(gotoBillGiver, () => this.job.GetTargetQueue(TargetIndex.B).NullOrEmpty<LocalTargetInfo>());
            Toil extract = Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B, true);
            yield return extract;
            yield return new Toil
            {
                initAction = delegate ()
                {
                    var alien = TargetB.Thing;
                    var container = (Building_СontainmentBreach)TargetA.Thing;
                    if (container.innerContainer.Contains(alien) && ReservationUtility.CanReserveAndReach
                        (GetActor(), container, PathEndMode.ClosestTouch, DangerUtility.NormalMaxDanger(GetActor())
                        , 1, -1, null, false))
                    {
                        Log.Message(GetActor() + " JUMP");
                        //Toils_Reserve.Reserve(TargetIndex.A, 1);
                        this.JumpToToil(gotoBillGiver);
                    }
                }
            };
            Toil getToHaulTarget = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);//.FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return getToHaulTarget;
            yield return new Toil { initAction = delegate () { base.GetActor().CurJob.count = 1; } };
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, true, false, false);
            yield return JobDriver_PreciseVivisection.JumpToCollectNextIntoHandsForBill(getToHaulTarget, TargetIndex.B);
            //yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDestroyedOrNull(TargetIndex.B);
            yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.A, TargetIndex.B);
            Toil findPlaceTarget = Toils_JobTransforms.SetTargetToIngredientPlaceCell(TargetIndex.A, TargetIndex.B, TargetIndex.C);
            yield return findPlaceTarget;
            //yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, findPlaceTarget, false, false);
            yield return Toils_Jump.JumpIfHaveTargetInQueue(TargetIndex.B, extract);
            extract = null;
            getToHaulTarget = null;
            findPlaceTarget = null;
            yield return gotoBillGiver;
            yield return Toils_Recipe.MakeUnfinishedThingIfNeeded();
            yield return Toils_Recipe.DoRecipeWork().FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);

            yield return new Toil
            {
                initAction = delegate ()
                {
                    var alien = TargetB.Thing;
                    foreach (var product in alien.ButcherProducts(base.GetActor(), 1f))
                    {
                        GenPlace.TryPlaceThing(product, base.GetActor().Position, base.GetActor().Map, ThingPlaceMode.Near);
                    }
                    var container = (Building_СontainmentBreach)TargetA.Thing;
                    container.innerContainer.Remove(alien);
                    alien.Destroy(DestroyMode.Vanish);
                }
            };
            yield return new Toil
            {
                initAction = delegate ()
                {
                    var container = (Building_СontainmentBreach)TargetA.Thing;
                    if (GetActor().Map.reservationManager.ReservedBy(container, GetActor(), GetActor().CurJob))
                    {
                        GetActor().Map.reservationManager.Release(container, GetActor(), GetActor().CurJob);
                    }
                }
            };
            yield return Toils_Recipe.FinishRecipeAndStartStoringProduct();
            if (!this.job.bill.recipe.products.NullOrEmpty<ThingDefCountClass>() || !this.job.bill.recipe.specialProducts.NullOrEmpty<SpecialProductType>())
            {
                yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
                findPlaceTarget = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
                yield return findPlaceTarget;
                //yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, findPlaceTarget, true, true);
                Toil toil = new Toil();
                toil.initAction = delegate ()
                {
                    this.Map.resourceCounter.UpdateResourceCounts();
                };
                yield return toil;
                toil = null;
                findPlaceTarget = null;
            }
            yield return new Toil
            {
                initAction = delegate () {
                    Log.Message("Job ended");
                }
            };
            yield break;
        }

        private static Toil JumpToCollectNextIntoHandsForBill(Toil gotoGetTargetToil, TargetIndex ind)
        {
            Toil toil = new Toil();
            toil.initAction = delegate ()
            {
                Pawn actor = toil.actor;
                if (actor.carryTracker.CarriedThing == null)
                {
                    Log.Error("JumpToAlsoCollectTargetInQueue run on " + actor + " who is not carrying something.", false);
                    return;
                }
                if (actor.carryTracker.Full)
                {
                    return;
                }
                Job curJob = actor.jobs.curJob;
                List<LocalTargetInfo> targetQueue = curJob.GetTargetQueue(ind);
                if (targetQueue.NullOrEmpty<LocalTargetInfo>())
                {
                    return;
                }
                for (int i = 0; i < targetQueue.Count; i++)
                {
                    if (GenAI.CanUseItemForWork(actor, targetQueue[i].Thing) && targetQueue[i].Thing.CanStackWith(actor.carryTracker.CarriedThing) && (float)(actor.Position - targetQueue[i].Thing.Position).LengthHorizontalSquared <= 64f)
                    {
                        int num = (actor.carryTracker.CarriedThing == null) ? 0 : actor.carryTracker.CarriedThing.stackCount;
                        int num2 = curJob.countQueue[i];
                        num2 = Mathf.Min(num2, targetQueue[i].Thing.def.stackLimit - num);
                        num2 = Mathf.Min(num2, actor.carryTracker.AvailableStackSpace(targetQueue[i].Thing.def));
                        if (num2 > 0)
                        {
                            curJob.count = num2;
                            curJob.SetTarget(ind, targetQueue[i].Thing);
                            List<int> countQueue = curJob.countQueue;
                            int index = i;
                            countQueue[index] -= num2;
                            if (curJob.countQueue[i] <= 0)
                            {
                                curJob.countQueue.RemoveAt(i);
                                targetQueue.RemoveAt(i);
                            }
                            actor.jobs.curDriver.JumpToToil(gotoGetTargetToil);
                            return;
                        }
                    }
                }
            };
            return toil;
        }
    }
}

