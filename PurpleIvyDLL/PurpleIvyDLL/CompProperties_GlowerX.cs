using System;
using RimWorld;
using Verse;

namespace PurpleIvy
{
    public class CompProperties_Glowerx : CompProperties
    {
        public CompProperties_Glowerx()
        {
            this.compClass = typeof(CompGlowerX);
        }

        public float overlightRadius;

        public float glowRadius = 3f;

        public ColorInt glowColor = new ColorInt(150, 255, 255, 0);

    }
}
