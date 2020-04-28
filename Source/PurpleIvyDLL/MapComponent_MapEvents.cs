using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
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
            Scribe_Collections.Look<Building, int>(ref this.ToxicDamages, "ToxicDamages", LookMode.Reference, LookMode.Value, ref this.ToxicDamageKeys, ref this.ToxicDamageValues);
            base.ExposeData();
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (Find.TickManager.TicksGame % 250 == 0)
            {
                Log.Message("Alpha limit: " + PurpleIvySettings.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteAlpha.defName]);
                Log.Message("Beta limit: " + PurpleIvySettings.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteBeta.defName]);
                Log.Message("Gamma limit: " + PurpleIvySettings.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteGamma.defName]);
                Log.Message("Omega limit: " + PurpleIvySettings.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteOmega.defName]);
                int count = this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Count;
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

        public Dictionary<Building, int> ToxicDamages = new Dictionary<Building, int>();
        
        public List<Building> ToxicDamageKeys = new List<Building>();
        
        public List<int> ToxicDamageValues = new List<int>();

    }
}

