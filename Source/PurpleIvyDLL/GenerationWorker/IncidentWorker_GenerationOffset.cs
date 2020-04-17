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

        private static readonly IntRange TimeoutDaysRange = new IntRange(9, 14);

        private const int minDist = 8;
        private const int maxDist = 14;
		protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && TileFinder.TryFindNewSiteTile(out int tile, minDist, maxDist, false, true, -1) && CommsConsoleUtility.PlayerHasPoweredCommsConsole();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!TileFinder.TryFindNewSiteTile(out var tile, minDist, maxDist, false, true, -1))
            {
                return false;
            }
            var site = (Site)WorldObjectMaker.MakeWorldObject(WorldCoreDefOf.WorldOutpost);
            site.Tile = tile;
            site.AddPart(new SitePart(site, SiteCoreDefOf.OldOutpost, SiteCoreDefOf.OldOutpost.Worker.GenerateDefaultParams(StorytellerUtility.DefaultSiteThreatPointsNow(), tile, Find.FactionManager.RandomEnemyFaction(false, false, false, TechLevel.Industrial))));
            site.SetFaction(Find.FactionManager.RandomEnemyFaction(false, false, false, TechLevel.Industrial));
            site.GetComponent<TimeoutComp>().StartTimeout(TimeoutDaysRange.RandomInRange * 60000);
            if (Find.WorldObjects != null) Find.WorldObjects.Add(site);
            Find.LetterStack.ReceiveLetter("LetterLabelCrashedShip".Translate(), "LetterCrashedShip".Translate(), LetterDefOf.NeutralEvent, site, null);

            return true;
        }
    }
}

