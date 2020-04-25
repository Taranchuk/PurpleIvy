using System;
using System.Collections.Generic;
using System.Linq;
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
            PurpleIvyUtils.KillAllPawnsExceptAliens(this.Map);
        }
    }
}

