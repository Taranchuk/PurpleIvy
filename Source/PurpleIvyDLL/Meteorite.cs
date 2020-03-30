using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using System.Linq;

namespace PurpleIvy
{
    public class Building_Meteorite : Building
    {
        private int spawnticks = 1200;
        Faction factionDirect = Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamed("Genny", true));
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.SetFactionDirect(factionDirect);
        }

        public override void Tick()
        {
            base.Tick();
            spawnticks--;
            if (spawnticks == 0)
            {
                foreach (IntVec3 current in GenAdj.CellsAdjacentCardinal(this))
                {
                    if (current.GetPlant(this.Map) == null)
                    {
                        if (!GenCollection.Any<Thing>(GridsUtility.GetThingList(current, this.Map), (Thing t) => (t.def.IsBuildingArtificial || t.def.IsNonResourceNaturalRock)))
                        {
                            foreach (var t in GridsUtility.GetThingList(current, this.Map))
                            {
                                Log.Message(t.Label);
                            }
                            Plant newivy = (Plant)ThingMaker.MakeThing(ThingDef.Named("PurpleIvy"));
                            GenSpawn.Spawn(newivy, current, this.Map);
                        }
                    }
                    else
                    {
                        Plant plant = current.GetPlant(this.Map);
                        if (plant.def.defName != "PurpleIvy")
                        {
                            if (!GenCollection.Any<Thing>(GridsUtility.GetThingList(current, this.Map), (Thing t) => (t.def.IsBuildingArtificial || t.def.IsNonResourceNaturalRock)))
                            {
                                foreach (var t in GridsUtility.GetThingList(current, this.Map))
                                {
                                    Log.Message(t.Label);
                                }
                                plant.Destroy();
                                Plant newivy = (Plant)ThingMaker.MakeThing(ThingDef.Named("PurpleIvy"));
                                GenSpawn.Spawn(newivy, current, this.Map);
                            }
                        }
                        else
                        {
                            //dont destroy other ivy
                        }
                    }
                }
                spawnticks = 1200;
            }
        }
    }
}

