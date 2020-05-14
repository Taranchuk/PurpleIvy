using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using System.Linq;
using Verse.AI;

namespace PurpleIvy
{
    public class Building_Meteorite : Building, IAttackTarget
    {
        private int spawnticks = 200;
        public bool activeSpores = false;
        public int damageActiveTick = 0;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.SetFaction(PurpleIvyData.AlienFaction);
            if (this.activeSpores == true)
            {
                foreach (var dir in GenRadial.RadialCellsAround(this.Position, 5, true))
                {
                    if (GenGrid.InBounds(dir, this.Map))
                    {
                        Thing thing = ThingMaker.MakeThing(PurpleIvyDefOf.PI_Spores, null);
                        GenSpawn.Spawn(thing, dir, this.Map, 0);
                        var Spores = (Gas)thing;
                        Spores.destroyTick = Find.TickManager.TicksGame + new IntRange(160000, 180000).RandomInRange;
                    }
                }
            }
        }

        public override void SetFaction(Faction newFaction, Pawn recruiter = null)
        {
            this.factionInt = newFaction;
            IAttackTarget attackTarget = this as IAttackTarget;
            if (attackTarget != null)
            {
                this.Map.attackTargetsCache.UpdateTarget(attackTarget);
            }
        }

        Thing IAttackTarget.Thing
        {
            get
            {
                return this;
            }
        }
        public LocalTargetInfo TargetCurrentlyAimingAt
        {
            get
            {
                return LocalTargetInfo.Invalid;
            }
        }

        public float TargetPriorityFactor
        {
            get
            {
                return 0.1f;
            }
        }

        public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
        {
            return false;
        }

        public override void Tick()
        {
            base.Tick();
            if (this.activeSpores && Find.TickManager.TicksGame % 60 == 0)
            {
                List<Pawn> pawnsToDamage = new List<Pawn>();
                foreach (var dir in GenRadial.RadialCellsAround(this.Position, 5, true))
                {
                     if (GenGrid.InBounds(dir, this.Map))
                     {
                         foreach (var t in this.Map.thingGrid.ThingsListAt(dir))
                         {
                             if (t is Pawn pawn && !pawn.Dead && pawn.Faction != PurpleIvyData.AlienFaction)
                             {
                                 pawnsToDamage.Add(pawn);
                             }
                         }
                     }
                }
                foreach (var pawn in pawnsToDamage)
                {
                    pawn.TakeDamage(new DamageInfo(PurpleIvyDefOf.PI_ToxicBurn, 1f));
                    if (Rand.Chance(0.1f))
                    {
                        pawn.stances.stunner.StunFor(Rand.RangeInclusive(100, 200), null);
                    }
                    if (Rand.Chance(0.1f) && pawn.health.hediffSet.GetFirstHediffOfDef(PurpleIvyDefOf.PI_AlienMutation) == null)
                    {
                        var hediff3 = HediffMaker.MakeHediff(PurpleIvyDefOf.PI_AlienMutation, pawn, null);
                        pawn.health.AddHediff(hediff3, null, null, null);
                    }
                }
                if (Find.TickManager.TicksGame > this.damageActiveTick)
                {
                    this.activeSpores = false;
                }
            }
            spawnticks--;
            if (spawnticks <= 0)
            {
                PurpleIvyUtils.SpawnNests(this);
                spawnticks = 15000;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.spawnticks, "spawnticks", 0, false);
            Scribe_Values.Look<int>(ref this.damageActiveTick, "damageActiveTick", 0, false);
            Scribe_Values.Look<bool>(ref this.activeSpores, "activeSpores", false, false);
        }
    }
}

