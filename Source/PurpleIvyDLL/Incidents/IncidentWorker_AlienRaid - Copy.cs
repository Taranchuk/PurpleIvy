using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PurpleIvy
{
    public class IncidentWorker_CrashedShip : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            int num;
            return base.CanFireNowSub(parms) && TileFinder.TryFindNewSiteTile(out num);
        }

        public List<Thing> GenerateRewards()
        {
            ThingSetMakerParams value = default(ThingSetMakerParams);
            value.techLevel = TechLevel.Spacer;
            value.countRange = new IntRange?(new IntRange(1, 1));
            value.totalMarketValueRange = new FloatRange?(new FloatRange(500f, 3000f));
            return ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(value);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = parms.target as Map;
            int tile = 0; ;
            if (map == null)
            {
                return false;
            }
            else if (!TileFinder.TryFindNewSiteTile(out tile))
            {
                return false;
            }
            Site site = SiteMaker.MakeSite(PurpleIvyDefOf.PI_CrashedShip, tile, PurpleIvyData.KorsolianFaction);
            Find.WorldObjects.Add(site);
            base.SendStandardLetter(parms, site);
            return true;
        }
    }
}

