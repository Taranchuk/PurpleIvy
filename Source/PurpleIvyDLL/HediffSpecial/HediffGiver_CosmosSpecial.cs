using System;
using Verse;

namespace HediffSpecial
{
	public class HediffGiver_CosmosSpecial : HediffGiver
	{
		public override void OnIntervalPassed(Pawn pawn, Hediff cause)
		{
			base.TryApply(pawn, null);
		}
	}
}
