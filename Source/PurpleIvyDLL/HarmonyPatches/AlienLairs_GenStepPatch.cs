using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace PurpleIvy
{
    [HarmonyPatch(typeof(GenStep_Settlement))]
    [HarmonyPatch("ScatterAt")]
    public static class AlienLairs_GenStepPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
        {
            bool result;
            if (map.Parent.Faction.def == PurpleIvyDefOf.Genny)
            {
                GenStep_AlienLair.DoScatterAt(c, map, parms, stackCount);
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }

        public static readonly IntRange SettlementSizeRange = new IntRange(34, 38);
    }
}

