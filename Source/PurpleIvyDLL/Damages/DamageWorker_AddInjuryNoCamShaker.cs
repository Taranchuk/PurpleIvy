using System;
using System.Collections.Generic;
using RimWorld;
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
            Log.Message("5 MoteMaker");

            MoteMaker.MakeStaticMote(explosion.Position, explosion.Map, ThingDefOf.Mote_ExplosionFlash, explosion.radius * 6f);
            this.ExplosionVisualEffectCenter(explosion);
        }
    }
}

