﻿using System;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace PurpleIvy
{
    [StaticConstructorOnStartup]
    public class MeteorIncoming : Thing
    {
        protected const int MinTicksToImpact = 120;
        protected const int MaxTicksToImpact = 200;
        private const int SoundAnticipationTicks = 100;
        public MeteorInfo contents;
        protected int ticksToImpact = 120;
        private bool soundPlayed;
        private static readonly SoundDef LandSound = SoundDef.Named("DropPod_Fall");
        private static readonly SoundDef ExplodeSound = SoundDef.Named("Explosion_Bomb");
        private static readonly Material ShadowMat = MaterialPool.MatFrom("Things/Special/DropPodShadow", ShaderDatabase.Transparent);
        public override Vector3 DrawPos
        {
            get
            {
                Vector3 result = base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Item);
                float num = (float)(this.ticksToImpact * this.ticksToImpact) * 0.01f;
                result.x -= num * 0.4f;
                result.z += num * 0.6f;
                return result;
            }
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.ticksToImpact = Rand.RangeInclusive(120, 200);
            if (GridsUtility.Roofed(base.Position, map))
            {
                Log.Warning("Meteor dropped on roof at " + base.Position);
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.ticksToImpact, "ticksToImpact", 0, false);
            Scribe_Deep.Look<MeteorInfo>(ref this.contents, "contents");
        }
        public override void Tick()
        {
            this.ticksToImpact--;
            if (this.ticksToImpact <= 0)
            {
                this.PodImpact();
            }
            if (!this.soundPlayed && this.ticksToImpact < 100)
            {
                this.soundPlayed = true;
                MeteorIncoming.LandSound.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
            }
        }
        public override void DrawAt(Vector3 drawLoc, bool flip)
        {
            base.DrawAt(drawLoc);
            float num = 2f + (float)this.ticksToImpact / 100f;
            Vector3 s = new Vector3(num, 1f, num);
            Matrix4x4 matrix = default(Matrix4x4);
            drawLoc.y = Altitudes.AltitudeFor(AltitudeLayer.Shadows);
            matrix.SetTRS(this.TrueCenter(), base.Rotation.AsQuat, s);
            Graphics.DrawMesh(MeshPool.plane10Back, matrix, MeteorIncoming.ShadowMat, 0);
        }
        private void PodImpact()
        {
            for (int i = 0; i < 6; i++)
            {
                Vector3 spawnLoc = base.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(1f);
                MoteMaker.ThrowDustPuff(spawnLoc, this.Map, 1.2f);
            }
            MoteMaker.ThrowLightningGlow(base.Position.ToVector3Shifted(), this.Map, 2f);
            MoteMaker.ThrowSmoke(base.Position.ToVector3Shifted(), this.Map, 2f);
            //MoteMaker.ThrowExplosionCell(base.Position, this.Map);

            MeteorIncoming.ExplodeSound.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
            foreach (IntVec3 current in GenAdj.CellsAdjacent8Way(this))
            {
                MoteMaker.ThrowAirPuffUp(base.Position.ToVector3Shifted(), this.Map);
            }
            Meteor meteor = new Meteor(ThingDef.Named("PI_Meteorite"));
            meteor.info = this.contents;
            GenSpawn.Spawn(meteor.meteor, base.Position, this.Map, base.Rotation);
            this.Destroy(DestroyMode.Vanish);
        }
    }
}
