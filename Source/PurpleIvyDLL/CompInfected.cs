using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PurpleIvy
{
    public class Infected : ThingComp
    {
        public override void CompTickRare()
        {
            base.CompTickRare();
            Log.Message(this.parent.Label + " produces a parasite");
            PawnKindDef pawnKindDef = PawnKindDef.Named("Genny_ParasiteBeta");
            Pawn NewPawn = PawnGenerator.GeneratePawn(pawnKindDef, null);
            NewPawn.ageTracker.AgeBiologicalTicks = 0;
            NewPawn.ageTracker.AgeChronologicalTicks = 0;
            NewPawn.SetFactionDirect(factionDirect);
            GenSpawn.Spawn(NewPawn, this.parent.Position, this.parent.Map);
        }

        Faction factionDirect = Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamed("Genny", true));

    }
}
