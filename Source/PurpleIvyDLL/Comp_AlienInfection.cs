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

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            this.startIncubation = Find.TickManager.TicksGame;
        }

        public void TryStartSpawn()
        {
            if (this.startIncubation + this.Props.IncubationData.tickStartHediff.RandomInRange < Find.TickManager.TicksGame)
            {
                if (this.Props.IncubationData.hediff != null)
                {
                    Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named(this.Props.IncubationData.hediff),
                    (Pawn)this.parent, null);
                    ((Pawn)this.parent).health.AddHediff(hediff, null, null, null);
                }
            }
            if (this.Props.incubationPeriod.RandomInRange > 0)
            {
                if (this.startIncubation + this.Props.incubationPeriod.RandomInRange < Find.TickManager.TicksGame)
                {
                    if (this.Props.resetIncubation == true)
                    {
                        this.startIncubation = 0;
                    }
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
                                if (this.parent is Corpse)
                                {
                                    HatchFromCorpse(NewPawn);
                                }
                                else if (this.parent is Pawn)
                                {
                                    HatchFromPawn(NewPawn);
                                }
                                currentCountOfCreatures++;
                            }
                        }
                    }
                }
            }
        }

        public void HatchFromCorpse(Pawn NewPawn)
        {
            CompRottable compRottable = this.parent.TryGetComp<CompRottable>();
            if (compRottable != null && compRottable.Stage < RotStage.Dessicated)
            {
                compRottable.RotProgress += this.Props.rotProgressPerSpawn.RandomInRange;
                FilthMaker.TryMakeFilth(this.parent.Position, this.parent.Map, ThingDefOf.Filth_Blood);
            }
            GenSpawn.Spawn(NewPawn, this.parent.Position, this.parent.Map);

        }

        public void HatchFromPawn(Pawn NewPawn)
        {
            Pawn host = (Pawn)this.parent;
            this.parent.TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, 25f));
            if (host.Dead)
            {
                Corpse corpse = (Corpse)this.parent.ParentHolder;
                corpse.AllComps.Add(this);
                FilthMaker.TryMakeFilth(corpse.Position, corpse.Map, ThingDefOf.Filth_Blood);
                GenSpawn.Spawn(NewPawn, corpse.Position, corpse.Map);
            }
            else if (this.Props.IncubationData.deathChance >= Rand.Range(0f, 100f))
            {
                this.parent.Kill();
                Corpse corpse = (Corpse)this.parent.ParentHolder;
                corpse.AllComps.Add(this);
                FilthMaker.TryMakeFilth(corpse.Position, corpse.Map, ThingDefOf.Filth_Blood);
                GenSpawn.Spawn(NewPawn, corpse.Position, corpse.Map);
            }
            else
            {
                GenSpawn.Spawn(NewPawn, this.parent.Position, this.parent.Map);
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            if (Find.TickManager.TicksGame % this.Props.ticksPerSpawn.RandomInRange == 0)
            {
                this.TryStartSpawn();
            }
        }
        public override void CompTickRare()
        {
            base.CompTickRare();
            this.TryStartSpawn();
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