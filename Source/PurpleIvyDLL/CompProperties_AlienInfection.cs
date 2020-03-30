using System;
using System.Collections.Generic;
using Verse;

namespace PurpleIvy
{
    public class CompProperties_AlienInfection : CompProperties
    {
        public CompProperties_AlienInfection()
        {
            this.compClass = typeof(AlienInfection);
        }

        public List<string> typesOfCreatures = null;
        public int numberOfCreaturesPerSpawn = 0;
        public int numberOfCreaturesPerSpawnRandom = 0;
        public int maxNumberOfCreatures = 0;
        public int incubationPeriod = 0;
        public int growthTick = 0;

    }
}

