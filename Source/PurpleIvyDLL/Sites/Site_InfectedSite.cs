using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PurpleIvy
{
    public class InfectedSite : Site
    {
        public override string GetInspectString()
        {
            var comp = this.GetComponent<WorldObjectComp_InfectedTile>();
            return comp.radius.ToString() +
                "\n" + this.Tile + "\n" + base.GetInspectString();
        }
        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            if (this.Map != null)
            {
                int count = this.Map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Count;
                if (count == 0 && !base.Map.mapPawns.AnyPawnBlockingMapRemoval)
                {
                    alsoRemoveWorldObject = true;
                    return true;
                }
            }
            alsoRemoveWorldObject = false;
            return false;
        }
    }
}

