using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace EvaineQMentalWorker
{
	public class JobDriver_StormMark : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		private static Toil AbuseTarget(Pawn target)
		{
			Toil toil = new Toil();
			toil.AddFailCondition(() => target == null || target.Destroyed || target.Downed || !target.Spawned || target.Dead);
			toil.socialMode = 0;
			toil.initAction = delegate()
			{
				Pawn actor = toil.GetActor();
				if (Rand.Value < 0.3f)
				{
					actor.interactions.TryInteractWith(target, InteractionDefOf.Chitchat);
					return;
				}
				actor.interactions.TryInteractWith(target, InteractionDefOf.DeepTalk);
			};
			return toil;
		}

		private static Toil ReachTarget(Pawn target)
		{
			Toil toil = new Toil();
			toil.AddFailCondition(() => target == null || target.Destroyed || target.Downed || !target.Spawned || target.Dead);
			toil.socialMode = 0;
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			toil.initAction = delegate()
			{
				toil.GetActor().pather.StartPath(target, PathEndMode.Touch);
			};
			return toil;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			ToilFailConditions.FailOnDespawnedOrNull<JobDriver_StormMark>(this, TargetIndex.A);
			ToilFailConditions.FailOnDowned<JobDriver_StormMark>(this, TargetIndex.A);
			Pawn target = base.TargetA.Thing as Pawn;
			yield return JobDriver_StormMark.ReachTarget(target);
			yield return JobDriver_StormMark.AbuseTarget(target);
			yield break;
		}
	}
}
