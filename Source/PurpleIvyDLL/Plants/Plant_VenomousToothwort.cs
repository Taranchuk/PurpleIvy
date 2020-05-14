using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace PurpleIvy
{
    public class Plant_VenomousToothwort : AlienPlant, IAttackTarget
    {
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
                return 0.3f;
            }
        }

        public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
        {
            return false;
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.SetFaction(PurpleIvyData.AlienFaction);
        }

        public override void SetFaction(Faction newFaction, Pawn recruiter = null)
        {
            this.factionInt = newFaction;
            if (this.Spawned)
            {
                IAttackTarget attackTarget = this as IAttackTarget;
                if (attackTarget != null)
                {
                    this.Map.attackTargetsCache.UpdateTarget(attackTarget);
                }
            }
        }
        public override void PostMapInit()
        {
            base.PostMapInit();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
        }

        public Job TryGiveJob(Pawn pawn)
        {
            if (pawn.playerSettings != null && pawn.playerSettings.UsesConfigurableHostilityResponse &&
                pawn.playerSettings.hostilityResponse != HostilityResponseMode.Attack)
            {
                return null;
            }
            if (Find.TickManager.TicksGame > pawn.mindState.lastMeleeThreatHarmTick + 400)
            {
                return null;
            }
            if (pawn.WorkTagIsDisabled(WorkTags.Violent))
            {
                return null;
            }
            Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, this);
            job.maxNumMeleeAttacks = 1;
            job.expiryInterval = 200;
            return job;
        }

        public void AttackPawnsNearby(out bool activeThreat)
        {
            List<Thing> list = new List<Thing>();
            List<Pawn> pawnsToAttack = new List<Pawn>();
            foreach (var pos in GenAdj.CellsAdjacent8Way(this))
            {
                if (GenGrid.InBounds(pos, this.Map))
                {
                    foreach (var t in this.Map.thingGrid.ThingsListAt(pos))
                    {
                        if (t is Pawn pawn && pawn.Faction != PurpleIvyData.AlienFaction && pawn?.health?.hediffSet?
                            .GetFirstHediffOfDef(PurpleIvyDefOf.PI_MaskingSprayHigh) == null)
                        {
                            pawnsToAttack.Add(pawn);
                        }
                    }
                }
            }

            foreach (Pawn pawn in pawnsToAttack)
            {
                try
                {
                    pawn.pather.StopDead();
                    pawn.TakeDamage(new DamageInfo(PurpleIvyDefOf.AlienToxicSting, 5f, 0, -1, this));
                    pawn.mindState.lastMeleeThreatHarmTick = Find.TickManager.TicksGame;
                    Log.Message(pawn + " - " + pawn.CurJob.def.defName);
                    Job job = this.TryGiveJob(pawn);
                    if (job != null)
                    {
                        pawn.jobs.TryTakeOrderedJob(job);
                    }
                }
                catch { };

            }
            if (pawnsToAttack.Count == 0)
            {
                activeThreat = false;
            }
            else
            {
                activeThreat = true;
            }
        }
        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % 2000 == 0)
            {
                base.TickLong();
                if (this.Growth >= 0.25f)
                {
                    base.DoDamageToBuildings();
                }
            }
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                if (this.activeThreat && this.Growth >= 0.25f)
                {
                    base.DoDamageToThings();
                    this.AttackPawnsNearby(out activeThreat);
                }
            }
            if (Find.TickManager.TicksGame % 10 == 0)
            {
                if (this.activeThreat != true && this.Growth >= 0.75f)
                {
                    this.AttackPawnsNearby(out activeThreat);
                }
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.activeThreat, "activeThreat");
        }

        public bool activeThreat = false;

    }
}

