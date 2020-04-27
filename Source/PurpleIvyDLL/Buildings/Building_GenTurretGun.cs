using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace PurpleIvy
{
    [StaticConstructorOnStartup]
    class Building_GenTurretGun : Building_TurretGun
    {

        private bool WarmingUp
        {
            get
            {
                return this.burstWarmupTicksLeft > 0;
            }
        }

        public Building_GenTurretGun()
        {
            this.top = new TurretTop(this);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.dormantComp = base.GetComp<CompCanBeDormant>();
            this.powerComp = base.GetComp<CompPowerTrader>();
            this.mannableComp = base.GetComp<CompMannable>();
            if (!respawningAfterLoad)
            {
                this.top.SetRotationFromOrientation();
                this.burstCooldownTicksLeft = this.def.building.turretInitialCooldownTime.SecondsToTicks();
            }
        }

        public override void PostMake()
        {
            base.PostMake();
            this.MakeGun();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            this.ResetCurrentTarget();
        }

        public override void Tick()
        {
            base.Tick();

            if (this.forcedTarget.IsValid)
            {
                this.ResetForcedTarget();
            }
            if (this.forcedTarget.ThingDestroyed)
            {
                this.ResetForcedTarget();
            }
            if (this.Active && (this.mannableComp == null || this.mannableComp.MannedNow) && base.Spawned)
            {
                this.GunCompEq.verbTracker.VerbsTick();
                if (!this.stunner.Stunned && this.AttackVerb.state != VerbState.Bursting)
                {
                    if (this.WarmingUp)
                    {
                        this.burstWarmupTicksLeft--;
                        if (this.burstWarmupTicksLeft == 0)
                        {
                            this.BeginBurst();
                        }
                    }
                    else
                    {
                        if (this.burstCooldownTicksLeft > 0)
                        {
                            this.burstCooldownTicksLeft--;
                        }
                        if (this.burstCooldownTicksLeft <= 0 && this.IsHashIntervalTick(10))
                        {
                            this.TryStartShootSomethingPlus(true);
                        }
                    }
                    this.top.TurretTopTick();
                    return;
                }
            }
            else
            {
                this.ResetCurrentTarget();
            }
        }

        protected void TryStartShootSomethingPlus(bool canBeginBurstImmediately)
        {
            if (!base.Spawned || (this.AttackVerb.ProjectileFliesOverhead() && base.Map.roofGrid.Roofed(base.Position)) || !this.AttackVerb.Available())
            {
                this.ResetCurrentTarget();
                return;
            }
            bool isValid = this.currentTargetInt.IsValid;
            if (this.forcedTarget.IsValid)
            {
                this.currentTargetInt = this.forcedTarget;
            }
            else
            {
                this.currentTargetInt = this.TryFindNewTarget();
            }
            if (!isValid && this.currentTargetInt.IsValid)
            {
                SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
            }
            //if (!this.currentTargetInt.IsValid)
            //{
            //    this.ResetCurrentTarget();
            //    return;
            //}
            if (this.def.building.turretBurstWarmupTime > 0f)
            {
                this.burstWarmupTicksLeft = this.def.building.turretBurstWarmupTime.SecondsToTicks();
                return;
            }
            if (canBeginBurstImmediately)
            {
                this.BeginBurst();
                return;
            }
            this.burstWarmupTicksLeft = 1;
        }

        protected new void BeginBurst()
        {
            this.AttackVerb.TryStartCastOn(this.CurrentTarget, false, true);
            base.OnAttackedTarget(this.CurrentTarget);
        }

        protected new void BurstComplete()
        {
            this.burstCooldownTicksLeft = this.BurstCooldownTime().SecondsToTicks();
        }

        protected new float BurstCooldownTime()
        {
            if (this.def.building.turretBurstCooldownTime >= 0f)
            {
                return this.def.building.turretBurstCooldownTime;
            }
            return this.AttackVerb.verbProps.defaultCooldownTime;
        }

        private void ResetForcedTarget()
        {
            this.forcedTarget = LocalTargetInfo.Invalid;
            this.burstWarmupTicksLeft = 0;
            if (this.burstCooldownTicksLeft <= 0)
            {
                this.TryStartShootSomething(false);
            }
        }

        private void ResetCurrentTarget()
        {
            this.currentTargetInt = LocalTargetInfo.Invalid;
            this.burstWarmupTicksLeft = 0;
        }

        private void UpdateGunVerbs()
        {
            List<Verb> allVerbs = this.gun.TryGetComp<CompEquippable>().AllVerbs;
            for (int i = 0; i < allVerbs.Count; i++)
            {
                Verb verb = allVerbs[i];
                verb.caster = this;
                verb.castCompleteCallback = new Action(this.BurstComplete);
            }
        }
    }
}
