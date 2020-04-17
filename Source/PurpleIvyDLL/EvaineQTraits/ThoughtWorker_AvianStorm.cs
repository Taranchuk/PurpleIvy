using System;
using RimWorld;
using Verse;

namespace EvaineQTraits
{
	public class ThoughtWorker_AvianStorm : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!p.Spawned)
			{
				return ThoughtState.Inactive;
			}
			if (!p.RaceProps.Humanlike)
			{
				return ThoughtState.Inactive;
			}
			if (!p.story.traits.HasTrait(TraitDefOfEvaineQ.AvianStorm))
			{
				return ThoughtState.Inactive;
			}
			if (p.Map.weatherManager.RainRate < 0.2f)
			{
				return ThoughtState.Inactive;
			}
			if (p.Map.weatherManager.SnowRate > 0.2f)
			{
				return ThoughtState.Inactive;
			}
			if (p.Map.weatherManager.CurWindSpeedFactor < 0.2f)
			{
				return ThoughtState.Inactive;
			}
			if (GridsUtility.Roofed(p.Position, p.Map))
			{
				return ThoughtState.ActiveAtStage(0);
			}
			return ThoughtState.ActiveAtStage(1);
		}
	}
}

