using UnityEngine;
using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;

namespace PurpleIvy
{
    public class HediffInfected : HediffWithComps
    {
        public override void PostAdd(DamageInfo? dinfo)
        {
            ThingDef dummyCorpse = PurpleIvyDefOf.InfectedCorpseDummy;
            AlienInfection comp = new AlienInfection();
            comp.Initialize(dummyCorpse.GetCompProperties<CompProperties_AlienInfection>());
            comp.parent = this.pawn;
            PawnKindDef parasite = GenCollection.RandomElement<PawnKindDef>(
            DefDatabase<PawnKindDef>.AllDefsListForReading
            .Where(x => x.defName.Contains("Genny_Parasite")).ToList());
            comp.Props.typesOfCreatures = new List<string>()
            {
                parasite.defName
            };
            if (parasite.race.defName == PurpleIvyDefOf.Genny_ParasiteAlpha.defName)
            {
                IntRange range = new IntRange(1, 1);
                comp.maxNumberOfCreatures = range.RandomInRange;
                comp.Props.maxNumberOfCreatures = range;
            }
            else if (parasite.race.defName == PurpleIvyDefOf.Genny_ParasiteBeta.defName)
            {
                IntRange range = new IntRange(1, 3);
                comp.maxNumberOfCreatures = range.RandomInRange;
                comp.Props.maxNumberOfCreatures = range;
            }
            else if (parasite.race.defName == PurpleIvyDefOf.Genny_ParasiteOmega.defName)
            {
                IntRange range = new IntRange(1, 5);
                comp.maxNumberOfCreatures = range.RandomInRange;
                comp.Props.maxNumberOfCreatures = range;
            }
            else
            {
                Log.Error("1 Something went wrong while adding infected comp: " + comp.parent + " - " + parasite);
            }
            comp.Props.incubationPeriod = new IntRange(10000, 40000);
            comp.Props.IncubationData = new IncubationData();
            comp.Props.IncubationData.tickStartHediff = new IntRange(2000, 4000);
            comp.Props.IncubationData.deathChance = 90f;
            comp.Props.IncubationData.hediff = HediffDefOf.Pregnant.defName;
            this.pawn.AllComps.Add(comp);
        }

    }
}

