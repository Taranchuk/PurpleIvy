using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
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

        public void fillRadius()
        {
            List<int> tiles = new List<int>();
            Find.WorldFloodFiller.FloodFill(this.infectedTile, (int tile) => true, delegate (int tile, int dist)
            {
                if (dist > this.radius + 1)
                {
                    return true;
                }
                Log.Message("Add tile: " + tile.ToString());
                tiles.Add(tile);
                return false;
            }, int.MaxValue, null);
            foreach (int tile in tiles)
            {
                var origBiome = Find.WorldGrid[tile].biome;
                if (!origBiome.defName.StartsWith("PI_"))
                {
                    Log.Message("Change biome");
                    var infectedBiome = BiomeDef.Named("PI_" + origBiome.defName);
                    Find.WorldGrid[tile].biome = infectedBiome;
                }
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            if (this.active)
            {
                if (Find.TickManager.TicksGame % 600 == 0)
                {
                    this.counter++;
                    int newRadius = (int)(this.counter / 100);
                    if (this.radius != newRadius)
                    {
                        this.radius = newRadius;
                        this.fillRadius();
                    }
                    if (this.counter > 750 && this.delay < Find.TickManager.TicksGame)
                    {
                        int num;
                        Predicate<int> predicate = (int x) => this.infectedTile != x && !Find.WorldObjects.AnyWorldObjectAt
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
                            site.GetComponent<WorldObjectComp_InfectedTile>().radius = (int)(0 / 100);
                            site.GetComponent<WorldObjectComp_InfectedTile>().fillRadius();
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
                            this.counter = count;
                        }
                        else if (count <= 0)
                        {
                            this.counter = 0;
                        }
                        bool temp;
                        if (PurpleIvyData.getFogProgressWithOuterSources(this.counter, this.parent.GetComponent<WorldObjectComp_InfectedTile>(), out temp) <= 0f)
                        {
                            this.StopQuest();
                        }
                    }
                    PurpleIvyData.TotalFogProgress[this] = PurpleIvyData.getFogProgress(this.counter);
                }
            }
        }

        public List<Pawn> generateAliens()
        {
            List<Pawn> list = new List<Pawn>();

            return null;
        }
        public void AlienAmbush(Caravan caravan)
        {
            LongEventHandler.QueueLongEvent(delegate ()
            {
                IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow
                (IncidentCategoryDefOf.ThreatBig, caravan);
                List<Pawn> list = this.generateAliens();
                Map map = CaravanIncidentUtility.SetupCaravanAttackMap(caravan, list, false);
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, null, false, false, null, false);
                }
                Find.TickManager.CurTimeSpeed = 0;
                GlobalTargetInfo globalTargetInfo = (!GenCollection.Any<Pawn>(list))
                ? GlobalTargetInfo.Invalid : new GlobalTargetInfo(list[0].Position, map, false);
                Find.LetterStack.ReceiveLetter("AlienAmbush".Translate(), "AlienAmbushDesc".Translate()
                    , LetterDefOf.ThreatBig, globalTargetInfo, null, null, null, null);
            }, "GeneratingMapForNewEncounter", false, null, true);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.active, "active", false, false);
            Scribe_Values.Look<int>(ref this.counter, "counter", 0, false);
            Scribe_Values.Look<int>(ref this.infectedTile, "infectedTile", 0, false);
            Scribe_Values.Look<int>(ref this.AlienPower, "AlienPower", 0, false);
            Scribe_Values.Look<int>(ref this.AlienPowerSpent, "AlienPowerSpent", 0, false);
            Scribe_Values.Look<int>(ref this.radius, "radius", 0, false);
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
            MapParent infectedSite = Find.World.worldObjects.ObjectsAt(this.infectedTile) as MapParent;
            if (infectedSite != null && infectedSite.Map != null)
            {
                GameConditionManager gameConditionManager = infectedSite.Map.gameConditionManager;
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

        public int radius;

        public int infectedTile;

        public int delay;

        public int AlienPower;

        public int AlienPowerSpent;

        public GameConditionDef gameConditionCaused;

    }
}
