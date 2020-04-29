using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Noise;

namespace PurpleIvy
{
    public class MapComponent_MapEvents : MapComponent
    {
        public MapComponent_MapEvents(Map map) : base(map)
        {

        }

        public override void ExposeData()
        {
            Scribe_Collections.Look<Building, int>(ref this.ToxicDamages, "ToxicDamages", LookMode.Deep, LookMode.Value, ref this.ToxicDamageKeys, ref this.ToxicDamageValues);
            Scribe_Values.Look<bool>(ref this.OrbitalHelpActive, "OrbitalHelpActive", false);
            base.ExposeData();
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (Find.TickManager.TicksGame % 250 == 0)
            {
                var plants = this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy);
                Log.Message("Checking orbital strike, " + this.OrbitalHelpActive + " - " + plants.Count);
                if (plants != null && ((this.OrbitalHelpActive == true && plants.Count > 0)
                    || plants.Count > 1500)) // && Rand.Chance(PurpleIvyData.getFogProgress(plants.Count)))
                {
                    if (this.OrbitalHelpActive == false)
                    {
                        this.OrbitalHelpActive = true;
                        Find.LetterStack.ReceiveLetter("OrbitalHelpFromAncients".Translate(),
                        "OrbitalHelpFromAncientsDesc".Translate(),
                        LetterDefOf.NeutralEvent, new TargetInfo(plants.RandomElement().Position, map, false));
                    }
                    PowerBeam powerBeam = (PowerBeam)GenSpawn.Spawn(PurpleIvyDefOf.PI_PowerBeam,
                        plants.RandomElement().Position, this.map, 0);
                    powerBeam.duration = 200;
                    powerBeam.instigator = null;
                    powerBeam.weaponDef = null;
                    powerBeam.StartStrike();
                }
                if ((plants.Count <= 0 || plants == null) && this.OrbitalHelpActive == true)
                {
                    Log.Message("Orbital help");
                    this.OrbitalHelpActive = false;
                    List<Pawn> list = new List<Pawn>();

                    var alpha = this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteAlpha);
                    var beta = this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteBeta);
                    var gamma = this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteGamma);
                    var omega = this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteOmega);
                    var guard = this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteNestGuard);

                    int pawnCount = alpha.Count;
                    pawnCount += beta.Count;
                    pawnCount += gamma.Count;
                    pawnCount += omega.Count;
                    pawnCount += guard.Count;
                    Predicate<IntVec3> predicate = delegate (IntVec3 c)
                    {
                        return !GridsUtility.Fogged(c, map) &&
                        !GridsUtility.Roofed(c, map) &&
                        GenGrid.InBounds(c, map) && 
                        GenRadial.RadialCellsAround(c, 10, true).Where(x => 
                        map.thingGrid.ThingsListAt(x).Where(y => y.Faction == PurpleIvyData.AlienFaction)
                        != null) != null;
                    };
                    IntVec3 position = CellFinder.RandomClosewalkCellNear(this.map.Center, this.map, 500,
                        predicate);
                    foreach (var num in Enumerable.Range(1, pawnCount / 2))
                    {
                        Faction faction = FactionUtility.DefaultFactionFrom(PurpleIvyDefOf.KorsolianFaction);
                        Pawn NewPawn = PawnGenerator.GeneratePawn(PurpleIvyDefOf.KorsolianSoldier, faction);
                        if (faction != null && faction != Faction.OfPlayer)
                        {
                            Lord lord = null;
                            if (this.map.mapPawns.SpawnedPawnsInFaction(faction).Any((Pawn p) =>
                            p != NewPawn))
                            {
                                lord = ((Pawn)GenClosest.ClosestThing_Global(NewPawn.Position,
                                    this.map.mapPawns.SpawnedPawnsInFaction(faction), 99999f,
                                    (Thing p) => p != NewPawn && ((Pawn)p).GetLord() != null, null)).GetLord();
                            }
                            if (lord == null)
                            {
                                var lordJob = new LordJob_AssistColony(Faction.OfPlayer, position);
                                //LordJob_DefendPoint lordJob = new LordJob_DefendPoint(position);
                                lord = LordMaker.MakeNewLord(faction, lordJob, this.map, null);
                            }
                            lord.AddPawn(NewPawn);
                        }
                        Log.Message(NewPawn?.Faction?.def?.defName);
                        list.Add(NewPawn);
                    }
                    DropPodUtility.DropThingsNear(position, this.map, list, 30, false, true, true, true);
                    Find.LetterStack.ReceiveLetter("AncientsLandOnTheGround".Translate(),
                    "AncientsLandOnTheGroundDesc".Translate(),
                    LetterDefOf.NeutralEvent, new TargetInfo(position, map, false));
                }
                //Log.Message("Alpha limit: " + PurpleIvySettings.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteAlpha.defName]);
                //Log.Message("Beta limit: " + PurpleIvySettings.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteBeta.defName]);
                //Log.Message("Gamma limit: " + PurpleIvySettings.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteGamma.defName]);
                //Log.Message("Omega limit: " + PurpleIvySettings.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteOmega.defName]);
                int count = plants.Count;
                bool comeFromOuterSource;
                var tempComp = new WorldObjectComp_InfectedTile();
                tempComp.infectedTile = map.Tile;
                if (PurpleIvyData.getFogProgressWithOuterSources(count, tempComp, out comeFromOuterSource) > 0f &&
                    !map.gameConditionManager.ConditionIsActive(PurpleIvyDefOf.PurpleFogGameCondition))
                {
                    GameCondition_PurpleFog gameCondition =
                        (GameCondition_PurpleFog)GameConditionMaker.MakeConditionPermanent
                        (PurpleIvyDefOf.PurpleFogGameCondition);
                    map.gameConditionManager.RegisterCondition(gameCondition);
                    if (comeFromOuterSource == false)
                    {
                        Find.LetterStack.ReceiveLetter(gameCondition.LabelCap,
                        gameCondition.LetterText, gameCondition.def.letterDef,
                        new TargetInfo(map.Center, map, false));
                    }
                    else
                    {
                        Find.LetterStack.ReceiveLetter("PurpleFogСomesFromInfectedSites.".Translate(),
                        "PurpleFogСomesFromInfectedSitesDesc".Translate(),
                        LetterDefOf.ThreatBig, new TargetInfo(map.Center, map, false));
                        Log.Message("PurpleFogСomesFromInfectedSites: " + map.ToString()
                            + " - " + Find.TickManager.TicksGame.ToString());
                    }
                    if (map.Parent.GetComponent<WorldObjectComp_InfectedTile>() == null)
                    {
                        var comp = new WorldObjectComp_InfectedTile();
                        comp.parent = map.Parent;
                        comp.StartInfection();
                        comp.gameConditionCaused = PurpleIvyDefOf.PurpleFogGameCondition;
                        comp.counter = count;
                        comp.infectedTile = map.Tile;
                        comp.radius = comp.GetRadius();
                        PurpleIvyData.TotalFogProgress[comp] = PurpleIvyData.getFogProgress(comp.counter);
                        comp.fillRadius();
                        map.Parent.AllComps.Add(comp);
                        Log.Message("Adding comp to: " + map.Parent.ToString());
                    }
                }
            }
            if (Find.TickManager.TicksGame % 60000 == 0)
            {
                int count = 0;
                var alphaEggs = this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSac);
                var betaEggs = this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSacBeta);
                var gammaEggs = this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSacGamma);
                var nestsEggs = this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSacNestGuard);
                var omegaEggs = this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.ParasiteEgg);

                Log.Message("Total PurpleIvy count on the map: " +
                    this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Count.ToString(), true);

                count = this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteAlpha).Count;
                if (count > PurpleIvySettings.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteAlpha.defName])
                {
                    foreach (var egg in alphaEggs)
                    {
                        var eggSac = (Building_EggSac)egg;
                        eggSac.TryGetComp<AlienInfection>().stopSpawning = true;
                    }
                }
                else
                {
                    foreach (var egg in alphaEggs)
                    {
                        var eggSac = (Building_EggSac)egg;
                        eggSac.TryGetComp<AlienInfection>().stopSpawning = false;
                    }
                }
                Log.Message("Total Genny_ParasiteAlpha count on the map: " + count.ToString(), true);
                count = this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteBeta).Count;
                if (count > PurpleIvySettings.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteBeta.defName])
                {
                    foreach (var egg in betaEggs)
                    {
                        var eggSac = (Building_EggSac)egg;
                        eggSac.TryGetComp<AlienInfection>().stopSpawning = true;
                    }
                }
                else
                {
                    foreach (var egg in betaEggs)
                    {
                        var eggSac = (Building_EggSac)egg;
                        eggSac.TryGetComp<AlienInfection>().stopSpawning = false;
                    }
                }
                Log.Message("Total Genny_ParasiteBeta count on the map: " + count.ToString(), true);
                count = this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteGamma).Count;
                if (count > PurpleIvySettings.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteGamma.defName])
                {
                    foreach (var egg in gammaEggs)
                    {
                        var eggSac = (Building_EggSac)egg;
                        eggSac.TryGetComp<AlienInfection>().stopSpawning = true;
                    }
                }
                else
                {
                    foreach (var egg in gammaEggs)
                    {
                        var eggSac = (Building_EggSac)egg;
                        eggSac.TryGetComp<AlienInfection>().stopSpawning = false;
                    }
                }
                Log.Message("Total Genny_ParasiteGamma count on the map: " + count.ToString(), true);
                count = this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteOmega).Count;
                if (count > PurpleIvySettings.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteOmega.defName])
                {
                    foreach (var egg in omegaEggs)
                    {
                        var eggSac = (Building_EggSac)egg;
                        eggSac.TryGetComp<AlienInfection>().stopSpawning = true;
                    }
                }
                else
                {
                    foreach (var egg in omegaEggs)
                    {
                        var eggSac = (Building_EggSac)egg;
                        eggSac.TryGetComp<AlienInfection>().stopSpawning = false;
                    }
                }
                Log.Message("Total Genny_ParasiteOmega count on the map: " + count.ToString(), true);
                Log.Message("Total Genny_ParasiteNestGuard count on the map: " +
this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteNestGuard).Count.ToString(), true);
                Log.Message("Total EggSac count on the map: " + alphaEggs.Count.ToString(), true);
                Log.Message("Total EggSac beta count on the map: " + betaEggs.Count.ToString(), true);
                Log.Message("Total EggSac gamma count on the map: " + gammaEggs.Count.ToString(), true);
                Log.Message("Total EggSac NestGuard count on the map: " + nestsEggs.Count.ToString(), true);
                Log.Message("Total ParasiteEgg count on the map: " + omegaEggs.Count.ToString(), true);
                Log.Message("Total GasPump count on the map: " +
this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.GasPump).Count.ToString(), true);
                Log.Message("Total GenTurretBase count on the map: " +
this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.GenTurretBase).Count.ToString(), true);
                Log.Message("Total Turret_GenMortarSeed count on the map: " +
this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.Turret_GenMortarSeed).Count.ToString(), true);
                Log.Message("Total Nest count on the map: " +
this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.PI_Nest).Count.ToString(), true);
            }
        }

        public bool OrbitalHelpActive = false;

        public Dictionary<Building, int> ToxicDamages = new Dictionary<Building, int>();

        public List<Building> ToxicDamageKeys = new List<Building>();

        public List<int> ToxicDamageValues = new List<int>();

    }
}

