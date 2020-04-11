using System;
using System.Collections.Generic;
using System.Linq;
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

        //public override void PostDestroy()
        //{
        //    base.PostDestroy();
        //    Log.Message("PostDestroy");
        //    this.StopInfection();
        //}

        //public override void PostMyMapRemoved()
        //{
        //    Log.Message("PostMyMapRemoved");
        //    this.StopInfection();
        //    base.PostMyMapRemoved();
        //}

        public override void PostPostRemove()
        {
            Log.Message("PostPostRemove");
            this.counter = 0;
            this.StopInfection();
            this.radius = -1;
            this.ClearAlienBiomesOuterTheSources();
            base.PostPostRemove();
        }
        public bool TileNotInRadiusOfOtherSites(int tile)
        {
            foreach (var comp in PurpleIvyData.TotalFogProgress)
            {
                if (Find.WorldGrid.TraversalDistanceBetween
                (comp.Key.infectedTile, tile, true, int.MaxValue) <= comp.Key.radius)
                {
                    return false;
                }
            }
            return true;
        }

        public void ClearAlienBiomesOuterTheSources()
        {
            for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
            {
                if (Find.WorldGrid[i].biome.defName.Contains("PI_"))
                {
                    this.pollutedTiles.Add(i);
                }
            }
            foreach (int tile in this.pollutedTiles)
            {
                if (TileNotInRadiusOfOtherSites(tile))
                {
                    Log.Message("Return old biome");
                    BiomeDef origBiome = Find.WorldGrid[tile].biome;
                    BiomeDef newBiome = BiomeDef.Named(origBiome.defName.ReplaceFirst("PI_", string.Empty));
                    Find.WorldGrid[tile].biome = newBiome;
                    WorldUpdater.WorldUpdater2();
                    WorldUpdater.RenderSingleTile(tile, Find.WorldGrid[tile].biome.DrawMaterial, WorldUpdater.LayersSubMeshes["WorldLayer_Terrain"]);
                }
            }
            this.pollutedTiles.Clear();
        }

        public void fillRadius()
        {
            int newRadius = (int)((this.counter - 500) / 100);
            if (newRadius < 0)
            {
                newRadius = 0;
            }
            if (this.radius != newRadius)
            {
                this.radius = newRadius;
                List<int> tiles = new List<int>();
                Find.WorldFloodFiller.FloodFill(this.infectedTile, (int tile) => true, delegate (int tile, int dist)
                {
                    if (dist > this.radius + 1)
                    {
                        return true;
                    }
                    tiles.Add(tile);
                    return false;
                }, int.MaxValue, null);
                foreach (int tile in tiles)
                {
                    BiomeDef origBiome = Find.WorldGrid[tile].biome;
                    if (!origBiome.defName.StartsWith("PI_"))
                    {
                        Log.Message("Change biome");
                        BiomeDef infectedBiome = BiomeDef.Named("PI_" + origBiome.defName);
                        Find.WorldGrid[tile].biome = infectedBiome;
                        WorldUpdater.WorldUpdater2();
                        WorldUpdater.RenderSingleTile(tile, Find.WorldGrid[tile].biome.DrawMaterial, WorldUpdater.LayersSubMeshes["WorldLayer_Terrain"]);
                    }
                }
            }
            this.ClearAlienBiomesOuterTheSources();
        }
        public override void CompTick()
        {
            base.CompTick();
            Log.Message("Tick");
            if (this.active)
            {
                if (Find.TickManager.TicksGame % 600 == 0)
                {
                    this.counter++;
                    this.fillRadius();
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
                            site.GetComponent<WorldObjectComp_InfectedTile>().StartInfection();
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
                            this.fillRadius();
                        }
                        else if (count <= 0)
                        {
                            this.counter = 0;
                            this.StopInfection();
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

        public void StartInfection()
        {
            this.active = true;
        }

        public void StopInfection()
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
            this.fillRadius();
        }

        private bool active;

        public int counter;

        public int radius;

        public int infectedTile;

        public int delay;

        public int AlienPower;

        public int AlienPowerSpent;

        public List<int> pollutedTiles = new List<int>();

        public GameConditionDef gameConditionCaused;

    }
}
