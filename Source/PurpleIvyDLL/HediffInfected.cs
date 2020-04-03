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
            Thing dummyCorpse = ThingMaker.MakeThing(PurpleIvyDefOf.InfectedCorpseDummy);
            var comp = dummyCorpse.TryGetComp<AlienInfection>();
            comp.parent = this.pawn;
            string parasite = GenCollection.RandomElement<PawnKindDef>(
            DefDatabase<PawnKindDef>.AllDefsListForReading
            .Where(x => x.defName.Contains("Genny_Parasite")).ToList()).defName;
            comp.Props.typesOfCreatures = new List<string>()
            {
                parasite
            };
            comp.Props.incubationPeriod = new IntRange(10000, 40000);
            comp.Props.IncubationData = new IncubationData();
            comp.Props.IncubationData.tickStartHediff = new IntRange(2000, 4000);
            comp.Props.IncubationData.deathChance = 90f;
            comp.Props.IncubationData.hediff = HediffDefOf.Pregnant.defName;
            this.pawn.AllComps.Add(comp);
        }

    }
}

