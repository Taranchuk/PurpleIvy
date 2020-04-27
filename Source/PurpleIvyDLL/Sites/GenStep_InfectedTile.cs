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
        public override int SeedPart => 54649541;

        public static bool HasNoBuildings(IntVec3 dir, Map map)
        {
            return GenAdj.CellsAdjacent8Way(new TargetInfo(dir, map, false)).All(current => current.Standable(map));
        }

        public static bool SpreadBuilding(IntVec3 dir, Map map)
        {
            if (!HasNoBuildings(dir, map)) return false;
            var random = new System.Random(dir.GetHashCode());
            var mutateChance = random.Next(1, 100);
            if (30 < mutateChance) return false;
            random = new System.Random(dir.GetHashCode() + dir.GetHashCode());
            var mutateRate = random.Next(1, 100);
            if (mutateRate >= 0 && mutateRate <= 5)
            {
                var gasPump = (Building_GasPump)ThingMaker.MakeThing(PurpleIvyDefOf.GasPump);
                gasPump.SetFactionDirect(PurpleIvyData.AlienFaction);
                GenSpawn.Spawn(gasPump, dir, map);
                return true;
            }
            if (mutateRate >= 6 && mutateRate <= 10)
            {
                var genMortar = (Building_Turret)ThingMaker.MakeThing(PurpleIvyDefOf.Turret_GenMortarSeed);
                genMortar.SetFactionDirect(PurpleIvyData.AlienFaction);
                GenSpawn.Spawn(genMortar, dir, map);
                return true;
            }
            if (mutateRate >= 11 && mutateRate <= 15)
            {
                var genTurret = (Building_Turret)ThingMaker.MakeThing(PurpleIvyDefOf.GenTurretBase);
                genTurret.SetFactionDirect(PurpleIvyData.AlienFaction);
                GenSpawn.Spawn(genTurret, dir, map);
                return true;
            }
            if (mutateRate >= 16 && mutateRate <= 17)
            {
                var eggSac = (Building_EggSac)ThingMaker.MakeThing(PurpleIvyDefOf.EggSac);
                eggSac.SetFactionDirect(PurpleIvyData.AlienFaction);
                GenSpawn.Spawn(eggSac, dir, map);
                return true;
            }
            if (mutateRate >= 18 && mutateRate <= 19)
            {
                var eggSac = (Building_EggSac)ThingMaker.MakeThing(PurpleIvyDefOf.EggSacBeta);
                eggSac.SetFactionDirect(PurpleIvyData.AlienFaction);
                GenSpawn.Spawn(eggSac, dir, map);
                return true;
            }
            if (mutateRate >= 20 && mutateRate <= 21)
            {
                var eggSac = (Building_EggSac)ThingMaker.MakeThing(PurpleIvyDefOf.EggSacGamma);
                eggSac.SetFactionDirect(PurpleIvyData.AlienFaction);
                GenSpawn.Spawn(eggSac, dir, map);
                return true;
            }
            if (mutateRate < 18 || mutateRate > 23) return false;
            var parasiteEgg = (Building_EggSac)ThingMaker.MakeThing(PurpleIvyDefOf.ParasiteEgg);
            parasiteEgg.SetFactionDirect(PurpleIvyData.AlienFaction);
            GenSpawn.Spawn(parasiteEgg, dir, map);
            return true;

        }

        public static int GetRadius(Map map)
        {
            var counter = Find.WorldObjects.WorldObjectAt(map.Tile, PurpleIvyDefOf.PI_InfectedTile)
                .GetComponent<WorldObjectComp_InfectedTile>().counter;
            return PurpleIvyData.RadiusData.Where(raduisData => counter < raduisData.Value).Select(raduisData => raduisData.Key).FirstOrDefault();
        }
        public override void Generate(Map map, GenStepParams parms)
        {
            var intVec = new IntVec3();
            var invalid = IntVec3.Invalid;
            var flag = !RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 sq) 
                => sq.Standable(map)
                && !sq.Roofed(map)
                && !sq.Fogged(map)
                && sq.CanBeSeenOver(map)
                && sq.Walkable(map)
                && sq.InBounds(map)
                , map, out intVec);
            if (flag)
            {
                invalid = IntVec3.Invalid;
                intVec = CellFinderLoose.RandomCellWith(
                sq => sq.Standable(map)
                        && !sq.Roofed(map)
                        && !sq.Fogged(map)
                        && sq.CanBeSeenOver(map)
                        && sq.Walkable(map)
                        && sq.InBounds(map), map);
            }
            var meteor = ThingMaker.MakeThing(ThingDef.Named("PI_Meteorite"));
            GenSpawn.Spawn(meteor, intVec, map);

            var radius = GetRadius(map);
            Log.Message("Radius: " + radius.ToString());
            var radialCells = GenRadial.RadialCellsAround(meteor.Position, radius, true)
                .ToList();

            var plantCount = 0;
            var infectedComp = map.Parent.GetComponent<WorldObjectComp_InfectedTile>();
            var counter = infectedComp.counter;
            const float origGrowth = 1f;
            var newivyGrowth = 1f;
            foreach (IntVec3 vec in radialCells)
            {
                if (SpreadBuilding(vec, map)) continue;
                if (plantCount >= counter || !vec.InBounds(map) || !vec.Standable(map)) continue;
                var newivy = new Plant();
                newivy = (Plant)ThingMaker.MakeThing(ThingDef.Named("PurpleIvy"));
                GenSpawn.Spawn(newivy, vec, map);
                newivy.Growth = newivyGrowth;
                newivyGrowth -= (origGrowth / (float)counter);
                plantCount++;
            }

            var alphaParasitesCount = map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSac).Count * 10;
            int betaParasitesCount = map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSacBeta).Count * 10;
            int gammaParasitesCount = map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSacGamma).Count * 10;
            var omegaParasitesCount = map.listerThings.ThingsOfDef(PurpleIvyDefOf.ParasiteEgg).Count * 10;

            alphaParasitesCount -= (infectedComp.AlienPowerSpent / 50);
            betaParasitesCount = (infectedComp.AlienPowerSpent / 50);
            gammaParasitesCount = (infectedComp.AlienPowerSpent / 50);
            omegaParasitesCount -= (infectedComp.AlienPowerSpent / 50);

            foreach (var i in Enumerable.Range(1, alphaParasitesCount))
            {
                var spawnPlace = radialCells.Where(x => x.Walkable(map)).RandomElement();
                radialCells.Remove(spawnPlace);
                var pawnKindDef = PawnKindDef.Named(PurpleIvyData.Genny_ParasiteAlpha.RandomElement());
                var newPawn = PawnGenerator.GeneratePawn(pawnKindDef, null);
                newPawn.SetFaction(PurpleIvyData.AlienFaction);
                newPawn.ageTracker.AgeBiologicalTicks = 40000;
                newPawn.ageTracker.AgeChronologicalTicks = 40000;
                GenSpawn.Spawn(newPawn, spawnPlace, map);
            }
            foreach (var i in Enumerable.Range(1, betaParasitesCount))
            {
                var spawnPlace = radialCells.Where(x => x.Walkable(map)).RandomElement();
                radialCells.Remove(spawnPlace);
                var pawnKindDef = PawnKindDef.Named(PurpleIvyData.Genny_ParasiteBeta.RandomElement());
                var newPawn = PawnGenerator.GeneratePawn(pawnKindDef, null);
                newPawn.SetFaction(PurpleIvyData.AlienFaction);
                newPawn.ageTracker.AgeBiologicalTicks = 40000;
                newPawn.ageTracker.AgeChronologicalTicks = 40000;
                GenSpawn.Spawn(newPawn, spawnPlace, map);
            }

            foreach (var i in Enumerable.Range(1, gammaParasitesCount))
            {
                var spawnPlace = radialCells.Where(x => x.Walkable(map)).RandomElement();
                radialCells.Remove(spawnPlace);
                var pawnKindDef = PawnKindDef.Named(PurpleIvyData.Genny_ParasiteGamma.RandomElement());
                var newPawn = PawnGenerator.GeneratePawn(pawnKindDef, null);
                newPawn.SetFaction(PurpleIvyData.AlienFaction);
                newPawn.ageTracker.AgeBiologicalTicks = 40000;
                newPawn.ageTracker.AgeChronologicalTicks = 40000;
                GenSpawn.Spawn(newPawn, spawnPlace, map);
            }

            foreach (var i in Enumerable.Range(1, omegaParasitesCount))
            {
                var spawnPlace = radialCells.Where(x => x.Walkable(map)).RandomElement();
                radialCells.Remove(spawnPlace);
                var pawnKindDef = PawnKindDef.Named(PurpleIvyData.Genny_ParasiteOmega.RandomElement());
                var newPawn = PawnGenerator.GeneratePawn(pawnKindDef, null);
                newPawn.SetFaction(PurpleIvyData.AlienFaction);
                newPawn.ageTracker.AgeBiologicalTicks = 40000;
                newPawn.ageTracker.AgeChronologicalTicks = 40000;
                GenSpawn.Spawn(newPawn, spawnPlace, map);
            }
            foreach (var ivy in map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Cast<Plant_Ivy>().Where(ivy => ivy.Growth > 0.1f))
            {
                ivy.CanMutate = false;
            }
            var count = map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Count;
            Log.Message("New map created! plants - " + map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Count.ToString());
            var comp = map.Parent.GetComponent<WorldObjectComp_InfectedTile>();
            if (comp != null && PurpleIvyData.getFogProgressWithOuterSources(count, comp, out var temp) > 0f)
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
            Log.Message("Total Genny_ParasiteGamma count on the map: " +
map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteGamma).Count.ToString(), true);
            Log.Message("Total Genny_ParasiteOmega count on the map: " +
            map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteOmega).Count.ToString(), true);
            Log.Message("Total Genny_ParasiteNestGuard count on the map: " +
map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteNestGuard).Count.ToString(), true);
            Log.Message("Total EggSac count on the map: " +
            map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSac).Count.ToString(), true);
            Log.Message("Total EggSac beta count on the map: " +
map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSacBeta).Count.ToString(), true);
            Log.Message("Total EggSac gamma count on the map: " +
map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSacGamma).Count.ToString(), true);
            Log.Message("Total EggSac NestGuard count on the map: " +
map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSacNestGuard).Count.ToString(), true);
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

