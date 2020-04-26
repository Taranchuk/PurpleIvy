﻿using System;
using Verse;
using RimWorld;

namespace PurpleIvy
{
    public class IncidentWorker_MeteorImpact : IncidentWorker
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target; 
            //ThingDef skyfaller = DefDatabase<ThingDef>.GetNamed("PI_MeteorIncoming", true);
            ThingDef meteor = DefDatabase<ThingDef>.GetNamed("PI_Meteorite", true);
            IntVec3 intVec = CellFinderLoose.RandomCellWith(
                (IntVec3 sq) => GenGrid.Standable(sq, map) 
                && !GridsUtility.Roofed(sq, map) 
                && !GridsUtility.Fogged(sq, map)
                && GenGrid.InBounds(sq, map), map);
            //SkyfallerMaker.SpawnSkyfaller(skyfaller, meteor, intVec, map);
            Thing singleContainedThing = ThingMaker.MakeThing(meteor);
            var meteorIncoming = (MeteorIncoming)ThingMaker.MakeThing(ThingDef.Named("PI_MeteorIncoming"), null);
            MeteorUtility.MakeMeteorAt(map, intVec, meteorIncoming, new MeteorInfo 
            {
                SingleContainedThing = singleContainedThing,
                openDelay = 1,
                leaveSlag = false
            }); 
            Find.LetterStack.ReceiveLetter("MeteoritePurple".Translate(), "MeteoritePurpleDesc".Translate(), LetterDefOf.NeutralEvent, new TargetInfo(intVec, map, false)); //"Look a giant flying purple rock....its purple it has to be something good...right?"
            return true;
        } 
    }
}

