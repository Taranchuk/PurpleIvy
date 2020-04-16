using System;
using Verse;

namespace HediffSpecial
{
	public class DefModExtension_BionicSpecial : DefModExtension
	{
		public int healTicks = 1000;

		public bool regrowParts = true;

		public int growthTicks = 1000;

		public string growthText = "Regrowing: ";

		public HediffDef protoBodyPart;

		public HediffDef curedBodyPart;

		public HediffDef autoHealHediff;
	}
}
