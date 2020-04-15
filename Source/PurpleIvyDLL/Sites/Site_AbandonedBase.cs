using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PurpleIvy
{
    public class AbandonedBase : Site
    {
        public override void PostMapGenerate()
        {
            base.PostMapGenerate();
            foreach (var pawn in this.Map.mapPawns.AllPawns.Where(pawn => pawn.Faction != Faction.OfPlayer))
            {
                pawn.Destroy(DestroyMode.Vanish);
            }
            foreach (var item in this.Map.listerThings.AllThings.Where(item => item.Faction != Faction.OfPlayer))
            {
                item.Destroy(DestroyMode.Vanish);
            }
        }
    }
}
