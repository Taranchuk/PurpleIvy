using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    [HarmonyPatch(typeof(Pawn_PathFollower))]
    [HarmonyPatch("CostToMoveIntoCell")]
    [HarmonyPatch(new Type[]
    {
        typeof(Pawn),
        typeof(IntVec3)
    })]
    public static class Pawn_PathFollower_CostToMoveIntoCell_Patch
    {
        [HarmonyPrefix]
        public static bool MakeParasitesWalk(Pawn pawn, IntVec3 c, ref int __result)
        {
            if (pawn?.Faction?.def == PurpleIvyDefOf.Genny)
            {
                int num;
                if (c.x == pawn.Position.x || c.z == pawn.Position.z)
                {
                    num = pawn.TicksPerMoveCardinal;
                }
                else
                {
                    num = pawn.TicksPerMoveDiagonal;
                }
                TerrainDef terrainDef = pawn.Map.terrainGrid.TerrainAt(c);
                if (terrainDef == null)
                {
                    num = 10000;
                }
                else
                {
                    if (terrainDef.passability == Traversability.Impassable && !terrainDef.IsWater)
                    {
                        num = 10000;
                    }
                }
                List<Thing> list = pawn.Map.thingGrid.ThingsListAt(c);
                for (int i = 0; i < list.Count; i++)
                {
                    Thing thing = list[i];
                    if (thing.def.passability == Traversability.Impassable)
                    {
                        num = 10000;
                    }
                    if (thing is Building_Door)
                    {
                        num += 45;
                    }
                }
                __result = num;
                return false;
            }
            return true;
        }
    }
}

