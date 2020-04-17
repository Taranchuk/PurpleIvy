using System;
using System.Text;
using RimWorld;
using Verse;

namespace HediffSpecial
{
	public class Hediff_RegrowinBodyPart : Hediff_AddedPart
	{
		public override bool ShouldRemove
		{
			get
			{
				return this.Severity >= this.def.maxSeverity;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
		}

		public override string TipStringExtra
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(base.TipStringExtra);
				stringBuilder.AppendLine(Translator.Translate("Efficiency") + ": " + GenText.ToStringPercent(this.def.addedPartProps.partEfficiency));
				stringBuilder.AppendLine(this.pawn.health.hediffSet.GetFirstHediffOfDef(this.def.GetModExtension<DefModExtension_BionicSpecial>().autoHealHediff, false).def.GetModExtension<DefModExtension_BionicSpecial>().growthText + GenText.ToStringPercent(this.Severity));
				return stringBuilder.ToString();
			}
		}

		public override void PostRemoved()
		{
			base.PostRemoved();
			bool flag = this.Severity >= 1f;
			if (flag && this.pawn.health.hediffSet.GetFirstHediffOfDef(this.def.GetModExtension<DefModExtension_BionicSpecial>().autoHealHediff, false).def.TryGetModExtension<DefModExtension_BionicSpecial>().curedBodyPart != null)
			{
				this.pawn.ReplaceHediffFromBodypart(base.Part, HediffDefOf.MissingBodyPart, this.pawn.health.hediffSet.GetFirstHediffOfDef(this.def.GetModExtension<DefModExtension_BionicSpecial>().autoHealHediff, false).def.GetModExtension<DefModExtension_BionicSpecial>().curedBodyPart);
				return;
			}
			if (flag)
			{
				this.pawn.ReplaceHediffFromBodypart(base.Part, HediffDefOf.MissingBodyPart, HediffDefOf_CosmosInd.CosmosTech);
			}
		}
	}
}

