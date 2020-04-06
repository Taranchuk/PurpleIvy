using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PurpleIvy
{
    [StaticConstructorOnStartup]
    public static class PurpleIvyData
    {
        public static Faction factionDirect
        {
            get
            {
                return Find.FactionManager.FirstFactionOfDef(PurpleIvyDefOf.Genny);
            }
        }
        public static float getFogProgress(int count)
        {
            return (((float)count - 500) / (float)1500 * 100f) / 100f;
        }
        public static Dictionary<WorldObjectComp_InfectedTile, float> TotalFogProgress = new Dictionary<WorldObjectComp_InfectedTile, float>();

        public static Dictionary<int, int> RadiusData = new Dictionary<int, int>()
                {
                    {1, 5},
                    {2, 13},
                    {3, 29},
                    {4, 49},
                    {5, 81},
                    {6, 113},
                    {7, 149},
                    {8, 197},
                    {9, 253},
                    {10, 317},
                    {11, 377},
                    {12, 441},
                    {13, 529},
                    {14, 613},
                    {15, 709},
                    {16, 797},
                    {17, 901},
                    {18, 1009},
                    {19, 1129},
                    {20, 1257},
                    {21, 1373},
                    {22, 1517},
                    {23, 1653},
                    {24, 1793},
                    {25, 1961},
                    {26, 2121},
                    {27, 2289},
                    {28, 2453},
                    {29, 2629},
                    {30, 2821}
                };
    }
}