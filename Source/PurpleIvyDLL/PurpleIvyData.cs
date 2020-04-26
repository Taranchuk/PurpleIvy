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

        public static Faction AlienFaction => Find.FactionManager.FirstFactionOfDef(PurpleIvyDefOf.Genny);

        public static Color PurpleColor = new Color(0.368f, 0f, 1f);

        public static Dictionary<WorldObjectComp_InfectedTile, float> TotalFogProgress = new Dictionary<WorldObjectComp_InfectedTile, float>();

        public static List<int> TotalPollutedBiomes = new List<int>();

        public static List<int> BiomesToRenderNow = new List<int>();

        public static bool BiomesDirty = false;

        public static bool BiomesToClear = false;
        public static void UpdateBiomes()
        {
            if (PurpleIvyData.BiomesToClear == true)
            {
                PurpleIvyData.ClearAlienBiomesOuterTheSources();
                PurpleIvyData.BiomesToClear = false;
            }
            Find.World.renderer.SetDirty<WorldLayerRegenerateBiomes>();
            PurpleIvyData.BiomesDirty = false;
        }
        public static void ClearAlienBiomesOuterTheSources()
        {
            for (int i = PurpleIvyData.TotalPollutedBiomes.Count - 1; i >= 0; i--)
            {
                int tile = PurpleIvyData.TotalPollutedBiomes[i];
                if (PurpleIvyData.TileInRadiusOfInfectedSites(tile) != true)
                {
                    Log.Message("Return old biome: " + tile.ToString());
                    BiomeDef origBiome = Find.WorldGrid[tile].biome;
                    BiomeDef newBiome = BiomeDef.Named(origBiome.defName.ReplaceFirst("PI_", string.Empty));
                    Find.WorldGrid[tile].biome = newBiome;
                    PurpleIvyData.TotalPollutedBiomes.Remove(tile);
                    PurpleIvyData.BiomesToRenderNow.Add(tile);
                }
            }
        }
        public static float GetPartFromPercentage(float percent, float whole)
        {
            return (float)(percent * whole) / 100f;
        }

        public static float GetPercentageFromPartWhole(float part, int whole)
        {
            return 100 * part / (float)(whole) - 100f;
        }
    

        public static float getFogProgressWithOuterSources(int count, WorldObjectComp_InfectedTile comp, out bool comeFromOuterSource)
        {
            var result = PurpleIvyData.getFogProgress(count);
            //Log.Message("fog progress: " + result.ToString());
            var outerSource = 0f;
            foreach (var data in PurpleIvyData.TotalFogProgress.Where(data => data.Key != comp))
            {
                int distance = Find.WorldGrid.TraversalDistanceBetween(comp.infectedTile, data.Key.infectedTile, true, int.MaxValue);
                if (distance <= data.Key.radius)
                {
                    float floatRadius = ((float)data.Key.counter - 500f) / 100f;
                    if (floatRadius < 0)
                    {
                        floatRadius = 0;
                    }
                    float newValue = GetPartFromPercentage(GetPercentageFromPartWhole(floatRadius, distance), data.Value);
                    if (newValue > data.Value)
                    {
                        outerSource += data.Value;
                    }
                    else
                    {
                        outerSource += newValue;
                    }
                }
            }
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

        public static bool TileInRadiusOfInfectedSites(int tile)
        {
            foreach (var comp in PurpleIvyData.TotalFogProgress)
            {
                //Log.Message("Checking tile: " + tile.ToString() + " against "
                //    + comp.Key.infectedTile.ToString() + " - radius: " + comp.Key.radius.ToString()
                //    + " - distance: " + (Find.WorldGrid.TraversalDistanceBetween
                //(comp.Key.infectedTile, tile, true, int.MaxValue)).ToString());
                if (Find.WorldGrid.TraversalDistanceBetween
                (comp.Key.infectedTile, tile, true, int.MaxValue) <= comp.Key.radius)
                {
                    //Log.Message("Tile in radius: " + tile.ToString());
                    return true;
                }
            }
            //Log.Message("Tile not in radius: " + tile.ToString());
            return false;
        }
        public static float getFogProgress(int count)
        {
            var result = (float)(count - 500) / (float)1000;
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
            "Genny_ParasiteBetaD"
        };

        public static List<string> Genny_ParasiteGamma = new List<string>
        {
            "Genny_ParasiteGammaA"
        };

        public static List<string> Genny_ParasiteOmega = new List<string>
        {
            "Genny_ParasiteOmega"
        };

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

