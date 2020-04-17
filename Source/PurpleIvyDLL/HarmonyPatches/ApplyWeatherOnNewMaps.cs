using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PurpleIvy
{

    //[HarmonyPatch(typeof(MapComponentUtility))]
    //[HarmonyPatch("MapGenerated")]
    //[HarmonyPatch(new Type[]
    //{
    //    typeof(Map)
    //})]
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
            Log.Message(__instance.ToString());
            bool comeFromOuterSource;
            var tempComp = new WorldObjectComp_InfectedTile();
            tempComp.infectedTile = __instance.Tile;
            var result = PurpleIvyData.getFogProgressWithOuterSources(0, tempComp, out comeFromOuterSource);
            Log.Message("REsult: " + result.ToString());
            if (PurpleIvyData.getFogProgressWithOuterSources(0, tempComp, out comeFromOuterSource) > 0f &&
    !__instance.gameConditionManager.ConditionIsActive(PurpleIvyDefOf.PurpleFogGameCondition))
            {
                GameCondition_PurpleFog gameCondition =
                    (GameCondition_PurpleFog)GameConditionMaker.MakeConditionPermanent
                    (PurpleIvyDefOf.PurpleFogGameCondition);
                __instance.gameConditionManager.RegisterCondition(gameCondition);
            }
            Log.Message("WeatherChecker", true);
        }
    }
}

