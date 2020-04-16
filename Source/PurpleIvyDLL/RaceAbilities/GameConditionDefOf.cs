using System;
using RimWorld;
using Verse;

namespace RaceAbilities
{
	[DefOf]
	public static class GameConditionDefOf
	{
		static GameConditionDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(GameConditionDefOf));
		}

		public static GameConditionDef RunicStorm;
	}
}
