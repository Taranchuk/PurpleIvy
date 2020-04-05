using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
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
                    random = new System.Random(map.GetHashCode());
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
                    else if (mutateRate >= 16 && mutateRate <= 18)
                    {
                        Building_EggSac EggSac = (Building_EggSac)ThingMaker.MakeThing(PurpleIvyDefOf.EggSac);
                        EggSac.SetFactionDirect(PurpleIvyData.factionDirect);
                        GenSpawn.Spawn(EggSac, dir, map);
                        return true;
                    }
                    else if (mutateRate >= 19 && mutateRate <= 23)
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
            foreach (KeyValuePair<int,int> test in PurpleIvyData.RadiusData)
            {
                if (counter > test.Value)
                {
                    radius = test.Key;
                }
                else if (counter < test.Value)
                {
                    return radius;
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
            Log.Message(radius.ToString());
            var radialCells = GenRadial.RadialCellsAround(meteor.Position, radius, true)
                .ToList();
            foreach (IntVec3 vec in radialCells)
            {
                if (!SpreadBuilding(vec, map))
                {
                    if (GenGrid.InBounds(vec, map) && GenGrid.Standable(vec, map))
                    {
                        Plant newivy = new Plant();
                        newivy = (Plant)ThingMaker.MakeThing(ThingDef.Named("PurpleIvy"));
                        GenSpawn.Spawn(newivy, vec, map);
                        newivy.Growth = this.growth;
                    }

                }
                this.growth = this.growth - 0.001f;
            }

            foreach (var i in Enumerable.Range(1, 3))
            {
                IntVec3 spawnPlace = radialCells.Where(x => GenGrid.Walkable(x, map)).RandomElement();
                radialCells.Remove(spawnPlace);
                PawnKindDef pawnKindDef = PawnKindDef.Named("Genny_ParasiteAlphaA");
                Pawn NewPawn = PawnGenerator.GeneratePawn(pawnKindDef, null);
                NewPawn.SetFaction(PurpleIvyData.factionDirect);
                NewPawn.ageTracker.AgeBiologicalTicks = 40000;
                NewPawn.ageTracker.AgeChronologicalTicks = 40000;
                GenSpawn.Spawn(NewPawn, spawnPlace, map);
            }

            foreach (var i in Enumerable.Range(1, 20))
            {
                IntVec3 spawnPlace = radialCells.Where(x => GenGrid.Walkable(x, map)).RandomElement();
                radialCells.Remove(spawnPlace);
                PawnKindDef pawnKindDef = PawnKindDef.Named("Genny_ParasiteOmega");
                Pawn NewPawn = PawnGenerator.GeneratePawn(pawnKindDef, null);
                NewPawn.SetFaction(PurpleIvyData.factionDirect);
                NewPawn.ageTracker.AgeBiologicalTicks = 40000;
                NewPawn.ageTracker.AgeChronologicalTicks = 40000;
                GenSpawn.Spawn(NewPawn, spawnPlace, map);
            }
            foreach (Plant_Ivy ivy in map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy))
            {
                if (ivy.Growth > 10)
                {
                    ivy.MutateTry = false;
                }
            }
            if (map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Count - 500 > 500)
            {
                GameCondition_PurpleFog gameCondition =
                (GameCondition_PurpleFog)GameConditionMaker.MakeConditionPermanent
                (PurpleIvyDefOf.PurpleFogGameCondition);
                map.gameConditionManager.RegisterCondition(gameCondition);
            }
        }

        public float growth = 1f;
    }
}