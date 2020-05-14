using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    public class PurpleGas : Gas
    {
        public override void Tick()
        {
            base.Tick();
            if (this.activeDamage && Find.TickManager.TicksGame % Rand.RangeInclusive(40, 80) == 0)
            {
                List<Pawn> pawnsToDamage = new List<Pawn>();
                if (GenGrid.InBounds(this.Position, this.Map))
                {
                    foreach (var t in this.Map.thingGrid.ThingsListAt(this.Position))
                    {
                        if (t is Pawn pawn && !pawn.Dead && pawn.Faction != PurpleIvyData.AlienFaction)
                        {
                            pawnsToDamage.Add(pawn);
                        }
                    }
                }
                foreach (var pawn in pawnsToDamage)
                {
                    pawn.TakeDamage(new DamageInfo(PurpleIvyDefOf.PI_ToxicBurn, 1f));
                    if (Rand.Chance(0.1f))
                    {
                        pawn.stances.stunner.StunFor(Rand.RangeInclusive(100, 200), null);
                    }
                    if (Rand.Chance(0.1f) && pawn.health.hediffSet.GetFirstHediffOfDef(PurpleIvyDefOf.PI_AlienMutation) == null)
                    {
                        var hediff3 = HediffMaker.MakeHediff(PurpleIvyDefOf.PI_AlienMutation, pawn, null);
                        pawn.health.AddHediff(hediff3, null, null, null);
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.activeDamage, "activeDamage", false, false);
        }

        public bool activeDamage = false;
    }
}

