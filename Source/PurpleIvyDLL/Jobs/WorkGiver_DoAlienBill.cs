using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    public class WorkGiver_DoAlienBill : WorkGiver_DoBill
    {
        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            Job job = base.JobOnThing(pawn, thing, forced);
            var billGiver = job?.bill?.billStack?.billGiver;
            //try
            //{
            //    Log.Message("----------------", true);
            //    Log.Message(job.RecipeDef.defName, true);
            //    Log.Message("TARGET A: " + job.targetA.Thing.Label, true);
            //    Log.Message("TARGET B: " + job.targetB.Thing.Label, true);
            //}
            //catch
            //{
            //
            //}
            //Log.Message("----------------", true);
            JobDef jobDef = null;
            Job result;
            if (job?.RecipeDef != null && billGiver is Building_СontainmentBreach building_WorkTable 
                && building_WorkTable.HasJobOnRecipe(job, out jobDef) && jobDef != null)
            {
                result = new Job(jobDef, job.targetA, job.targetB)
                {
                    targetQueueB = job.targetQueueB,
                    countQueue = job.countQueue,
                    haulMode = job.haulMode,
                    bill = job.bill
                };
            }
            else
            {
                result = job;
            }
            return result;
        }
    }
}

