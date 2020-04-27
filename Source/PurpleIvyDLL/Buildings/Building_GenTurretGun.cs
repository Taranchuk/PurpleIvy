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
    public class Building_GenTurretGun : Building_Turret
    {
        public bool Active
        {
            get
            {
                return (this.powerComp == null || this.powerComp.PowerOn) && (this.dormantComp == null || this.dormantComp.Awake);
            }
        }

        public CompEquippable GunCompEq
        {
            get
            {
                return this.gun.TryGetComp<CompEquippable>();
            }
        }

        public override LocalTargetInfo CurrentTarget
        {
            get
            {
                return this.currentTargetInt;
            }
        }

        public bool WarmingUp
        {
            get
            {
                return this.burstWarmupTicksLeft > 0;
            }
        }

        public override Verb AttackVerb
        {
            get
            {
                return this.GunCompEq.PrimaryVerb;
            }
        }

        public bool IsMannable
        {
            get
            {
                return this.mannableComp != null;
            }
        }

        public bool PlayerControlled
        {
            get
            {
                return (base.Faction == Faction.OfPlayer || this.MannedByColonist) && !this.MannedByNonColonist;
            }
        }

        public bool CanSetForcedTarget
        {
            get
            {
                return this.mannableComp != null && this.PlayerControlled;
            }
        }

        public bool CanToggleHoldFire
        {
            get
            {
                return this.PlayerControlled;
            }
        }

        public bool IsMortar
        {
            get
            {
                return this.def.building.IsMortar;
            }
        }

        public bool IsMortarOrProjectileFliesOverhead
        {
            get
            {
                return this.AttackVerb.ProjectileFliesOverhead() || this.IsMortar;
            }
        }

        public bool CanExtractShell
        {
            get
            {
                if (!this.PlayerControlled)
                {
                    return false;
                }
                CompChangeableProjectile compChangeableProjectile = this.gun.TryGetComp<CompChangeableProjectile>();
                return compChangeableProjectile != null && compChangeableProjectile.Loaded;
            }
        }

        public bool MannedByColonist
        {
            get
            {
                return this.mannableComp != null && this.mannableComp.ManningPawn != null && this.mannableComp.ManningPawn.Faction == Faction.OfPlayer;
            }
        }

        public bool MannedByNonColonist
        {
            get
            {
                return this.mannableComp != null && this.mannableComp.ManningPawn != null && this.mannableComp.ManningPawn.Faction != Faction.OfPlayer;
            }
        }

        public Building_GenTurretGun()
        {
            Log.Message("Building_GenTurretGun : Building_Turret - Building_GenTurretGun - this.top = new TurretTop(this); - 1", true);
            this.top = new TurretTop(this);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            Log.Message("Building_GenTurretGun : Building_Turret - SpawnSetup - base.SpawnSetup(map, respawningAfterLoad); - 2", true);
            base.SpawnSetup(map, respawningAfterLoad);
            Log.Message("Building_GenTurretGun : Building_Turret - SpawnSetup - this.dormantComp = base.GetComp<CompCanBeDormant>(); - 3", true);
            this.dormantComp = base.GetComp<CompCanBeDormant>();
            Log.Message("Building_GenTurretGun : Building_Turret - SpawnSetup - this.powerComp = base.GetComp<CompPowerTrader>(); - 4", true);
            this.powerComp = base.GetComp<CompPowerTrader>();
            Log.Message("Building_GenTurretGun : Building_Turret - SpawnSetup - this.mannableComp = base.GetComp<CompMannable>(); - 5", true);
            this.mannableComp = base.GetComp<CompMannable>();
            Log.Message("Building_GenTurretGun : Building_Turret - SpawnSetup - if (!respawningAfterLoad) - 6", true);
            if (!respawningAfterLoad)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - SpawnSetup - this.top.SetRotationFromOrientation(); - 7", true);
                this.top.SetRotationFromOrientation();
                this.burstCooldownTicksLeft = this.def.building.turretInitialCooldownTime.SecondsToTicks();
            }
        }

        public override void PostMake()
        {
            Log.Message("Building_GenTurretGun : Building_Turret - PostMake - base.PostMake(); - 9", true);
            base.PostMake();
            Log.Message("Building_GenTurretGun : Building_Turret - PostMake - this.MakeGun(); - 10", true);
            this.MakeGun();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            Log.Message("Building_GenTurretGun : Building_Turret - DeSpawn - base.DeSpawn(mode); - 11", true);
            base.DeSpawn(mode);
            Log.Message("Building_GenTurretGun : Building_Turret - DeSpawn - this.ResetCurrentTarget(); - 12", true);
            this.ResetCurrentTarget();
        }

        public override void ExposeData()
        {
            Log.Message("Building_GenTurretGun : Building_Turret - ExposeData - base.ExposeData(); - 13", true);
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.burstCooldownTicksLeft, "burstCooldownTicksLeft", 0, false);
            Scribe_Values.Look<int>(ref this.burstWarmupTicksLeft, "burstWarmupTicksLeft", 0, false);
            Log.Message("Building_GenTurretGun : Building_Turret - ExposeData - Scribe_TargetInfo.Look(ref this.currentTargetInt, \"currentTarget\"); - 16", true);
            Scribe_TargetInfo.Look(ref this.currentTargetInt, "currentTarget");
            Log.Message("Building_GenTurretGun : Building_Turret - ExposeData - Scribe_Values.Look<bool>(ref this.holdFire, \"holdFire\", false, false); - 17", true);
            Scribe_Values.Look<bool>(ref this.holdFire, "holdFire", false, false);
            Log.Message("Building_GenTurretGun : Building_Turret - ExposeData - Scribe_Deep.Look<Thing>(ref this.gun, \"gun\", Array.Empty<object>()); - 18", true);
            Scribe_Deep.Look<Thing>(ref this.gun, "gun", Array.Empty<object>());
            Log.Message("Building_GenTurretGun : Building_Turret - ExposeData - BackCompatibility.PostExposeData(this); - 19", true);
            BackCompatibility.PostExposeData(this);
            Log.Message("Building_GenTurretGun : Building_Turret - ExposeData - if (Scribe.mode == LoadSaveMode.PostLoadInit) - 20", true);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - ExposeData - this.UpdateGunVerbs(); - 21", true);
                this.UpdateGunVerbs();
            }
            throw new Exception();
        }

        public override bool ClaimableBy(Faction by)
        {
            Log.Message("Building_GenTurretGun : Building_Turret - ClaimableBy - return base.ClaimableBy(by) && (this.mannableComp == null || this.mannableComp.ManningPawn == null) && (!this.Active || this.mannableComp != null) && (((this.dormantComp == null || this.dormantComp.Awake)) || (this.powerComp != null && !this.powerComp.PowerOn)); - 22", true);
            return base.ClaimableBy(by) && (this.mannableComp == null || this.mannableComp.ManningPawn == null) && (!this.Active || this.mannableComp != null) && (((this.dormantComp == null || this.dormantComp.Awake)) || (this.powerComp != null && !this.powerComp.PowerOn));
        }

        public override void OrderAttack(LocalTargetInfo targ)
        {
            Log.Message("Building_GenTurretGun : Building_Turret - OrderAttack - if (!targ.IsValid) - 23", true);
            if (!targ.IsValid)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - OrderAttack - if (this.forcedTarget.IsValid) - 24", true);
                if (this.forcedTarget.IsValid)
                {
                    Log.Message("Building_GenTurretGun : Building_Turret - OrderAttack - this.ResetForcedTarget(); - 25", true);
                    this.ResetForcedTarget();
                }
                return;
            }
            Log.Message("Building_GenTurretGun : Building_Turret - OrderAttack - if ((targ.Cell - base.Position).LengthHorizontal < this.AttackVerb.verbProps.EffectiveMinRange(targ, this)) - 27", true);
            if ((targ.Cell - base.Position).LengthHorizontal < this.AttackVerb.verbProps.EffectiveMinRange(targ, this))
            {
                Log.Message("Building_GenTurretGun : Building_Turret - OrderAttack - Messages.Message(\"MessageTargetBelowMinimumRange\".Translate(), this, MessageTypeDefOf.RejectInput, false); - 28", true);
                Messages.Message("MessageTargetBelowMinimumRange".Translate(), this, MessageTypeDefOf.RejectInput, false);
                Log.Message("Building_GenTurretGun : Building_Turret - OrderAttack - return; - 29", true);
                return;
            }
            Log.Message("Building_GenTurretGun : Building_Turret - OrderAttack - if ((targ.Cell - base.Position).LengthHorizontal > this.AttackVerb.verbProps.range) - 30", true);
            if ((targ.Cell - base.Position).LengthHorizontal > this.AttackVerb.verbProps.range)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - OrderAttack - Messages.Message(\"MessageTargetBeyondMaximumRange\".Translate(), this, MessageTypeDefOf.RejectInput, false); - 31", true);
                Messages.Message("MessageTargetBeyondMaximumRange".Translate(), this, MessageTypeDefOf.RejectInput, false);
                Log.Message("Building_GenTurretGun : Building_Turret - OrderAttack - return; - 32", true);
                return;
            }
            Log.Message("Building_GenTurretGun : Building_Turret - OrderAttack - if (this.forcedTarget != targ) - 33", true);
            if (this.forcedTarget != targ)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - OrderAttack - this.forcedTarget = targ; - 34", true);
                this.forcedTarget = targ;
                if (this.burstCooldownTicksLeft <= 0)
                {
                    Log.Message("Building_GenTurretGun : Building_Turret - OrderAttack - this.TryStartShootSomething(false); - 36", true);
                    this.TryStartShootSomething(false);
                }
            }
            Log.Message("Building_GenTurretGun : Building_Turret - OrderAttack - if (this.holdFire) - 37", true);
            if (this.holdFire)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - OrderAttack - Messages.Message(\"MessageTurretWontFireBecauseHoldFire\".Translate(this.def.label), this, MessageTypeDefOf.RejectInput, false); - 38", true);
                Messages.Message("MessageTurretWontFireBecauseHoldFire".Translate(this.def.label), this, MessageTypeDefOf.RejectInput, false);
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (this.CanExtractShell && this.MannedByColonist)
            {
                CompChangeableProjectile compChangeableProjectile = this.gun.TryGetComp<CompChangeableProjectile>();
                if (!compChangeableProjectile.allowedShellsSettings.AllowedToAccept(compChangeableProjectile.LoadedShell))
                {
                    this.ExtractShell();
                }
            }
            if (this.forcedTarget.IsValid && !this.CanSetForcedTarget)
            {
                this.ResetForcedTarget();
            }
            if (!this.CanToggleHoldFire)
            {
                this.holdFire = false;
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
                            this.TryStartShootSomething(true);
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

        protected void TryStartShootSomething(bool canBeginBurstImmediately)
        {
            Log.Message("Building_GenTurretGun : Building_Turret - TryStartShootSomething - if (!base.Spawned || (this.holdFire && this.CanToggleHoldFire) || (this.AttackVerb.ProjectileFliesOverhead() && base.Map.roofGrid.Roofed(base.Position)) || !this.AttackVerb.Available()) - 64", true);
            if (!base.Spawned || (this.holdFire && this.CanToggleHoldFire) || (this.AttackVerb.ProjectileFliesOverhead() && base.Map.roofGrid.Roofed(base.Position)) || !this.AttackVerb.Available())
            {
                Log.Message("Building_GenTurretGun : Building_Turret - TryStartShootSomething - this.ResetCurrentTarget(); - 65", true);
                this.ResetCurrentTarget();
                Log.Message("Building_GenTurretGun : Building_Turret - TryStartShootSomething - return; - 66", true);
                return;
            }
            bool isValid = this.currentTargetInt.IsValid;
            Log.Message("Building_GenTurretGun : Building_Turret - TryStartShootSomething - if (this.forcedTarget.IsValid) - 68", true);
            if (this.forcedTarget.IsValid)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - TryStartShootSomething - this.currentTargetInt = this.forcedTarget; - 69", true);
                this.currentTargetInt = this.forcedTarget;
            }
            else
            {
                Log.Message("Building_GenTurretGun : Building_Turret - TryStartShootSomething - this.currentTargetInt = this.TryFindNewTarget(); - 70", true);
                this.currentTargetInt = this.TryFindNewTarget();
            }
            Log.Message("Building_GenTurretGun : Building_Turret - TryStartShootSomething - if (!isValid && this.currentTargetInt.IsValid) - 71", true);
            if (!isValid && this.currentTargetInt.IsValid)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - TryStartShootSomething - SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(base.Position, base.Map, false)); - 72", true);
                SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
            }
            //if (!this.currentTargetInt.IsValid)
            //{
            //    this.ResetCurrentTarget();
            //    return;
            //}
            Log.Message("Building_GenTurretGun : Building_Turret - TryStartShootSomething - if (this.def.building.turretBurstWarmupTime > 0f) - 76", true);
            if (this.def.building.turretBurstWarmupTime > 0f)
            {
                this.burstWarmupTicksLeft = this.def.building.turretBurstWarmupTime.SecondsToTicks();
                Log.Message("Building_GenTurretGun : Building_Turret - TryStartShootSomething - return; - 78", true);
                return;
            }
            Log.Message("Building_GenTurretGun : Building_Turret - TryStartShootSomething - if (canBeginBurstImmediately) - 79", true);
            if (canBeginBurstImmediately)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - TryStartShootSomething - this.BeginBurst(); - 80", true);
                this.BeginBurst();
                Log.Message("Building_GenTurretGun : Building_Turret - TryStartShootSomething - return; - 81", true);
                return;
            }
            this.burstWarmupTicksLeft = 1;
        }

        protected LocalTargetInfo TryFindNewTarget()
        {
            Log.Message("Building_GenTurretGun : Building_Turret - TryFindNewTarget - IAttackTargetSearcher attackTargetSearcher = this.TargSearcher(); - 83", true);
            IAttackTargetSearcher attackTargetSearcher = this.TargSearcher();
            Log.Message("Building_GenTurretGun : Building_Turret - TryFindNewTarget - Faction faction = attackTargetSearcher.Thing.Faction; - 84", true);
            Faction faction = attackTargetSearcher.Thing.Faction;
            Log.Message("Building_GenTurretGun : Building_Turret - TryFindNewTarget - float range = this.AttackVerb.verbProps.range; - 85", true);
            float range = this.AttackVerb.verbProps.range;
            Log.Message("Building_GenTurretGun : Building_Turret - TryFindNewTarget - Building t; - 86", true);
            Building t;
            Log.Message("Building_GenTurretGun : Building_Turret - TryFindNewTarget - if (Rand.Value < 0.5f && this.AttackVerb.ProjectileFliesOverhead() && faction.HostileTo(Faction.OfPlayer) && base.Map.listerBuildings.allBuildingsColonist.Where(delegate (Building x) - 87", true);
            if (Rand.Value < 0.5f && this.AttackVerb.ProjectileFliesOverhead() && faction.HostileTo(Faction.OfPlayer) && base.Map.listerBuildings.allBuildingsColonist.Where(delegate (Building x)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - TryFindNewTarget - float num = this.AttackVerb.verbProps.EffectiveMinRange(x, this); - 88", true);
                float num = this.AttackVerb.verbProps.EffectiveMinRange(x, this);
                Log.Message("Building_GenTurretGun : Building_Turret - TryFindNewTarget - float num2 = (float)x.Position.DistanceToSquared(this.Position); - 89", true);
                float num2 = (float)x.Position.DistanceToSquared(this.Position);
                Log.Message("Building_GenTurretGun : Building_Turret - TryFindNewTarget - return num2 > num * num && num2 < range * range; - 90", true);
                return num2 > num * num && num2 < range * range;
            }).TryRandomElement(out t))
            {
                Log.Message("Building_GenTurretGun : Building_Turret - TryFindNewTarget - return t; - 91", true);
                return t;
            }
            TargetScanFlags targetScanFlags = TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
            Log.Message("Building_GenTurretGun : Building_Turret - TryFindNewTarget - if (!this.AttackVerb.ProjectileFliesOverhead()) - 93", true);
            if (!this.AttackVerb.ProjectileFliesOverhead())
            {
                Log.Message("Building_GenTurretGun : Building_Turret - TryFindNewTarget - targetScanFlags |= TargetScanFlags.NeedLOSToAll; - 94", true);
                targetScanFlags |= TargetScanFlags.NeedLOSToAll;
                Log.Message("Building_GenTurretGun : Building_Turret - TryFindNewTarget - targetScanFlags |= TargetScanFlags.LOSBlockableByGas; - 95", true);
                targetScanFlags |= TargetScanFlags.LOSBlockableByGas;
            }
            Log.Message("Building_GenTurretGun : Building_Turret - TryFindNewTarget - if (this.AttackVerb.IsIncendiary()) - 96", true);
            if (this.AttackVerb.IsIncendiary())
            {
                Log.Message("Building_GenTurretGun : Building_Turret - TryFindNewTarget - targetScanFlags |= TargetScanFlags.NeedNonBurning; - 97", true);
                targetScanFlags |= TargetScanFlags.NeedNonBurning;
            }
            return (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(attackTargetSearcher, targetScanFlags, new Predicate<Thing>(this.IsValidTarget), 0f, 9999f);
        }

        public IAttackTargetSearcher TargSearcher()
        {
            Log.Message("Building_GenTurretGun : Building_Turret - TargSearcher - if (this.mannableComp != null && this.mannableComp.MannedNow) - 99", true);
            if (this.mannableComp != null && this.mannableComp.MannedNow)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - TargSearcher - return this.mannableComp.ManningPawn; - 100", true);
                return this.mannableComp.ManningPawn;
            }
            return this;
        }

        public bool IsValidTarget(Thing t)
        {
            Log.Message("Building_GenTurretGun : Building_Turret - IsValidTarget - Pawn pawn = t as Pawn; - 102", true);
            Pawn pawn = t as Pawn;
            Log.Message("Building_GenTurretGun : Building_Turret - IsValidTarget - if (pawn != null) - 103", true);
            if (pawn != null)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - IsValidTarget - if (this.AttackVerb.ProjectileFliesOverhead()) - 104", true);
                if (this.AttackVerb.ProjectileFliesOverhead())
                {
                    Log.Message("Building_GenTurretGun : Building_Turret - IsValidTarget - RoofDef roofDef = base.Map.roofGrid.RoofAt(t.Position); - 105", true);
                    RoofDef roofDef = base.Map.roofGrid.RoofAt(t.Position);
                    Log.Message("Building_GenTurretGun : Building_Turret - IsValidTarget - if (roofDef != null && roofDef.isThickRoof) - 106", true);
                    if (roofDef != null && roofDef.isThickRoof)
                    {
                        Log.Message("Building_GenTurretGun : Building_Turret - IsValidTarget - return false; - 107", true);
                        return false;
                    }
                }
                Log.Message("Building_GenTurretGun : Building_Turret - IsValidTarget - if (this.mannableComp == null) - 108", true);
                if (this.mannableComp == null)
                {
                    Log.Message("Building_GenTurretGun : Building_Turret - IsValidTarget - return !GenAI.MachinesLike(base.Faction, pawn); - 109", true);
                    return !GenAI.MachinesLike(base.Faction, pawn);
                }
                Log.Message("Building_GenTurretGun : Building_Turret - IsValidTarget - if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer) - 110", true);
                if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer)
                {
                    Log.Message("Building_GenTurretGun : Building_Turret - IsValidTarget - return false; - 111", true);
                    return false;
                }
            }
            return true;
        }

        protected void BeginBurst()
        {
            Log.Message("Building_GenTurretGun : Building_Turret - BeginBurst - this.AttackVerb.TryStartCastOn(this.CurrentTarget, false, true); - 113", true);
            this.AttackVerb.TryStartCastOn(this.CurrentTarget, false, true);
            Log.Message("Building_GenTurretGun : Building_Turret - BeginBurst - base.OnAttackedTarget(this.CurrentTarget); - 114", true);
            base.OnAttackedTarget(this.CurrentTarget);
        }

        protected void BurstComplete()
        {
            this.burstCooldownTicksLeft = this.BurstCooldownTime().SecondsToTicks();
        }

        protected float BurstCooldownTime()
        {
            Log.Message("Building_GenTurretGun : Building_Turret - BurstCooldownTime - if (this.def.building.turretBurstCooldownTime >= 0f) - 116", true);
            if (this.def.building.turretBurstCooldownTime >= 0f)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - BurstCooldownTime - return this.def.building.turretBurstCooldownTime; - 117", true);
                return this.def.building.turretBurstCooldownTime;
            }
            return this.AttackVerb.verbProps.defaultCooldownTime;
        }

        public override string GetInspectString()
        {
            Log.Message("Building_GenTurretGun : Building_Turret - GetInspectString - StringBuilder stringBuilder = new StringBuilder(); - 119", true);
            StringBuilder stringBuilder = new StringBuilder();
            Log.Message("Building_GenTurretGun : Building_Turret - GetInspectString - string inspectString = base.GetInspectString(); - 120", true);
            string inspectString = base.GetInspectString();
            Log.Message("Building_GenTurretGun : Building_Turret - GetInspectString - if (!inspectString.NullOrEmpty()) - 121", true);
            if (!inspectString.NullOrEmpty())
            {
                Log.Message("Building_GenTurretGun : Building_Turret - GetInspectString - stringBuilder.AppendLine(inspectString); - 122", true);
                stringBuilder.AppendLine(inspectString);
            }
            Log.Message("Building_GenTurretGun : Building_Turret - GetInspectString - if (this.AttackVerb.verbProps.minRange > 0f) - 123", true);
            if (this.AttackVerb.verbProps.minRange > 0f)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - GetInspectString - stringBuilder.AppendLine(\"MinimumRange\".Translate() + \": \" + this.AttackVerb.verbProps.minRange.ToString(\"F0\")); - 124", true);
                stringBuilder.AppendLine("MinimumRange".Translate() + ": " + this.AttackVerb.verbProps.minRange.ToString("F0"));
            }
            Log.Message("Building_GenTurretGun : Building_Turret - GetInspectString - if (base.Spawned && this.IsMortarOrProjectileFliesOverhead && base.Position.Roofed(base.Map)) - 125", true);
            if (base.Spawned && this.IsMortarOrProjectileFliesOverhead && base.Position.Roofed(base.Map))
            {
                Log.Message("Building_GenTurretGun : Building_Turret - GetInspectString - stringBuilder.AppendLine(\"CannotFire\".Translate() + \": \" + \"Roofed\".Translate().CapitalizeFirst()); - 126", true);
                stringBuilder.AppendLine("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
            }
            else if (base.Spawned && this.burstCooldownTicksLeft > 0 && this.BurstCooldownTime() > 5f)
            {
                stringBuilder.AppendLine("CanFireIn".Translate() + ": " + this.burstCooldownTicksLeft.ToStringSecondsFromTicks());
            }
            CompChangeableProjectile compChangeableProjectile = this.gun.TryGetComp<CompChangeableProjectile>();
            Log.Message("Building_GenTurretGun : Building_Turret - GetInspectString - if (compChangeableProjectile != null) - 130", true);
            if (compChangeableProjectile != null)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - GetInspectString - if (compChangeableProjectile.Loaded) - 131", true);
                if (compChangeableProjectile.Loaded)
                {
                    Log.Message("Building_GenTurretGun : Building_Turret - GetInspectString - stringBuilder.AppendLine(\"ShellLoaded\".Translate(compChangeableProjectile.LoadedShell.LabelCap, compChangeableProjectile.LoadedShell)); - 132", true);
                    stringBuilder.AppendLine("ShellLoaded".Translate(compChangeableProjectile.LoadedShell.LabelCap, compChangeableProjectile.LoadedShell));
                }
                else
                {
                    Log.Message("Building_GenTurretGun : Building_Turret - GetInspectString - stringBuilder.AppendLine(\"ShellNotLoaded\".Translate()); - 133", true);
                    stringBuilder.AppendLine("ShellNotLoaded".Translate());
                }
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override void Draw()
        {
            this.top.DrawTurret();
            base.Draw();
        }

        public override void DrawExtraSelectionOverlays()
        {
            Log.Message("Building_GenTurretGun : Building_Turret - DrawExtraSelectionOverlays - float range = this.AttackVerb.verbProps.range; - 137", true);
            float range = this.AttackVerb.verbProps.range;
            Log.Message("Building_GenTurretGun : Building_Turret - DrawExtraSelectionOverlays - if (range < 90f) - 138", true);
            if (range < 90f)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - DrawExtraSelectionOverlays - GenDraw.DrawRadiusRing(base.Position, range); - 139", true);
                GenDraw.DrawRadiusRing(base.Position, range);
            }
            float num = this.AttackVerb.verbProps.EffectiveMinRange(true);
            Log.Message("Building_GenTurretGun : Building_Turret - DrawExtraSelectionOverlays - if (num < 90f && num > 0.1f) - 141", true);
            if (num < 90f && num > 0.1f)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - DrawExtraSelectionOverlays - GenDraw.DrawRadiusRing(base.Position, num); - 142", true);
                GenDraw.DrawRadiusRing(base.Position, num);
            }
            Log.Message("Building_GenTurretGun : Building_Turret - DrawExtraSelectionOverlays - if (this.WarmingUp) - 143", true);
            if (this.WarmingUp)
            {
                int degreesWide = (int)((float)this.burstWarmupTicksLeft * 0.5f);
                Log.Message("Building_GenTurretGun : Building_Turret - DrawExtraSelectionOverlays - GenDraw.DrawAimPie(this, this.CurrentTarget, degreesWide, (float)this.def.size.x * 0.5f); - 145", true);
                GenDraw.DrawAimPie(this, this.CurrentTarget, degreesWide, (float)this.def.size.x * 0.5f);
            }
            Log.Message("Building_GenTurretGun : Building_Turret - DrawExtraSelectionOverlays - if (this.forcedTarget.IsValid && (!this.forcedTarget.HasThing || this.forcedTarget.Thing.Spawned)) - 146", true);
            if (this.forcedTarget.IsValid && (!this.forcedTarget.HasThing || this.forcedTarget.Thing.Spawned))
            {
                Log.Message("Building_GenTurretGun : Building_Turret - DrawExtraSelectionOverlays - Vector3 vector; - 147", true);
                Vector3 vector;
                Log.Message("Building_GenTurretGun : Building_Turret - DrawExtraSelectionOverlays - if (this.forcedTarget.HasThing) - 148", true);
                if (this.forcedTarget.HasThing)
                {
                    Log.Message("Building_GenTurretGun : Building_Turret - DrawExtraSelectionOverlays - vector = this.forcedTarget.Thing.TrueCenter(); - 149", true);
                    vector = this.forcedTarget.Thing.TrueCenter();
                }
                else
                {
                    Log.Message("Building_GenTurretGun : Building_Turret - DrawExtraSelectionOverlays - vector = this.forcedTarget.Cell.ToVector3Shifted(); - 150", true);
                    vector = this.forcedTarget.Cell.ToVector3Shifted();
                }
                Vector3 a = this.TrueCenter();
                Log.Message("Building_GenTurretGun : Building_Turret - DrawExtraSelectionOverlays - vector.y = AltitudeLayer.MetaOverlays.AltitudeFor(); - 152", true);
                vector.y = AltitudeLayer.MetaOverlays.AltitudeFor();
                Log.Message("Building_GenTurretGun : Building_Turret - DrawExtraSelectionOverlays - a.y = vector.y; - 153", true);
                a.y = vector.y;
                Log.Message("Building_GenTurretGun : Building_Turret - DrawExtraSelectionOverlays - GenDraw.DrawLineBetween(a, vector, Building_TurretGun.ForcedTargetLineMat); - 154", true);
                GenDraw.DrawLineBetween(a, vector, Building_TurretGun.ForcedTargetLineMat);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - foreach (Gizmo gizmo in base.GetGizmos()) - 155", true);
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            IEnumerator<Gizmo> enumerator = null;
            Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - if (this.CanExtractShell) - 158", true);
            if (this.CanExtractShell)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - CompChangeableProjectile compChangeableProjectile = this.gun.TryGetComp<CompChangeableProjectile>(); - 159", true);
                CompChangeableProjectile compChangeableProjectile = this.gun.TryGetComp<CompChangeableProjectile>();
                yield return new Command_Action
                {
                    defaultLabel = "CommandExtractShell".Translate(),
                    defaultDesc = "CommandExtractShellDesc".Translate(),
                    icon = compChangeableProjectile.LoadedShell.uiIcon,
                    iconAngle = compChangeableProjectile.LoadedShell.uiIconAngle,
                    iconOffset = compChangeableProjectile.LoadedShell.uiIconOffset,
                    iconDrawScale = GenUI.IconDrawScale(compChangeableProjectile.LoadedShell),
                    action = delegate ()
                    {
                        Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - this.ExtractShell(); - 160", true);
                        this.ExtractShell();
                    }
                };
            }
            CompChangeableProjectile compChangeableProjectile2 = this.gun.TryGetComp<CompChangeableProjectile>();
            Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - if (compChangeableProjectile2 != null) - 163", true);
            if (compChangeableProjectile2 != null)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - StorageSettings storeSettings = compChangeableProjectile2.GetStoreSettings(); - 164", true);
                StorageSettings storeSettings = compChangeableProjectile2.GetStoreSettings();
                Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - foreach (Gizmo gizmo2 in StorageSettingsClipboard.CopyPasteGizmosFor(storeSettings)) - 165", true);
                foreach (Gizmo gizmo2 in StorageSettingsClipboard.CopyPasteGizmosFor(storeSettings))
                {
                    yield return gizmo2;
                }
                enumerator = null;
            }
            Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - if (this.CanSetForcedTarget) - 168", true);
            if (this.CanSetForcedTarget)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - Command_VerbTarget command_VerbTarget = new Command_VerbTarget(); - 169", true);
                Command_VerbTarget command_VerbTarget = new Command_VerbTarget();
                Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - command_VerbTarget.defaultLabel = \"CommandSetForceAttackTarget\".Translate(); - 170", true);
                command_VerbTarget.defaultLabel = "CommandSetForceAttackTarget".Translate();
                Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - command_VerbTarget.defaultDesc = \"CommandSetForceAttackTargetDesc\".Translate(); - 171", true);
                command_VerbTarget.defaultDesc = "CommandSetForceAttackTargetDesc".Translate();
                Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - command_VerbTarget.icon = ContentFinder<Texture2D>.Get(\"UI/Commands/Attack\", true); - 172", true);
                command_VerbTarget.icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack", true);
                Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - command_VerbTarget.verb = this.AttackVerb; - 173", true);
                command_VerbTarget.verb = this.AttackVerb;
                Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - command_VerbTarget.hotKey = KeyBindingDefOf.Misc4; - 174", true);
                command_VerbTarget.hotKey = KeyBindingDefOf.Misc4;
                Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - command_VerbTarget.drawRadius = false; - 175", true);
                command_VerbTarget.drawRadius = false;
                Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - if (base.Spawned && this.IsMortarOrProjectileFliesOverhead && base.Position.Roofed(base.Map)) - 176", true);
                if (base.Spawned && this.IsMortarOrProjectileFliesOverhead && base.Position.Roofed(base.Map))
                {
                    Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - command_VerbTarget.Disable(\"CannotFire\".Translate() + \": \" + \"Roofed\".Translate().CapitalizeFirst()); - 177", true);
                    command_VerbTarget.Disable("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
                }
                yield return command_VerbTarget;
            }
            Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - if (this.forcedTarget.IsValid) - 179", true);
            if (this.forcedTarget.IsValid)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - Command_Action command_Action = new Command_Action(); - 180", true);
                Command_Action command_Action = new Command_Action();
                Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - command_Action.defaultLabel = \"CommandStopForceAttack\".Translate(); - 181", true);
                command_Action.defaultLabel = "CommandStopForceAttack".Translate();
                Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - command_Action.defaultDesc = \"CommandStopForceAttackDesc\".Translate(); - 182", true);
                command_Action.defaultDesc = "CommandStopForceAttackDesc".Translate();
                Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - command_Action.icon = ContentFinder<Texture2D>.Get(\"UI/Commands/Halt\", true); - 183", true);
                command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt", true);
                command_Action.action = delegate ()
                {
                    Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - this.ResetForcedTarget(); - 184", true);
                    this.ResetForcedTarget();
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
                    Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - }; - 186", true);
                };
                Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - if (!this.forcedTarget.IsValid) - 187", true);
                if (!this.forcedTarget.IsValid)
                {
                    Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - command_Action.Disable(\"CommandStopAttackFailNotForceAttacking\".Translate()); - 188", true);
                    command_Action.Disable("CommandStopAttackFailNotForceAttacking".Translate());
                }
                command_Action.hotKey = KeyBindingDefOf.Misc5;
                yield return command_Action;
            }
            Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - if (this.CanToggleHoldFire) - 191", true);
            if (this.CanToggleHoldFire)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "CommandHoldFire".Translate(),
                    defaultDesc = "CommandHoldFireDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/HoldFire", true),
                    hotKey = KeyBindingDefOf.Misc6,
                    toggleAction = delegate ()
                    {
                        Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - this.holdFire = !this.holdFire; - 192", true);
                        this.holdFire = !this.holdFire;
                        Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - if (this.holdFire) - 193", true);
                        if (this.holdFire)
                        {
                            Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - this.ResetForcedTarget(); - 194", true);
                            this.ResetForcedTarget();
                        }
                    },
                    isActive = (() => this.holdFire)
                };
            }
            yield break;
            Log.Message("Building_GenTurretGun : Building_Turret - GetGizmos - yield break; - 197", true);
            yield break;
        }

        public void ExtractShell()
        {
            Log.Message("Building_GenTurretGun : Building_Turret - ExtractShell - GenPlace.TryPlaceThing(this.gun.TryGetComp<CompChangeableProjectile>().RemoveShell(), base.Position, base.Map, ThingPlaceMode.Near, null, null, default(Rot4)); - 198", true);
            GenPlace.TryPlaceThing(this.gun.TryGetComp<CompChangeableProjectile>().RemoveShell(), base.Position, base.Map, ThingPlaceMode.Near, null, null, default(Rot4));
        }

        public void ResetForcedTarget()
        {
            Log.Message("Building_GenTurretGun : Building_Turret - ResetForcedTarget - this.forcedTarget = LocalTargetInfo.Invalid; - 199", true);
            this.forcedTarget = LocalTargetInfo.Invalid;
            this.burstWarmupTicksLeft = 0;
            if (this.burstCooldownTicksLeft <= 0)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - ResetForcedTarget - this.TryStartShootSomething(false); - 202", true);
                this.TryStartShootSomething(false);
            }
        }

        public void ResetCurrentTarget()
        {
            Log.Message("Building_GenTurretGun : Building_Turret - ResetCurrentTarget - this.currentTargetInt = LocalTargetInfo.Invalid; - 203", true);
            this.currentTargetInt = LocalTargetInfo.Invalid;
            this.burstWarmupTicksLeft = 0;
        }

        public void MakeGun()
        {
            Log.Message("Building_GenTurretGun : Building_Turret - MakeGun - this.gun = ThingMaker.MakeThing(this.def.building.turretGunDef, null); - 205", true);
            this.gun = ThingMaker.MakeThing(this.def.building.turretGunDef, null);
            Log.Message("Building_GenTurretGun : Building_Turret - MakeGun - this.UpdateGunVerbs(); - 206", true);
            this.UpdateGunVerbs();
        }

        public void UpdateGunVerbs()
        {
            Log.Message("Building_GenTurretGun : Building_Turret - UpdateGunVerbs - List<Verb> allVerbs = this.gun.TryGetComp<CompEquippable>().AllVerbs; - 207", true);
            List<Verb> allVerbs = this.gun.TryGetComp<CompEquippable>().AllVerbs;
            for (int i = 0; i < allVerbs.Count; i++)
            {
                Log.Message("Building_GenTurretGun : Building_Turret - UpdateGunVerbs - Verb verb = allVerbs[i]; - 208", true);
                Verb verb = allVerbs[i];
                Log.Message("Building_GenTurretGun : Building_Turret - UpdateGunVerbs - verb.caster = this; - 209", true);
                verb.caster = this;
                Log.Message("Building_GenTurretGun : Building_Turret - UpdateGunVerbs - verb.castCompleteCallback = new Action(this.BurstComplete); - 210", true);
                verb.castCompleteCallback = new Action(this.BurstComplete);
            }
        }

        protected int burstCooldownTicksLeft;

        protected int burstWarmupTicksLeft;

        protected LocalTargetInfo currentTargetInt = LocalTargetInfo.Invalid;

        public bool holdFire;

        public Thing gun;

        protected TurretTop top;

        protected CompPowerTrader powerComp;

        protected CompCanBeDormant dormantComp;

        protected CompMannable mannableComp;

        public const int TryStartShootSomethingIntervalTicks = 10;

        public static Material ForcedTargetLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, new Color(1f, 0.5f, 0.5f));
    }
}