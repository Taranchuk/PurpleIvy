using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    public class DoBillsWorkGiverDrawKorsolianToxin : WorkGiver_DoBill
    {
        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            Job job = base.JobOnThing(pawn, thing, forced);
            //Log.Message(job.targetA.Thing.Label);
            //var workTable = (Building_СontainmentBreach)job.targetB.Thing;
            RecipeWorkerWithJob recipeWorkerWithJob = new RecipeWorkerWithJob();
            bool flag;
            //workTable.innerContainer.Any &&
            var billGiver = job?.bill?.billStack?.billGiver;
            if (billGiver is Building_СontainmentBreach building_WorkTable 
                && building_WorkTable.HasJobOnRecipe(job.RecipeDef))
            {
                recipeWorkerWithJob = (job.RecipeDef.Worker as RecipeWorkerWithJob);
                flag = (recipeWorkerWithJob != null);
            }
            else
            {
                flag = false;
                job = null;
            }
            Job result;
            if (flag)
            {
                result = new Job(recipeWorkerWithJob.DrawAlienBlood, job.targetA, job.targetB)
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

