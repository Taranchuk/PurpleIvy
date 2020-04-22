using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    internal class RecipeWorkerWithJob : RecipeWorker
    {
        public JobDef AlienStudy => PurpleIvyDefOf.PI_ConductResearchOnAliens;

        public JobDef DrawAlienBlood => PurpleIvyDefOf.PI_DrawAlienBlood;

        public JobDef PreciseVivisection => PurpleIvyDefOf.PI_DrawAlienBlood;
    }
}

