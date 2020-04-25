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
        public static void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
        {
            int randomInRange = SettlementSizeRange.RandomInRange;
            int randomInRange2 = SettlementSizeRange.RandomInRange;
            CellRect rect = new CellRect(c.x - randomInRange / 2, c.z - randomInRange2 / 2, randomInRange, randomInRange2);
            rect.ClipInsideMap(map);
            Faction faction = map.ParentFaction;
            ResolveParams rp = default(ResolveParams);
            rp.rect = rect;
            TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false);
            ResolveParams resolveParams = rp;
            resolveParams.rect = rp.rect;
            resolveParams.faction = faction;
            resolveParams.pawnGroupKindDef = (rp.pawnGroupKindDef ?? PawnGroupKindDefOf.Settlement);
            resolveParams.singlePawnSpawnCellExtraPredicate = (rp.singlePawnSpawnCellExtraPredicate ?? ((IntVec3 x) => map.reachability.CanReachMapEdge(x, traverseParms)));
            if (resolveParams.pawnGroupMakerParams == null)
            {
                resolveParams.pawnGroupMakerParams = new PawnGroupMakerParms();
                resolveParams.pawnGroupMakerParams.tile = map.Tile;
                resolveParams.pawnGroupMakerParams.faction = faction;
                resolveParams.pawnGroupMakerParams.points = (rp.settlementPawnGroupPoints ?? SymbolResolver_Settlement.DefaultPawnsPoints.RandomInRange);
                resolveParams.pawnGroupMakerParams.inhabitants = true;
                resolveParams.pawnGroupMakerParams.seed = rp.settlementPawnGroupSeed;
            }
            BaseGen.symbolStack.Push("pawnGroup", resolveParams, null);
            BaseGen.globalSettings.map = map;
            BaseGen.globalSettings.minBuildings = 0;
            BaseGen.globalSettings.minBarracks = 0;
            BaseGen.Generate();
        }

        [HarmonyPrefix]
        private static bool Prefix(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
        {
            bool result;
            if (map.Parent.Faction.def == PurpleIvyDefOf.Genny)
            {
                AlienLairs_GenStepPatch.ScatterAt(c, map, parms, stackCount);
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

