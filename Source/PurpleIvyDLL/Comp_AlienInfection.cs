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

        public void StartSpawn()
        {
            if (this.Props.incubationPeriod.RandomInRange > 0)
            {
                if (this.startIncubation + this.Props.incubationPeriod.RandomInRange > Find.TickManager.TicksGame)
                {
                    this.startIncubation = 0;
                }
                else
                {
                    return;
                }
            }

            if (this.Props.typesOfCreatures != null)
            {
                if (this.Props.maxNumberOfCreatures == 0 || currentCountOfCreatures < this.Props.maxNumberOfCreatures)
                {
                    foreach (string defName in this.Props.typesOfCreatures)
                    {
                        int numberOfSpawn = this.Props.numberOfCreaturesPerSpawn.RandomInRange;
                        if (numberOfSpawn > 0)
                        {
                            foreach (var i in Enumerable.Range(0, numberOfSpawn))
                            {
                                PawnKindDef pawnKindDef = PawnKindDef.Named(defName);
                                Pawn NewPawn = PawnGenerator.GeneratePawn(pawnKindDef, null);
                                Log.Message(i.ToString() + " - " + this.parent.Label + " produces " + NewPawn.Label);
                                if (this.Props.ageTick.RandomInRange > 0)
                                {
                                    int ageTick = this.Props.ageTick.RandomInRange;
                                    NewPawn.ageTracker.AgeBiologicalTicks = ageTick;
                                    NewPawn.ageTracker.AgeChronologicalTicks = ageTick;
                                }
                                else
                                {
                                    NewPawn.ageTracker.AgeBiologicalTicks = 0;
                                    NewPawn.ageTracker.AgeChronologicalTicks = 0;
                                }
                                NewPawn.SetFactionDirect(factionDirect);
                                GenSpawn.Spawn(NewPawn, this.parent.Position, this.parent.Map);
                                currentCountOfCreatures++;
                                if (this.parent is Corpse)
                                {
                                    DoDamageToCorpse();
                                }
                            }
                        }
                    }
                }
            }
        }

        public void DoDamageToCorpse()
        {
            CompRottable compRottable = this.parent.TryGetComp<CompRottable>();
            if (compRottable != null && compRottable.Stage < RotStage.Dessicated)
            {
                compRottable.RotProgress += this.Props.rotProgressPerSpawn.RandomInRange;
                FilthMaker.TryMakeFilth(this.parent.Position, this.parent.Map, ThingDefOf.Filth_Blood);
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            if (Find.TickManager.TicksGame % this.Props.ticksPerSpawn.RandomInRange == 0)
            {
                this.StartSpawn();
            }
        }
        public override void CompTickRare()
        {
            base.CompTickRare();
            Log.Message("StartSpawn 2");
            this.StartSpawn();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.currentCountOfCreatures, "currentCountOfCreatures", 0, false);
            Scribe_Values.Look<int>(ref this.startIncubation, "startIncubation", 0, false);
        }

        Faction factionDirect = Find.FactionManager.FirstFactionOfDef(PurpleIvyDefOf.Genny);
        private int currentCountOfCreatures = 0;
        public int startIncubation = 0;
    }
}

