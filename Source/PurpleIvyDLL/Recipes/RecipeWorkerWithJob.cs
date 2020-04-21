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
        public JobDef AlienStudy
        {
            get
            {
                return PurpleIvyDefOf.PI_ConductResearchOnAliens;
            }
        }

        public JobDef DrawAlienBlood
        {
            get
            {
                return PurpleIvyDefOf.PI_DrawAlienBlood;
            }
        }

        public JobDef PreciseVivisection
        {
            get
            {
                return PurpleIvyDefOf.PI_DrawAlienBlood;
            }
        }

    }
}

