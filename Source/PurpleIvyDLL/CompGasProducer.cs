using System;
using Verse;

namespace PurpleIvy
{
    internal class CompGasProducer : ThingComp
    {
        public CompProperties_GasProducer Props
        {
            get
            {
                Log.Message("CompGasProducer : ThingComp - return (CompProperties_GasProducer)this.props; - 1", true);
                return (CompProperties_GasProducer)this.props;
            }
        }

        public override void CompTick()
        {
            Log.Message("CompGasProducer : ThingComp - CompTick() - this.gasProgress++; - 2", true);
            this.gasProgress++;
            if (this.gasProgress > this.gasTickMax)
            {
                Log.Message("CompGasProducer : ThingComp - CompTick() - Pawn pawn = this.parent as Pawn; - 3", true);
                if (this.parent.Map != null)
                {
                    foreach (IntVec3 intVec in GenAdj.OccupiedRect(this.parent.Position, this.parent.Rotation, IntVec2.One).ExpandedBy(this.Props.radius).Cells)
                    {
                        Log.Message("CompGasProducer : ThingComp - CompTick() - bool flag4 = GenGrid.InBounds(intVec, pawn.Map) && this.rand.NextDouble() < (double)this.Props.rate; - 4", true);
                        bool flag4 = GenGrid.InBounds(intVec, this.parent.Map) && this.rand.NextDouble() < (double)this.Props.rate;
                        if (flag4)
                        {
                            Log.Message("CompGasProducer : ThingComp - CompTick() - Thing thing = ThingMaker.MakeThing(ThingDef.Named(this.Props.gasType), null); - 5", true);
                            Thing thing = ThingMaker.MakeThing(ThingDef.Named(this.Props.gasType), null);
                            Log.Message("CompGasProducer : ThingComp - CompTick() - GenSpawn.Spawn(thing, intVec, pawn.Map, 0); - 6", true);
                            GenSpawn.Spawn(thing, intVec, this.parent.Map, 0);
                        }
                    }
                    this.gasProgress = 0;
                }
            }
        }

        private int gasProgress = 0;

        private int gasTickMax = 64;

        private Random rand = new Random();
    }
}