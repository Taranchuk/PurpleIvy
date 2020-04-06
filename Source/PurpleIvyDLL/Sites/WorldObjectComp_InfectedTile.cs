using System;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PurpleIvy
{
    public class WorldObjectComp_InfectedTile : WorldObjectComp
    {
        public bool Active
        {
            get
            {
                return this.active;
            }
        }

        public override string CompInspectStringExtra()
        {
            if (Prefs.DevMode)
            {
                return this.counter.ToString();
            }
            return base.CompInspectStringExtra();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.active)
            {
                if (Find.TickManager.TicksGame % 250 == 0)
                {
                    this.counter++;
                    if (this.counter > 1000 && this.delay < Find.TickManager.TicksGame)
                    {
                        int num;
                        Predicate<int> predicate = (int x) => !Find.WorldObjects.AnyWorldObjectAt
                        (x, PurpleIvyDefOf.InfectedTile);
                        if (TileFinder.TryFindPassableTileWithTraversalDistance(this.parent.Tile,
                            0, 1, out num, predicate, false, true, false))
                        {
                            Site site = (Site)WorldObjectMaker.MakeWorldObject(PurpleIvyDefOf.InfectedTile);
                            site.Tile = num;
                            site.SetFaction(PurpleIvyData.factionDirect);
                            site.AddPart(new SitePart(site, PurpleIvyDefOf.InfectedSite,
                                PurpleIvyDefOf.InfectedSite.Worker.GenerateDefaultParams
                                (StorytellerUtility.DefaultSiteThreatPointsNow(), num, PurpleIvyData.factionDirect)));
                            site.GetComponent<WorldObjectComp_InfectedTile>().StartQuest();
                            site.GetComponent<WorldObjectComp_InfectedTile>().gameConditionCaused = PurpleIvyDefOf.PurpleFogGameCondition;
                            site.GetComponent<WorldObjectComp_InfectedTile>().counter = 0;
                            site.GetComponent<WorldObjectComp_InfectedTile>().infectedTile = site.Tile;
                            site.GetComponent<TimeoutComp>().StartTimeout(30 * 60000);
                            Find.WorldObjects.Add(site);
                            Find.LetterStack.ReceiveLetter("InfectedTileSpreading".Translate(),
                                "InfectedTileSpreadingDesc".Translate(), LetterDefOf.ThreatBig, site);
                            this.delay = Find.TickManager.TicksGame + new IntRange(220000, 270000).RandomInRange;
                        }
                    }
                }
                if (Find.TickManager.TicksGame % 60 == 0)
                {
                    MapParent mapParent = this.parent as MapParent;
                    if (mapParent != null && mapParent.Map != null)
                    {
                        int count = mapParent.Map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Count;
                        if (count > 0)
                        {
                            Log.Message("Change counter " + counter.ToString() + " to " + count.ToString());
                            this.counter = count;
                        }
                        else if (count <= 0)
                        {
                            this.counter = 0;
                            GameCondition_PurpleFog gameCondition = mapParent.Map
                                .gameConditionManager.GetActiveCondition<GameCondition_PurpleFog>();
                            this.StopQuest();
                        }
                    }
                    PurpleIvyData.TotalFogProgress[this] = PurpleIvyData.getFogProgress(this.counter);
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.active, "active", false, false);
            Scribe_Values.Look<int>(ref this.counter, "counter", 0, false);
            Scribe_Values.Look<int>(ref this.infectedTile, "infectedTile", 0, false);
            Scribe_Values.Look<int>(ref this.delay, "delay", 0, false);
            Scribe_Defs.Look<GameConditionDef>(ref this.gameConditionCaused, "gameConditionCaused");
        }

        public void StartQuest()
        {
            this.active = true;
        }

        public void StopQuest()
        {
            this.active = false;
            Settlement settlement = Find.World.worldObjects.SettlementAt(this.infectedTile);
            bool flag = settlement != null && settlement.HasMap;
            if (flag)
            {
                GameConditionManager gameConditionManager = settlement.Map.gameConditionManager;
                if (gameConditionManager.ConditionIsActive(this.gameConditionCaused))
                {
                    gameConditionManager.ActiveConditions.Remove(gameConditionManager.GetActiveCondition(this.gameConditionCaused));
                }
            }
        }

        public override void PostPostRemove()
        {
            this.StopQuest();
            base.PostPostRemove();
        }

        private bool active;

        public int counter;

        public int infectedTile;

        public int delay;

        public GameConditionDef gameConditionCaused;


    }
}
