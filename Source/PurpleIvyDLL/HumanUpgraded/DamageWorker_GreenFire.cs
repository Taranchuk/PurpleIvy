using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace HumanUpgraded
{
	public class DamageWorker_GreenFire : DamageWorker_AddInjury
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
			if (!damageResult.deflected && !dinfo.InstantPermanentInjury)
			{
				victim.TryAttachFire(Rand.Range(0.15f, 0.25f));
			}
			if (victim.Destroyed && map != null && pawn == null)
			{
				foreach (IntVec3 intVec in GenAdj.OccupiedRect(victim))
				{
					FilthMaker.TryMakeFilth(intVec, map, ThingDefOf.Filth_Ash, 1);
				}
				Plant plant = victim as Plant;
				if (plant != null && victim.def.plant.IsTree && plant.LifeStage != null && victim.def != ThingDefOf.BurnedTree)
				{
					((DeadPlant)GenSpawn.Spawn(ThingDefOf.BurnedTree, victim.Position, map, 0)).Growth = plant.Growth;
				}
			}
			return damageResult;
		}

        public override void ExplosionAffectCell(Explosion explosion, IntVec3 c, List<Thing> damagedThings, List<Thing> ignoredThings, bool canThrowMotes)
        {
            base.ExplosionAffectCell(explosion, c, damagedThings, ignoredThings, canThrowMotes);
            if (this.def == DamageDefOf.GreenFire && Rand.Chance(GreenFireUtility.ChanceToStartFireIn(c, explosion.Map)))
            {
                GreenFireUtility.TryStartFireIn(c, explosion.Map, Rand.Range(0.2f, 0.6f));
            }
        }
	}
}
