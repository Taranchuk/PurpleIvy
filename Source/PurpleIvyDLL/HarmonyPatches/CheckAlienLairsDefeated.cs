using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PurpleIvy
{

    [HarmonyPatch(typeof(SettlementDefeatUtility))]
    [HarmonyPatch("IsDefeated")]
    public static class Patch_SettlementDefeatUtility_IsDefeated
    {
        private static bool IsDefeated(Map map, Faction faction)
        {
            List<Pawn> list = map.mapPawns.SpawnedPawnsInFaction(faction);
            for (int i = 0; i < list.Count; i++)
            {
                Pawn pawn = list[i];
                if (pawn.Faction.def == PurpleIvyDefOf.Genny)
                {
                    return false;
                }
            }
            return true;
        }
        [HarmonyPrefix]
        private static bool Prefix(ref bool __result, Map map, Faction faction)
        {
            bool result;
            if (faction.def == PurpleIvyDefOf.Genny)
            {
                __result = IsDefeated(map, faction);
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }
    }
}

