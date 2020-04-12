using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PurpleIvy
{
    [StaticConstructorOnStartup]
    public static class PurpleIvyData
    {

        public static Faction factionDirect => Find.FactionManager.FirstFactionOfDef(PurpleIvyDefOf.Genny);

        public static Color PurpleColor = new Color(0.368f, 0f, 1f);

        public static float getFogProgressWithOuterSources(int count, WorldObjectComp_InfectedTile comp, out bool comeFromOuterSource)
        {
            var result = PurpleIvyData.getFogProgress(count);
            var outerSource = 0f;
            foreach (var data in PurpleIvyData.TotalFogProgress.Where(data => data.Key != comp).Where(data => Find.WorldGrid.TraversalDistanceBetween
                (comp.infectedTile, data.Key.infectedTile, true, int.MaxValue) - 1 <= data.Key.radius))
            {
<<<<<<< HEAD
                if (data.Key != comp)
                {
                    if (Find.WorldGrid.TraversalDistanceBetween
                        (comp.infectedTile, data.Key.infectedTile, true, int.MaxValue) - 1 <= data.Key.radius)
                    {
                        outerSource += data.Value;
                    }
                }
            }
            //outerSource = outerSource / 4;
=======
                Log.Message("outerSource from " + data.Key);
                outerSource += data.Value;
            }
            outerSource /= 4;
>>>>>>> EvaineQ-Patch_N.1
            outerSource -= 0.5f;
            if (outerSource < 0f)
            {
                outerSource = 0f;
            }
            if (result == 0f && outerSource > 0f)
            {
                comeFromOuterSource = true;
            }
            else
            {
                comeFromOuterSource = false;
            }
            return result + outerSource;
        }

        public static float getFogProgress(int count)
        {
            var result = ((float)(count - 500) / (float)1000 * 100f) / 100f;
            if (result < 0f)
            {
                result = 0f;
            }
            return result;
        }

        public static List<string> Genny_ParasiteAlpha = new List<string>
        {
            "Genny_ParasiteAlphaA",
            "Genny_ParasiteAlphaB",
            "Genny_ParasiteAlphaC"
        };

        public static List<string> Genny_ParasiteBeta = new List<string>
        {
            "Genny_ParasiteBetaA",
            "Genny_ParasiteBetaB",
            "Genny_ParasiteBetaC",
            "Genny_ParasiteBetaD",
            "Genny_ParasiteBetaE"
        };

        public static List<string> Genny_ParasiteOmega = new List<string>
        {
            "Genny_ParasiteOmega"
        };

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