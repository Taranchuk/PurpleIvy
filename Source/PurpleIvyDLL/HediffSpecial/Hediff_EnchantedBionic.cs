using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace HediffSpecial
{
	public class Hediff_EnchantedBionic : HediffWithComps
	{
		public override void PostMake()
		{
			base.PostMake();
			this.SetNextHealTick();
			this.SetNextGrowTick();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.ticksUntilNextHeal, "ticksUntilNextHeal", 0, false);
		}

		public override void Tick()
		{
			base.Tick();
			if (Current.Game.tickManager.TicksGame >= this.ticksUntilNextHeal)
			{
				this.TrySealWounds();
				this.SetNextHealTick();
			}
			if (Current.Game.tickManager.TicksGame >= this.ticksUntilNextGrow && this.def.TryGetModExtension<DefModExtension_BionicSpecial>().regrowParts)
			{
				this.TryRegrowBodyparts();
				this.SetNextGrowTick();
			}
		}

		public void TrySealWounds()
		{
			IEnumerable<Hediff> enumerable = from hd in this.pawn.health.hediffSet.hediffs
			where hd.TendableNow(false)
			select hd;
			if (enumerable != null)
			{
				foreach (Hediff hediff in enumerable)
				{
					HediffWithComps hediffWithComps = hediff as HediffWithComps;
					if (hediffWithComps != null)
					{
						HediffComp_TendDuration hediffComp_TendDuration = HediffUtility.TryGetComp<HediffComp_TendDuration>(hediffWithComps);
						hediffComp_TendDuration.tendQuality = 2f;
						hediffComp_TendDuration.tendTicksLeft = Find.TickManager.TicksGame;
						this.pawn.health.Notify_HediffChanged(hediff);
					}
				}
			}
		}

		public void TryRegrowBodyparts()
		{
			if (this.def.TryGetModExtension<DefModExtension_BionicSpecial>().protoBodyPart != null)
			{
				using (IEnumerator<BodyPartRecord> enumerator = this.pawn.GetFirstMatchingBodyparts(this.pawn.RaceProps.body.corePart, HediffDefOf.MissingBodyPart, this.def.TryGetModExtension<DefModExtension_BionicSpecial>().protoBodyPart, (Hediff hediff) => hediff is Hediff_AddedPart).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						BodyPartRecord part = enumerator.Current;
						Hediff hediff3 = this.pawn.health.hediffSet.hediffs.First((Hediff hediff) => hediff.Part == part && hediff.def == HediffDefOf.MissingBodyPart);
						if (hediff3 != null)
						{
							this.pawn.health.RemoveHediff(hediff3);
							this.pawn.health.AddHediff(this.def.TryGetModExtension<DefModExtension_BionicSpecial>().protoBodyPart, part, null, null);
							this.pawn.health.hediffSet.DirtyCache();
						}
					}
					return;
				}
			}
			using (IEnumerator<BodyPartRecord> enumerator2 = this.pawn.GetFirstMatchingBodyparts(this.pawn.RaceProps.body.corePart, HediffDefOf.MissingBodyPart, HediffDefOf_CosmosInd.CosmosRegrowingTech, (Hediff hediff) => hediff is Hediff_AddedPart).GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					BodyPartRecord part = enumerator2.Current;
					Hediff hediff2 = this.pawn.health.hediffSet.hediffs.First((Hediff hediff) => hediff.Part == part && hediff.def == HediffDefOf.MissingBodyPart);
					if (hediff2 != null)
					{
						this.pawn.health.RemoveHediff(hediff2);
						this.pawn.health.AddHediff(HediffDefOf_CosmosInd.CosmosRegrowingTech, part, null, null);
						this.pawn.health.hediffSet.DirtyCache();
					}
				}
			}
		}

		public void SetNextHealTick()
		{
			this.ticksUntilNextHeal = Current.Game.tickManager.TicksGame + this.def.TryGetModExtension<DefModExtension_BionicSpecial>().healTicks;
		}

		public void SetNextGrowTick()
		{
			this.ticksUntilNextGrow = Current.Game.tickManager.TicksGame + this.def.TryGetModExtension<DefModExtension_BionicSpecial>().growthTicks;
		}

		public int ticksUntilNextHeal;

		public int ticksUntilNextGrow;
	}
}
