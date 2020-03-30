﻿using System;
using Verse;

namespace PurpleIvy
{
    public class CompProperties_GasProducer : CompProperties
    {
        public CompProperties_GasProducer()
        {
            this.compClass = typeof(CompGasProducer);
        }

        public string gasType = "";

        public float rate = 0f;

        public int radius = 0;
    }
}
