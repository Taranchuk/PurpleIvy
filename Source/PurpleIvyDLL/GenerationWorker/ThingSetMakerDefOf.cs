using System;
using RimWorld;

namespace GenerationWorker
{
	[DefOf]
	public static class ThingSetMakerDefOf
	{
		static ThingSetMakerDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ThingSetMakerDefOf));
		}

		public static ThingSetMakerDef Gen_OldOutpost;

		public static ThingSetMakerDef RewardOptions;
	}
}
