﻿using System;
using System.Collections.Generic;
using RimWorld;
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

        public int delay = 0;

        public override void Init()
        {
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Critical);
            WeatherDef purpleFog = PurpleIvyDefOf.PurpleFog;
            purpleFog.durationRange = new IntRange(10000000, 10000000);
            foreach (Map map in this.AffectedMaps)
            {
                int count = map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Count;
                if (count < 500)
                {
                    this.fogProgress[map] = 0f;
                    if (count < 400)
                    {
                        this.End();
                        Find.LetterStack.ReceiveLetter("PurpleFogReceded".Translate(),
                            "PurpleFogRecededDesc".Translate(), LetterDefOf.PositiveEvent);
                    }
                }
                else
                {
                    count -= 500;
                    this.fogProgress[map] = ((float)count / (float)1500 * 100f) / 100f;
                }
                map.weatherManager.TransitionTo(purpleFog);
            }
        }

        public override void GameConditionTick()
        {
            List<Map> affectedMaps = base.AffectedMaps;
            foreach (Map map in affectedMaps)
            {
                if (Find.TickManager.TicksGame % 60 == 0) // for performance
                {
                    int count = map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Count;
                    if (count < 500)
                    {
                        this.fogProgress[map] = 0f;
                        if (count < 400)
                        {
                            this.End();
                            Find.LetterStack.ReceiveLetter("PurpleFogReceded".Translate(),
                                "PurpleFogRecededDesc".Translate(), LetterDefOf.PositiveEvent);
                        }
                    }
                    else
                    {
                        count -= 500;
                        this.fogProgress[map] = ((float)count / (float)1500 * 100f) / 100f;
                    }
                    Log.Message(map + " - total plants " + count.ToString() + " = fog progress - " + this.fogProgress[map].ToString());
                    if (map.weatherManager.curWeather != PurpleIvyDefOf.PurpleFog &&
                        Find.TickManager.TicksGame > this.weatherEndingTick)
                    {
                        WeatherDef purpleFog = PurpleIvyDefOf.PurpleFog;
                        purpleFog.durationRange = new IntRange(10000000, 10000000);
                        Log.Message("Transitioning to purple fog in the " + map);
                        map.weatherManager.TransitionTo(purpleFog);
                        delay = new IntRange(1000, 10000).RandomInRange + Find.TickManager.TicksGame;
                    }

                }

                if (Find.TickManager.TicksGame % 3451 == 0)
                {
                    Log.Message(map.weatherManager.curWeather.defName);
                    if (map.weatherManager.curWeather != PurpleIvyDefOf.PurpleFoggyRain
                        && this.delay < Find.TickManager.TicksGame)
                    {
                        System.Random random = new System.Random();
                        int chance = (int)(this.fogProgress[map] * 100);
                        Log.Message("Chance of rain: " + chance.ToString());
                        if (random.Next(0, 100) < chance)
                        {
                            Log.Message("Success!");
                            WeatherDef purpleFoggyRain = PurpleIvyDefOf.PurpleFoggyRain;
                            WeatherOverlay_PurpleRain weatherOverlay = new WeatherOverlay_PurpleRain();
                            foreach (var overlay in purpleFoggyRain.Worker.overlays)
                            {
                                if (overlay is WeatherOverlay_PurpleRain)
                                {
                                    overlay.worldOverlayMat = MaterialPool.MatFrom("Weather/PurpleRainOverlayWorld2", ShaderDatabase.MetaOverlay);
                                    break;
                                }
                            }
                            weatherEndingTick = new IntRange(10000, 60000).RandomInRange + Find.TickManager.TicksGame;
                            Log.Message("Transitioning to purple rain in the " + map);
                            map.weatherManager.TransitionTo(purpleFoggyRain);
                        }
                    }
                    this.DoPawnsToxicDamage(map);
                }
                for (int j = 0; j < this.overlays.Count; j++)
                {
                    for (int k = 0; k < affectedMaps.Count; k++)
                    {
                        this.overlays[j].TickOverlay(affectedMaps[k]);
                    }
                }
            }
        }

        private void DoPawnsToxicDamage(Map map)
        {
            List<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < allPawnsSpawned.Count; i++)
            {
                GameCondition_PurpleFog.DoPawnToxicDamage(allPawnsSpawned[i]);
            }
        }

        public static void DoPawnToxicDamage(Pawn p)
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
            float num = 0.028758334f;
            num *= p.GetStatValue(StatDefOf.ToxicSensitivity, true);
            if (num != 0f)
            {
                float num2 = Mathf.Lerp(0.85f, 1.15f, Rand.ValueSeeded(p.thingIDNumber ^ 74374237));
                num *= num2;
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

        public override void GameConditionDraw(Map map)
        {
            for (int i = 0; i < this.overlays.Count; i++)
            {
                this.overlays[i].DrawOverlay(map);
            }
        }

        public override float SkyTargetLerpFactor(Map map)
        {
            return this.fogProgress[map];
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

        public override List<SkyOverlay> SkyOverlays(Map map)
        {
            return this.overlays;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.weatherEndingTick, "weatherEndingTick", 0, true);
            Scribe_Values.Look<int>(ref this.delay, "delay", 0, true);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                foreach (Map map in AffectedMaps)
                {
                    int count = map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Count;
                    if (count < 500)
                    {
                        this.fogProgress[map] = 0f;
                    }
                    else
                    {
                        count -= 500;
                        this.fogProgress[map] = ((float)count / (float)1500 * 100f) / 100f;
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

        private List<SkyOverlay> overlays = new List<SkyOverlay>
        {
            new WeatherOverlay_Fog()
        };

        public const int CheckInterval = 3451;

        private const float ToxicPerDay = 0.5f;

        private const float PlantKillChance = 0.0065f;

        private const float CorpseRotProgressAdd = 3000f;
    }
}
