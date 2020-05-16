using System.Collections.Generic;
using Verse;
using UnityEngine;
using System.Reflection;
using HarmonyLib;
namespace PurpleIvy
{
    [StaticConstructorOnStartup]
    public class PurpleIvySettings : ModSettings
    {
        public PurpleIvySettings settings;
        public override void ExposeData()
        {
            Scribe_Collections.Look<string, int>(ref TotalAlienLimit, "TotalAlienLimit",
                LookMode.Value, LookMode.Value, ref this.TotalAlienLimitKeys, ref this.TotalAlienLimitValue);
            base.ExposeData();
        }

        public static void Reset()
        {
            TotalAlienLimit["Genny_ParasiteAlpha"] = 7;
            TotalAlienLimit["Genny_ParasiteBeta"] = 15;
            TotalAlienLimit["Genny_ParasiteGamma"] = 15;
            TotalAlienLimit["Genny_ParasiteOmega"] = 25;
        }

        public static void DoWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
        
            listingStandard.Label("TotalAlphaCreaturesOnMap".Translate() 
                + TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteAlpha.defName]);
            TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteAlpha.defName] =
            (int)listingStandard.Slider(TotalAlienLimit
            [PurpleIvyDefOf.Genny_ParasiteAlpha.defName], 0, 1000);
        
            listingStandard.Label("TotalBetaCreaturesOnMap".Translate() 
                + TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteBeta.defName]);
            TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteBeta.defName] =
            (int)listingStandard.Slider(TotalAlienLimit
            [PurpleIvyDefOf.Genny_ParasiteBeta.defName], 0, 1000);
        
            listingStandard.Label("TotalGammaCreaturesOnMap".Translate() 
                + TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteGamma.defName]);
            TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteGamma.defName] =
            (int)listingStandard.Slider(TotalAlienLimit
            [PurpleIvyDefOf.Genny_ParasiteGamma.defName], 0, 1000);
        
            listingStandard.Label("TotalOmegaCreaturesOnMap".Translate()
                + TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteOmega.defName]);
            TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteOmega.defName] =
            (int)listingStandard.Slider(TotalAlienLimit
            [PurpleIvyDefOf.Genny_ParasiteOmega.defName], 0, 1000);
            if (listingStandard.ButtonText("Reset to default values", null))
            {
                Reset();
            }
            listingStandard.End();
        }

        public static Dictionary<string, int> TotalAlienLimit = new Dictionary<string, int>()
        {
            {"Genny_ParasiteAlpha", 7},
            {"Genny_ParasiteBeta", 15},
            {"Genny_ParasiteGamma", 15},
            { "Genny_ParasiteOmega", 25},
        };

        private List<string> TotalAlienLimitKeys;

        private List<int> TotalAlienLimitValue;
    }
}

