using System;
using RimWorld;
using Verse;

namespace GenerationWorker
{
    [RimWorld.DefOf]
    public static class SiteCoreDefOf
    {
        static SiteCoreDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(SiteCoreDefOf));
        }

        public static SitePartDef OldOutpost;
    }
    [RimWorld.DefOf]
    public static class WorldCoreDefOf
    {
        static WorldCoreDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(WorldCoreDefOf));
        }

        public static WorldObjectDef WorldOutpost;
    }
}

