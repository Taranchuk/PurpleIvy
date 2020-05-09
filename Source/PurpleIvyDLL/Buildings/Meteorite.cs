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

        public override void Tick()
        {
            base.Tick();
            spawnticks--;
            if (spawnticks == 0)
            {
                int nestCount = 0;
                List<IntVec3> freeTiles = new List<IntVec3>();
                foreach (IntVec3 dir in GenRadial.RadialCellsAround(this.Position, 50, true))
                {
                    if (GenGrid.InBounds(dir, this.Map) && this.Map.fertilityGrid.FertilityAt(dir) >= 0.5)
                    {
                        var plant = dir.GetPlant(this.Map);
                        if (plant?.def == PurpleIvyDefOf.PI_Nest)
                        {
                            nestCount++;
                        }
                        if (plant?.Faction != PurpleIvyData.AlienFaction && !GenCollection.Any<Thing>
                            (GridsUtility.GetThingList(dir, this.Map), (Thing t) => (t.def.IsBuildingArtificial
                            || t.def.IsNonResourceNaturalRock)))
                        {
                            if (freeTiles.Count < 50)
                            {
                                freeTiles.Add(dir);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    if (nestCount == 4) break;
                }
                if (nestCount < 4)
                {
                    var rnd = new System.Random();
                    foreach (IntVec3 current in freeTiles.OrderBy(x => rnd.Next()).Take(4 - nestCount))
                    {
                        var plant = current.GetPlant(this.Map);
                        if (plant == null)
                        {
                            Plant newNest = (Plant)ThingMaker.MakeThing(ThingDef.Named("PI_Nest"));
                            GenSpawn.Spawn(newNest, current, this.Map);
                        }
                        else
                        {
                            plant.Destroy();
                            Plant newNest = (Plant)ThingMaker.MakeThing(ThingDef.Named("PI_Nest"));
                            GenSpawn.Spawn(newNest, current, this.Map);
                        }
                    }
                }
                spawnticks = 1200;
            }
        }
    }
}

