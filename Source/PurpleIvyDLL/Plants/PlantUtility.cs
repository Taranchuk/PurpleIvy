using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;

namespace PurpleIvy
{
    public static class PlantUtility
    {

        public static void SpawnIvy(IntVec3 dir, Map map)
        {
            if (GenCollection.Any<Thing>(GridsUtility.GetThingList(dir, map),
                (Thing t) =>
                    (t.def.IsBuildingArtificial ||
                     t.def.IsNonResourceNaturalRock | t.def.defName == "PurpleIvy"))) return;
            Plant newivy = new Plant();
            newivy = (Plant)ThingMaker.MakeThing(ThingDef.Named("PurpleIvy"));
            GenSpawn.Spawn(newivy, dir, map);
        }

        public static bool IvyInCell(IntVec3 dir, Map map)
        {
            //List all things in that random direction cell
            List<Thing> list = map.thingGrid.ThingsListAt(dir);
            return list.Count > 0 && list.OfType<Plant>().Any(t => t.def.defName == "PurpleIvy");
        }

        public static bool HasNoBuildings(IntVec3 dir, Map map)
        {
            return GenAdj.CellsAdjacent8Way(new TargetInfo(dir, map, false)).All(current => current.Standable(map));
        }

        public static void DoDamageToBuildings(IntVec3 pos, Map map)
        {
            List<Thing> list = new List<Thing>();
            foreach (var pos2 in GenAdj.CellsAdjacent8Way(new TargetInfo(pos, map, false)))
            {
                try
                {
                    list = map.thingGrid.ThingsListAt(pos2);
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
                        var comp = map.GetComponent<MapComponent_MapEvents>();
                        if (comp != null)
                        {
                            int oldDamage = 0;
                            if (comp.ToxicDamages == null)
                            {
                                comp.ToxicDamages = new Dictionary<Building, int>();
                                comp.ToxicDamages[b] = b.MaxHitPoints;
                            }
                            Log.Message("Taking damage to " + b);
                            if (!comp.ToxicDamages.ContainsKey(b))
                            {
                                oldDamage = b.MaxHitPoints;
                                comp.ToxicDamages[b] = b.MaxHitPoints - 1;
                            }
                            else
                            {
                                oldDamage = comp.ToxicDamages[b];
                                comp.ToxicDamages[b] -= 1;
                            }
                            BuildingsToxicDamageSectionLayerUtility.Notify_BuildingHitPointsChanged((Building)list[i], oldDamage);
                            if (comp.ToxicDamages[b] / 2 < b.MaxHitPoints)
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
    }
}

