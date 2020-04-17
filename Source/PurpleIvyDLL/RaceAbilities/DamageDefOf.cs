using System;
using RimWorld;
using Verse;

namespace RaceAbilities
{
	[DefOf]
	public static class DamageDefOf
	{
		static DamageDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(DamageDefOf));
		}

		public static DamageDef BlueFire;
	}
}

