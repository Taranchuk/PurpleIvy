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

        public override void PostPostRemove()
        {
            Log.Message("PostPostRemove");
            this.counter = 0;
            this.StopInfection();
            this.radius = -1; // -1 is set to allow filRadius remove alien biome in the place
            PurpleIvyData.BiomesDirty = true;
            PurpleIvyData.BiomesToClear = true;
            PurpleIvyUtils.UpdateBiomes();
            base.PostPostRemove();
        }

        public int GetRadius()
        {
            int radius = (int)((this.counter - 500) / 100);
            if (radius < 0)
            {
                radius = 0;
            }
            return radius;
        }

        public void fillRadius(bool forced = false)
        {
            int newRadius = this.GetRadius();
            if (this.radius != newRadius || forced == true)
            {
                //PurpleIvyData.BiomesToRenderNow = new List<int>();
                this.radius = newRadius;
                List<int> tiles = new List<int>();
                PurpleIvyData.BiomesDirty = true;
                if (this.radius < newRadius)
                {
                    PurpleIvyData.BiomesToClear = true;
                }
                Find.WorldFloodFiller.FloodFill(this.infectedTile, (int tile) => true, delegate (int tile, int dist)
                {
                    if (dist > this.radius)
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
                        Log.Message("Change biome: " + tile.ToString());
                        BiomeDef infectedBiome = BiomeDef.Named("PI_" + origBiome.defName);
                        Find.WorldGrid[tile].biome = infectedBiome;
                        PurpleIvyData.TotalPollutedBiomes.Add(tile);
                        PurpleIvyData.BiomesToRenderNow.Add(tile);
                    }
                }
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            if (this.active && this.infected)
            {
                if (Find.TickManager.TicksGame % 600 == 0)
                {
                    this.counter++;
                    this.fillRadius();
                    if (this.counter > 750 && this.delay < Find.TickManager.TicksGame)
                    {
                        int num;
                        Predicate<int> predicate = (int x) => this.infectedTile != x && !Find.WorldObjects.AnyWorldObjectAt
                        (x, PurpleIvyDefOf.PI_InfectedTile);
                        if (TileFinder.TryFindPassableTileWithTraversalDistance(this.parent.Tile,
                            0, 1, out num, predicate, false, true, false))
                        {
                            Site site = null;
                            bool infected = false;
                            if (!Find.WorldObjects.AnyMapParentAt(num))
                            {
                                site = (Site)WorldObjectMaker.MakeWorldObject(PurpleIvyDefOf.PI_InfectedTile);
                                site.Tile = num;
                                site.SetFaction(PurpleIvyData.AlienFaction);
                            }
                            else
                            {
                                var worldObject = Find.WorldObjects.MapParentAt(num);
                                if (worldObject is Site)
                                {
                                    Log.Message("Infect other non-infected sites");
                                    site = (Site)worldObject;
                                    infected = true;
                                }
                                else
                                {
                                    Log.Message("Error: " + worldObject.ToString());
                                }
                            }
                            if (infected == true)
                            {
                                site.AddPart(new SitePart(site, PurpleIvyDefOf.InfectedSite,
                                    PurpleIvyDefOf.InfectedSite.Worker.GenerateDefaultParams
                                    (StorytellerUtility.DefaultSiteThreatPointsNow(), num, PurpleIvyData.AlienFaction)));
                                site.AddPart(new SitePart(site, PurpleIvyDefOf.InfectedSite,
                                    PurpleIvyDefOf.InfectedSite.Worker.GenerateDefaultParams
                                    (StorytellerUtility.DefaultSiteThreatPointsNow(), num, PurpleIvyData.AlienFaction)));
                                var comp = site.GetComponent<WorldObjectComp_InfectedTile>();
                                comp.StartInfection();
                                comp.gameConditionCaused = PurpleIvyDefOf.PurpleFogGameCondition;
                                comp.counter = 0;
                                comp.infectedTile = site.Tile;
                                comp.radius = comp.GetRadius();
                                PurpleIvyData.TotalFogProgress[comp] = PurpleIvyUtils.getFogProgress(comp.counter);
                                comp.fillRadius();
                                site.GetComponent<TimeoutComp>().StartTimeout(30 * 60000);
                                Find.WorldObjects.Add(site);
                                Find.LetterStack.ReceiveLetter("InfectedTileSpreading".Translate(),
                                    "InfectedTileSpreadingDesc".Translate(), LetterDefOf.ThreatBig, site);
                                this.delay = Find.TickManager.TicksGame + new IntRange(220000, 270000).RandomInRange;
                            }
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
                            this.StopInfection();
                        }
                    }
                    PurpleIvyData.TotalFogProgress[this] = PurpleIvyUtils.getFogProgress(this.counter);
                }
            }
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

        public bool infected = true;

        public List<int> pollutedTiles = new List<int>();

        public GameConditionDef gameConditionCaused;

    }
}

