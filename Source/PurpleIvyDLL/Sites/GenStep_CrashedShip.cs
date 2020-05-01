using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace PurpleIvy
{
    public class GenStep_CrashedAlienShip : GenStep
    {
        public override int SeedPart
        {
            get
            {
                return 12346547;
            }
        }
        public override void Generate(Map map, GenStepParams parms)
        {
            var geoterms = map.listerThings.AllThings.Where(x => x.def == ThingDefOf.SteamGeyser).ToList();
            for (int i = geoterms.Count - 1; i >= 0; i--)
            {
                geoterms[i].DeSpawn(DestroyMode.Vanish);
            }
            int num = map.Size.x / 5;
            int num2 = 8 * map.Size.x / 5;
            int num3 = map.Size.z / 5;
            int num4 = 8 * map.Size.z / 5;
            this.adventureRegion = new CellRect(num, num3, num2, num4);
            this.adventureRegion.ClipInsideMap(map);
            IntVec3 playerStartSpot;
            CellFinder.TryFindRandomEdgeCellWith((IntVec3 v) =>
            GenGrid.Standable(v, map), map, 0f, out playerStartSpot);
            MapGenerator.PlayerStartSpot = playerStartSpot;
            this.rp = default(ResolveParams);
            this.rp.rect = this.adventureRegion;
            BaseGen.globalSettings.map = map;
            BaseGen.globalSettings.minBuildings = 0;
            BaseGen.globalSettings.minBarracks = 0;

            if (rp.wallStuff == null)
            {
                rp.wallStuff = ThingDefOf.Plasteel;
            }
            if (rp.floorDef == null)
            {
                rp.floorDef = TerrainDef.Named("SilverTile");
            }

            ResolveParams resolveParams = rp;
            resolveParams.rect = new CellRect(rp.rect.minX, rp.rect.maxX / 2, 5, 13).ContractedBy(1);
            resolveParams.thingRot = Rot4.North;
            resolveParams.fillWithThingsPadding = new int?(resolveParams.fillWithThingsPadding ?? 1);
            resolveParams.edgeThingAvoidOtherEdgeThings = new bool?(true);
            BaseGen.symbolStack.Push("chargeBatteries", rp, null);
            resolveParams.singleThingDef = ThingDefOf.Battery;
            BaseGen.symbolStack.Push("fillWithThings", resolveParams, null);

            ResolveParams batteryRoom = rp;
            batteryRoom.rect = new CellRect(rp.rect.minX, rp.rect.maxX / 2, 5, 13);
            BaseGen.symbolStack.Push("emptyRoom", batteryRoom, null);

            // half way
            ResolveParams halfway = rp;
            halfway.rect = new CellRect(rp.rect.minX + 4, rp.rect.maxX / 2 + 4, 24, 5);
            BaseGen.symbolStack.Push("emptyRoom", halfway, null);

            //left wing
            ResolveParams leftWing = rp;
            leftWing.rect = new CellRect(rp.rect.minX + 6, rp.rect.maxX / 2 + 12, 9, 6);
            leftWing.fillWithThingsPadding = new int?(leftWing.fillWithThingsPadding ?? 0);
            leftWing.singleThingStuff = ThingDefOf.Plasteel;
            leftWing.singleThingDef = ThingDefOf.Wall;
            BaseGen.symbolStack.Push("fillWithThings", leftWing, null);


            //generators
            ResolveParams generators = rp;
            generators.rect = new CellRect(rp.rect.minX + 4, rp.rect.maxX / 2 + 8, 8, 5);
            BaseGen.symbolStack.Push("emptyRoom", generators, null);

            //dining room
            ResolveParams diningRoom = rp;
            diningRoom.rect = new CellRect(rp.rect.minX + 4, rp.rect.maxX / 2, 8, 5);
            BaseGen.symbolStack.Push("emptyRoom", diningRoom, null);

            // tech rooms
            ResolveParams techRooms = rp;
            techRooms.rect = new CellRect(rp.rect.minX + 10, rp.rect.maxX / 2, 18, 5);
            BaseGen.symbolStack.Push("emptyRoom", techRooms, null);

            // laboratory
            ResolveParams laboratory = rp;
            laboratory.rect = new CellRect(rp.rect.minX + 10, rp.rect.maxX / 2 + 8, 18, 5);
            BaseGen.symbolStack.Push("emptyRoom", laboratory, null);
            BaseGen.Generate();

            var doorsHalfway = new List<int>() { 4, 14, 25, 36, 46, 52 };
            for (var i = 0; i < halfway.rect.EdgeCells.Count(); i++)
            {
                if (doorsHalfway.Contains(i))
                {
                    Log.Message(halfway.rect.EdgeCells.ToList()[i] + " position " + i);
                    ThingDef stuff = ThingDefOf.Plasteel;
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.Door, stuff);
                    thing.SetFaction(rp.faction, null);
                    GenSpawn.Spawn(thing, halfway.rect.EdgeCells.ToList()[i], map);
                }
            }

            for (var i = 0; i < diningRoom.rect.ToList().Count(); i++)
            {
                if (i == 25 || i == 26 || i == 29 || i == 30)
                {
                    ThingDef stuff = ThingDefOf.Steel;
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.DiningChair, stuff);
                    thing.SetFaction(rp.faction, null);
                    GenSpawn.Spawn(thing, diningRoom.rect.ToList()[i], map, Rot4.South);
                }
                else if (i == 10 || i == 9 || i == 14 || i == 13)
                {
                    ThingDef stuff = ThingDefOf.Steel;
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.DiningChair, stuff);
                    thing.SetFaction(rp.faction, null);
                    GenSpawn.Spawn(thing, diningRoom.rect.ToList()[i], map, Rot4.North);
                }
                else if (i == 18 ||i == 22)
                {
                    ThingDef stuff = ThingDefOf.Steel;
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.Table1x2c, stuff);
                    thing.SetFaction(rp.faction, null);
                    GenSpawn.Spawn(thing, diningRoom.rect.ToList()[i], map, Rot4.West);
                }
            }

            var generatorsInd = new List<int>() { 17, 18, 19, 20, 21, 22 };
            for (var i = 0; i < generators.rect.ToList().Count(); i++)
            {
                if (generatorsInd.Contains(i))
                {
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.VanometricPowerCell, null);
                    thing.SetFaction(rp.faction, null);
                    GenSpawn.Spawn(thing, generators.rect.ToList()[i], map);
                }
            }

            for (var i = 0; i < techRooms.rect.ToList().Count(); i++)
            {
                if (i == 22 || i == 46)
                {
                    ThingDef stuff = ThingDefOf.Steel;
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.DiningChair, stuff);
                    thing.SetFaction(rp.faction, null);
                    thing.Rotation = Rot4.North;
                    GenSpawn.Spawn(thing, techRooms.rect.ToList()[i], map);
                }
                else if (i == 8)
                {
                    ThingDef stuff = ThingDefOf.Plasteel;
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.Door, stuff);
                    thing.SetFaction(rp.faction, null);
                    GenSpawn.Spawn(thing, techRooms.rect.ToList()[i], map);
                }
                else if (i == 64)
                {
                    ThingDef stuff = ThingDefOf.Steel;
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named("DrugLab"), stuff);
                    thing.SetFaction(rp.faction, null);
                    GenSpawn.Spawn(thing, techRooms.rect.ToList()[i], map);
                }
                else if (i == 40)
                {
                    ThingDef stuff = ThingDefOf.Steel;
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named("HiTechResearchBench"), stuff);
                    thing.SetFaction(rp.faction, null);
                    GenSpawn.Spawn(thing, techRooms.rect.ToList()[i], map);
                }
                else if (i == 48)
                {
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named("MultiAnalyzer"), null);
                    thing.SetFaction(rp.faction, null);
                    GenSpawn.Spawn(thing, techRooms.rect.ToList()[i], map);
                }
                else if (i == 69)
                {
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named("CommsConsole"), null);
                    thing.SetFaction(rp.faction, null);
                    GenSpawn.Spawn(thing, techRooms.rect.ToList()[i], map, Rot4.South);
                }
            }

            for (var i = 0; i < laboratory.rect.ToList().Count(); i++)
            {
                if (i == 38 || i == 40 || i == 42)
                {
                    Thing thing = ThingMaker.MakeThing(PurpleIvyDefOf.PI_ContainmentBreach, null);
                    thing.SetFaction(rp.faction, null);
                    thing.Rotation = Rot4.North;
                    GenSpawn.Spawn(thing, laboratory.rect.ToList()[i], map);
                }
                else if (i == 65 || i == 69 || i == 29 || i == 33)
                {
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named("HydroponicsBasin"), null);
                    thing.SetFaction(rp.faction, null);
                    thing.Rotation = Rot4.West;
                    GenSpawn.Spawn(thing, laboratory.rect.ToList()[i], map, Rot4.West);
                }
            }
            var toRemove = new List<int>() { 26, 34, 35, 42, 43, 44, 50, 51, 52, 53 };
            List<Thing> list = new List<Thing>();

            for (var i = 0; i < leftWing.rect.ToList().Count(); i++)
            {
                if (toRemove.Contains(i))
                {
                    try
                    {
                        if (GenGrid.InBounds(leftWing.rect.ToList()[i], map))
                        {
                            list = map.thingGrid.ThingsListAt(leftWing.rect.ToList()[i]);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                    for (int ind = list.Count - 1; ind >= 0; ind--)
                    {
                        if (list[ind].def.IsBuildingArtificial)
                        {
                            list[ind].DeSpawn(DestroyMode.Vanish);
                        }
                    }
                }
                else if (i == 19)
                {
                    Log.Message("SPAWN ENGINE");
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.Ship_Engine, null);
                    thing.SetFaction(rp.faction, null);
                    thing.Rotation = Rot4.West;
                    GenSpawn.Spawn(thing, leftWing.rect.ToList()[i], map, Rot4.East, WipeMode.Vanish);
                }
                Log.Message(leftWing.rect.ToList()[i] + " leftWing - position " + i);
            }
            FloodFillerFog.FloodUnfog(leftWing.rect.CenterCell, map);
        }

        protected CellRect adventureRegion;

        protected ResolveParams rp;
    }
}
