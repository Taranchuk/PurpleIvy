using System;
using Verse;
using Verse.Sound;
using RimWorld;
using Verse.AI;

namespace PurpleIvy
{
    public class InfectedCorpse : Corpse
    {
        public InfectedCorpse()
        {

        }

        public override void TickRare()
        {
            base.TickRare();
            Log.Message("INFECTED");
            PawnKindDef pawnKindDef = PawnKindDef.Named("Genny_ParasiteBeta");
            Pawn NewPawn = PawnGenerator.GeneratePawn(pawnKindDef, null);
            NewPawn.kindDef = pawnKindDef;
            NewPawn.SetFactionDirect(factionDirect);
            NewPawn.thinker = new Pawn_Thinker(NewPawn);
            GenSpawn.Spawn(NewPawn, Position, this.Map);
        }

        Faction factionDirect = Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamed("Genny", true));

    }
}
