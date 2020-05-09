using UnityEngine;
using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;

namespace PurpleIvy
{
    public class AlienMutationHediff : HediffWithComps
    {
        public int tickEndHediff = 0;
        public bool mutationActive = false;
        public int tickStartHediff = 0;
        public override void PostAdd(DamageInfo? dinfo)
        {
            var comp = this.pawn.TryGetComp<AlienMutation>();
            if (comp == null)
            {
                comp = new AlienMutation();
                comp.Initialize(null);
                comp.parent = this.pawn;
                comp.mutationActive = false;
                comp.tickStartHediff = Find.TickManager.TicksGame;
                comp.tickEndHediff = Find.TickManager.TicksGame + new IntRange(120000, 180000).RandomInRange;
                this.pawn.AllComps.Add(comp);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                var comp = this.pawn.TryGetComp<AlienMutation>();
                if (comp != null)
                {
                    this.tickStartHediff = comp.tickStartHediff;
                    this.tickEndHediff = comp.tickEndHediff;
                    this.mutationActive = comp.mutationActive;
                }
            }
            Scribe_Values.Look<int>(ref this.tickStartHediff, "tickStartHediff", 0, false);
            Scribe_Values.Look<int>(ref this.tickEndHediff, "tickEndHediff", 0, false);
            Scribe_Values.Look<bool>(ref this.mutationActive, "mutationActive", false, false);
        }
    }
}

