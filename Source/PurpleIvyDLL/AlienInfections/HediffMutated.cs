using UnityEngine;
using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using Verse.AI.Group;

namespace PurpleIvy
{
    public class AlienMutationHediff : HediffWithComps
    {
        public int tickEndHediff = 0;
        public bool mutationActive = false;
        public int tickStartHediff = 0;
        public override void PostAdd(DamageInfo? dinfo)
        {
            this.tickStartHediff = Find.TickManager.TicksGame;
            this.tickEndHediff = Find.TickManager.TicksGame + new IntRange(120000, 180000).RandomInRange;
        }

        public override void Tick()
        {
            base.Tick();
            if (mutationActive == false && Find.TickManager.TicksGame > tickEndHediff &&
                GenRadial.RadialCellsAround(this.pawn.Position, 5, true)
                .Where(x => GenGrid.InBounds(x, this.pawn.Map) && this.pawn.Map.thingGrid.ThingsListAt(x)
                .Where(y => y.Faction == this.pawn.Faction).Count() > 0).Count() > 0)
            {
                mutationActive = true;
                this.pawn.SetFaction(PurpleIvyData.AlienFaction);
                this.pawn.RaceProps.thinkTreeMain = PurpleIvyDefOf.PI_HumanlikeMutant;
                Lord lord = null;
                if (this.pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction).Any((Pawn p) =>
                p != this.pawn))
                {
                    lord = ((Pawn)GenClosest.ClosestThing_Global(this.pawn.Position,
                        this.pawn.Map.mapPawns.SpawnedPawnsInFaction(this.pawn.Faction), 99999f,
                        (Thing p) => p != this.pawn && ((Pawn)p).GetLord() != null, null)).GetLord();
                }
                if (lord == null)
                {
                    var lordJob = new LordJob_AssaultColony(this.pawn.Faction);
                    lord = LordMaker.MakeNewLord(this.pawn.Faction, lordJob, this.pawn.Map, null);
                }
                lord.AddPawn(this.pawn);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.tickStartHediff, "tickStartHediff", 0, false);
            Scribe_Values.Look<int>(ref this.tickEndHediff, "tickEndHediff", 0, false);
            Scribe_Values.Look<bool>(ref this.mutationActive, "mutationActive", false, false);
        }
    }
}

