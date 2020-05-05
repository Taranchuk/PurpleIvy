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
        private int spawnticks = 1200;
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
                foreach (IntVec3 current in GenAdj.CellsAdjacentCardinal(this))
                {
                    if (GenGrid.InBounds(current, this.Map))
                    {
                        if (current.GetPlant(this.Map) == null)
                        {
                            if (!GenCollection.Any<Thing>(GridsUtility.GetThingList(current, this.Map), (Thing t) => (t.def.IsBuildingArtificial || t.def.IsNonResourceNaturalRock)))
                            {
                                Plant newNest = (Plant)ThingMaker.MakeThing(ThingDef.Named("PI_Nest"));
                                GenSpawn.Spawn(newNest, current, this.Map);
                            }
                        }
                        else
                        {
                            Plant plant = current.GetPlant(this.Map);
                            if (plant.def.defName != "PI_Nest")
                            {
                                if (!GenCollection.Any<Thing>(GridsUtility.GetThingList(current, this.Map), (Thing t) => (t.def.IsBuildingArtificial || t.def.IsNonResourceNaturalRock)))
                                {
                                    plant.Destroy();
                                    Plant newNest = (Plant)ThingMaker.MakeThing(ThingDef.Named("PI_Nest"));
                                    GenSpawn.Spawn(newNest, current, this.Map);
                                }
                            }
                        }
                    }
                }
                spawnticks = 1200;
            }
        }
    }
}

