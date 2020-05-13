using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using Verse.AI;

namespace PurpleIvy
{
    public class Building_GasPump : Building, IAttackTarget
    {
        private int pumpfreq = 10;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.SetFaction(PurpleIvyData.AlienFaction);
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

        public override void TickRare()
        {
            base.TickRare();
            pumpfreq--;
            if (pumpfreq <= 0)
            {
                PurpleIvyMoteMaker.ThrowToxicGas(base.Position.ToVector3Shifted(), this.Map, 1f);
                pumpfreq = 3;
            }
        }
    }
}

