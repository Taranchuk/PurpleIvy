using System;
using UnityEngine;
using Verse;
using Verse.Sound;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using RimWorld;

namespace PurpleIvy
{
    public class Projectile_Seed : Projectile_Explosive
    {
        public Thing Gun => ThingMaker.MakeThing(this.def.building.turretGunDef, null);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.ticksToDetonation, "ticksToDetonation", 0, false);
        }

        public override void Tick()
        {
            base.Tick();
            if (this.ticksToDetonation > 0)
            {
                this.ticksToDetonation--;
                if (this.ticksToDetonation <= 0)
                {
                    this.Explode();
                }
            }
        }

        protected override void Impact(Thing hitThing)
        {
            if (this.def.projectile.explosionDelay == 0)
            {
                this.Explode();
                return;
            }
            if (hitThing == null)
            {

                Log.Message("IMPACT NULL" + this.landed, true);
            }
            else
            {
                foreach (IntVec3 current in hitThing.CellsAdjacent8WayAndInside())
                {
                    MoteMaker.ThrowDustPuff(current, this.Map, 2f);

                    var t = GenClosest.ClosestThingReachable(hitThing.Position, hitThing.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.ClosestTouch, TraverseParms.For((Pawn)hitThing, Danger.Deadly, TraverseMode.ByPawn), 9999, new Predicate<Thing>(this.IsValidTarget), null, 0, -1, false, RegionType.Set_Passable, false);

                    //Thing t = GenAI.BestAttackTarget(hitThing.Position, this, new Predicate<Thing>(this.IsValidTarget), 2f, 0f, false, false, false, true);

                    var pawn = t as Pawn;
                    pawn?.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Wander_Psychotic, null,
                        false, false, null, false);

                    //pawn.thinker.mindState.Sanity.Equals(SanityState.Psychotic);
                }
            }
            this.landed = true;
            this.ticksToDetonation = this.def.projectile.explosionDelay;
            GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(this, this.def.projectile.damageDef, this.launcher.Faction);
        }

        protected virtual void Explode()
        {
            Map map = base.Map;
            this.Destroy(DestroyMode.Vanish);
            if (this.def.projectile.explosionEffect != null)
            {
                Effecter effecter = this.def.projectile.explosionEffect.Spawn();
                effecter.Trigger(new TargetInfo(base.Position, map, false), new TargetInfo(base.Position, map, false));
                effecter.Cleanup();
            }
            IntVec3 position = base.Position;
            Map map2 = map;
            float explosionRadius = this.def.projectile.explosionRadius;
            DamageDef damageDef = this.def.projectile.damageDef;
            Thing launcher = this.launcher;
            int damageAmount = base.DamageAmount;
            float armorPenetration = base.ArmorPenetration;
            SoundDef soundExplode = this.def.projectile.soundExplode;
            ThingDef equipmentDef = this.equipmentDef;
            ThingDef def = this.def;
            Thing thing = this.intendedTarget.Thing;
            ThingDef postExplosionSpawnThingDef = this.def.projectile.postExplosionSpawnThingDef;
            float postExplosionSpawnChance = this.def.projectile.postExplosionSpawnChance;
            int postExplosionSpawnThingCount = this.def.projectile.postExplosionSpawnThingCount;
            ThingDef preExplosionSpawnThingDef = this.def.projectile.preExplosionSpawnThingDef;
            float preExplosionSpawnChance = this.def.projectile.preExplosionSpawnChance;
            int preExplosionSpawnThingCount = this.def.projectile.preExplosionSpawnThingCount;
            GenExplosion.DoExplosion(position, map2, explosionRadius, damageDef, launcher, damageAmount, armorPenetration, soundExplode, equipmentDef, def, thing, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, this.def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, preExplosionSpawnChance, preExplosionSpawnThingCount, this.def.projectile.explosionChanceToStartFire, this.def.projectile.explosionDamageFalloff, new float?(this.origin.AngleToFlat(this.destination)), null);
        }


        private bool IsValidTarget(Thing t)
        {
            if (!(t is Pawn pawn)) return true;
            if (!this.def.projectile.flyOverhead) return !GenAI.MachinesLike(base.Faction, pawn);
            var roofDef = this.Map.roofGrid.RoofAt(t.Position);
            if (roofDef != null && roofDef.isThickRoof)
            {
                return false;
            }
            return !GenAI.MachinesLike(base.Faction, pawn);
        }

        private int ticksToDetonation;
    }
}

