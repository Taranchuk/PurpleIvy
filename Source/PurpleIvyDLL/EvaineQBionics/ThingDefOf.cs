using System;
using RimWorld;
using Verse;

namespace EvaineQBionics
{
	[DefOf]
	public static class ThingDefOf
	{
		static ThingDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
		}

		public static ThingDef Filth_BlueVomit;
	}
}

