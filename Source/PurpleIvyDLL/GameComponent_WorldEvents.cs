using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace PurpleIvy
{
    public class GameComponent_WorldEvents : GameComponent
    {
        public GameComponent_WorldEvents()
        {

        }

        public GameComponent_WorldEvents(Game game)
        {

        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            PurpleIvyData.TotalFogProgress = new Dictionary<WorldObjectComp_InfectedTile, float>();
            PurpleIvyData.TotalPollutedBiomes = new List<int>();

    }

    public override void LoadedGame()
        {
            base.LoadedGame();
            PurpleIvyData.TotalFogProgress = new Dictionary<WorldObjectComp_InfectedTile, float>();
            PurpleIvyData.TotalPollutedBiomes = new List<int>();
            foreach (var worldObject in Find.WorldObjects.AllWorldObjects)
            {
                var comp = worldObject.GetComponent<WorldObjectComp_InfectedTile>();
                if (comp != null)
                {
                    PurpleIvyData.TotalFogProgress[comp] = PurpleIvyUtils.getFogProgress(comp.counter);
                    //comp.fillRadius(forced: true); // causes crash when loading a save with infected sites
                }
            }
            for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
            {
                if (Find.WorldGrid[i].biome.defName.Contains("PI_"))
                {
                    PurpleIvyData.TotalPollutedBiomes.Add(i);
                }
            }

        }

        public List<Pawn> generateAliensFrom(WorldObjectComp_InfectedTile site)
        {
            List<Pawn> list = new List<Pawn>();
            int AlphaParasitesCount = site.counter / 33;
            int BetaParasitesCount = site.counter / 22;
            int GammaParasitesCount = site.counter / 22;
            int OmegaParasitesCount = site.counter / 11;

            AlphaParasitesCount = (AlphaParasitesCount / 10) - (site.AlienPowerSpent / 33);
            BetaParasitesCount = (BetaParasitesCount / 10) - (site.AlienPowerSpent / 33);
            GammaParasitesCount = (GammaParasitesCount / 10) - (site.AlienPowerSpent / 33);
            OmegaParasitesCount = (OmegaParasitesCount / 10) - (site.AlienPowerSpent / 33);

            Log.Message("counter: " + site.counter.ToString()
            + " AlphaParasitesCount " + AlphaParasitesCount.ToString()
            + " BetaParasitesCount " + BetaParasitesCount.ToString()
            + " GammaParasitesCount " + GammaParasitesCount.ToString()
            + " OmegaParasitesCount " + OmegaParasitesCount.ToString());

            foreach (var i in Enumerable.Range(1, AlphaParasitesCount))
            {
                Pawn NewPawn = PurpleIvyUtils.GenerateParasite(PurpleIvyData.Genny_ParasiteAlpha);
                list.Add(NewPawn);
            }
            foreach (var i in Enumerable.Range(1, BetaParasitesCount))
            {
                Pawn NewPawn = PurpleIvyUtils.GenerateParasite(PurpleIvyData.Genny_ParasiteBeta);
                list.Add(NewPawn);
            }

            foreach (var i in Enumerable.Range(1, GammaParasitesCount))
            {
                Pawn NewPawn = PurpleIvyUtils.GenerateParasite(PurpleIvyData.Genny_ParasiteGamma);
                list.Add(NewPawn);
            }
            foreach (var i in Enumerable.Range(1, OmegaParasitesCount))
            {
                Pawn NewPawn = PurpleIvyUtils.GenerateParasite(PurpleIvyData.Genny_ParasiteOmega);
                list.Add(NewPawn);
            }
            site.AlienPowerSpent = AlphaParasitesCount + BetaParasitesCount
                + GammaParasitesCount + OmegaParasitesCount;
            return list;
        }
        public void AlienAmbush(Caravan caravan, WorldObjectComp_InfectedTile site)
        {
            LongEventHandler.QueueLongEvent(delegate ()
            {
                IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow
                (IncidentCategoryDefOf.ThreatBig, caravan);
                List<Pawn> list = this.generateAliensFrom(site);
                Map map = CaravanIncidentUtility.SetupCaravanAttackMap(caravan, list, false);
                Find.TickManager.CurTimeSpeed = 0;
                GlobalTargetInfo globalTargetInfo = (!GenCollection.Any<Pawn>(list))
                ? GlobalTargetInfo.Invalid : new GlobalTargetInfo(list[0].Position, map, false);
                Find.LetterStack.ReceiveLetter("AlienAmbush".Translate(), "AlienAmbushDesc".Translate()
                    , LetterDefOf.ThreatBig, globalTargetInfo, null, null, null, null);
            }, "GeneratingMapForNewEncounter", false, null, true);
        }

        public void AlienRaid(Map map, WorldObjectComp_InfectedTile site)
        {
            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow
            (IncidentCategoryDefOf.ThreatBig, map);
            Log.Message("ALIEN RAID: map tile - " + map.Tile + " - infected site tile: " + site.infectedTile);
            List<Pawn> list = this.generateAliensFrom(site);
            Log.Message((map != null).ToString());
            incidentParms.target = map;
            Dictionary<Pawn, int> pawnList = new Dictionary<Pawn, int>();
            foreach (Pawn alien in list)
            {
                pawnList[alien] = 0;
            }
            incidentParms.pawnGroups = pawnList;
            Find.Storyteller.incidentQueue.Add(PurpleIvyDefOf.PI_AlienRaid, Find.TickManager.TicksGame, incidentParms, 0);
        }

        public List<WorldObjectComp_InfectedTile> getInfectedTilesNearby(int tile)
        {
            List<WorldObjectComp_InfectedTile> InfectedTilesNearby = new List<WorldObjectComp_InfectedTile>();
            foreach (KeyValuePair<WorldObjectComp_InfectedTile, float> data in PurpleIvyData.TotalFogProgress)
            {
                if (data.Key.counter >= 250 && tile != data.Key.infectedTile && Find.WorldGrid.TraversalDistanceBetween
                    (tile, data.Key.infectedTile, true, int.MaxValue) <= data.Key.radius)
                {
                    InfectedTilesNearby.Add(data.Key);
                }
            }
            return InfectedTilesNearby;
        }
        public override void GameComponentTick()
        {
            base.GameComponentTick();
            bool temp;
            if (PurpleIvyData.BiomesDirty == true)
            {
                PurpleIvyUtils.UpdateBiomes();
            }
            if (Find.TickManager.TicksGame % 3451 == 0) // same as for toxic weather
            {
                bool raidHappened = false;
                var tempComp = new WorldObjectComp_InfectedTile();
                Log.Message("Total polluted biomes to check: " + PurpleIvyData.TotalPollutedBiomes.Count);
                foreach (var tile in PurpleIvyData.TotalPollutedBiomes)
                {
                    var worldObjects = Find.WorldObjects.ObjectsAt(tile);
                    foreach (var worldObject in worldObjects)
                    {
                        if (worldObject is Caravan caravan)
                        {
                            tempComp.infectedTile = caravan.Tile;
                            float fogProgress = PurpleIvyUtils.getFogProgressWithOuterSources(0, tempComp, out temp);
                            if (fogProgress > 0f)
                            {
                                foreach (Pawn p in caravan.pawns)
                                {
                                    if (p.Faction?.def?.defName != PurpleIvyDefOf.Genny.defName && p.RaceProps.IsFlesh)
                                    {
                                        float num = fogProgress / 20; //TODO: balance it
                                        num *= p.GetStatValue(StatDefOf.ToxicSensitivity, true);
                                        if (num != 0f)
                                        {
                                            HealthUtility.AdjustSeverity(p, HediffDefOf.ToxicBuildup, num);
                                        }
                                    }
                                }
                            }
                            if (raidHappened != true)
                            {
                                var infectedSites = getInfectedTilesNearby(worldObject.Tile);
                                if (infectedSites != null && infectedSites.Count > 0)
                                {
                                    if (fogProgress > 0.7f)
                                    {
                                        fogProgress = 0.7f;
                                    }
                                    int raidChance = (int)(fogProgress * 100);
                                    System.Random random = new System.Random(caravan.Tile);
                                    Log.Message("An attempt to ambush caravan, raid chance: " + raidChance.ToString()
                                        + " - fogProgress: " + fogProgress.ToString());
                                    if (raidChance >= random.Next(1, 100))
                                    {
                                        Log.Message("The caravan has been ambushed! RaidChance: " + raidChance.ToString()
                                            + " - fogProgress: " + fogProgress.ToString());
                                        this.AlienAmbush(caravan, infectedSites.RandomElement());
                                        raidHappened = true;
                                    }
                                }
                            }
                        }
                        else if (worldObject is MapParent mapParent)
                        {
                            tempComp.infectedTile = mapParent.Tile;
                            float? fogProgress = null;
                            if (raidHappened != true && mapParent.Map != null)
                            {
                                var infectedSites = getInfectedTilesNearby(mapParent.Tile);
                                if (infectedSites != null && infectedSites.Count > 0)
                                {
                                    fogProgress = PurpleIvyUtils.getFogProgressWithOuterSources(0, tempComp, out temp);
                                    int raidChance = (int)(fogProgress * 100) / 10;
                                    System.Random random = new System.Random();
                                    int randomChance = random.Next(1, 100);
                                    Log.Message("An attempt to raid map, raid chance: " + raidChance.ToString()
                                        + " - randomChance: " + randomChance.ToString()
                                        + " - fogProgress: " + fogProgress.ToString());
                                    if (raidChance >= randomChance)
                                    {
                                        Log.Message("Alien Raid! RaidChance: " + raidChance.ToString()
                                            + " - fogProgress: " + fogProgress.ToString() + " map: " + mapParent.Map.ToString());
                                        this.AlienRaid(mapParent.Map, infectedSites.RandomElement());
                                        raidHappened = true;
                                    }
                                }
                            }
                            if (mapParent.Faction != Faction.OfPlayer && mapParent.Faction != PurpleIvyData.AlienFaction)
                            {
                                if (!fogProgress.HasValue)
                                {
                                    fogProgress = PurpleIvyUtils.getFogProgressWithOuterSources(0, tempComp, out temp);
                                }
                                if (fogProgress > 0f)
                                {
                                    int abandonChance = (int)(fogProgress * 100) / 10;
                                    System.Random random = new System.Random(Find.TickManager.TicksGame);
                                    Log.Message(mapParent.Faction.ToString());
                                    Log.Message("An attempt to abandon NPC base, abandon chance: " + abandonChance.ToString()
                                        + " - fogProgress: " + fogProgress.ToString());
                                    if (abandonChance >= random.Next(1, 100))
                                    {
                                        Log.Message("NPC base abandoned! Chance: " + abandonChance.ToString()
                                            + " - fogProgress: " + fogProgress.ToString() + " base: " + mapParent.ToString());
                                        Site site = (Site)WorldObjectMaker.MakeWorldObject(PurpleIvyDefOf.PI_AbandonedBase);
                                        site.Tile = mapParent.Tile;
                                        site.SetFaction(mapParent.Faction);
                                        mapParent.Destroy();
                                        Find.LetterStack.ReceiveLetter("AbandonedBase".Translate(), 
                                            "AbandonedBaseDesc".Translate(), LetterDefOf.NeutralEvent,
                                            site, mapParent.Faction, null, null, null);
                                    }
                                    else
                                    {
                                        int defeatChance = (int)(fogProgress * 100) / 10;
                                        random = new System.Random(Find.TickManager.TicksGame + mapParent.Tile);
                                        Log.Message(mapParent.Faction.ToString());
                                        Log.Message("An attempt to defeat NPC base, defeat chance: " + abandonChance.ToString()
                                            + " - fogProgress: " + fogProgress.ToString());
                                        if (defeatChance >= random.Next(1, 100))
                                        {
                                            Log.Message("NPC base defeated! Chance: " + defeatChance.ToString()
                                                + " - fogProgress: " + fogProgress.ToString() + " base: " + mapParent.ToString());
                                            Site site = (Site)WorldObjectMaker.MakeWorldObject(PurpleIvyDefOf.PI_DefeatedBase);
                                            site.Tile = mapParent.Tile;
                                            site.SetFaction(mapParent.Faction);
                                            mapParent.Destroy();
                                            Find.LetterStack.ReceiveLetter("DefeatedBase".Translate(),
                                                "DefeatedBaseDesc".Translate(), LetterDefOf.NegativeEvent,
                                                site, mapParent.Faction, null, null, null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

