using System;
using RimWorld;
using Verse;

namespace EvaineQTraits
{
	public class ThoughtWorker_EyesInteraction : ThoughtWorker
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
			if (!p.story.traits.HasTrait(TraitDefOfEvaineQ.EyesInteractive))
			{
				return ThoughtState.Inactive;
			}
			IntVec3 position = p.Position;
			if (!RestUtility.Awake(p))
			{
				return ThoughtState.Inactive;
			}
			if (p.Map.glowGrid.PsychGlowAt(p.Position) == null)
			{
				return ThoughtState.ActiveAtStage(0);
			}
			if (p.Map.glowGrid.PsychGlowAt(p.Position) == PsychGlow.Overlit)
			{
				return ThoughtState.ActiveAtStage(2);
			}
			return ThoughtState.ActiveAtStage(1);
		}
	}
}
