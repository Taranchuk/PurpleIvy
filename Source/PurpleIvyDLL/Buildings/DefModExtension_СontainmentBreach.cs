using System;
using System.Collections.Generic;
using Verse;

namespace PurpleIvy
{
    public class DefModExtension_СontainmentBreach : DefModExtension
    {
        public static readonly DefModExtension_СontainmentBreach defaultValues = new DefModExtension_СontainmentBreach();

        public int maxNumAliens = 0;

        public IntRange blackoutProtection = new IntRange(0, 0);
    }
}

