using System.Collections.Generic;
using Verse;
using UnityEngine;
using System.Reflection;
using HarmonyLib;
namespace PurpleIvy
{
    public class PurpleIvySettings : ModSettings
    {

        public override void ExposeData()
        {
            Scribe_Collections.Look<ThingDef, int>(ref PurpleIvyData.TotalAlienLimit, "TotalAlienLimit", LookMode.Def, LookMode.Value);
            base.ExposeData();
        }
    }

    public class PurpleIvyMod : Mod
    {
        PurpleIvySettings settings;
        public PurpleIvyMod(ModContentPack content) : base(content)
        {
            Harmony harmonyInstance = new Harmony("rimworld.PurpleIvy.org");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            this.settings = GetSettings<PurpleIvySettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label("TotalAlphaCreaturesOnMap".Translate());
            PurpleIvyData.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteAlpha] = 
            (int)listingStandard.Slider(PurpleIvyData.TotalAlienLimit
            [PurpleIvyDefOf.Genny_ParasiteAlpha], 0, 1000);

            listingStandard.Label("TotalBetaCreaturesOnMap".Translate());
            PurpleIvyData.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteBeta] =
            (int)listingStandard.Slider(PurpleIvyData.TotalAlienLimit
            [PurpleIvyDefOf.Genny_ParasiteBeta], 0, 1000);

            listingStandard.Label("TotalGammaCreaturesOnMap".Translate());
            PurpleIvyData.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteGamma] =
            (int)listingStandard.Slider(PurpleIvyData.TotalAlienLimit
            [PurpleIvyDefOf.Genny_ParasiteGamma], 0, 1000);


            listingStandard.Label("TotalOmegaCreaturesOnMap".Translate());
            PurpleIvyData.TotalAlienLimit[PurpleIvyDefOf.Genny_ParasiteOmega] =
            (int)listingStandard.Slider(PurpleIvyData.TotalAlienLimit
            [PurpleIvyDefOf.Genny_ParasiteOmega], 0, 1000);

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Purple Ivy";
        }
    }
}