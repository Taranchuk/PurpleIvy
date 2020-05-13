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
    public class Plant_Ivy : AlienPlant//, IAttackTarget // tanks fps, had to disable it
    {
        public bool CanMutate = false;
        private Gas Spores = null;

        //Thing IAttackTarget.Thing
        //{
        //    get
        //    {
        //        return this;
        //    }
        //}
        //public LocalTargetInfo TargetCurrentlyAimingAt
        //{
        //    get
        //    {
        //        return LocalTargetInfo.Invalid;
        //    }
        //}
        //
        //public float TargetPriorityFactor
        //{
        //    get
        //    {
        //        return 0.1f;
        //    }
        //}
        //
        //public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
        //{
        //    return false;
        //}
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.SetFaction(PurpleIvyData.AlienFaction);
            if (this.Growth >= 0.25f)
            {
                base.ThrowGasOrAdjustGasSize(4f);
            }
        }
        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            if (dinfo.Instigator is Pawn)
            {
                this.Map.GetComponent<MapComponent_MapEvents>().LastAttacked = Find.TickManager.TicksGame;
            }
            if (dinfo.Def != DamageDefOf.Deterioration && GenGrid.InBounds(this.Position.ToVector3Shifted(), this.Map))
            {
                PurpleIvyMoteMaker.ThrowToxicGas(this.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(1f), this.Map, 1f);
            }
            base.PreApplyDamage(ref dinfo, out absorbed);
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

        public override void PostMake()
        {
            base.PostMake();
            if (Rand.Chance(0.15f))
            {
                CanMutate = true;
            }
            else
            {
                CanMutate = false;
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
        }

        public bool HasNoBuildings(IntVec3 dir)
        {
            return GenAdj.CellsAdjacent8Way(new TargetInfo(dir, this.Map, false)).All(current => current.Standable(this.Map));
        }

        public bool NoNestsNearby(int radius)
        {
            List<Thing> list = new List<Thing>();
            foreach (var dir in GenRadial.RadialCellsAround(this.Position, radius, true))
            {
                try
                {
                    if (GenGrid.InBounds(dir, this.Map))
                    {
                        list = this.Map.thingGrid.ThingsListAt(dir);
                    }
                }
                catch
                {
                    return false;
                }
                foreach (var t in list)
                {
                    if (t.def == PurpleIvyDefOf.PI_Nest)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public void TryMutate()
        {
            if (this.Map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSacNestGuard).Count == 0)
            {
                Building_EggSac EggSac = (Building_EggSac)ThingMaker.MakeThing(PurpleIvyDefOf.EggSacNestGuard);
                GenSpawn.Spawn(EggSac, Position, this.Map);
                Log.Message("No other guard eggs - " + this + " mutate into EggSac NestGuard", true);
            }
            else if (HasNoBuildings(Position))
            {
                float randChance = Rand.Range(0f, 1f);
                Log.Message(this + " - rand chance: " + randChance.ToString());
                if (randChance >= 0f && randChance <= 0.3f)
                {
                    if (NoNestsNearby(5))
                    {
                        var nest = ThingMaker.MakeThing(PurpleIvyDefOf.PI_Nest);
                        GenSpawn.Spawn(nest, Position, this.Map);
                        Log.Message(this + " mutate into Nest");
                        this.Destroy(DestroyMode.Vanish);
                    }
                }
                else if (randChance >= 0.35f && randChance <= 0.359f)
                {
                    var GenMortar = ThingMaker.MakeThing(PurpleIvyDefOf.Turret_GenMortarSeed);
                    GenSpawn.Spawn(GenMortar, Position, this.Map);
                    Log.Message("Rand chance: " + randChance.ToString() + " - " + this + " mutate into GenMortar", true);

                }
                else if (randChance >= 0.36f && randChance <= 0.369f)
                {
                    Thing thing = ThingMaker.MakeThing(PurpleIvyDefOf.GenTurretBase, null);
                    GenSpawn.Spawn(thing, this.Position, this.Map, thing.Rotation, WipeMode.Vanish, false);
                    Log.Message("Rand chance: " + randChance.ToString() + " - " + this + " mutate into GenTurret", true);
                }
                else if (randChance >= 0.44f && randChance <= 0.4459f) // 0.005 - 0.5%
                {
                    var EggSac = ThingMaker.MakeThing(PurpleIvyDefOf.EggSac);
                    GenSpawn.Spawn(EggSac, Position, this.Map);
                    if (EggSac.Map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteAlpha).Count
                        > PurpleIvySettings.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteAlpha.defName])
                    {
                        EggSac.TryGetComp<AlienInfection>().stopSpawning = true;
                    }
                    Log.Message("Rand chance: " + randChance.ToString() + " - " + this + " mutate into EggSac", true);
                }
                else if (randChance >= 0.446f && randChance <= 0.4549f)
                {
                    var EggSac = ThingMaker.MakeThing(PurpleIvyDefOf.EggSacBeta);
                    GenSpawn.Spawn(EggSac, Position, this.Map);
                    if (EggSac.Map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSacBeta).Count
                        > PurpleIvySettings.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteBeta.defName])
                    {
                        EggSac.TryGetComp<AlienInfection>().stopSpawning = true;
                    }
                    Log.Message("Rand chance: " + randChance.ToString() + " - " + this + " mutate into EggSac beta", true);
                }
                else if (randChance >= 0.455f && randChance <= 0.469f)
                {
                    var EggSac = ThingMaker.MakeThing(PurpleIvyDefOf.EggSacGamma);
                    GenSpawn.Spawn(EggSac, Position, this.Map);
                    if (EggSac.Map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSacGamma).Count
                        > PurpleIvySettings.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteGamma.defName])
                    {
                        EggSac.TryGetComp<AlienInfection>().stopSpawning = true;
                    }
                    Log.Message("Rand chance: " + randChance.ToString() + " - " + this + " mutate into EggSac gamma", true);
                }
                else if (randChance >= 0.47f && randChance <= 0.4739f)
                {
                    if (this.Map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSacNestGuard).Count < 4)
                    {
                        var EggSac = ThingMaker.MakeThing(PurpleIvyDefOf.EggSacNestGuard);
                        GenSpawn.Spawn(EggSac, Position, this.Map);
                        Log.Message("Rand chance: " + randChance.ToString() + " - " + this + " mutate into EggSac NestGuard", true);
                    }
                }
                else if (randChance >= 0.50f && randChance <= 0.519f)
                {
                    var EggSac = ThingMaker.MakeThing(PurpleIvyDefOf.ParasiteEgg);
                    GenSpawn.Spawn(EggSac, Position, this.Map);
                    if (EggSac.Map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteOmega).Count
                        > PurpleIvySettings.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteOmega.defName])
                    {
                        EggSac.TryGetComp<AlienInfection>().stopSpawning = true;
                    }
                    Log.Message(this + " mutate into ParasiteEgg");
                }
                else if (randChance >= 0.55f && randChance <= 0.59f)
                {
                    var PlantVenomousToothwort = ThingMaker.MakeThing(PurpleIvyDefOf.PlantVenomousToothwort);
                    GenSpawn.Spawn(PlantVenomousToothwort, Position, this.Map);
                    Log.Message("Rand chance: " + randChance.ToString() + " - " + this + " mutate into PlantVenomousToothwort", true);
                    this.Destroy(DestroyMode.Vanish);
                }
                else if (randChance >= 0.30f && randChance <= 0.329f)
                {
                    var GasPump = ThingMaker.MakeThing(PurpleIvyDefOf.GasPump);
                    GenSpawn.Spawn(GasPump, Position, this.Map);
                    Log.Message("Rand chance: " + randChance.ToString() + " - " + this + " mutate into GasPump", true);
                }
            }
        }

        public bool NestsNearby()
        {
            try
            {
                foreach (var t in this.Map.listerThings.ThingsOfDef(PurpleIvyDefOf.PI_Nest))
                {
                    var nest = t as Plant_Nest;
                    if (this.Position.InHorDistOf(nest.Position, Convert.ToInt32(nest.Growth * 20)))
                    {
                        return true;
                    }
                }
            }
            catch { };
            return false;
        }
        public override void Tick()
        {
            base.Tick();
            if (this.Growth >= 0.75f && Rand.Chance(0.1f) && Find.TickManager.TicksGame
                % Rand.RangeInclusive(60, 100) == 0)
            {
                //PurpleIvyMoteMaker.ThrowEMPLightningGlow(this.Position.ToVector3Shifted(), this.Map, 0.3f);
                PurpleIvyMoteMaker.ThrowLightningBolt(this.Position.ToVector3Shifted(), this.Map);
            }
            if (Find.TickManager.TicksGame % 2000 == 0)
            {
                base.TickLong();
                if (this.Growth >= 0.25f)
                {
                    base.ThrowGasOrAdjustGasSize(4f);
                    base.DoDamageToBuildings();
                    if (this.CanMutate == true)
                    {
                        this.TryMutate();
                    }
                }
            }
            if (Find.TickManager.TicksGame % Rand.RangeInclusive(250, 500) == 0)
            {
                if (this.Growth >= 0.25f)
                {
                    base.DoDamageToThings();
                }
                if (!NestsNearby())
                {
                    this.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 1f));
                }
            }

        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.CanMutate, "MutateTry", true, true);
        }
    }
}

