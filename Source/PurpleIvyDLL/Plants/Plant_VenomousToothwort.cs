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
    public class Plant_VenomousToothwort : Plant, IAttackTarget
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

        public void DoDamageToBuildings(IntVec3 pos)
        {
            List<Thing> list = new List<Thing>();
            foreach (var pos2 in GenAdj.CellsAdjacent8Way(this))
            {
                try
                {
                    if (GenGrid.InBounds(pos2, this.Map))
                    {
                        list = this.Map.thingGrid.ThingsListAt(pos2);
                    }
                }
                catch
                {
                    continue;
                }
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is Building && list[i].Faction != PurpleIvyData.AlienFaction)
                    {
                        Building b = (Building)list[i];
                        var comp = this.Map.GetComponent<MapComponent_MapEvents>();
                        if (comp != null)
                        {
                            int oldDamage = 0;
                            if (comp.ToxicDamages == null)
                            {
                                comp.ToxicDamages = new Dictionary<IntVec3, int>();
                                comp.ToxicDamages[b.Position] = b.MaxHitPoints;
                            }
                            Log.Message("Taking damage to " + b);
                            if (!comp.ToxicDamages.ContainsKey(b.Position))
                            {
                                oldDamage = b.MaxHitPoints;
                                comp.ToxicDamages[b.Position] = b.MaxHitPoints - 1;
                            }
                            else
                            {
                                oldDamage = comp.ToxicDamages[b.Position];
                                comp.ToxicDamages[b.Position] -= 1;
                            }
                            BuildingsToxicDamageSectionLayerUtility.Notify_BuildingHitPointsChanged((Building)list[i], oldDamage);
                            if (comp.ToxicDamages[b.Position] / 2 < b.MaxHitPoints)
                            {
                                if (b.GetComp<CompBreakdownable>() != null)
                                {
                                    b.GetComp<CompBreakdownable>().DoBreakdown();
                                }
                                if (b.GetComp<CompPowerPlantWind>() != null)
                                {
                                    b.GetComp<CompPowerPlantWind>().PowerOutput /= 2f;
                                }
                                if (b.GetComp<CompPowerTrader>() != null)
                                {
                                    b.GetComp<CompPowerTrader>().PowerOn = false;
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }

        public void DoDamageToThings(IntVec3 pos)
        {
            List<Thing> list = new List<Thing>();
            try
            {
                if (GenGrid.InBounds(pos, this.Map))
                {
                    list = this.Map.thingGrid.ThingsListAt(pos);
                }
            }
            catch
            {
                return;
            }
            //Loop over things if there are things
            if (list == null || list.Count <= 0) return;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null || list[i].Faction == PurpleIvyData.AlienFaction) continue;
                switch (list[i])
                {
                    //If we find a pawn and its not a hatchling
                    case Pawn _:
                    {
                        var pawn = (Pawn)list[i];
                        if (!pawn.RaceProps.IsMechanoid)
                        {
                            var damageInfo = new DamageInfo(DamageDefOf.Scratch, 1, 0f, -1f, this, null, null);
                                pawn.TakeDamage(damageInfo);
                            var hediff = HediffMaker.MakeHediff(PurpleIvyDefOf.PoisonousPurpleHediff,
                                pawn, null);
                            hediff.Severity = 0.1f;
                            (pawn).health.AddHediff(hediff, null, null, null);
                            var hediff2 = HediffMaker.MakeHediff(PurpleIvyDefOf.HarmfulBacteriaHediff,
                                pawn, null);
                            hediff2.Severity = 0.1f;
                            (pawn).health.AddHediff(hediff2, null, null, null);
                        }
                        break;
                    }

                    //If we find a plant
                    case Plant _:
                    {
                        if (list[i].def != PurpleIvyDefOf.PurpleIvy
                            && list[i].def != PurpleIvyDefOf.PI_Nest
                            && list[i].def != PurpleIvyDefOf.PlantVenomousToothwort
                            && list[i].def != PurpleIvyDefOf.PI_CorruptedTree)
                        {
                                PurpleIvyMoteMaker.ThrowToxicSmoke(this.Position.ToVector3Shifted(), this.Map);
                                //FilthMaker.TryMakeFilth(this.Position, this.Map, PurpleIvyDefOf.PI_ToxicFilth);
                                list[i].TakeDamage(new DamageInfo(PurpleIvyDefOf.PI_ToxicBurn, 1));
                        }
                        break;
                    }
                }
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
                    this.DoDamageToBuildings(Position);
                }
            }
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                if (this.Growth >= 0.25f)
                {
                    this.DoDamageToThings(Position);
                }
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
        }

    }
}

