﻿using System;
using Verse;
using Verse.Sound;
namespace RimWorld
{
    public class Meteor : Thing
    {
        public int age;
        public MeteorInfo info;
        private static readonly SoundDef OpenSound = SoundDef.Named("DropPodOpen");
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
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named("ChunkSlag"), null);
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
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named("ChunkSlag"), null);
                    GenPlace.TryPlaceThing(thing, base.Position, this.Map, ThingPlaceMode.Near);
                }
            }
            
            Meteor.OpenSound.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
            this.Destroy(DestroyMode.Vanish);
        }
    }
}