using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace GenerationWorker
{
	public class IncidentWorker_GenerationOffset : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			int num;
			Faction faction;
			return base.CanFireNowSub(parms) && Find.FactionManager.RandomNonHostileFaction(false, false, false, 4) != null && this.TryFindTile(out num) && SiteMakerHelper.TryFindRandomFactionFor(SiteCoreDefOf.OldOutpost, null, ref faction, true, null);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Faction faction = parms.faction;
			if (faction == null)
			{
				faction = Find.FactionManager.RandomNonHostileFaction(false, false, false, 4);
			}
			if (faction == null)
			{
				return false;
			}
			int tile;
			if (!this.TryFindTile(out tile))
			{
				return false;
			}
			SitePartDef sitePart;
			Faction siteFaction;
			if (!SiteMakerHelper.TryFindSiteParams_SingleSitePart(SiteCoreDefOf.OldOutpost, Rand.Chance(1f) ? "SiteTreatSecured" : null, ref sitePart, ref siteFaction, null, true, null))
			{
				return false;
			}
			IntRange questSiteTimeoutDaysRange = SiteTuning.QuestSiteTimeoutDaysRange;
			int randomInRange = questSiteTimeoutDaysRange.RandomInRange;
			Site site = IncidentWorker_GenerationOffset.CreateSite(tile, sitePart, randomInRange, siteFaction);
			List<Thing> list = this.GenerateItems(siteFaction, site.desiredThreatPoints);
			site.GetComponent<ItemStashContentsComp>().contents.TryAddRangeOrTransfer(list, false, false);
			string letterText = this.GetLetterText(faction, list, randomInRange, site, site.parts.FirstOrDefault<SitePart>());
			Find.LetterStack.ReceiveLetter(this.def.letterLabel, letterText, this.def.letterDef, site, faction, null);
			return true;
		}

		private bool TryFindTile(out int tile)
		{
			IntRange itemStashQuestSiteDistanceRange = SiteTuning.ItemStashQuestSiteDistanceRange;
			return TileFinder.TryFindNewSiteTile(ref tile, itemStashQuestSiteDistanceRange.min, itemStashQuestSiteDistanceRange.max, false, true, -1);
		}

		protected virtual List<Thing> GenerateItems(Faction siteFaction, float siteThreatPoints)
		{
			ThingSetMakerParams thingSetMakerParams = default(ThingSetMakerParams);
			thingSetMakerParams.totalMarketValueRange = new FloatRange?(SiteTuning.ItemStashQuestMarketValueRange * SiteTuning.QuestRewardMarketValueThreatPointsFactor.Evaluate(siteThreatPoints));
			return ThingSetMakerDefOf.Gen_OldOutpost.root.Generate(thingSetMakerParams);
		}

		public static Site CreateSite(int tile, SitePartDef sitePart, int days, Faction siteFaction)
		{
			Site site = SiteMaker.MakeSite(SiteCoreDefOf.OldOutpost, sitePart, tile, siteFaction, true, null);
			site.sitePartsKnown = true;
			site.GetComponent<TimeoutComp>().StartTimeout(days * 60000);
			Find.WorldObjects.Add(site);
			return site;
		}

		private string GetLetterText(Faction alliedFaction, List<Thing> items, int days, Site site, SitePart sitePart)
		{
			string result = GenText.CapitalizeFirst(GrammarResolverSimpleStringExtensions.Formatted(this.def.letterText, alliedFaction.leader.LabelShort, alliedFaction.def.leaderTitle, alliedFaction.Name, GenLabel.ThingsLabel(items, "  - "), days.ToString(), SitePartUtility.GetDescriptionDialogue(site, sitePart), GenText.ToStringMoney(GenThing.GetMarketValue(items), null)));
			GenThing.TryAppendSingleRewardInfo(ref result, items);
			return result;
		}
	}
}
