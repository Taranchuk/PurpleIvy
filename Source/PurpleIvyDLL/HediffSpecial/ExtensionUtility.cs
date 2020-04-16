using System;
using Verse;

namespace HediffSpecial
{
	public static class ExtensionUtility
	{
		public static T TryGetModExtension<T>(this Def def) where T : DefModExtension
		{
			T result;
			if (def.HasModExtension<T>())
			{
				result = def.GetModExtension<T>();
			}
			else
			{
				result = default(T);
			}
			return result;
		}
	}
}
