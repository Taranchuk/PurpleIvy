using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace PurpleIvy
{
    public class DamageWorker_AddInjuryNoCamShaker : DamageWorker_AddInjury
    {
        public override void ExplosionStart(Explosion explosion, List<IntVec3> cellsToAffect)
        {
            if (this.def.explosionHeatEnergyPerCell > 1.401298E-45f)
            {
                GenTemperature.PushHeat(explosion.Position, explosion.Map, this.def.explosionHeatEnergyPerCell * (float)cellsToAffect.Count);
            }
            this.ExplosionVisualEffectCenter(explosion);
        }

        public override void ExplosionAffectCell(Explosion explosion, IntVec3 c, List<Thing> damagedThings, List<Thing> ignoredThings, bool canThrowMotes)
        {
            if (this.def.explosionCellMote != null && canThrowMotes)
            {
                Mote mote = c.GetFirstThing(explosion.Map, this.def.explosionCellMote) as Mote;
                if (mote != null)
                {
                    mote.spawnTick = Find.TickManager.TicksGame;
                }
                else
                {
                    float t = Mathf.Clamp01((explosion.Position - c).LengthHorizontal / explosion.radius);
                    Color color = Color.Lerp(this.def.explosionColorCenter, this.def.explosionColorEdge, t);
                    PurpleIvyMoteMaker.ThrowExplosionCell(c, explosion.Map, this.def.explosionCellMote, color);
                }
            }
            DamageWorker_AddInjuryNoCamShaker.thingsToAffect.Clear();
            float num = float.MinValue;
            bool flag = false;
            List<Thing> list = explosion.Map.thingGrid.ThingsListAt(c);
            for (int i = 0; i < list.Count; i++)
            {
                Thing thing = list[i];
                if (thing.def.category != ThingCategory.Mote && thing.def.category != ThingCategory.Ethereal)
                {
                    if (thing.Faction != PurpleIvyData.AlienFaction)
                    {
                        DamageWorker_AddInjuryNoCamShaker.thingsToAffect.Add(thing);
                        if (thing.def.Fillage == FillCategory.Full && thing.def.Altitude > num)
                        {
                            flag = true;
                            num = thing.def.Altitude;
                        }
                    }
                }
            }
            for (int j = 0; j < DamageWorker_AddInjuryNoCamShaker.thingsToAffect.Count; j++)
            {
                if (DamageWorker_AddInjuryNoCamShaker.thingsToAffect[j].def.Altitude >= num)
                {
                    if (DamageWorker_AddInjuryNoCamShaker.thingsToAffect[j] is Pawn)
                    {
                        Pawn pawn = (Pawn)DamageWorker_AddInjuryNoCamShaker.thingsToAffect[j];
                        if (Rand.Chance(0.3f))
                        {
                            pawn.stances.stunner.StunFor(Rand.RangeInclusive(100, 200), explosion.instigator);
                        }

                    }
                    this.ExplosionDamageThing(explosion, DamageWorker_AddInjuryNoCamShaker.thingsToAffect[j], damagedThings, ignoredThings, c);
                }
            }
            if (!flag)
            {
                this.ExplosionDamageTerrain(explosion, c);
            }
            if (this.def.explosionSnowMeltAmount > 0.0001f)
            {
                float lengthHorizontal = (c - explosion.Position).LengthHorizontal;
                float num2 = 1f - lengthHorizontal / explosion.radius;
                if (num2 > 0f)
                {
                    explosion.Map.snowGrid.AddDepth(c, -num2 * this.def.explosionSnowMeltAmount);
                }
            }
            if (this.def == DamageDefOf.Bomb || this.def == DamageDefOf.Flame)
            {
                List<Thing> list2 = explosion.Map.listerThings.ThingsOfDef(ThingDefOf.RectTrigger);
                for (int k = 0; k < list2.Count; k++)
                {
                    RectTrigger rectTrigger = (RectTrigger)list2[k];
                    if (rectTrigger.activateOnExplosion && rectTrigger.Rect.Contains(c))
                    {
                        rectTrigger.ActivatedBy(null);
                    }
                }
            }
        }

        protected override void ExplosionVisualEffectCenter(Explosion explosion)
        {
            for (int i = 0; i < 4; i++)
            {
                PurpleIvyMoteMaker.ThrowToxicGas(explosion.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(explosion.radius * 0.7f), explosion.Map, 1f);
            }
            //if (this.def.explosionInteriorMote != null)
            //{
            //    int num = Mathf.RoundToInt(3.14159274f * explosion.radius * explosion.radius / 6f);
            //    for (int j = 0; j < num; j++)
            //    {
            //        MoteMaker.ThrowExplosionInteriorMote(explosion.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(explosion.radius * 0.7f), explosion.Map, this.def.explosionInteriorMote);
            //    }
            //}
        }

        private static List<Thing> thingsToAffect = new List<Thing>();

    }
}

