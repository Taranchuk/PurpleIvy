using System;
using Verse;
using Verse.AI;

namespace EvaineQMentalWorker
{
	public class MentalStateWorker_StormMark : MentalStateWorker
	{
		public override bool StateCanOccur(Pawn pawn)
		{
			return pawn.Map != null && base.StateCanOccur(pawn) && pawn.Map.mapPawns.FreeColonistsSpawnedCount > 1;
		}
	}
}

