using UnityEngine;
using RimWorld;
using Verse;
using System.Linq;
using System.Text;

namespace PurpleIvy
{

    public class AlienInfection : ThingComp
    {
        Faction factionDirect = Find.FactionManager.FirstFactionOfDef(PurpleIvyDefOf.Genny);
        public int currentCountOfCreatures = 0;
        public int startOfIncubation = 0;
        public int prevTick = 0;

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
            this.startOfIncubation = Find.TickManager.TicksGame;
        }

        public void TryStartSpawn()
        {
            if (this.Props.IncubationData.tickStartHediff.min > 0 &&
                this.startOfIncubation + this.Props.IncubationData.tickStartHediff.RandomInRange
                < Find.TickManager.TicksGame)
            {
                if (this.parent is Pawn && this.Props.IncubationData.hediff != null)
                {
                    Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named(this.Props.IncubationData.hediff),
                    (Pawn)this.parent, null);
                    ((Pawn)this.parent).health.AddHediff(hediff, null, null, null);
                    this.Props.IncubationData.tickStartHediff.min = 0;
                }
            }

            if (this.Props.incubationPeriod.min > 0)
            {
                if (this.startOfIncubation + this.Props.incubationPeriod.RandomInRange
                    < Find.TickManager.TicksGame)
                {
                    if (this.Props.maxNumberOfCreatures <= this.currentCountOfCreatures &&
                        this.Props.resetIncubation == true)
                    {
                        this.startOfIncubation = Find.TickManager.TicksGame;
                        this.currentCountOfCreatures = 0;
                    }

                    if (this.startOfIncubation + this.Props.incubationPeriod.RandomInRange
                        + this.Props.ticksPerSpawn.RandomInRange
                        > Find.TickManager.TicksGame)
                    {
                        return;
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
                                else if (this.parent is Building)
                                {
                                    GenSpawn.Spawn(NewPawn, this.parent.Position, this.parent.Map);
                                }
                                else
                                {
                                    Log.Error("Unknown parent. Cant spawn. " +
                                        "Parent: " + this.parent.Label);
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
            this.TryStartSpawn();
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Concat(new string[]
            {
                Translator.Translate("birthTime"),
                ": ", (this.startOfIncubation + this.Props.incubationPeriod.min
                    - Find.TickManager.TicksGame).ToString(), "~",
                (this.startOfIncubation + this.Props.incubationPeriod.max
                    - Find.TickManager.TicksGame).ToString()
            }));

            stringBuilder.AppendLine(string.Concat(new string[]
            {
                Translator.Translate("TypesOfCreatures"),
                ":"
            }));
            foreach (string type in this.Props.typesOfCreatures)
            {
                    stringBuilder.AppendLine(string.Concat(new string[]
                    {
                        "\t", type
                    }));
            }
            stringBuilder.AppendLine(string.Concat(new string[]
            {
                    Translator.Translate("MaxNumberOfCreatures"),
                    ": ", this.Props.maxNumberOfCreatures.ToString()
            }));
            stringBuilder.AppendLine(string.Concat(new string[]
            {
                    Translator.Translate("numberOfCreaturesPerSpawn"),
                    ": ", this.Props.numberOfCreaturesPerSpawn.min.ToString(), "~",
                    this.Props.numberOfCreaturesPerSpawn.max.ToString()
            }));

            stringBuilder.AppendLine(string.Concat(new string[]
            {
                Translator.Translate("incubationPeriod"),
                ": ", this.Props.incubationPeriod.min.ToString(), "~",
                this.Props.incubationPeriod.max.ToString()
            }));
            stringBuilder.AppendLine(string.Concat(new string[]
            {
                Translator.Translate("ageTick"),
                ": ", this.Props.ageTick.min.ToString(), "~",
                this.Props.ageTick.max.ToString()
            }));
            stringBuilder.AppendLine(string.Concat(new string[]
            {
                Translator.Translate("ticksPerSpawn"),
                ": ", this.Props.ticksPerSpawn.min.ToString(), "~",
                this.Props.ticksPerSpawn.max.ToString()
            }));
            stringBuilder.AppendLine(string.Concat(new string[]
            {
               Translator.Translate("rotProgressPerSpawn"),
               ": ", this.Props.rotProgressPerSpawn.min.ToString(), "~",
               this.Props.rotProgressPerSpawn.max.ToString()
            }));
            stringBuilder.AppendLine(string.Concat(new string[]
            {
               Translator.Translate("resetIncubation"),
               ": ", this.Props.resetIncubation.ToString()
            }));
            stringBuilder.AppendLine(string.Concat(new string[]
            {
               Translator.Translate("currentCountOfCreatures"),
               ": ", this.currentCountOfCreatures.ToString()
            }));
            stringBuilder.AppendLine(string.Concat(new string[]
            {
               Translator.Translate("startOfIncubation"),
               ": ", this.startOfIncubation.ToString()
            }));
            return GenText.TrimEndNewlines(stringBuilder.ToString());
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
            Scribe_Values.Look<int>(ref this.startOfIncubation, "startOfIncubation", 0, false);
            Scribe_Values.Look<int>(ref this.prevTick, "prevTick", 0, false);

        }
    }
}
