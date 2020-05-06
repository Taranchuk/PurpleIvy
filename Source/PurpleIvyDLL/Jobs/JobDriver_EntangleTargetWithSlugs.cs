using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace PurpleIvy
{
    public class JobDriver_EntangleTargetWithSlugs : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected Pawn Takee
        {
            get
            {
                return (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnAggroMentalState(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDestroyedNullOrForbidden(TargetIndex.A)
                .FailOn(() => !this.pawn.CanReach(this.Takee, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
                .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    this.pawn.jobs.curJob.count = 1;
                },
            };
            yield return Toils_General.Wait(100, TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    this.pawn.jobs.curJob.count = 1;
                    var stickySlugs = (StickySlugs)ThingMaker.MakeThing(PurpleIvyDefOf.PI_StickySlugs);
                    GenSpawn.Spawn(stickySlugs, this.pawn.Position, this.pawn.Map);
                    stickySlugs.TryAcceptThing(Takee);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield break;
        }

        public TargetIndex a = TargetIndex.A;
    }
}

