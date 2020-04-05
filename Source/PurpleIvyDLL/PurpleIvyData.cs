using System;
using RimWorld;
using Verse;

namespace PurpleIvy
{
	public static class PurpleIvyData
	{
        public static Faction factionDirect
        {
            get
            {
                return Find.FactionManager.FirstFactionOfDef(PurpleIvyDefOf.Genny);
            }
        }
    }
}

