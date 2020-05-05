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
        public static Pawn GenerateKorsolian(string PawnKind)
        {
            PawnKindDef pawnKindDef = PawnKindDef.Named(PawnKind);
            Pawn NewPawn = PawnGenerator.GeneratePawn(pawnKindDef, null);
            NewPawn.SetFaction(PurpleIvyData.KorsolianFaction);
            NewPawn.ageTracker.AgeBiologicalTicks = new IntRange(40000000, 400000000).RandomInRange;
            NewPawn.ageTracker.AgeChronologicalTicks = new IntRange(40000000, 400000000).RandomInRange;
            return NewPawn;
        }

        public static Pawn GenerateParasite(List<string> PawnKinds)
        {
            PawnKindDef pawnKindDef = PawnKindDef.Named(PawnKinds.RandomElement());
            Pawn NewPawn = PawnGenerator.GeneratePawn(pawnKindDef, null);
            NewPawn.SetFaction(PurpleIvyData.AlienFaction);
            NewPawn.ageTracker.AgeBiologicalTicks = 40000;
            NewPawn.ageTracker.AgeChronologicalTicks = 40000;
            return NewPawn;
        }

        public static void UpdateBiomes()
        {
            if (PurpleIvyData.BiomesToClear == true)
            {
                PurpleIvyUtils.ClearAlienBiomesOuterTheSources();
                PurpleIvyData.BiomesToClear = false;
            }
            Find.World.renderer.SetDirty<WorldLayerRegenerateBiomes>();
            PurpleIvyData.BiomesDirty = false;
        }
        public static void ClearAlienBiomesOuterTheSources()
        {
            for (int i = PurpleIvyData.TotalPollutedBiomes.Count - 1; i >= 0; i--)
            {
                int tile = PurpleIvyData.TotalPollutedBiomes[i];
                if (PurpleIvyUtils.TileInRadiusOfInfectedSites(tile) != true)
                {
                    Log.Message("Return old biome: " + tile.ToString());
                    BiomeDef origBiome = Find.WorldGrid[tile].biome;
                    BiomeDef newBiome = BiomeDef.Named(origBiome.defName.ReplaceFirst("PI_", string.Empty));
                    Find.WorldGrid[tile].biome = newBiome;
                    PurpleIvyData.TotalPollutedBiomes.Remove(tile);
                    PurpleIvyData.BiomesToRenderNow.Add(tile);
                }
            }
        }
        public static float GetPartFromPercentage(float percent, float whole)
        {
            return (float)(percent * whole) / 100f;
        }

        public static float GetPercentageFromPartWhole(float part, int whole)
        {
            return 100 * part / (float)(whole);
        }

        public static float getFogProgressWithOuterSources(int count, WorldObjectComp_InfectedTile comp, out bool comeFromOuterSource)
        {
            var result = PurpleIvyUtils.getFogProgress(count);
            //Log.Message("fog progress: " + result.ToString());
            var outerSource = 0f;
            foreach (var data in PurpleIvyData.TotalFogProgress.Where(data => data.Key != comp))
            {
                int distance = Find.WorldGrid.TraversalDistanceBetween(comp.infectedTile, data.Key.infectedTile, true, int.MaxValue);
                if (distance <= data.Key.radius)
                {
                    float floatRadius = ((float)data.Key.counter - 500f) / 100f;
                    if (floatRadius < 0)
                    {
                        floatRadius = 0;
                    }
                    float newValue = GetPartFromPercentage(GetPercentageFromPartWhole(floatRadius, distance) - 100f, data.Value);
                    if (newValue > data.Value)
                    {
                        outerSource += data.Value;
                    }
                    else
                    {
                        outerSource += newValue;
                    }
                }
            }
            if (outerSource < 0f)
            {
                outerSource = 0f;
            }
            if (result == 0f && outerSource > 0f)
            {
                comeFromOuterSource = true;
            }
            else
            {
                comeFromOuterSource = false;
            }
            return result + outerSource;
        }

        public static bool TileInRadiusOfInfectedSites(int tile)
        {
            foreach (var comp in PurpleIvyData.TotalFogProgress)
            {
                //Log.Message("Checking tile: " + tile.ToString() + " against "
                //    + comp.Key.infectedTile.ToString() + " - radius: " + comp.Key.radius.ToString()
                //    + " - distance: " + (Find.WorldGrid.TraversalDistanceBetween
                //(comp.Key.infectedTile, tile, true, int.MaxValue)).ToString());
                if (Find.WorldGrid.TraversalDistanceBetween
                (comp.Key.infectedTile, tile, true, int.MaxValue) <= comp.Key.radius)
                {
                    //Log.Message("Tile in radius: " + tile.ToString());
                    return true;
                }
            }
            //Log.Message("Tile not in radius: " + tile.ToString());
            return false;
        }
        public static float getFogProgress(int count)
        {
            var result = (float)(count - 1000) / (float)1500;
            if (result < 0f)
            {
                result = 0f;
            }
            return result / 1.5f;
        }
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

        public static Job SmokeAttackJob(Pawn pawn, Thing target)
        {
            var job2 = JobMaker.MakeJob(PurpleIvyDefOf.PI_ThrowSmoke, target);
            job2.maxNumMeleeAttacks = 1;
            job2.expiryInterval = Rand.Range(420, 900);
            job2.canBash = true;
            job2.killIncappedTarget = true;
            return job2;
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

