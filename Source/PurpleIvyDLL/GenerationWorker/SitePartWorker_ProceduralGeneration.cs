using System;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace GenerationWorker
{
	public class SitePartWorker_ProceduralGeneration : SitePartWorker
	{
		public override string GetArrivedLetterPart(Map map, out LetterDef preferredLetterDef, out LookTargets lookTargets)
		{
			string arrivedLetterPart = base.GetArrivedLetterPart(map, out preferredLetterDef, out lookTargets);
			lookTargets = (from x in map.mapPawns.AllPawnsSpawned
			where x.RaceProps.Humanlike && GenHostility.HostileTo(x, Faction.OfPlayer)
			select x).FirstOrDefault<Pawn>();
			return arrivedLetterPart;
		}



        public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
        {
            return string.Concat(new object[]
			{
				base.GetPostProcessedThreatLabel(site, sitePart),
				" (",
				this.GetEnemiesCount(site, sitePart.parms),
				" ",
				Translator.Translate("Enemies"),
				")"
			});
		}

        public override SitePartParams GenerateDefaultParams(float myThreatPoints, int tile, Faction faction)
        {
            return base.GenerateDefaultParams(myThreatPoints, tile, faction);
        }
        //public override SiteCoreOrPartParams GenerateDefaultParams(Site site, float myThreatPoints)
        //{
        //	SiteCoreOrPartParams siteCoreOrPartParams = base.GenerateDefaultParams(site, myThreatPoints);
        //	siteCoreOrPartParams.threatPoints = Mathf.Max(siteCoreOrPartParams.threatPoints, site.Faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
        //	return siteCoreOrPartParams;
        //}

        private int GetEnemiesCount(Site site, SitePartParams parms)
		{
			return PawnGroupMakerUtility.GeneratePawnKindsExample(new PawnGroupMakerParms
			{
				tile = site.Tile,
				faction = site.Faction,
				groupKind = PawnGroupKindDefOf.Combat,
				points = parms.threatPoints,
				inhabitants = true,
				seed = new int?(OutpostSitePartUtility.GetPawnGroupMakerSeed(parms))
			}).Count<PawnKindDef>();
		}
	}
}
