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
        public static Faction KorsolianFaction => Find.FactionManager.FirstFactionOfDef(PurpleIvyDefOf.KorsolianFaction);

        public static Color PurpleColor = new Color(0.368f, 0f, 1f);

        public static Dictionary<WorldObjectComp_InfectedTile, float> TotalFogProgress = new Dictionary<WorldObjectComp_InfectedTile, float>();

        public static List<int> TotalPollutedBiomes = new List<int>();

        public static List<int> BiomesToRenderNow = new List<int>();

        public static bool BiomesDirty = false;

        public static bool BiomesToClear = false;

        public static int LastAttacked = 0;

        public static List<ResearchProjectDef> AlienStudy = new List<ResearchProjectDef>
        {
            PurpleIvyDefOf.PI_MaskingTechnologies,
            PurpleIvyDefOf.PI_Vivisection,
            PurpleIvyDefOf.PI_ResourceExtraction,
            PurpleIvyDefOf.PI_AdvAlienContainment
        };

        public static Dictionary<ThingDef, List<ResearchProjectDef>> BioStudy = new Dictionary<ThingDef, List<ResearchProjectDef>>() {
            {
                PurpleIvyDefOf.PI_AlphaBlood, new List <ResearchProjectDef> {
                    PurpleIvyDefOf.PI_AdvDrugs,
                    PurpleIvyDefOf.PI_BasicDrugs,
                    PurpleIvyDefOf.PI_MutagensA,
                    PurpleIvyDefOf.PI_MutagensB,
                    PurpleIvyDefOf.PI_MutagensC,
                    PurpleIvyDefOf.PI_ResourceProcession,
                    PurpleIvyDefOf.PI_BasicBionics,
                    PurpleIvyDefOf.PI_AdvBionics,
                }
            },
            {
                PurpleIvyDefOf.PI_BetaBlood, new List<ResearchProjectDef> {
                    PurpleIvyDefOf.PI_AdvDrugs,
                    PurpleIvyDefOf.PI_BasicDrugs,
                    PurpleIvyDefOf.PI_MutagensB,
                    PurpleIvyDefOf.PI_MutagensC,
                    PurpleIvyDefOf.PI_ResourceProcession,
                    PurpleIvyDefOf.PI_BasicBionics,
                    PurpleIvyDefOf.PI_AdvBionics,

                }
            },
            {
                PurpleIvyDefOf.PI_GammaBlood, new List<ResearchProjectDef> {
                    PurpleIvyDefOf.PI_AdvDrugs,
                    PurpleIvyDefOf.PI_BasicDrugs,
                    PurpleIvyDefOf.PI_MutagensC,
                    PurpleIvyDefOf.PI_ResourceProcession,
                    PurpleIvyDefOf.PI_BasicBionics

                }
            },
            {
                PurpleIvyDefOf.PI_OmegaBlood, new List<ResearchProjectDef> {
                    PurpleIvyDefOf.PI_AdvDrugs,
                    PurpleIvyDefOf.PI_BasicDrugs,
                    PurpleIvyDefOf.PI_ResourceProcession,
                    PurpleIvyDefOf.PI_BasicBionics,
                }
            },
            {
                PurpleIvyDefOf.PI_PlasmaNucleus, new List<ResearchProjectDef> {
                    PurpleIvyDefOf.PI_ResourceProcession
                }
            },
            {
                PurpleIvyDefOf.PI_QueenNervousSystem, new List<ResearchProjectDef> {
                    PurpleIvyDefOf.PI_ResourceProcession,
                    PurpleIvyDefOf.PI_AdvDrugs,
                    PurpleIvyDefOf.PI_BasicDrugs,
                    PurpleIvyDefOf.PI_MutagensA,
                    PurpleIvyDefOf.PI_MutagensB,
                    PurpleIvyDefOf.PI_MutagensC,
                }
            },
            {
                PurpleIvyDefOf.PI_NestGuardNervousSystem, new List<ResearchProjectDef> {
                    PurpleIvyDefOf.PI_ResourceProcession,
                    PurpleIvyDefOf.PI_AdvDrugs,
                    PurpleIvyDefOf.PI_BasicDrugs,
                    PurpleIvyDefOf.PI_MutagensA,
                    PurpleIvyDefOf.PI_MutagensB,
                    PurpleIvyDefOf.PI_MutagensC,
                }
            },
            {
                PurpleIvyDefOf.PI_AlphaNervousSystem, new List<ResearchProjectDef> {
                    PurpleIvyDefOf.PI_ResourceProcession,
                    PurpleIvyDefOf.PI_AdvDrugs,
                    PurpleIvyDefOf.PI_BasicDrugs,
                    PurpleIvyDefOf.PI_MutagensA,
                    PurpleIvyDefOf.PI_MutagensB,
                    PurpleIvyDefOf.PI_MutagensC,
                }
            },
            {
                PurpleIvyDefOf.PI_BetaNervousSystem, new List<ResearchProjectDef> {
                    PurpleIvyDefOf.PI_ResourceProcession,
                    PurpleIvyDefOf.PI_AdvDrugs,
                    PurpleIvyDefOf.PI_BasicDrugs,
                    PurpleIvyDefOf.PI_MutagensB,
                    PurpleIvyDefOf.PI_MutagensC,
                }
            },
            {
                PurpleIvyDefOf.PI_GammaNervousSystem, new List<ResearchProjectDef> {
                    PurpleIvyDefOf.PI_ResourceProcession,
                    PurpleIvyDefOf.PI_AdvDrugs,
                    PurpleIvyDefOf.PI_BasicDrugs,
                    PurpleIvyDefOf.PI_MutagensC,
                }
            },
            {
                PurpleIvyDefOf.PI_OmegaNervousSystem, new List<ResearchProjectDef> {
                    PurpleIvyDefOf.PI_ResourceProcession,
                    PurpleIvyDefOf.PI_AdvDrugs,
                    PurpleIvyDefOf.PI_BasicDrugs,
                }
            },
            {
                PurpleIvyDefOf.PI_Tentacles, new List<ResearchProjectDef> {
                    PurpleIvyDefOf.PI_ResourceProcession,
                    PurpleIvyDefOf.PI_BasicBionics,
                }
            },
            {
                PurpleIvyDefOf.PI_BigTentacles, new List<ResearchProjectDef> {
                    PurpleIvyDefOf.PI_ResourceProcession,
                    PurpleIvyDefOf.PI_BasicBionics,
                    PurpleIvyDefOf.PI_AdvBionics,
                }
            },
            {
                PurpleIvyDefOf.PI_ToxicSac, new List<ResearchProjectDef> {
                    PurpleIvyDefOf.PI_ResourceProcession,
                    PurpleIvyDefOf.PI_AdvBioprotection
                }
            },
        };

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

        public static List<string> Genny_ParasiteNestGuard = new List<string>
        {
            "Genny_ParasiteNestGuard"
        };

        public static Dictionary<string, IntRange> maxNumberOfCreatures = new Dictionary<string, IntRange>()
        {
            { "Genny_ParasiteAlpha", new IntRange(1, 1) },
            { "Genny_ParasiteBeta", new IntRange(1, 3) },
            { "Genny_ParasiteGamma", new IntRange(1, 3) },
            { "Genny_ParasiteOmega", new IntRange(1, 5) }
        };

        public static Dictionary<List<string>, int> combatPoints = new Dictionary<List<string>, int>()
        {
            { Genny_ParasiteAlpha, 150 },
            { Genny_ParasiteBeta, 100 },
            { Genny_ParasiteGamma, 50 },
            { Genny_ParasiteOmega, 35 }
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

