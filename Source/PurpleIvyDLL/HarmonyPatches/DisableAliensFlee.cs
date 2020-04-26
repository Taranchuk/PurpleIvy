using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace PurpleIvy
{

    [HarmonyPatch(typeof(Pawn_MindState))]
    [HarmonyPatch("CanStartFleeingBecauseOfPawnAction")]
    public static class DisableAlienFlee
    {

        [HarmonyPrefix]
        private static bool Prefix(ref bool __result, Pawn p)
        {
            bool result;
            if (p.Faction.def == PurpleIvyDefOf.Genny)
            {
                __result = false;
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

