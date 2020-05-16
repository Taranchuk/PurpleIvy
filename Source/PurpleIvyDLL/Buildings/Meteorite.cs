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
        public List<PurpleGas> spores = new List<PurpleGas>();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.SetFaction(PurpleIvyData.AlienFaction);
            if (this.spores != null && this.spores.Count > 0)
            {
                foreach (var spore in this.spores)
                {
                    if (!spore.Destroyed)
                    {
                        Log.Message(spore + " spore " + spore.destroyTick);
                        GenSpawn.Spawn(spore, spore.Position, this.Map);
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
                return 1f;
            }
        }

        public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
        {
            return false;
        }

        public override void Tick()
        {
            base.Tick();
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
            Scribe_Collections.Look<PurpleGas>(ref this.spores, "spores", LookMode.Deep, Array.Empty<object>());
        }
    }
}

