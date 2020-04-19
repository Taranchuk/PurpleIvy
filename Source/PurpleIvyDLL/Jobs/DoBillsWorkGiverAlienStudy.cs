using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    public class DoBillsWorkGiverAlienStudy : WorkGiver_DoBill
    {
        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            Job job = base.JobOnThing(pawn, thing, forced);
            RecipeWorkerWithJob recipeWorkerWithJob = new RecipeWorkerWithJob();
            bool flag;
            if (job != null && job.def == JobDefOf.DoBill)
            {
                recipeWorkerWithJob = (job.RecipeDef.Worker as RecipeWorkerWithJob);
                flag = (recipeWorkerWithJob != null);
            }
            else
            {
                flag = false;
            }
            bool flag2 = flag;
            Job result;
            if (flag2)
            {
                result = new Job(recipeWorkerWithJob.AlienStudy, job.targetA)
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