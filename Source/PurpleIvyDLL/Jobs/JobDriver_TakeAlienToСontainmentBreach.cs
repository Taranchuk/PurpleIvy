using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    public class JobDriver_TakeAlienToСontainmentBreach : JobDriver
    {
        public JobDriver_TakeAlienToСontainmentBreach()
        {
            this.rotateToFace = TargetIndex.B;
        }
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed);
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
            yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false);
            yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
            yield return DepositHauledThingInContainer(TargetIndex.B, TargetIndex.A);
            yield return Toils_Reserve.Release(TargetIndex.A);
            yield return Toils_Reserve.Release(TargetIndex.B);
            yield break;
        }

        public static Toil DepositHauledThingInContainer(TargetIndex containerInd, TargetIndex reserveForContainerInd)
        {
            Toil toil = new Toil();
            toil.initAction = delegate ()
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                if (actor.carryTracker.CarriedThing == null)
                {
                    Log.Error(actor + " tried to place hauled thing in container but is not hauling anything.", false);
                    return;
                }
                Thing thing = curJob.GetTarget(containerInd).Thing;
                ThingOwner thingOwner = thing.TryGetInnerInteractableThingOwner();
                if (thingOwner != null)
                {
                    int num = actor.carryTracker.CarriedThing.stackCount;
                    if (thing is IConstructible)
                    {
                        num = Mathf.Min(GenConstruct.AmountNeededByOf((IConstructible)thing, actor.carryTracker.CarriedThing.def), num);
                        if (reserveForContainerInd != TargetIndex.None)
                        {
                            Thing thing2 = curJob.GetTarget(reserveForContainerInd).Thing;
                            if (thing2 != null && thing2 != thing)
                            {
                                int num2 = GenConstruct.AmountNeededByOf((IConstructible)thing2, actor.carryTracker.CarriedThing.def);
                                num = Mathf.Min(num, actor.carryTracker.CarriedThing.stackCount - num2);
                            }
                        }
                    }
                    if (actor.carryTracker.innerContainer.TryTransferToContainer(actor.carryTracker.CarriedThing, thingOwner, num, true) != 0)
                    {
                        Building_Grave building_Grave = thing as Building_Grave;
                        if (building_Grave != null)
                        {
                            building_Grave.Notify_CorpseBuried(actor);
                            return;
                        }
                    }
                }
                else
                {
                    if (curJob.GetTarget(containerInd).Thing.def.Minifiable)
                    {
                        actor.carryTracker.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
                        return;
                    }
                    Log.Error("Could not deposit hauled thing in container: " + curJob.GetTarget(containerInd).Thing, false);
                }
            };
            return toil;
        }
    }
}

