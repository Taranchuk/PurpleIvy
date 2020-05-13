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
                MapComponent_MapEvents comp = null;
                if (__instance.Map != null)
                {
                    comp = __instance.Map.GetComponent<MapComponent_MapEvents>();
                }
                else if (__instance.ParentHolder is Thing t && t.Map != null)
                {
                    comp = t.Map.GetComponent<MapComponent_MapEvents>();
                }
                if (comp != null && comp.ToxicDamages.ContainsKey(__instance))
                {
                    //Thing newNest = ThingMaker.MakeThing(ThingDefOf.AIPersonaCore);
                    //GenSpawn.Spawn(newNest, building.Position, building.Map);
                    comp.ToxicDamages.Remove(__instance);
                }
                return true;
            }
        }
    }
}

