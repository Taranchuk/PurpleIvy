using System;
using RimWorld;
using Verse;

namespace EvaineQBionics
{
	[DefOf]
	public static class JobDefOf
	{
		static JobDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
		}

		public static JobDef BlueVomit;
	}
}

