using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace PurpleIvy
{
    public class Building_GasPump : Building
    {
        private int pumpfreq = 10;
        Faction factionDirect = Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamed("Genny", true));
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.SetFactionDirect(factionDirect);
        }

        public override void TickRare()
        {
            base.TickRare();
            pumpfreq--;
            if (pumpfreq == 0)
            {
                MoteMaker.ThrowSmoke(base.Position.ToVector3Shifted(), this.Map, 2f);
                pumpfreq = 10;
            }
        }
    }
}