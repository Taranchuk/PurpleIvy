using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    public static class DropPurpleDustFromInfectedThingsPatch
    {
        [HarmonyPatch(typeof(Thing), "Destroy")]
        public static class DropPurpleDustFromInfectedThings
        {
            [HarmonyPrefix]
            private static bool Prefix(Thing __instance, DestroyMode mode)
            {
                if (__instance is Building building)
                {
                    var comp = building.Map.GetComponent<MapComponent_MapEvents>();
                    if (comp != null && comp.ToxicDamages.ContainsKey(building))
                    {
                        //Thing newNest = ThingMaker.MakeThing(ThingDefOf.AIPersonaCore);
                        //GenSpawn.Spawn(newNest, building.Position, building.Map);

                        comp.ToxicDamages.Remove(building);
                    }
                }
                return true;
            }
        }
    }
}

