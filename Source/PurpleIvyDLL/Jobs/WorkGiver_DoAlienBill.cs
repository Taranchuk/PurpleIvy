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
            JobDef jobDef = null;
            Job result;
            if (job?.RecipeDef != null && billGiver is Building_СontainmentBreach building_WorkTable 
                && ReservationUtility.CanReserveAndReach
                (pawn, building_WorkTable, PathEndMode.ClosestTouch, DangerUtility.NormalMaxDanger(pawn)
                , 1, -1, null, false) && building_WorkTable.HasJobOnRecipe(job, out jobDef) &&
                (building_WorkTable.innerContainer.Contains(job.targetB.Thing) ||
                ReservationUtility.CanReserveAndReach
                (pawn, job.targetB.Thing, PathEndMode.ClosestTouch, DangerUtility.NormalMaxDanger(pawn)
                , 1, -1, null, false)) &&
                jobDef != null)
            {
                try
                {
                    Log.Message(pawn + " - SUCCESS ----------------", true);
                    Log.Message(job.RecipeDef.defName, true);
                    Log.Message("TARGET A: " + job.targetA.Thing, true);
                    Log.Message("TARGET B: " + job.targetB.Thing, true);
                }
                catch
                {

                }
                Log.Message("----------------", true);
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
                if (job?.RecipeDef != null)
                {
                    try
                    {
                        //Log.Message("FAIL ----------------", true);
                        Log.Message(job.RecipeDef.defName, true);
                        //Log.Message("TARGET A: " + job.targetA.Thing, true);
                        //Log.Message("TARGET B: " + job.targetB.Thing, true);
                        //var building_WorkTable2 = (Building_СontainmentBreach)billGiver;
                        //Log.Message("1" + (job?.RecipeDef != null).ToString());
                        //Log.Message("2" + (billGiver is Building_СontainmentBreach).ToString());
                        //Log.Message("3" + (ReservationUtility.CanReserveAndReach
                        //(pawn, building_WorkTable2, PathEndMode.ClosestTouch, DangerUtility.NormalMaxDanger(pawn)
                        //, 1, -1, null, false)).ToString());
                        //Log.Message("4" + building_WorkTable2.HasJobOnRecipe(job, out jobDef).ToString());
                        //Log.Message("5" + (building_WorkTable2.innerContainer.Contains(job.targetB.Thing) || ReservationUtility.CanReserveAndReach
                        //(pawn, job.targetB.Thing, PathEndMode.ClosestTouch, DangerUtility.NormalMaxDanger(pawn)
                        //, 1, -1, null, false)).ToString());
                        //Log.Message("6" + (jobDef != null).ToString());
                
                    }
                    catch
                    {
                
                    }
                    //Log.Message("----------------", true);
                }
                result = null;
            }
            return result;
        }
    }
}

