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
    public static class AlienLairsBasesGeneration_MapGeneratedPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Map __instance)
        {
            if (__instance.Parent.Faction.def == PurpleIvyDefOf.Genny)
            {
                Log.Message("Test");
            }
        }
    }
}

