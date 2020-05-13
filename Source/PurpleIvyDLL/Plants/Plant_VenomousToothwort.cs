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
                if (this.Growth >= 0.25f)
                {
                    base.DoDamageToThings();
                }
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
        }

    }
}

