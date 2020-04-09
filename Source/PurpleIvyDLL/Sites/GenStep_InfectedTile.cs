using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PurpleIvy
{
    public class GenStep_InfectedTile : GenStep
    {
        public override int SeedPart
        {
            get
            {
                return 54649541;
            }
        }
        public bool hasNoBuildings(IntVec3 dir, Map map)
        {
            foreach (IntVec3 current in GenAdj.CellsAdjacent8Way(new TargetInfo(dir, map, false)))
            {
                if (!current.Standable(map))
                {
                    return false;
                }
            }
            return true;
        }

        public bool SpreadBuilding(IntVec3 dir, Map map)
        {
            if (hasNoBuildings(dir, map))
            {
                System.Random random = new System.Random(dir.GetHashCode());
                int mutateChance = random.Next(1, 100);
                if (30 >= mutateChance)
                {
                    random = new System.Random(dir.GetHashCode() + dir.GetHashCode());
                    int mutateRate = random.Next(1, 100);
                    if (mutateRate >= 0 && mutateRate <= 5)
                    {
                        Building_GasPump GasPump = (Building_GasPump)ThingMaker.MakeThing(PurpleIvyDefOf.GasPump);
                        GasPump.SetFactionDirect(PurpleIvyData.factionDirect);
                        GenSpawn.Spawn(GasPump, dir, map);
                        return true;
                    }
                    else if (mutateRate >= 6 && mutateRate <= 10)
                    {
                        Building_Turret GenMortar = (Building_Turret)ThingMaker.MakeThing(PurpleIvyDefOf.Turret_GenMortarSeed);
                        GenMortar.SetFactionDirect(PurpleIvyData.factionDirect);
                        GenSpawn.Spawn(GenMortar, dir, map);
                        return true;
                    }
                    else if (mutateRate >= 11 && mutateRate <= 15)
                    {
                        Building_Turret GenTurret = (Building_Turret)ThingMaker.MakeThing(PurpleIvyDefOf.GenTurretBase);
                        GenTurret.SetFactionDirect(PurpleIvyData.factionDirect);
                        GenSpawn.Spawn(GenTurret, dir, map);
                        return true;
                    }
                    else if (mutateRate >= 16 && mutateRate <= 17)
                    {
                        Building_EggSac EggSac = (Building_EggSac)ThingMaker.MakeThing(PurpleIvyDefOf.EggSac);
                        EggSac.SetFactionDirect(PurpleIvyData.factionDirect);
                        GenSpawn.Spawn(EggSac, dir, map);
                        return true;
                    }
                    else if (mutateRate >= 18 && mutateRate <= 23)
                    {
                        Building_ParasiteEgg ParasiteEgg = (Building_ParasiteEgg)ThingMaker.MakeThing(PurpleIvyDefOf.ParasiteEgg);
                        ParasiteEgg.SetFactionDirect(PurpleIvyData.factionDirect);
                        GenSpawn.Spawn(ParasiteEgg, dir, map);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }


        public int getRadius(Map map)
        {
            int counter = Find.WorldObjects.WorldObjectAt(map.Tile, PurpleIvyDefOf.InfectedTile)
                .GetComponent<WorldObjectComp_InfectedTile>().counter;
            int radius = 0;
            foreach (KeyValuePair<int,int> raduisData in PurpleIvyData.RadiusData)
            {
                if (counter < raduisData.Value)
                {
                    return raduisData.Key;
                }
            }
            return radius;
        }
        public override void Generate(Map map, GenStepParams parms)
        {
            IntVec3 intVec = new IntVec3();
            IntVec3 invalid = IntVec3.Invalid;
            bool flag = !RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 sq) 
                => GenGrid.Standable(sq, map)
                && !GridsUtility.Roofed(sq, map)
                && !GridsUtility.Fogged(sq, map)
                && GenGrid.CanBeSeenOver(sq, map)
                && GenGrid.Walkable(sq, map)
                && GenGrid.InBounds(sq, map)
                , map, out intVec);
            if (flag)
            {
                invalid = IntVec3.Invalid;
                intVec = CellFinderLoose.RandomCellWith(
                (IntVec3 sq) => GenGrid.Standable(sq, map)
                && !GridsUtility.Roofed(sq, map)
                && !GridsUtility.Fogged(sq, map)
                && GenGrid.CanBeSeenOver(sq, map)
                && GenGrid.Walkable(sq, map)
                && GenGrid.InBounds(sq, map), map);
            }
            Thing meteor = ThingMaker.MakeThing(ThingDef.Named("PI_Meteorite"));
            GenSpawn.Spawn(meteor, intVec, map);

            int radius = getRadius(map);
            Log.Message("Radius: " + radius.ToString());
            var radialCells = GenRadial.RadialCellsAround(meteor.Position, radius, true)
                .ToList();
            int plantCount = 0;
            var infectedComp = map.Parent.GetComponent<WorldObjectComp_InfectedTile>();
            int counter = infectedComp.counter;
            float origGrowth = 1f;
            float growth = 1f;
            foreach (IntVec3 vec in radialCells)
            {
                if (!SpreadBuilding(vec, map))
                {
                    if (plantCount < counter && GenGrid.InBounds(vec, map) 
                        && GenGrid.Standable(vec, map))
                    {
                        Plant newivy = new Plant();
                        newivy = (Plant)ThingMaker.MakeThing(ThingDef.Named("PurpleIvy"));
                        GenSpawn.Spawn(newivy, vec, map);
                        newivy.Growth = growth;
                        growth -= (origGrowth / (float)counter);
                        plantCount++;
                    }
                }
            }

            foreach (var i in Enumerable.Range(1, map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSac).Count * 10))
            {
                IntVec3 spawnPlace = radialCells.Where(x => GenGrid.Walkable(x, map)).RandomElement();
                radialCells.Remove(spawnPlace);
                PawnKindDef pawnKindDef = PawnKindDef.Named(PurpleIvyData.Genny_ParasiteAlpha.RandomElement());
                Pawn NewPawn = PawnGenerator.GeneratePawn(pawnKindDef, null);
                NewPawn.SetFaction(PurpleIvyData.factionDirect);
                NewPawn.ageTracker.AgeBiologicalTicks = 40000;
                NewPawn.ageTracker.AgeChronologicalTicks = 40000;
                GenSpawn.Spawn(NewPawn, spawnPlace, map);
            }

            foreach (var i in Enumerable.Range(1, map.listerThings.ThingsOfDef(PurpleIvyDefOf.ParasiteEgg).Count * 10))
            {
                IntVec3 spawnPlace = radialCells.Where(x => GenGrid.Walkable(x, map)).RandomElement();
                radialCells.Remove(spawnPlace);
                PawnKindDef pawnKindDef = PawnKindDef.Named(PurpleIvyData.Genny_ParasiteOmega.RandomElement());
                Pawn NewPawn = PawnGenerator.GeneratePawn(pawnKindDef, null);
                NewPawn.SetFaction(PurpleIvyData.factionDirect);
                NewPawn.ageTracker.AgeBiologicalTicks = 40000;
                NewPawn.ageTracker.AgeChronologicalTicks = 40000;
                GenSpawn.Spawn(NewPawn, spawnPlace, map);
            }
            foreach (Plant_Ivy ivy in map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy))
            {
                if (ivy.Growth > 0.1f)
                {
                    ivy.MutateTry = false;
                }
            }
            int count = map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Count;
            Log.Message("New map created! plants - " + map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Count.ToString());
            var comp = map.Parent.GetComponent<WorldObjectComp_InfectedTile>();
            bool temp;
            if (comp != null && PurpleIvyData.getFogProgressWithOuterSources(count, comp, out temp) > 0f)
            {
                GameCondition_PurpleFog gameCondition =
                (GameCondition_PurpleFog)GameConditionMaker.MakeConditionPermanent
                (PurpleIvyDefOf.PurpleFogGameCondition);
                map.gameConditionManager.RegisterCondition(gameCondition);
            }
            Log.Message("Total PurpleIvy count on the map: " +
            map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Count.ToString(), true);
            Log.Message("Total Genny_ParasiteAlpha count on the map: " +
            map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteAlpha).Count.ToString(), true);
            Log.Message("Total Genny_ParasiteBeta count on the map: " +
            map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteBeta).Count.ToString(), true);
            Log.Message("Total Genny_ParasiteOmega count on the map: " +
            map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteOmega).Count.ToString(), true);
            Log.Message("Total EggSac count on the map: " +
            map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSac).Count.ToString(), true);
            Log.Message("Total ParasiteEgg count on the map: " +
            map.listerThings.ThingsOfDef(PurpleIvyDefOf.ParasiteEgg).Count.ToString(), true);
            Log.Message("Total GasPump count on the map: " +
            map.listerThings.ThingsOfDef(PurpleIvyDefOf.GasPump).Count.ToString(), true);
            Log.Message("Total GenTurretBase count on the map: " +
            map.listerThings.ThingsOfDef(PurpleIvyDefOf.GenTurretBase).Count.ToString(), true);
            Log.Message("Total Turret_GenMortarSeed count on the map: " +
            map.listerThings.ThingsOfDef(PurpleIvyDefOf.Turret_GenMortarSeed).Count.ToString(), true);
        }

        public float growth = 1f;
    }
}