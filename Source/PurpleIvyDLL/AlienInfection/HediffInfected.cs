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
            var dummyCorpse = PurpleIvyDefOf.InfectedCorpseDummy;
            var comp = new AlienInfection();
            comp.Initialize(dummyCorpse.GetCompProperties<CompProperties_AlienInfection>());
            comp.parent = this.pawn;
            var parasite = DefDatabase<PawnKindDef>.AllDefsListForReading
                .Where(x => x.defName.Contains("Genny_Parasite")).ToList().RandomElement<PawnKindDef>();
            comp.Props.typesOfCreatures = new List<string>()
            {
                parasite.defName
            };
            if (parasite.race.defName == PurpleIvyDefOf.Genny_ParasiteAlpha.defName)
            {
                var range = new IntRange(1, 1);
                comp.maxNumberOfCreatures = range.RandomInRange;
                comp.Props.maxNumberOfCreatures = range;
            }
            else if (parasite.race.defName == PurpleIvyDefOf.Genny_ParasiteBeta.defName)
            {
                var range = new IntRange(1, 3);
                comp.maxNumberOfCreatures = range.RandomInRange;
                comp.Props.maxNumberOfCreatures = range;
            }
            else if (parasite.race.defName == PurpleIvyDefOf.Genny_ParasiteGamma.defName)
            {
                var range = new IntRange(1, 3);
                comp.maxNumberOfCreatures = range.RandomInRange;
                comp.Props.maxNumberOfCreatures = range;
            }
            else if (parasite.race.defName == PurpleIvyDefOf.Genny_ParasiteOmega.defName)
            {
                var range = new IntRange(1, 5);
                comp.maxNumberOfCreatures = range.RandomInRange;
                comp.Props.maxNumberOfCreatures = range;
            }
            else if (parasite.race.defName == PurpleIvyDefOf.Genny_ParasiteNestGuard.defName)
            {
                var range = new IntRange(1, 1);
                comp.maxNumberOfCreatures = range.RandomInRange;
                comp.Props.maxNumberOfCreatures = range;
            }
            else if (parasite.defName != PurpleIvyDefOf.Genny_Queen.defName)
            {
                Log.Error("1 Something went wrong while adding infected comp: " + comp.parent + " - " + parasite);
            }
            comp.Props.incubationPeriod = new IntRange(10000, 40000);
            comp.Props.IncubationData = new IncubationData
            {
                tickStartHediff = new IntRange(2000, 4000),
                deathChance = 90,
                hediff = HediffDefOf.Pregnant.defName
            };
            this.pawn.AllComps.Add(comp);
        }

    }
}

