using System;
using Verse;

namespace HediffSpecial
{
	public class RemovableHediff : Hediff
	{
		public override bool ShouldRemove
		{
			get
			{
				return true;
			}
		}
	}
}
