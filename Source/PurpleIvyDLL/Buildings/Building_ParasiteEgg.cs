using System;
using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;

namespace PurpleIvy
{
    public class Building_ParasiteEgg : Building
    {
        Faction factionDirect = Find.FactionManager.FirstFactionOfDef(PurpleIvyDefOf.Genny);
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            this.SetFactionDirect(factionDirect);
            base.SpawnSetup(map, respawningAfterLoad);
        }
    }
}

