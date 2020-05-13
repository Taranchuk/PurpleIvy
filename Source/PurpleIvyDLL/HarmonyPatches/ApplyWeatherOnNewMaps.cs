using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PurpleIvy
{
    [HarmonyPatch(typeof(Map))]
    [HarmonyPatch("FinalizeInit")]
    [HarmonyPatch(new Type[]
    {

    })]
    public static class MapGeneratedPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Map __instance)
        {
            Log.Message("WeatherChecker: " + __instance.ToString());
            bool comeFromOuterSource;
            var tempComp = new WorldObjectComp_InfectedTile();
            tempComp.infectedTile = __instance.Tile;
            if (PurpleIvyUtils.getFogProgressWithOuterSources(0, tempComp, out comeFromOuterSource) > 0f &&
    !__instance.gameConditionManager.ConditionIsActive(PurpleIvyDefOf.PurpleFogGameCondition))
            {
                GameCondition_PurpleFog gameCondition =
                    (GameCondition_PurpleFog)GameConditionMaker.MakeConditionPermanent
                    (PurpleIvyDefOf.PurpleFogGameCondition);
                __instance.gameConditionManager.RegisterCondition(gameCondition);
                tempComp.parent = __instance.Parent;
                tempComp.StartInfection();
                tempComp.gameConditionCaused = PurpleIvyDefOf.PurpleFogGameCondition;
                tempComp.counter = 0;
                tempComp.infected = false;
                tempComp.infectedTile = __instance.Tile;
                tempComp.radius = tempComp.GetRadius();
                PurpleIvyData.TotalFogProgress[tempComp] = PurpleIvyUtils.getFogProgress(tempComp.counter);
                tempComp.fillRadius();
                __instance.Parent.AllComps.Add(tempComp);

            }
        }
    }
}

