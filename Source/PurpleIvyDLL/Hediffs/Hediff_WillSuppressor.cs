using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PurpleIvy
{
	public class Hediff_WillSuppressor : HediffWithComps
	{
		public override void PostMake()
		{
			base.PostMake();
		}

		public override void ExposeData()
		{
			base.ExposeData();
		}

		public override void Tick()
		{
			base.Tick();
			Log.Message(this.CurStageIndex.ToString(), true);
			if (this.CurStageIndex == 1)
			{
				InteractionWorker_RecruitAttempt.DoRecruit(null, this.pawn, 1f, true);
			}
		}
	}
}

