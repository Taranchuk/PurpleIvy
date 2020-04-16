using System;
using RimWorld;
using Verse;

namespace EvaineQBionics
{
	[DefOf]
	public static class EffecterDefOf
	{
		static EffecterDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(EffecterDefOf));
		}

		public static EffecterDef Blue_Vomit;

		public static EffecterDef BerserkStyle;
	}
}
