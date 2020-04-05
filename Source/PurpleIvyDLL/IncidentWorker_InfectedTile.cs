using System;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PurpleIvy
{
    public class IncidentWorker_InfectedTile : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            int num;
            return base.CanFireNowSub(parms) && TileFinder.TryFindNewSiteTile(out num, 7, 27, false, true, -1);
        }

        Faction factionDirect = Find.FactionManager.FirstFactionOfDef(PurpleIvyDefOf.Genny);

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = parms.target as Map;
            bool result;
            int num;
            if (TileFinder.TryFindNewSiteTile(out num, 7, 27, false, true, -1))
            {
                Site site = (Site)WorldObjectMaker.MakeWorldObject(PurpleIvyDefOf.InfectedTile);
                site.Tile = num;
                site.SetFaction(factionDirect);
                site.AddPart(new SitePart(site, PurpleIvyDefOf.InfectedSite, 
                    PurpleIvyDefOf.InfectedSite.Worker.GenerateDefaultParams
                    (StorytellerUtility.DefaultSiteThreatPointsNow(), num, factionDirect)));
                site.GetComponent<WorldObjectComp_InfectedTile>().StartQuest();
                site.GetComponent<WorldObjectComp_InfectedTile>().gameConditionCaused = PurpleIvyDefOf.PurpleFogGameCondition;
                site.GetComponent<WorldObjectComp_InfectedTile>().worldTileAffected = site.Tile;
                int num2 = 30;
                GameCondition gameCondition = GameConditionMaker.MakeCondition(PurpleIvyDefOf.PurpleFogGameCondition, 60000 * num2);
                map.gameConditionManager.RegisterCondition(gameCondition);
                site.GetComponent<TimeoutComp>().StartTimeout(num2 * 60000);
                Find.WorldObjects.Add(site);
                Find.LetterStack.ReceiveLetter("InfectedTile".Translate(), 
                    "InfectedTileDesc".Translate(), LetterDefOf.ThreatBig, site);
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }
    }
}
