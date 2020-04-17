using System;
using RimWorld;
using Verse;

namespace HumanUpgraded
{
	[DefOf]
	public static class DamageDefOf
	{
		static DamageDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(DamageDefOf));
		}

		public static DamageDef GreenFire;
	}
}

