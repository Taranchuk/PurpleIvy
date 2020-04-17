using System;
using RimWorld;
using Verse;

namespace RaceWeapons
{
	[DefOf]
	public static class ThingDefOf
	{
		static ThingDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
		}

		public static ThingDef LaserMoteWorker;
	}
}

