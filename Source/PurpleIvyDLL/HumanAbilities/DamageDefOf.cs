using System;
using RimWorld;
using Verse;

namespace HumanAbilities
{
	[DefOf]
	public static class DamageDefOf
	{
		static DamageDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(DamageDefOf));
		}

		public static DamageDef RedFire;
	}
}

