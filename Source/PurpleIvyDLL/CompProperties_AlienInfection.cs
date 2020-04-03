using System;
using System.Collections.Generic;
using Verse;

namespace PurpleIvy
{
    public class CompProperties_AlienInfection : CompProperties
    {

        public List<string> typesOfCreatures = null;
        public int maxNumberOfCreatures = 0;
        public IntRange numberOfCreaturesPerSpawn = new IntRange(0, 0);
        public IntRange incubationPeriod = new IntRange(0, 0);
        public IntRange ageTick = new IntRange(0, 0);
        public IntRange ticksPerSpawn = new IntRange(0, 0);
        public IntRange rotProgressPerSpawn = new IntRange(0, 0);
        public IncubationData IncubationData = new IncubationData();
        public bool resetIncubation = false;

        public CompProperties_AlienInfection()
        {
            this.compClass = typeof(AlienInfection);
        }

    }
}

