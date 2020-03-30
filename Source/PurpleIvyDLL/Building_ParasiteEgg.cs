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
        Faction factionDirect = Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamed("Genny", true));
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.SetFactionDirect(factionDirect);
        }
        public override void Tick()
        {
            base.Tick(); 
            if (Find.TickManager.TicksGame % 750 == 0)
            {
                if (EcosystemFull(this.Map))
                {
                    return;
                }
                Random random = new Random();
                int Spawnrate = random.Next(1, 50);
                if (Spawnrate == 5)
                {
                    PawnKindDef pawnKindDef = PawnKindDef.Named("Genny_ParasiteAlpha");
                    Pawn NewPawn = PawnGenerator.GeneratePawn(pawnKindDef, null);
                    NewPawn.kindDef = pawnKindDef;
                    NewPawn.SetFactionDirect(factionDirect);
                    NewPawn.thinker = new Pawn_Thinker(NewPawn);
                    GenSpawn.Spawn(NewPawn, Position, this.Map);
                }
                else if (Spawnrate == 10 || Spawnrate == 15 || Spawnrate == 20)
                {
                    PawnKindDef pawnKindDef = PawnKindDef.Named("Genny_ParasiteBeta");
                    Pawn NewPawn = PawnGenerator.GeneratePawn(pawnKindDef, null);
                    NewPawn.kindDef = pawnKindDef;
                    NewPawn.SetFactionDirect(factionDirect);
                    NewPawn.thinker = new Pawn_Thinker(NewPawn);
                    GenSpawn.Spawn(NewPawn, Position, this.Map);
                }
            }
        }

        public static bool EcosystemFull(Map map)
        {
            float num = 0f;
            foreach (Pawn current in map.mapPawns.AllPawns)
            {
                if (current.kindDef.race.defName.Equals("Genny_ParasiteAlpha"))
                {
                    num += 1;
                }
                else if (current.kindDef.race.defName.Equals("Genny_ParasiteBeta"))
                {
                    num += 1;
                }
            }
            return num >= 20;
        }
    }
}