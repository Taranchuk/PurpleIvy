using System;
using System.Linq;
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
                for (int i = __instance.mapPawns.AllPawns.Count - 1; i >= 0; i--)
                {
                    Pawn pawn = __instance.mapPawns.AllPawns[i];
                    if (pawn.Faction == null || pawn.Faction != Faction.OfPlayer
                        && pawn.Faction?.def != PurpleIvyDefOf.Genny)
                    {
                        pawn.Kill(null);
                        Corpse corpse = pawn.ParentHolder as Corpse;
                        corpse.TryGetComp<CompRottable>().RotProgress += 1000000;
                        if (Rand.Chance(0.3f))
                        {
                            foreach (IntVec3 current in GenAdj.CellsAdjacent8WayAndInside(corpse))
                            {
                                if (Rand.Chance(0.25f))
                                {
                                    FilthMaker.TryMakeFilth(current, __instance, corpse.InnerPawn.RaceProps.BloodDef);
                                }
                                else if (Rand.Chance(0.25f))
                                {
                                    FilthMaker.TryMakeFilth(current, __instance, PurpleIvyDefOf.AlienBloodFilth);
                                }
                            }
                        }
                    }
                    else
                    {
                        Log.Message(pawn + " has faction " + pawn.Faction);
                    }

                }
            }
        }
    }
}

