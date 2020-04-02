using System;
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

        public float fogProgress = 0f;

        public override void Init()
        {
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Critical);
            WeatherDef purpleFog = PurpleIvyDefOf.PurpleFog;
            purpleFog.durationRange = new IntRange(10000000, 10000000);
            foreach (Map map in this.AffectedMaps)
            {
                map.weatherManager.TransitionTo(purpleFog);
            }
        }

        public override void GameConditionTick()
        {
            List<Map> affectedMaps = base.AffectedMaps;
            int count = Find.CurrentMap.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Count;
            if (count < 250)
            {
                this.fogProgress = 0f;
                if (count < 200)
                {
                    this.End();
                    Find.LetterStack.ReceiveLetter("PurpleFogReceded".Translate(),
                        "PurpleFogRecededDesc".Translate(), LetterDefOf.PositiveEvent);
                }
            }
            else
            {
                count -= 250;
                this.fogProgress = ((float)count / (float)1000 * 100f) / 100f;
            }

            if (Find.TickManager.TicksGame % 3451 == 0)
            {
                for (int i = 0; i < affectedMaps.Count; i++)
                {
                    this.DoPawnsToxicDamage(affectedMaps[i]);
                }
            }
            for (int j = 0; j < this.overlays.Count; j++)
            {
                for (int k = 0; k < affectedMaps.Count; k++)
                {
                    this.overlays[j].TickOverlay(affectedMaps[k]);
                }
            }
        }

        private void DoPawnsToxicDamage(Map map)
        {
            List<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < allPawnsSpawned.Count; i++)
            {
                GameCondition_ToxicFallout.DoPawnToxicDamage(allPawnsSpawned[i]);
            }
        }

        public static void DoPawnToxicDamage(Pawn p)
        {
            if (p.Faction.def == PurpleIvyDefOf.Genny)
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
            return this.fogProgress;
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
            Scribe_Values.Look<float>(ref this.fogProgress, "fogProgress", 0f, true);
        }

        private const float MaxSkyLerpFactor = 0.5f;

        private const float SkyGlow = 0.85f;

        private SkyColorSet PurpleFogColors =
            new SkyColorSet(
            new Color(1.0f, 0f, 1.0f),
            new Color(0.920f, 0.920f, 0.920f),
            new Color(1.0f, 0f, 1.0f), 0.85f);

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