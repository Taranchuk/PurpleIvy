using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace PurpleIvy
{
    public class MapComponent_WeatherControl : MapComponent
    {
        public MapComponent_WeatherControl(Map map) : base(map)
        {

        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (Find.TickManager.TicksGame % 250 == 0)
            {
                int count = this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Count;
                {
                    if (count >= 500 && !map.gameConditionManager.ConditionIsActive(PurpleIvyDefOf.PurpleFogGameCondition))
                    {
                        GameCondition_PurpleFog gameCondition =
                            (GameCondition_PurpleFog)GameConditionMaker.MakeConditionPermanent
                            (PurpleIvyDefOf.PurpleFogGameCondition);
                        map.gameConditionManager.RegisterCondition(gameCondition);
                        Find.LetterStack.ReceiveLetter(gameCondition.LabelCap,
                            gameCondition.LetterText, gameCondition.def.letterDef,
                            LookTargets.Invalid);
                    }
                }
            }
            if (Find.TickManager.TicksGame % 60000 == 0)
            {
                Log.Message("Total PurpleIvy count on the map: " + 
                    this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Count.ToString());
                Log.Message("Total Genny_ParasiteAlpha count on the map: " +
                    this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteAlpha).Count.ToString());
                Log.Message("Total Genny_ParasiteBeta count on the map: " +
    this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteBeta).Count.ToString());
                Log.Message("Total Genny_ParasiteOmega count on the map: " +
    this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.Genny_ParasiteOmega).Count.ToString());
                Log.Message("Total EggSac count on the map: " +
    this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.EggSac).Count.ToString());
                Log.Message("Total ParasiteEgg count on the map: " +
this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.ParasiteEgg).Count.ToString());
                Log.Message("Total GasPump count on the map: " +
this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.GasPump).Count.ToString());
                Log.Message("Total GenTurretBase count on the map: " +
this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.GenTurretBase).Count.ToString());
                Log.Message("Total Turret_GenMortarSeed count on the map: " +
this.map.listerThings.ThingsOfDef(PurpleIvyDefOf.Turret_GenMortarSeed).Count.ToString());
            }
        }
    }
}

