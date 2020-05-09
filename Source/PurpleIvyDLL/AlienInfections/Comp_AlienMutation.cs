using UnityEngine;
using RimWorld;
using Verse;
using System.Linq;
using System.Text;
using Verse.AI;

namespace PurpleIvy
{

    public class AlienMutation : ThingComp
    {
        public int tickEndHediff = 0;
        public bool mutationActive = false;
        public int tickStartHediff = 0;

        public CompProperties_AlienMutation Props => this.props as CompProperties_AlienMutation;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }


        public override void PostPostMake()
        {
            base.PostPostMake();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (mutationActive == false && Find.TickManager.TicksGame > tickEndHediff &&
                    GenRadial.RadialCellsAround(this.parent.Position, 5, true)
                    .Where(x => GenGrid.InBounds(x, this.parent.Map) && this.parent.Map.thingGrid.ThingsListAt(x)
            .Where(y => y.Faction == this.parent.Faction).Count() > 0).Count() > 0)
            {
                mutationActive = true;
                Pawn pawn = (Pawn)this.parent;
                pawn.SetFaction(PurpleIvyData.AlienFaction);
            }
        }
        public override void CompTickRare()
        {
            base.CompTickRare();
        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.tickStartHediff, "tickStartHediff", 0, false);
            Scribe_Values.Look<int>(ref this.tickEndHediff, "tickEndHediff", 0, false);
            Scribe_Values.Look<bool>(ref this.mutationActive, "mutationActive", false, false);
        }
    }
}

