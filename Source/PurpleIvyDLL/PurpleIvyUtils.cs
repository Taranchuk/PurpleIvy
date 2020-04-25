using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PurpleIvy
{
    [StaticConstructorOnStartup]
    public static class PurpleIvyUtils
    {
        public static void KillAllPawnsExceptAliens(Map map)
        {
            for (int i = map.mapPawns.AllPawns.Count - 1; i >= 0; i--)
            {
                Pawn pawn = map.mapPawns.AllPawns[i];
                if (pawn.Faction == null || pawn.Faction != Faction.OfPlayer
                    && pawn.Faction?.def != PurpleIvyDefOf.Genny)
                {
                    if (pawn.RaceProps.deathActionWorkerClass == typeof(DeathActionWorker_SmallExplosion)
                        || pawn.RaceProps.deathActionWorkerClass == typeof(DeathActionWorker_BigExplosion))
                    {
                        pawn.Destroy(DestroyMode.Vanish);
                    }
                    else
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
                                    FilthMaker.TryMakeFilth(current, map, corpse.InnerPawn.RaceProps.BloodDef);
                                }
                                else if (Rand.Chance(0.25f))
                                {
                                    FilthMaker.TryMakeFilth(current, map, PurpleIvyDefOf.AlienBloodFilth);
                                }
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

