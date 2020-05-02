using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PurpleIvy
{
    public class DamageWorker_ToxicBurn : DamageWorker_AddInjury
    {
        public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            var pawn = victim as Pawn;
            if (pawn != null && pawn.Faction == Faction.OfPlayer)
            {
                Find.TickManager.slower.SignalForceNormalSpeedShort();
            }
            var map = victim.Map;
            var damageResult = base.Apply(dinfo, victim);
            if (!victim.Destroyed || map == null || pawn != null) return damageResult;
            if (victim is Plant plant && victim.def.plant.IsTree && plant.LifeStage != PlantLifeStage.Sowing && victim.def != ThingDefOf.BurnedTree)
            {
                ((DeadPlant)GenSpawn.Spawn(PurpleIvyDefOf.PI_CorruptedTree, victim.Position, map, WipeMode.Vanish)).Growth = plant.Growth;
            }
            return damageResult;
        }
    }
}

