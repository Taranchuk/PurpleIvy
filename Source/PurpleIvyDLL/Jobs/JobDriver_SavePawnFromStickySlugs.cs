using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace PurpleIvy
{
    public class JobDriver_SavePawnFromStickySlugs : JobDriver
    {
        private Thing PawnToRescue
        {
            get
            {
                return (Thing)this.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        private StickySlugs Slugs
        {
            get
            {
                return (StickySlugs)this.job.GetTarget(TargetIndex.B).Thing;
            }
        }

        private bool InSlugs
        {
            get
            {
                return this.Slugs != null;
            }
        }

        private Thing Target
        {
            get
            {
                return this.Slugs ?? this.PawnToRescue;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.Target, this.job, 1, -1, null, errorOnFailed);
        }

        public override string GetReport()
        {
            if (this.InSlugs && this.Slugs.def == PurpleIvyDefOf.PI_StickySlugs)
            {
                return "ReportSavingPawnFromStickySlugs".Translate();
            }
            return base.GetReport();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {

            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);
            yield return Toils_Misc.SetForbidden(TargetIndex.B, false);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
            yield return Toils_General.Wait(300, TargetIndex.None).WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
            yield return Toils_General.Open(TargetIndex.B);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    if (PawnToRescue is Pawn)
                    {
                        Building_Bed building_Bed = RestUtility.FindBedFor((Pawn)PawnToRescue, this.pawn, false, false, false);
                        if (building_Bed == null)
                        {
                            building_Bed = RestUtility.FindBedFor((Pawn)PawnToRescue, this.pawn, false, false, true);
                        }
                        if (building_Bed == null)
                        {
                            string t5;
                            if (pawn.RaceProps.Animal)
                            {
                                t5 = "NoAnimalBed".Translate();
                            }
                            else
                            {
                                t5 = "NoNonPrisonerBed".Translate();
                            }
                            Messages.Message("CannotRescue".Translate() + ": " + t5, pawn, MessageTypeDefOf.RejectInput, false);
                            return;
                        }
                        Job job = JobMaker.MakeJob(JobDefOf.Rescue, pawn, building_Bed);
                        job.count = 1;
                        this.pawn.jobs.TryTakeOrderedJob(job);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield break;
        }

        private static List<IntVec3> tmpCells = new List<IntVec3>();
    }
}
