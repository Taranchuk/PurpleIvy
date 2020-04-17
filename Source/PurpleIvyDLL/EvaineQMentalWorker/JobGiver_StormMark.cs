using System;
using Verse;
using Verse.AI;

namespace EvaineQMentalWorker
{
	public class JobGiver_StormMark : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			JobGiver_StormMark job = new JobGiver_StormMark();
			if (pawn.interactions.InteractedTooRecentlyToInteract() || this.lastInteractionTick > Find.TickManager.TicksGame - 500)
			{
				return null;
			}
			Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, 0, false), 9999f, null, null, 0, -1, false, RegionType.Set_Passable, false);
			if (pawn2 == null || Rand.Value > 0.5f)
			{
				return null;
			}
			this.lastInteractionTick = Find.TickManager.TicksGame;
			return new Job(JobDefOfEvaineQMentalWorker.StormMark, pawn2);
		}

		private int lastInteractionTick = -9999;
	}
}

