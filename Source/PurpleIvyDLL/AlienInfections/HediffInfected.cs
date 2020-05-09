using UnityEngine;
using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;

namespace PurpleIvy
{
    public class AlienInfectionHediff : HediffWithComps
    {
        public PawnKindDef instigator = null;

        public int currentCountOfCreatures = 0;
        public int startOfIncubation = 0;
        public int maxNumberOfCreatures = 0;
        public bool prevAngle = true;
        public int tickStartHediff = 0;
        public bool stopSpawning = false;
        public override void PostAdd(DamageInfo? dinfo)
        {
            var comp = this.pawn.TryGetComp<AlienInfection>();
            if (comp == null)
            {
                var dummyCorpse = PurpleIvyDefOf.InfectedCorpseDummy;
                comp = new AlienInfection();
                comp.Initialize(dummyCorpse.GetCompProperties<CompProperties_AlienInfection>());
                comp.parent = this.pawn;
                if (instigator == null)
                {
                    instigator = DefDatabase<PawnKindDef>.AllDefsListForReading
                    .Where(x => x.defName.Contains("Genny_Parasite")).ToList().RandomElement<PawnKindDef>();
                }
                var range = PurpleIvyData.maxNumberOfCreatures[instigator.race.defName];
                int randomRange = range.RandomInRange;
                comp.maxNumberOfCreatures = (int)(randomRange * this.pawn.BodySize);
                if (comp.maxNumberOfCreatures > 0)
                {
                    Log.Message("Infection count based on body size: " + this.pawn +
                    " - body size: " + this.pawn.BodySize + " - initial value: " + randomRange +
                    " after: " + comp.maxNumberOfCreatures);
                    comp.Props.maxNumberOfCreatures = range;
                    comp.Props.typesOfCreatures = new List<string>()
                    {
                        instigator.defName
                    };
                    comp.Props.incubationPeriod = new IntRange(10000, 40000);
                    comp.Props.IncubationData = new IncubationData
                    {
                        tickStartHediff = new IntRange(2000, 4000),
                        deathChance = 90,
                        hediff = HediffDefOf.Pregnant.defName
                    };
                    Log.Message("1 Adding infected comp to " + this.pawn);
                    this.pawn.AllComps.Add(comp);
                }
                else
                {
                    Log.Message(this.pawn + " - zero count of infection");
                    this.pawn.health.hediffSet.hediffs.Remove(this);
                    this.PostRemoved();
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                var comp = this.pawn.TryGetComp<AlienInfection>();
                if (comp != null)
                {
                    this.currentCountOfCreatures = comp.currentCountOfCreatures;
                    this.startOfIncubation = comp.startOfIncubation;
                    this.maxNumberOfCreatures = comp.maxNumberOfCreatures;
                    this.prevAngle = comp.prevAngle;
                    this.tickStartHediff = comp.tickStartHediff;
                    this.stopSpawning = comp.stopSpawning;
                }
            }
            Scribe_Defs.Look<PawnKindDef>(ref this.instigator, "instigator");
            Scribe_Values.Look<int>(ref this.currentCountOfCreatures, "currentCountOfCreatures", 0, false);
            Scribe_Values.Look<int>(ref this.startOfIncubation, "startOfIncubation", 0, false);
            Scribe_Values.Look<int>(ref this.maxNumberOfCreatures, "maxNumberOfCreatures", 0, false);
            Scribe_Values.Look<int>(ref this.tickStartHediff, "tickStartHediff", 0, false);
            Scribe_Values.Look<bool>(ref this.stopSpawning, "stopSpawning", false, false);
        }
    }
}

