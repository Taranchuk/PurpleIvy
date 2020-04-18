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
            Pawn pawn = victim as Pawn;
            if (pawn != null && pawn.Faction == Faction.OfPlayer)
            {
                Find.TickManager.slower.SignalForceNormalSpeedShort();
            }
            Map map = victim.Map;
            DamageWorker.DamageResult damageResult = base.Apply(dinfo, victim);
            if (victim.Destroyed && map != null && pawn == null)
            {
                foreach (IntVec3 c in victim.OccupiedRect())
                {
                    FilthMaker.TryMakeFilth(c, map, ThingDefOf.Filth_Ash, 1, FilthSourceFlags.None);
                }
                Plant plant = victim as Plant;
                if (plant != null && victim.def.plant.IsTree && plant.LifeStage != PlantLifeStage.Sowing && victim.def != ThingDefOf.BurnedTree)
                {
                    ((DeadPlant)GenSpawn.Spawn(ThingDefOf.BurnedTree, victim.Position, map, WipeMode.Vanish)).Growth = plant.Growth;
                }
            }
            return damageResult;
        }
    }
}
