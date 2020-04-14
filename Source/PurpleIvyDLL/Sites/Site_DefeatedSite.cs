using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PurpleIvy
{
    public class DefeatedBase : Site
    {
        public override void PostMapGenerate()
        {
            base.PostMapGenerate();
            foreach (Pawn pawn in this.Map.mapPawns.AllPawns)
            {
                if (pawn.Faction != Faction.OfPlayer)
                {
                    pawn.Destroy(DestroyMode.Vanish);
                }
            }
        }
    }
}
