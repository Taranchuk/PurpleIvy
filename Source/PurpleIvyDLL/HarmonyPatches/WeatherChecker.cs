using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace PurpleIvy
{
    [HarmonyPatch(typeof(MapComponentUtility))]
    [HarmonyPatch("MapGenerated")]
    [HarmonyPatch(new Type[]
    {
        typeof(Map)
    })]
    public static class MapGeneratedPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Map __instance)
        {
            Log.Message("WeatherChecker", true);
        }
    }
}