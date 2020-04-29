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
           //foreach (var c in victim.OccupiedRect())
           //{
           //    FilthMaker.TryMakeFilth(c, map, PurpleIvyDefOf.PI_ToxicFilth, 1, FilthSourceFlags.None);
           //
           //    if (Rand.Chance(0.3f))
           //    {
           //        FilthMaker.TryMakeFilth(c, map, PurpleIvyDefOf.PI_ToxicFilth, 1, FilthSourceFlags.None);
           //    }
           //}

            if (victim is Plant plant && victim.def.plant.IsTree && plant.LifeStage != PlantLifeStage.Sowing && victim.def != ThingDefOf.BurnedTree)
            {
                ((DeadPlant)GenSpawn.Spawn(ThingDefOf.BurnedTree, victim.Position, map, WipeMode.Vanish)).Growth = plant.Growth;
            }
            return damageResult;
        }
    }
}

