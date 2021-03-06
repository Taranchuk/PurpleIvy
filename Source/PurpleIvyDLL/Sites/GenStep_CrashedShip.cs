﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using Verse;
using Verse.AI;

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

            //right wing
            ResolveParams rightWing = rp;
            rightWing.rect = new CellRect(rp.rect.minX + 6, rp.rect.maxX / 2 - 5, 9, 6);
            rightWing.fillWithThingsPadding = new int?(rightWing.fillWithThingsPadding ?? 0);
            rightWing.singleThingStuff = ThingDefOf.Plasteel;
            rightWing.singleThingDef = ThingDefOf.Wall;
            BaseGen.symbolStack.Push("fillWithThings", rightWing, null);

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

            // pilot room
            ResolveParams pilotRoom = rp;
            pilotRoom.rect = new CellRect(rp.rect.minX + 27, rp.rect.maxX / 2 + 1, 8, 11);
            BaseGen.symbolStack.Push("emptyRoom", pilotRoom, null);

            BaseGen.Generate();

            var doorsHalfway = new List<int>() { 4, 14, 25, 36, 46, 52 };
            for (var i = 0; i < halfway.rect.EdgeCells.Count(); i++)
            {
                if (doorsHalfway.Contains(i))
                {
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
                else if (i == 18 || i == 22)
                {
                    ThingDef stuff = ThingDefOf.Steel;
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.Table1x2c, stuff);
                    thing.SetFaction(rp.faction, null);
                    GenSpawn.Spawn(thing, diningRoom.rect.ToList()[i], map, Rot4.West);
                }
                else if (i == 11)
                {
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.PsychicEmanator);
                    thing.SetFaction(rp.faction, null);
                    GenSpawn.Spawn(thing, diningRoom.rect.ToList()[i], map);
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
                else if (i == 80)
                {
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.Door, ThingDefOf.Plasteel);
                    thing.SetFaction(rp.faction, null);
                    GenSpawn.Spawn(thing, laboratory.rect.ToList()[i], map);
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
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.Ship_Engine, null);
                    thing.SetFaction(rp.faction, null);
                    thing.Rotation = Rot4.West;
                    GenSpawn.Spawn(thing, leftWing.rect.ToList()[i], map, Rot4.East, WipeMode.Vanish);
                }
            }

            Thing thing2 = ThingMaker.MakeThing(ThingDefOf.Turret_MiniTurret, ThingDefOf.Steel);
            thing2.SetFaction(rp.faction, null);
            GenSpawn.Spawn(thing2, leftWing.rect.ToList()[26], map, WipeMode.Vanish);

            var toRemove2 = new List<int>() { 5, 6, 15, 7, 16, 25, 8, 17, 26, 35 };
            List<Thing> list2 = new List<Thing>();
            for (var i = 0; i < rightWing.rect.ToList().Count(); i++)
            {
                if (toRemove2.Contains(i))
                {
                    try
                    {
                        if (GenGrid.InBounds(rightWing.rect.ToList()[i], map))
                        {
                            list2 = map.thingGrid.ThingsListAt(rightWing.rect.ToList()[i]);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                    for (int ind = list2.Count - 1; ind >= 0; ind--)
                    {
                        if (list2[ind].def.IsBuildingArtificial)
                        {
                            list2[ind].DeSpawn(DestroyMode.Vanish);
                        }
                    }
                }
                else if (i == 28)
                {
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.Ship_Engine, null);
                    thing.SetFaction(rp.faction, null);
                    thing.Rotation = Rot4.West;
                    GenSpawn.Spawn(thing, rightWing.rect.ToList()[i], map, Rot4.East, WipeMode.Vanish);
                }
            }

            Thing thing3 = ThingMaker.MakeThing(ThingDefOf.Turret_MiniTurret, ThingDefOf.Steel);
            thing3.SetFaction(rp.faction, null);
            GenSpawn.Spawn(thing3, rightWing.rect.ToList()[35], map, WipeMode.Vanish);

            var toRemove3 = new List<int>() { 4, 5, 6, 7, 15, 84, 85, 86, 79, 87 };
            var toBuildWalls = new List<int>() { 22, 11, 12, 13, 14, 70, 75, 76, 77, 78 };

            List<Thing> list3 = new List<Thing>();
            for (var i = 0; i < pilotRoom.rect.ToList().Count(); i++)
            {
                if (toRemove3.Contains(i))
                {
                    try
                    {
                        if (GenGrid.InBounds(pilotRoom.rect.ToList()[i], map))
                        {
                            list3 = map.thingGrid.ThingsListAt(pilotRoom.rect.ToList()[i]);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                    for (int ind = list3.Count - 1; ind >= 0; ind--)
                    {
                        if (list3[ind].def.IsBuildingArtificial)
                        {
                            list3[ind].DeSpawn(DestroyMode.Vanish);
                        }
                    }
                    if (map.terrainGrid.TerrainAt(pilotRoom.rect.ToList()[i]).defName.Equals("SilverTile"))
                    {
                        map.terrainGrid.RemoveTopLayer(pilotRoom.rect.ToList()[i], true);
                        map.roofGrid.SetRoof(pilotRoom.rect.ToList()[i], null);
                    }
                }
                else if (toBuildWalls.Contains(i))
                {
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.Wall, ThingDefOf.Plasteel);
                    thing.SetFaction(rp.faction, null);
                    GenSpawn.Spawn(thing, pilotRoom.rect.ToList()[i], map, WipeMode.Vanish);
                }
                else if (i == 73)
                {
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named("Ship_CryptosleepCasket"));
                    thing.SetFaction(rp.faction, null);
                    GenSpawn.Spawn(thing, pilotRoom.rect.ToList()[i], map, Rot4.East, WipeMode.Vanish);
                }

                else if (i == 49 || i == 25)
                {
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named("Ship_CryptosleepCasket"));
                    thing.SetFaction(rp.faction, null);
                    GenSpawn.Spawn(thing, pilotRoom.rect.ToList()[i], map, Rot4.North, WipeMode.Vanish);
                }
                else if (i == 10)
                {
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named("Ship_CryptosleepCasket"));
                    thing.SetFaction(rp.faction, null);
                    GenSpawn.Spawn(thing, pilotRoom.rect.ToList()[i], map, Rot4.West, WipeMode.Vanish);
                }
                else if (i == 38)
                {
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named("Ship_CryptosleepCasket"));
                    thing.SetFaction(rp.faction, null);
                    GenSpawn.Spawn(thing, pilotRoom.rect.ToList()[i], map, Rot4.South, WipeMode.Vanish);
                }
                else if (i == 69)
                {
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named("Ship_ComputerCore"));
                    thing.SetFaction(rp.faction, null);
                    GenSpawn.Spawn(thing, pilotRoom.rect.ToList()[i], map, Rot4.South, WipeMode.Vanish);
                }
                else if (i == 62)
                {
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.OrbitalTradeBeacon);
                    thing.SetFaction(rp.faction, null);
                    GenSpawn.Spawn(thing, pilotRoom.rect.ToList()[i], map, Rot4.South, WipeMode.Vanish);
                }
            }

            var intvecList = laboratory.rect.ContractedBy(1).Where(x => map.thingGrid.ThingsListAt(x)
            .Where(y => y is Building).Count() == 0);

            FilthMaker.TryMakeFilth(intvecList.RandomElement(), map, ThingDefOf.Filth_Blood);
            FilthMaker.TryMakeFilth(intvecList.RandomElement(), map, ThingDefOf.Filth_Blood);
            FilthMaker.TryMakeFilth(intvecList.RandomElement(), map, ThingDefOf.Filth_Blood);
            Pawn pawn = PurpleIvyUtils.GenerateKorsolian("KorsolianScientist");
            GenSpawn.Spawn(pawn, intvecList.RandomElement(), map);
            pawn.Kill(new DamageInfo(PurpleIvyDefOf.AlienToxicSting, 50f));

            var corpse = (Corpse)pawn.ParentHolder;
            var dummyCorpse = ThingMaker.MakeThing(PurpleIvyDefOf.InfectedCorpseDummy);
            var comp = dummyCorpse.TryGetComp<AlienInfection>();
            comp.parent = corpse;
            var range = PurpleIvyData.maxNumberOfCreatures["Genny_ParasiteOmega"];
            comp.Props.maxNumberOfCreatures = range;
            comp.maxNumberOfCreatures = range.RandomInRange;
            comp.Props.typesOfCreatures = new List<string>()
            {
                "Genny_ParasiteOmega"
            };
            corpse.AllComps.Add(comp);

            GenSpawn.Spawn(PurpleIvyUtils.GenerateParasite(PurpleIvyData.Genny_ParasiteOmega),
                intvecList.RandomElement(), map);
            GenSpawn.Spawn(PurpleIvyUtils.GenerateParasite(PurpleIvyData.Genny_ParasiteOmega),
    intvecList.RandomElement(), map);

            intvecList = halfway.rect.ContractedBy(1).Where(x => map.thingGrid.ThingsListAt(x)
.Where(y => y is Building).Count() == 0);

            GenSpawn.Spawn(PurpleIvyUtils.GenerateParasite(PurpleIvyData.Genny_ParasiteOmega),
                intvecList.RandomElement(), map);
            GenSpawn.Spawn(PurpleIvyUtils.GenerateParasite(PurpleIvyData.Genny_ParasiteOmega),
                intvecList.RandomElement(), map);

            GenSpawn.Spawn(PurpleIvyUtils.GenerateParasite(PurpleIvyData.Genny_ParasiteBeta),
    intvecList.RandomElement(), map);
            GenSpawn.Spawn(PurpleIvyUtils.GenerateParasite(PurpleIvyData.Genny_ParasiteGamma),
                intvecList.RandomElement(), map);

            intvecList = techRooms.rect.ContractedBy(1).Where(x => map.thingGrid.ThingsListAt(x)
.Where(y => y is Building).Count() == 0);
            GenSpawn.Spawn(PurpleIvyUtils.GenerateParasite(PurpleIvyData.Genny_ParasiteOmega),
    intvecList.RandomElement(), map);
            GenSpawn.Spawn(PurpleIvyUtils.GenerateParasite(PurpleIvyData.Genny_ParasiteOmega),
intvecList.RandomElement(), map);
            FilthMaker.TryMakeFilth(intvecList.RandomElement(), map, ThingDefOf.Filth_Blood);
            FilthMaker.TryMakeFilth(intvecList.RandomElement(), map, ThingDefOf.Filth_Blood);
            FilthMaker.TryMakeFilth(intvecList.RandomElement(), map, ThingDefOf.Filth_Blood);
            pawn = PurpleIvyUtils.GenerateKorsolian("KorsolianScientist");
            GenSpawn.Spawn(pawn, intvecList.RandomElement(), map);
            pawn.Kill(new DamageInfo(PurpleIvyDefOf.AlienToxicSting, 50f));

            intvecList = generators.rect.ContractedBy(1).Where(x => map.thingGrid.ThingsListAt(x)
.Where(y => y is Building).Count() == 0);
            GenSpawn.Spawn(PurpleIvyUtils.GenerateParasite(PurpleIvyData.Genny_ParasiteOmega),
    intvecList.RandomElement(), map);
            FilthMaker.TryMakeFilth(intvecList.RandomElement(), map, ThingDefOf.Filth_Blood);
            FilthMaker.TryMakeFilth(intvecList.RandomElement(), map, ThingDefOf.Filth_Blood);
            FilthMaker.TryMakeFilth(intvecList.RandomElement(), map, ThingDefOf.Filth_Blood);
            pawn = PurpleIvyUtils.GenerateKorsolian("KorsolianSoldier");
            GenSpawn.Spawn(pawn, intvecList.RandomElement(), map);
            pawn.Kill(new DamageInfo(PurpleIvyDefOf.AlienToxicSting, 50f));

            intvecList = diningRoom.rect.ContractedBy(1).Where(x => map.thingGrid.ThingsListAt(x)
.Where(y => y is Building).Count() == 0);
            GenSpawn.Spawn(PurpleIvyUtils.GenerateParasite(PurpleIvyData.Genny_ParasiteOmega),
intvecList.RandomElement(), map);

            intvecList = pilotRoom.rect.ContractedBy(1).Where(x => map.thingGrid.ThingsListAt(x)
.Where(y => y is Building).Count() == 0);
            GenSpawn.Spawn(PurpleIvyUtils.GenerateParasite(PurpleIvyData.Genny_ParasiteAlpha),
    intvecList.RandomElement(), map);
            GenSpawn.Spawn(PurpleIvyUtils.GenerateParasite(PurpleIvyData.Genny_ParasiteAlpha),
intvecList.RandomElement(), map);
            FilthMaker.TryMakeFilth(intvecList.RandomElement(), map, ThingDefOf.Filth_Blood);
            FilthMaker.TryMakeFilth(intvecList.RandomElement(), map, ThingDefOf.Filth_Blood);
            FilthMaker.TryMakeFilth(intvecList.RandomElement(), map, ThingDefOf.Filth_Blood);
            pawn = PurpleIvyUtils.GenerateKorsolian("KorsolianSoldier");
            GenSpawn.Spawn(pawn, intvecList.RandomElement(), map);
            pawn.Kill(new DamageInfo(PurpleIvyDefOf.AlienToxicSting, 50f));
            FilthMaker.TryMakeFilth(intvecList.RandomElement(), map, ThingDefOf.Filth_Blood);
            FilthMaker.TryMakeFilth(intvecList.RandomElement(), map, ThingDefOf.Filth_Blood);
            FilthMaker.TryMakeFilth(intvecList.RandomElement(), map, ThingDefOf.Filth_Blood);
            pawn = PurpleIvyUtils.GenerateKorsolian("KorsolianSoldier");
            GenSpawn.Spawn(pawn, intvecList.RandomElement(), map);
            pawn.Kill(new DamageInfo(PurpleIvyDefOf.AlienToxicSting, 50f));
            FilthMaker.TryMakeFilth(intvecList.RandomElement(), map, ThingDefOf.Filth_Blood);
            FilthMaker.TryMakeFilth(intvecList.RandomElement(), map, ThingDefOf.Filth_Blood);
            FilthMaker.TryMakeFilth(intvecList.RandomElement(), map, ThingDefOf.Filth_Blood);
            pawn = PurpleIvyUtils.GenerateKorsolian("KorsolianSoldier");
            GenSpawn.Spawn(pawn, intvecList.RandomElement(), map);
            pawn.Kill(new DamageInfo(PurpleIvyDefOf.AlienToxicSting, 50f));

            foreach (var vec in rp.rect.ToList())
            {
                foreach (var t in map.thingGrid.ThingsListAt(vec))
                {
                    if (t is Building)
                    {
                        var b = (Building)t;
                        b.HitPoints = Enumerable.Range(10, b.MaxHitPoints / 2).RandomElement();
                        if (b.GetComp<CompBreakdownable>() != null)
                        {
                            b.GetComp<CompBreakdownable>().DoBreakdown();
                        }
                    }
                }
            }
        }

        protected CellRect adventureRegion;

        protected ResolveParams rp;
    }
}

