using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PurpleIvy
{

    [HarmonyPatch(typeof(Building_TurretGun), "IsValidTarget")]
    internal class MakeAlienTurretsIgnoreMaskingPawns
    {
        [HarmonyPrefix]
        public static bool Prefix(Building_TurretGun __instance, ref bool __result, Thing t)
        {
            bool result;
            if (__instance?.Faction == PurpleIvyData.AlienFaction && t is Pawn pawn && pawn.health.hediffSet.GetFirstHediffOfDef(PurpleIvyDefOf.PI_MaskingSprayHigh) != null)
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

