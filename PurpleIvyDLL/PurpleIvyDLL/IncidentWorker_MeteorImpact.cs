using System;
using Verse;
namespace RimWorld
{
    public class IncidentWorker_MeteorImpact : IncidentWorker
    {
        private const float FogClearRadius = 4.5f;
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            ThingDef thingDef = ThingDef.Named("Meteorite");
            Thing singleContainedThing = ThingMaker.MakeThing(thingDef);
            IntVec3 intVec = CellFinderLoose.RandomCellWith((IntVec3 sq) => GenGrid.Standable(sq, map) && !GridsUtility.Roofed(sq, map) && !GridsUtility.Fogged(sq, map), map);
            MeteorUtility.MakeMeteorAt(intVec, new MeteorInfo
            {
                SingleContainedThing = singleContainedThing,
                openDelay = 1,
                leaveSlag = false
            });
            Find.LetterStack.ReceiveLetter("MeteoritePurple".Translate(), "MeteoritePurpleDesc".Translate(), LetterDefOf.NeutralEvent); //"Look a giant flying purple rock....its purple it has to be something good...right?"
            return true;
        }
    }
}
