using UnityEngine;
using RimWorld;
using Verse;
using System.Linq;

namespace PurpleIvy
{
    public class AlienInfection : ThingComp
    {
        public CompProperties_AlienInfection Props
        {
            get
            {
                return this.props as CompProperties_AlienInfection;
            }
        }

        public void startSpawn()
        {
            if (this.Props.typesOfCreatures != null)
            {
                if (currentCountOfCreatures < this.Props.maxNumberOfCreatures)
                {
                    foreach (string defName in this.Props.typesOfCreatures)
                    {
                        int numberOfSpawn = 0;
                        if (this.Props.numberOfCreaturesPerSpawnRandom > 0)
                        {
                            numberOfSpawn = Random.Range(0, this.Props.numberOfCreaturesPerSpawnRandom);
                        }
                        else if (this.Props.numberOfCreaturesPerSpawn > 0)
                        {
                            numberOfSpawn = this.Props.numberOfCreaturesPerSpawn;
                        }
                        if (numberOfSpawn > 0)
                        {
                            foreach (var i in Enumerable.Range(0, numberOfSpawn))
                            {
                                Log.Message(i.ToString() + " - " + this.parent.Label + " produces a parasite");
                                PawnKindDef pawnKindDef = PawnKindDef.Named(defName);
                                Pawn NewPawn = PawnGenerator.GeneratePawn(pawnKindDef, null);
                                if (this.Props.ageTick > 0)
                                {
                                    NewPawn.ageTracker.AgeBiologicalTicks = this.Props.ageTick;
                                    NewPawn.ageTracker.AgeChronologicalTicks = this.Props.ageTick;
                                }
                                else
                                {
                                    NewPawn.ageTracker.AgeBiologicalTicks = 0;
                                    NewPawn.ageTracker.AgeChronologicalTicks = 0;
                                }
                                NewPawn.SetFactionDirect(factionDirect);
                                GenSpawn.Spawn(NewPawn, this.parent.Position, this.parent.Map);
                            }
                        }
                    }
                }
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            if (Find.TickManager.TicksGame % 250 == 0)
            {
                this.startSpawn();
            }
        }
        public override void CompTickRare()
        {
            base.CompTickRare();
            this.startSpawn();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.currentCountOfCreatures, "currentCountOfCreatures", 0, false);
        }

        Faction factionDirect = Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamed("Genny", true));
        private int currentCountOfCreatures = 0;
    }
}

