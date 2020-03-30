using System;
using Verse;
using Verse.Sound;
using RimWorld;

namespace PurpleIvy
{
    public class Meteor : Thing
    {
        public int age;
        public MeteorInfo info;
        private ThingDef thingDef;
        public Thing meteor;
        private static readonly SoundDef OpenSound = SoundDef.Named("DropPod_Open");

        public Meteor(ThingDef thingDef)
        {
            this.meteor = ThingMaker.MakeThing(thingDef);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.age, "age", 0, false);
            Scribe_Deep.Look<MeteorInfo>(ref this.info, "info");
        }

        public override void Tick()
        {
            this.age++;
            if (this.age > this.info.openDelay)
            {
                this.PodOpen();
            }
        }
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            foreach (Thing current in this.info.containedThings)
            {
                current.Destroy(DestroyMode.Vanish);
            }
            base.Destroy(mode);
            if (mode == DestroyMode.KillFinalize)
            {
                for (int i = 0; i < 1; i++)
                {
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named("ChunkSlagSteel"), null);
                    GenPlace.TryPlaceThing(thing, base.Position, this.Map, ThingPlaceMode.Near);
                }
            }
        }
        private void PodOpen()
        {
            foreach (Thing current in this.info.containedThings)
            {
                GenPlace.TryPlaceThing(current, base.Position, this.Map, ThingPlaceMode.Near);
            }
            this.info.containedThings.Clear();
            if (this.info.leaveSlag)
            {
                for (int i = 0; i < 1; i++)
                {
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named("ChunkSlagSteel"), null);
                    GenPlace.TryPlaceThing(thing, base.Position, this.Map, ThingPlaceMode.Near);
                }
            }
            
            Meteor.OpenSound.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
            this.Destroy(DestroyMode.Vanish);
        }
    }
}
