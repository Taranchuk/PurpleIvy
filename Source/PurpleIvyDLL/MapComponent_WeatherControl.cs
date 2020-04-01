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
                    if (count >= 250 && !map.gameConditionManager.ConditionIsActive(PurpleIvyDefOf.PurpleFogGameCondition))
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
        }
    }
}

