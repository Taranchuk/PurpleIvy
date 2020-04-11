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
                    PurpleIvyData.TotalFogProgress[comp] = PurpleIvyData.getFogProgress(comp.counter);
                }
            }
        }

        // for testing purposes
        //public override void GameComponentTick()
        //{
        //    base.GameComponentTick();
        //    bool temp;
        //    if (Find.TickManager.TicksGame % 60 == 0)
        //    {
        //        foreach (var worldObject in Find.WorldObjects.AllWorldObjects)
        //        {
        //            var comp = worldObject.GetComponent<WorldObjectComp_InfectedTile>();
        //            if (comp != null)
        //            {
        //                Log.Message(comp.parent + " infected: " + comp.counter + " counter, "
        //                + PurpleIvyData.getFogProgressWithOuterSources(comp.counter, comp, out temp)
        //                .ToString() + " getFogProgressWithOuterSources", true);
        //            }
        //        }
        //    }
        //}

    }
}

