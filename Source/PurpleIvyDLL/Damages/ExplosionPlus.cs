using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PurpleIvy
{
    public class ExplosionPlus : Explosion
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                this.cellsToAffect = SimplePool<List<IntVec3>>.Get();
                this.cellsToAffect.Clear();
                this.damagedThings = SimplePool<List<Thing>>.Get();
                this.damagedThings.Clear();
                this.addedCellsAffectedOnlyByDamage = SimplePool<HashSet<IntVec3>>.Get();
                this.addedCellsAffectedOnlyByDamage.Clear();
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            this.cellsToAffect.Clear();
            SimplePool<List<IntVec3>>.Return(this.cellsToAffect);
            this.cellsToAffect = null;
            this.damagedThings.Clear();
            SimplePool<List<Thing>>.Return(this.damagedThings);
            this.damagedThings = null;
            this.addedCellsAffectedOnlyByDamage.Clear();
            SimplePool<HashSet<IntVec3>>.Return(this.addedCellsAffectedOnlyByDamage);
            this.addedCellsAffectedOnlyByDamage = null;
        }

        public override void StartExplosion(SoundDef explosionSound, List<Thing> ignoredThings)
        {
            if (!base.Spawned)
            {
                Log.Error("Called StartExplosion() on unspawned thing.", false);
                return;
            }
            this.startTick = Find.TickManager.TicksGame;
            this.ignoredThings = ignoredThings;
            this.cellsToAffect.Clear();
            this.damagedThings.Clear();
            this.addedCellsAffectedOnlyByDamage.Clear();
            this.cellsToAffect.AddRange(this.damType.Worker.ExplosionCellsToHit(this));
            if (this.applyDamageToExplosionCellsNeighbors)
            {
                this.AddCellsNeighbors(this.cellsToAffect);
            }
            this.damType.Worker.ExplosionStart(this, this.cellsToAffect);
            this.PlayExplosionSound(explosionSound);
            Log.Message("1 MoteMaker");
            MoteMaker.MakeWaterSplash(base.Position.ToVector3Shifted(), base.Map, this.radius * 6f, 20f);
            this.cellsToAffect.Sort((IntVec3 a, IntVec3 b) => this.GetCellAffectTick(b).CompareTo(this.GetCellAffectTick(a)));
            RegionTraverser.BreadthFirstTraverse(base.Position, base.Map, (Region from, Region to) => true, delegate (Region x)
            {
                List<Thing> allThings = x.ListerThings.AllThings;
                for (int i = allThings.Count - 1; i >= 0; i--)
                {
                    if (allThings[i].Spawned)
                    {
                        allThings[i].Notify_Explosion(this);
                    }
                }
                return false;
            }, 25, RegionType.Set_Passable);
        }

        public override void Tick()
        {
            int ticksGame = Find.TickManager.TicksGame;
            int num = this.cellsToAffect.Count - 1;
            while (num >= 0 && ticksGame >= this.GetCellAffectTick(this.cellsToAffect[num]))
            {
                try
                {
                    this.AffectCell(this.cellsToAffect[num]);
                }
                catch (Exception ex)
                {
                    Log.Error(string.Concat(new object[]
                    {
                        "Explosion could not affect cell ",
                        this.cellsToAffect[num],
                        ": ",
                        ex
                    }), false);
                }
                this.cellsToAffect.RemoveAt(num);
                num--;
            }
            if (!this.cellsToAffect.Any<IntVec3>())
            {
                this.Destroy(DestroyMode.Vanish);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.radius, "radius", 0f, false);
            Scribe_Defs.Look<DamageDef>(ref this.damType, "damType");
            Scribe_Values.Look<int>(ref this.damAmount, "damAmount", 0, false);
            Scribe_Values.Look<float>(ref this.armorPenetration, "armorPenetration", 0f, false);
            Scribe_References.Look<Thing>(ref this.instigator, "instigator", false);
            Scribe_Defs.Look<ThingDef>(ref this.weapon, "weapon");
            Scribe_Defs.Look<ThingDef>(ref this.projectile, "projectile");
            Scribe_References.Look<Thing>(ref this.intendedTarget, "intendedTarget", false);
            Scribe_Values.Look<bool>(ref this.applyDamageToExplosionCellsNeighbors, "applyDamageToExplosionCellsNeighbors", false, false);
            Scribe_Defs.Look<ThingDef>(ref this.preExplosionSpawnThingDef, "preExplosionSpawnThingDef");
            Scribe_Values.Look<float>(ref this.preExplosionSpawnChance, "preExplosionSpawnChance", 0f, false);
            Scribe_Values.Look<int>(ref this.preExplosionSpawnThingCount, "preExplosionSpawnThingCount", 1, false);
            Scribe_Defs.Look<ThingDef>(ref this.postExplosionSpawnThingDef, "postExplosionSpawnThingDef");
            Scribe_Values.Look<float>(ref this.postExplosionSpawnChance, "postExplosionSpawnChance", 0f, false);
            Scribe_Values.Look<int>(ref this.postExplosionSpawnThingCount, "postExplosionSpawnThingCount", 1, false);
            Scribe_Values.Look<float>(ref this.chanceToStartFire, "chanceToStartFire", 0f, false);
            Scribe_Values.Look<bool>(ref this.damageFalloff, "dealMoreDamageAtCenter", false, false);
            Scribe_Values.Look<IntVec3?>(ref this.needLOSToCell1, "needLOSToCell1", null, false);
            Scribe_Values.Look<IntVec3?>(ref this.needLOSToCell2, "needLOSToCell2", null, false);
            Scribe_Values.Look<int>(ref this.startTick, "startTick", 0, false);
            Scribe_Collections.Look<IntVec3>(ref this.cellsToAffect, "cellsToAffect", LookMode.Value, Array.Empty<object>());
            Scribe_Collections.Look<Thing>(ref this.damagedThings, "damagedThings", LookMode.Reference, Array.Empty<object>());
            Scribe_Collections.Look<Thing>(ref this.ignoredThings, "ignoredThings", LookMode.Reference, Array.Empty<object>());
            Scribe_Collections.Look<IntVec3>(ref this.addedCellsAffectedOnlyByDamage, "addedCellsAffectedOnlyByDamage", LookMode.Value);
            Scribe_Values.Look<IntVec3>(ref this.center, "center", IntVec3.Invalid, false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (this.damagedThings != null)
                {
                    this.damagedThings.RemoveAll((Thing x) => x == null);
                }
                if (this.ignoredThings != null)
                {
                    this.ignoredThings.RemoveAll((Thing x) => x == null);
                }
            }
        }

        private int GetCellAffectTick(IntVec3 cell)
        {
            return this.startTick + (int)((cell - base.Position).LengthHorizontal * 1.5f);
        }

        private void AffectCell(IntVec3 c)
        {
            if (!c.InBounds(base.Map))
            {
                return;
            }
            bool flag = this.ShouldCellBeAffectedOnlyByDamage(c);
            if (!flag && Rand.Chance(this.preExplosionSpawnChance) && c.Walkable(base.Map))
            {                
                if (c.DistanceTo(this.center) < 1)
                {
                    this.TrySpawnExplosionThing(this.preExplosionSpawnThingDef, c, this.preExplosionSpawnThingCount);
                }
            }
            this.damType.Worker.ExplosionAffectCell(this, c, this.damagedThings, this.ignoredThings, !flag);
            if (!flag && Rand.Chance(this.postExplosionSpawnChance) && c.Walkable(base.Map))
            {
                if (c.DistanceTo(this.center) < 1)
                {
                    this.TrySpawnExplosionThing(this.postExplosionSpawnThingDef, c, this.postExplosionSpawnThingCount);
                }
            }
            float num = this.chanceToStartFire;
            if (this.damageFalloff)
            {
                num *= Mathf.Lerp(1f, 0.2f, c.DistanceTo(base.Position) / this.radius);
            }
            if (Rand.Chance(num))
            {
                FireUtility.TryStartFireIn(c, base.Map, Rand.Range(0.1f, 0.925f));
            }
        }

        private void TrySpawnExplosionThing(ThingDef thingDef, IntVec3 c, int count)
        {
            if (thingDef == null)
            {
                return;
            }
            if (thingDef.IsFilth)
            {
                FilthMaker.TryMakeFilth(c, base.Map, thingDef, count, FilthSourceFlags.None);
                return;
            }
            Thing thing = ThingMaker.MakeThing(thingDef, null);
            thing.stackCount = count;
            GenSpawn.Spawn(thing, c, base.Map, WipeMode.Vanish);
        }

        private void PlayExplosionSound(SoundDef explosionSound)
        {
            bool flag;
            if (Prefs.DevMode)
            {
                flag = (explosionSound != null);
            }
            else
            {
                flag = !explosionSound.NullOrUndefined();
            }
            if (flag)
            {
                explosionSound.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
                return;
            }
            this.damType.soundExplosion.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
        }

        private void AddCellsNeighbors(List<IntVec3> cells)
        {
            ExplosionPlus.tmpCells.Clear();
            this.addedCellsAffectedOnlyByDamage.Clear();
            for (int i = 0; i < cells.Count; i++)
            {
                ExplosionPlus.tmpCells.Add(cells[i]);
            }
            for (int j = 0; j < cells.Count; j++)
            {
                if (cells[j].Walkable(base.Map))
                {
                    for (int k = 0; k < GenAdj.AdjacentCells.Length; k++)
                    {
                        IntVec3 intVec = cells[j] + GenAdj.AdjacentCells[k];
                        if (intVec.InBounds(base.Map) && ExplosionPlus.tmpCells.Add(intVec))
                        {
                            this.addedCellsAffectedOnlyByDamage.Add(intVec);
                        }
                    }
                }
            }
            cells.Clear();
            foreach (IntVec3 item in ExplosionPlus.tmpCells)
            {
                cells.Add(item);
            }
            ExplosionPlus.tmpCells.Clear();
        }

        private bool ShouldCellBeAffectedOnlyByDamage(IntVec3 c)
        {
            return this.applyDamageToExplosionCellsNeighbors && this.addedCellsAffectedOnlyByDamage.Contains(c);
        }

        private int startTick;

        private List<IntVec3> cellsToAffect;

        public IntVec3 center;

        private List<Thing> damagedThings;

        private List<Thing> ignoredThings;

        private HashSet<IntVec3> addedCellsAffectedOnlyByDamage;

        private const float DamageFactorAtEdge = 0.2f;

        private static HashSet<IntVec3> tmpCells = new HashSet<IntVec3>();
    }
}

