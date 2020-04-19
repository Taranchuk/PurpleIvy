using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    public abstract class JobDriver_DoBillPlus : JobDriver_DoBill
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            base.AddEndCondition(delegate ()
            {
                Thing thing = base.GetActor().jobs.curJob.GetTarget(TargetIndex.A).Thing;
                if (thing is Building && !thing.Spawned)
                {
                    return JobCondition.Incompletable;
                }
                return JobCondition.Ongoing;
            });
            ToilFailConditions.FailOnBurningImmobile<JobDriver_DoBill>(this, TargetIndex.A);
            ToilFailConditions.FailOn<JobDriver_DoBill>(this, delegate ()
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
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
            yield return Toils_Reserve.ReserveQueue(TargetIndex.B, 1, -1, null);
            yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B, true);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false, false);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            yield return Toils_JobTransforms.SetTargetToIngredientPlaceCell(TargetIndex.A, TargetIndex.B, TargetIndex.C);
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.A, null, false, false);
            yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
            yield return this.DoBill();
            yield break;
        }

        protected abstract Toil DoBill();
    }
}

