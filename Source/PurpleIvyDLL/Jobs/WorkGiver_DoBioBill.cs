using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    public class WorkGiver_DoBioBill : WorkGiver_DoBill
    {
        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            Job job = base.JobOnThing(pawn, thing, forced);
            Job result = null;
            if (job?.bill?.billStack?.Bills != null)
            {
                var billGiver = job.bill.billStack.billGiver;
                foreach (var bill in job.bill.billStack.Bills)
                {
                    job.bill = bill;
                    try
                    {
                        Log.Message("RECIPE: " + bill.recipe.defName);
                        Log.Message("TARGET A: " + job.targetA.Thing, true);
                        Log.Message("TARGET B: " + job.targetB.Thing, true);
                    }
                    catch { };
                    JobDef jobDef = null;
                    if (bill.recipe != null && billGiver is Building_BioLab building_BioLab
                        && ReservationUtility.CanReserveAndReach
                        (pawn, building_BioLab, PathEndMode.ClosestTouch, DangerUtility.NormalMaxDanger(pawn)
                        , 1, -1, null, false) && building_BioLab.HasJobOnRecipe(job, out jobDef) &&
                        jobDef != null)
                    {
                        try
                        {
                            Log.Message(pawn + " - SUCCESS ----------------", true);
                            Log.Message(job.bill.recipe.defName, true);
                            Log.Message("TARGET A: " + job.targetA.Thing, true);
                            Log.Message("TARGET B: " + job.targetB.Thing, true);
                        }
                        catch
                        {

                        }
                        result = new Job(jobDef, job.targetA, job.targetB)
                        {
                            targetQueueB = job.targetQueueB,
                            countQueue = job.countQueue,
                            haulMode = job.haulMode,
                            bill = job.bill
                        };
                        break;
                    }
                    else
                    {
                        if (job?.bill.recipe != null)
                        {
                            try
                            {
                                Log.Message("FAIL: " + job.bill.recipe.defName, true);
                                Log.Message("TARGET A: " + job.targetA.Thing, true);
                                Log.Message("TARGET B: " + job.targetB.Thing, true);
                                var building_WorkTable2 = (Building_BioLab)billGiver;
                                Log.Message("1" + (job?.RecipeDef != null).ToString());
                                Log.Message("2" + (billGiver is Building_BioLab).ToString());
                                Log.Message("3" + (ReservationUtility.CanReserveAndReach
                                (pawn, building_WorkTable2, PathEndMode.ClosestTouch, DangerUtility.NormalMaxDanger(pawn)
                                , 1, -1, null, false)).ToString());
                                Log.Message("4" + building_WorkTable2.HasJobOnRecipe(job, out jobDef).ToString());
                                Log.Message("6" + (jobDef != null).ToString());

                            }
                            catch
                            {

                            }
                            //Log.Message("----------------", true);
                        }
                    }
                }
            }
            return result;
        }
    }
}

