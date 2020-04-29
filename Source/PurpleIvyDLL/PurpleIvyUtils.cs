using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    [StaticConstructorOnStartup]
    public static class PurpleIvyUtils
    {

        public static void DoExplosion(IntVec3 center, Map map, float radius, DamageDef damType, Thing instigator, int damAmount = -1, float armorPenetration = -1f, SoundDef explosionSound = null, ThingDef weapon = null, ThingDef projectile = null, Thing intendedTarget = null, ThingDef postExplosionSpawnThingDef = null, float postExplosionSpawnChance = 0f, int postExplosionSpawnThingCount = 1, bool applyDamageToExplosionCellsNeighbors = false, ThingDef preExplosionSpawnThingDef = null, float preExplosionSpawnChance = 0f, int preExplosionSpawnThingCount = 1, float chanceToStartFire = 0f, bool damageFalloff = false, float? direction = null, List<Thing> ignoredThings = null)
        {
            if (map == null)
            {
                Log.Warning("Tried to do explosion in a null map.", false);
                return;
            }
            if (damAmount < 0)
            {
                damAmount = damType.defaultDamage;
                armorPenetration = damType.defaultArmorPenetration;
                if (damAmount < 0)
                {
                    Log.ErrorOnce("Attempted to trigger an explosion without defined damage", 91094882, false);
                    damAmount = 1;
                }
            }
            if (armorPenetration < 0f)
            {
                armorPenetration = (float)damAmount * 0.015f;
            }
            ExplosionPlus explosion = (ExplosionPlus)GenSpawn.Spawn(PurpleIvyDefOf.PI_ExplosionPlus, center, map, WipeMode.Vanish);
            IntVec3? needLOSToCell = null;
            IntVec3? needLOSToCell2 = null;
            explosion.radius = radius;
            explosion.damType = damType;
            explosion.instigator = instigator;
            explosion.damAmount = damAmount;
            explosion.armorPenetration = armorPenetration;
            explosion.weapon = weapon;
            explosion.projectile = projectile;
            explosion.intendedTarget = intendedTarget;
            explosion.preExplosionSpawnThingDef = preExplosionSpawnThingDef;
            explosion.preExplosionSpawnChance = preExplosionSpawnChance;
            explosion.preExplosionSpawnThingCount = preExplosionSpawnThingCount;
            explosion.postExplosionSpawnThingDef = postExplosionSpawnThingDef;
            explosion.postExplosionSpawnChance = postExplosionSpawnChance;
            explosion.postExplosionSpawnThingCount = postExplosionSpawnThingCount;
            explosion.applyDamageToExplosionCellsNeighbors = applyDamageToExplosionCellsNeighbors;
            explosion.chanceToStartFire = chanceToStartFire;
            explosion.center = center;
            explosion.damageFalloff = damageFalloff;
            explosion.needLOSToCell1 = needLOSToCell;
            explosion.needLOSToCell2 = needLOSToCell2;
            explosion.StartExplosion(explosionSound, ignoredThings);
        }

        public static Job KillAttackJob(Pawn pawn, Thing target)
        {
            var job2 = JobMaker.MakeJob(PurpleIvyDefOf.PI_Kill, target);
            job2.maxNumMeleeAttacks = 1;
            job2.expiryInterval = Rand.Range(420, 900);
            job2.canBash = true;
            job2.killIncappedTarget = true;
            return job2;
        }

        public static Job JumpOnTargetJob(Pawn pawn, Thing target)
        {
            var job = JobMaker.MakeJob(PurpleIvyDefOf.PI_JumpOnTarget, target);
            job.maxNumMeleeAttacks = 1;
            job.expiryInterval = Rand.Range(420, 900);
            job.attackDoorIfTargetLost = true;
            job.canBash = true;
            return job;
        }
        public static Job MeleeAttackJob(Pawn pawn, Thing target)
        {
            var job = JobMaker.MakeJob(PurpleIvyDefOf.PI_AttackMelee, target);
            job.maxNumMeleeAttacks = 1;
            job.expiryInterval = Rand.Range(420, 900);
            job.attackDoorIfTargetLost = true;
            job.canBash = true;
            return job;
        }

        public static Job RangeAttackJob(Pawn pawn, Thing target)
        {
            Verb verb = pawn.TryGetAttackVerb(target, true);
            JobDef jobDef;
            jobDef = PurpleIvyDefOf.PI_AnimalRangeAttack;
            Job job = new Job(jobDef);
            job.verbToUse = verb;
            job.expiryInterval = Rand.Range(420, 900);
            job.targetA = new LocalTargetInfo(target);
            job.playerForced = true;
            job.killIncappedTarget = true;
            job.ignoreForbidden = true;
            job.ignoreDesignations = true;
            job.ignoreJoyTimeAssignment = true;
            return job;
        }
        public static void KillAllPawnsExceptAliens(Map map)
        {
            for (int i = map.mapPawns.AllPawns.Count - 1; i >= 0; i--)
            {
                Pawn pawn = map.mapPawns.AllPawns[i];
                if (pawn.Faction == null || pawn.Faction != Faction.OfPlayer
                    && pawn.Faction?.def != PurpleIvyDefOf.Genny)
                {
                    if (pawn.RaceProps.deathActionWorkerClass == typeof(DeathActionWorker_SmallExplosion)
                        || pawn.RaceProps.deathActionWorkerClass == typeof(DeathActionWorker_BigExplosion))
                    {
                        pawn.Destroy(DestroyMode.Vanish);
                    }
                    else
                    {
                        pawn.Kill(null);
                        Corpse corpse = pawn.ParentHolder as Corpse;
                        corpse.TryGetComp<CompRottable>().RotProgress += 1000000;
                        if (Rand.Chance(0.3f))
                        {
                            foreach (IntVec3 current in GenAdj.CellsAdjacent8WayAndInside(corpse))
                            {
                                if (Rand.Chance(0.25f))
                                {
                                    FilthMaker.TryMakeFilth(current, map, corpse.InnerPawn.RaceProps.BloodDef);
                                }
                                else if (Rand.Chance(0.25f))
                                {
                                    FilthMaker.TryMakeFilth(current, map, PurpleIvyDefOf.AlienBloodFilth);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Log.Message(pawn + " has faction " + pawn.Faction);
                }
            }
        }
    }
}

