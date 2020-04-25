using System;
using System.Linq;
using RimWorld;
using Verse;

namespace PurpleIvy
{
    public class SitePartWorker_AlienLair : SitePartWorker
    {
        public override void PostMapGenerate(Map map)
        {
            Log.Message(map.ParentFaction.def.defName + " _ faction");
        }
    }
}

