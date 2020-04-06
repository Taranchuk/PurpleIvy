using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace PurpleIvy
{
    public class GameComponent_WeatherControl : GameComponent
    {
        public GameComponent_WeatherControl()
        {

        }

        public GameComponent_WeatherControl(Game game)
        {

        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            PurpleIvyData.TotalFogProgress = new Dictionary<WorldObjectComp_InfectedTile, float>(); ;
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            PurpleIvyData.TotalFogProgress = new Dictionary<WorldObjectComp_InfectedTile, float>(); ;
            foreach (var worldObject in Find.WorldObjects.AllWorldObjects)
            {
                var comp = worldObject.GetComponent<WorldObjectComp_InfectedTile>();
                if (comp != null)
                {
                    PurpleIvyData.TotalFogProgress[comp] = comp.counter;
                }
            }
        }
        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (PurpleIvyData.TotalFogProgress != null && Find.TickManager.TicksGame % 60 == 0)
            {
                float totalFogProgress = 0f;
                foreach (var fog in PurpleIvyData.TotalFogProgress)
                {
                    totalFogProgress += fog.Value;
                }
                Log.Message("Total fog progress on the world map: " + totalFogProgress.ToString());
                if (totalFogProgress >= 1f)
                {
                    foreach (Map map in Find.Maps)
                    {
                        if (map.IsPlayerHome)
                        {
                            GameCondition_PurpleFog gameCondition = map.gameConditionManager.GetActiveCondition<GameCondition_PurpleFog>();
                            if (gameCondition == null)
                            {
                                gameCondition =
                                (GameCondition_PurpleFog)GameConditionMaker.MakeConditionPermanent
                                (PurpleIvyDefOf.PurpleFogGameCondition);
                                gameCondition.forcedFogProgress = true;
                                map.gameConditionManager.RegisterCondition(gameCondition);
                                Find.LetterStack.ReceiveLetter("PurpleFogСomesFromInfectedSites.".Translate(),
                                "PurpleFogСomesFromInfectedSitesDesc".Translate(), LetterDefOf.ThreatBig, LookTargets.Invalid);
                            }
                            float result = (totalFogProgress - 1f - 0.003f) / 3;
                            if (result < 0)
                            {
                                result = 0f;
                            }
                            gameCondition.fogProgress[map] = result;
                            Log.Message("Home map fog progress: " + gameCondition.fogProgress[map].ToString());
                        }
                    }
                }
            }
        }
    }
}

