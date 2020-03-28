using System;
using RimWorld;
using Verse;

namespace PurpleIvy
{
    public class CompProperties_GlowerX : CompProperties
    {
        public CompProperties_GlowerX() 
        {
            this.compClass = typeof(CompGlowerX);
        }

        public float overlightRadius;

        public float glowRadius = 3f;

        public ColorInt glowColor = new ColorInt(150, 255, 255, 0);

    }
}
