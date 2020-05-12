using System;
using Verse;

namespace RaceWeapons
{
	public class CompFutureDamage : ThingComp
	{
		public override void Initialize(CompProperties vprops)
		{
			base.Initialize(vprops);
			this.compProp = (vprops as CompProperties_RaceDamage);
			if (this.compProp != null)
			{
				this.damageDef = this.compProp.damageDef;
				this.damageAmount = this.compProp.damageAmount;
				this.chanceToProc = this.compProp.chanceToProc;
				return;
			}
			Log.Message("Error", false);
			this.count = 9876;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<int>(ref this.count, "count", 1, false);
		}

		public CompProperties_RaceDamage compProp;

		public string damageDef;

		public int damageAmount;

		public float chanceToProc;

		public int count;
	}
}

