using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace PurpleIvy
{
    public static class MeteorUtility 
    {
        private static List<List<Thing>> tempList = new List<List<Thing>>();
        public static void MakeMeteorAt(Map map, IntVec3 loc, MeteorInfo info)
        {
            MeteorIncoming meteorIncoming = (MeteorIncoming)ThingMaker.MakeThing(ThingDef.Named("PI_MeteorIncoming"), null);
            meteorIncoming.contents = info;
            GenSpawn.Spawn(meteorIncoming, loc, map);
        }
        public static void DropThingsNear(IntVec3 dropCenter, IEnumerable<Thing> things, int openDelay = 110, bool canInstaDropDuringInit = true, bool leaveSlag = false)
        {
            foreach (Thing current in things)
            {
                List<Thing> list = new List<Thing>();
                list.Add(current);
                MeteorUtility.tempList.Add(list);
            }
            MeteorUtility.DropThingGroupsNear(dropCenter, MeteorUtility.tempList, openDelay, canInstaDropDuringInit, leaveSlag);
            MeteorUtility.tempList.Clear();
        }
        public static void DropThingGroupsNear(IntVec3 dropCenter, List<List<Thing>> thingsGroups, int openDelay = 110, bool canInstaDropDuringInit = true, bool leaveSlag = false)
        {
            foreach (List<Thing> current in thingsGroups)
            {
                
                IntVec3 intVec;
                if (!DropCellFinder.TryFindDropSpotNear(dropCenter, current[0].Map, out intVec, false, false))
                {
                    Log.Warning(string.Concat(new object[]
					{
						"DropThingsNear failed to find a place to drop ",
						current.FirstOrDefault<Thing>(),
						" near ",
						dropCenter,
						". Dropping on random square instead."
					}));
                    intVec = CellFinderLoose.RandomCellWith((IntVec3 sq) => sq.Walkable(current[0].Map), current[0].Map);
                }
                foreach (Thing current2 in current)
                {
                    ThingWithComps thingWithComponents = current2 as ThingWithComps;
                    if (thingWithComponents != null && thingWithComponents.GetComp<CompForbiddable>() != null)
                    {
                        thingWithComponents.GetComp<CompForbiddable>().Forbidden = true;
                    }
                }
                if (canInstaDropDuringInit)// && Find.TickManager.tickCount < 2)
                {
                    foreach (Thing current3 in current)
                    {
                        GenPlace.TryPlaceThing(current3, intVec, current3.Map, ThingPlaceMode.Near);
                    }
                }
                else
                {
                    MeteorInfo meteorInfo = new MeteorInfo();
                    foreach (Thing current4 in current)
                    {
                        meteorInfo.containedThings.Add(current4);
                    }
                    meteorInfo.openDelay = openDelay;
                    meteorInfo.leaveSlag = leaveSlag; 
                    MeteorUtility.MakeMeteorAt(current[0].Map, intVec, meteorInfo);
                }
            }
        }
    }
}

