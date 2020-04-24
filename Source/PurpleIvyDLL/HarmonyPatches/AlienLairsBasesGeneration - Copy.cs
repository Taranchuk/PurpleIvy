using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace PurpleIvy
{
    [HarmonyPatch(typeof(GenStep_Settlement))]
    [HarmonyPatch("ScatterAt")]
    public static class Patch_GenStep_Settlement_ScatterAt
    {
        public static void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
        {
            int randomInRange = SettlementSizeRange.RandomInRange;
            int randomInRange2 = SettlementSizeRange.RandomInRange;
            CellRect rect = new CellRect(c.x - randomInRange / 2, c.z - randomInRange2 / 2, randomInRange, randomInRange2);
            Faction faction;
            if (map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer)
            {
                faction = Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
            }
            else
            {
                faction = map.ParentFaction;
            }
            rect.ClipInsideMap(map);
            ResolveParams resolveParams = default(ResolveParams);
            resolveParams.rect = rect;
            resolveParams.faction = faction;
            BaseGen.globalSettings.map = map;
            BaseGen.globalSettings.minBuildings = 0;
            BaseGen.globalSettings.minBarracks = 0;
            BaseGen.symbolStack.Push("settlement", resolveParams, null);
            BaseGen.Generate();


            //int num = Rand.Range(20, 50);
            //int num2 = Rand.Range(20, 50);
            //CellRect cellRect = new CellRect(c.x, c.z, num, num2);
            //CellRect cellRect2 = cellRect.ClipInsideMap(map);
            //ResolveParams resolveParams = default(ResolveParams);
            //resolveParams.faction = map.ParentFaction;
            //BaseGen.globalSettings.map = map;
            //BaseGen.globalSettings.minBuildings = 0;
            //BaseGen.globalSettings.minBarracks = 0;
            //BaseGen.symbolStack.Push("ancientRuins", cellRect2);
            //BaseGen.Generate();

        }
        [HarmonyPrefix]
        private static bool Prefix(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
        {
            bool result;
            if (map.Parent.Faction.def == PurpleIvyDefOf.Genny)
            {
                Patch_GenStep_Settlement_ScatterAt.ScatterAt(c, map, parms, stackCount);
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }

        private static readonly IntRange SettlementSizeRange = new IntRange(34, 38);
    }
}

