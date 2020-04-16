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

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = parms.target as Map;
            bool result;
            int num;
            if (TileFinder.TryFindNewSiteTile(out num, 7, 27, false, true, -1))
            {
                Site site = (Site)WorldObjectMaker.MakeWorldObject(PurpleIvyDefOf.PI_InfectedTile);
                site.Tile = num;
                site.SetFaction(PurpleIvyData.AlienFaction);
                site.AddPart(new SitePart(site, PurpleIvyDefOf.InfectedSite, 
                    PurpleIvyDefOf.InfectedSite.Worker.GenerateDefaultParams
                    (StorytellerUtility.DefaultSiteThreatPointsNow(), num, PurpleIvyData.AlienFaction)));
                var comp = site.GetComponent<WorldObjectComp_InfectedTile>();
                comp.StartInfection();
                comp.gameConditionCaused = PurpleIvyDefOf.PurpleFogGameCondition;
                comp.counter = 600;
                comp.infectedTile = site.Tile;
                comp.radius = comp.GetRadius();
                PurpleIvyData.TotalFogProgress[comp] = PurpleIvyData.getFogProgress(comp.counter);
                comp.fillRadius(true);
                site.GetComponent<TimeoutComp>().StartTimeout(30 * 60000);
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
