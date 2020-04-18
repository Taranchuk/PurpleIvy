using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PurpleIvy
{
    public class GameCondition_PurpleFog : GameCondition
    {
        public override int TransitionTicks
        {
            get
            {
                return 5000;
            }
        }

        public Dictionary<Map, float> fogProgress = new Dictionary<Map, float>();

        public int weatherEndingTick = 0;

        public int weatherAge = 0;

        public WeatherDef purpleFog = PurpleIvyDefOf.PI_PurpleFog;

        public WeatherDef purpleFoggyRain = PurpleIvyDefOf.PI_PurpleFoggyRain;

        public WeatherDef empStorm = PurpleIvyDefOf.PI_EMPStorm;
        public override void Init()
        {
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Critical);
            purpleFog.durationRange = new IntRange(10000, 10000);
            purpleFoggyRain.durationRange = new IntRange(10000, 10000);
            empStorm.durationRange = new IntRange(10000, 10000);
            foreach (Map map in this.AffectedMaps)
            {
                this.fogProgress[map] = 0f;
                int count = map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Count;
                var comp = map.Parent.GetComponent<WorldObjectComp_InfectedTile>();
                if (comp != null)
                {
                    bool temp;
                    this.fogProgress[map] = PurpleIvyData.getFogProgressWithOuterSources(count, comp, out temp);
                    if (this.fogProgress[map] <= 0)
                    {
                        this.End();
                        Find.LetterStack.ReceiveLetter("PurpleFogReceded".Translate(),
                            "PurpleFogRecededDesc".Translate(), LetterDefOf.PositiveEvent);
                    }
                }
                else
                {

                    Log.Message("1 Comp null - GameCondition_PurpleFog");
                }
                weatherAge = map.weatherManager.curWeatherAge;
                bool fog = map.weatherManager.CurWeatherPerceived.overlayClasses
                    .Contains(typeof(WeatherOverlay_Fog));
                if (fog != true)
                {
                    Log.Message("Transitioning to purple fog in the " + map);
                    map.weatherManager.TransitionTo(purpleFog);
                }
                else
                {
                    Log.Message("Transitioning to purple fog in the " + map);
                    map.weatherManager.TransitionTo(WeatherDefOf.Clear);
                    map.weatherManager.TransitionTo(purpleFog);
                    map.weatherManager.curWeatherAge = weatherAge;
                }
            }
        }

        public override void GameConditionTick()
        {
            List<Map> affectedMaps = base.AffectedMaps;
            foreach (Map map in affectedMaps)
            {
                Log.Message(map.weatherManager.curWeather + " - " + map.weatherManager.lastWeather, true);
                if (Find.TickManager.TicksGame % 60 == 0) // for performance
                {
                    int count = map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Count;
                    var comp = map.Parent.GetComponent<WorldObjectComp_InfectedTile>();
                    bool temp;
                    if (comp != null)
                    {
                        this.fogProgress[map] = PurpleIvyData.getFogProgressWithOuterSources(count, comp, out temp);
                        if (this.fogProgress[map] <= 0)
                        {
                            this.End();
                            Find.LetterStack.ReceiveLetter("PurpleFogReceded".Translate(),
                                "PurpleFogRecededDesc".Translate(), LetterDefOf.PositiveEvent);
                        }
                    }
                    else
                    {
                        Log.Message("2 Comp null - GameCondition_PurpleFog");
                    }
                    Log.Message(map + " - total plants " + count.ToString() + " = fog progress - " + this.fogProgress[map].ToString(), true);
                    weatherAge = map.weatherManager.curWeatherAge;
                    bool fog = map.weatherManager.CurWeatherPerceived.overlayClasses
                    .Contains(typeof(WeatherOverlay_Fog));
                    if (map.weatherManager.curWeather != purpleFog &&
                        Find.TickManager.TicksGame > this.weatherEndingTick 
                        || !map.weatherManager.curWeather.defName.StartsWith("PI_"))
                    {
                        weatherEndingTick = new IntRange(10000, 30000).RandomInRange + Find.TickManager.TicksGame;
                        Log.Message("Transitioning to purple fog in the " + map, true);
                        if (fog == true)
                        {
                            map.weatherManager.TransitionTo(WeatherDefOf.Clear);
                            map.weatherManager.TransitionTo(purpleFog);
                        }
                        else
                        {
                            map.weatherManager.TransitionTo(purpleFog);
                        }
                        map.weatherManager.curWeatherAge = weatherAge;
                    }
                }

                if (Find.TickManager.TicksGame % 3451 == 0)
                {
                    Log.Message("weatherEndingTick: " + this.weatherEndingTick + " - TicksGame: " + Find.TickManager.TicksGame, true);
                    if (this.weatherEndingTick < Find.TickManager.TicksGame && this.fogProgress[map] >= 0.20f)
                    {
                        if (Rand.Chance(this.fogProgress[map]) && map.weatherManager.curWeather != purpleFoggyRain)
                        {
                            WeatherOverlay_PurpleRain weatherOverlay = new WeatherOverlay_PurpleRain();
                            foreach (var overlay in purpleFoggyRain.Worker.overlays)
                            {
                                if (overlay is WeatherOverlay_PurpleRain)
                                {
                                    overlay.worldOverlayMat = MaterialPool.MatFrom("Weather/PurpleRainOverlayWorld2", ShaderDatabase.MetaOverlay);
                                    break;
                                }
                            }
                            weatherEndingTick = new IntRange(10000, 20000).RandomInRange + Find.TickManager.TicksGame;
                            Log.Message("Transitioning to purple rain in the " + map, true);
                            bool fog = map.weatherManager.CurWeatherPerceived.overlayClasses
    .Contains(typeof(WeatherOverlay_Fog));
                            weatherAge = map.weatherManager.curWeatherAge;
                            if (fog == true)
                            {
                                map.weatherManager.TransitionTo(WeatherDefOf.Clear);
                                map.weatherManager.TransitionTo(purpleFoggyRain);
                            }
                            else
                            {
                                map.weatherManager.TransitionTo(purpleFoggyRain);
                            }
                            map.weatherManager.curWeatherAge = weatherAge;
                            Find.LetterStack.ReceiveLetter("PurpleRainTripleToxicDamage".Translate(), "PurpleRainTripleToxicDamageDesc".Translate(), LetterDefOf.NeutralEvent, null, null);
                        }
                        else if (Rand.Chance(this.fogProgress[map]) 
                            && map.weatherManager.curWeather != empStorm)
                        {

                            weatherEndingTick = new IntRange(10000, 20000).RandomInRange + Find.TickManager.TicksGame;
                            Log.Message("Transitioning to emp storm in the " + map, true);
                            bool fog = map.weatherManager.CurWeatherPerceived.overlayClasses
.Contains(typeof(WeatherOverlay_Fog));
                            if (map.weatherManager.curWeather.defName.StartsWith("PI_"))
                            {
                                weatherAge = map.weatherManager.curWeatherAge;
                            }
                            if (fog == true)
                            {
                                map.weatherManager.TransitionTo(WeatherDefOf.Clear);
                                map.weatherManager.TransitionTo(empStorm);
                            }
                            else
                            {
                                map.weatherManager.TransitionTo(empStorm);
                            }
                            map.weatherManager.curWeatherAge = weatherAge;
                            Find.LetterStack.ReceiveLetter("EmpStormTripleToxicDamage".Translate(), "EmpStormTripleToxicDamageDesc".Translate(), LetterDefOf.NeutralEvent, null, null);
                        }
                    }
                    this.DoPawnsToxicDamage(map);
                }
            }
        }
        private void DoPawnsToxicDamage(Map map)
        {
            List<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < allPawnsSpawned.Count; i++)
            {
                this.DoPawnToxicDamage(allPawnsSpawned[i]);
            }
        }

        public void DoPawnToxicDamage(Pawn p)
        {
            if (p.Faction?.def?.defName == PurpleIvyDefOf.Genny.defName)
            {
                return;
            }
            if (p.Spawned && p.Position.Roofed(p.Map))
            {
                return;
            }
            if (!p.RaceProps.IsFlesh)
            {
                return;
            }
            float num = this.fogProgress[p.Map] / 20; //TODO: balance it
            num *= p.GetStatValue(StatDefOf.ToxicSensitivity, true);
            if (num != 0f)
            {
                if (p.Map.weatherManager.curWeather == purpleFoggyRain ||
                    p.Map.weatherManager.curWeather == empStorm)
                {
                    num *= 3f;
                }
                HealthUtility.AdjustSeverity(p, HediffDefOf.ToxicBuildup, num);
            }
        }

        public override void DoCellSteadyEffects(IntVec3 c, Map map)
        {
            if (!c.Roofed(map))
            {
                List<Thing> thingList = c.GetThingList(map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Thing thing = thingList[i];
                    if (thing is Plant && thing.def.defName != PurpleIvyDefOf.PurpleIvy.defName)
                    {
                        if (Rand.Value < 0.0065f)
                        {
                            thing.Kill(null, null);
                        }
                    }
                    else if (thing.def.category == ThingCategory.Item)
                    {
                        CompRottable compRottable = thing.TryGetComp<CompRottable>();
                        if (compRottable != null && compRottable.Stage < RotStage.Dessicated)
                        {
                            compRottable.RotProgress += 3000f;
                        }
                    }
                }
            }
        }

        public override float SkyTargetLerpFactor(Map map)
        {
            if (this.fogProgress.ContainsKey(map))
            {
                return this.fogProgress[map];
            }
            else
            {
                Log.Error("Something went wrong with the map " + map + ". It was not represented in " +
                    "the fogProgress dictionary.");
                bool temp;
                var comp = map.Parent.GetComponent<WorldObjectComp_InfectedTile>();
                if (comp == null)
                {
                    Log.Message("Adding new comp, due missing the one in the mapParent");
                    comp = new WorldObjectComp_InfectedTile();
                    int count = map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Count;
                    comp.parent = map.Parent;
                    comp.StartInfection();
                    comp.gameConditionCaused = PurpleIvyDefOf.PurpleFogGameCondition;
                    comp.counter = count;
                    comp.infectedTile = map.Tile;
                    comp.radius = comp.GetRadius();
                    PurpleIvyData.TotalFogProgress[comp] = PurpleIvyData.getFogProgress(comp.counter);
                    comp.fillRadius();
                    map.Parent.AllComps.Add(comp);
                    PurpleIvyData.TotalFogProgress[comp] = PurpleIvyData.getFogProgress(count);
                    this.fogProgress[map] = PurpleIvyData.getFogProgressWithOuterSources(count, comp, out temp);
                }
                else
                {
                    this.fogProgress[map] = 0f;
                    Log.Message("The fogProgress value is reset to 0");
                }
                return this.fogProgress[map];
            }
        }

        public override SkyTarget? SkyTarget(Map map)
        {
            return new SkyTarget?(new SkyTarget(0.85f, this.PurpleFogColors, 1f, 1f));
        }

        public override float AnimalDensityFactor(Map map)
        {
            return 0f;
        }

        public override float PlantDensityFactor(Map map)
        {
            return 0f;
        }

        public override bool AllowEnjoyableOutsideNow(Map map)
        {
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.weatherEndingTick, "weatherEndingTick", 0, true);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                purpleFog.durationRange = new IntRange(10000, 10000);
                purpleFoggyRain.durationRange = new IntRange(10000, 10000);
                empStorm.durationRange = new IntRange(10000, 10000);
                foreach (Map map in AffectedMaps)
                {
                    var comp = map.Parent.GetComponent<WorldObjectComp_InfectedTile>();
                    if (comp != null)
                    {
                        int count = map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Count;
                        bool temp;
                        this.fogProgress[map] = PurpleIvyData.getFogProgressWithOuterSources(count, comp, out temp);
                    }
                    else
                    {
                        Log.Message("3 Comp null - GameCondition_PurpleFog");
                    }
                }
            }
        }

        private const float MaxSkyLerpFactor = 0.5f;

        private const float SkyGlow = 0.85f;

        private SkyColorSet PurpleFogColors =
            new SkyColorSet(
            new Color(0.368f, 0f, 1f),
            new Color(0.920f, 0.920f, 0.920f),
            new Color(0.368f, 0f, 1f), 0.85f);

        //0.368f, 0f, 1f
        //1.0f, 0f, 1.0f

        public const int CheckInterval = 3451;

        private const float ToxicPerDay = 0.5f;

        private const float PlantKillChance = 0.0065f;

        private const float CorpseRotProgressAdd = 3000f;
    }
}

