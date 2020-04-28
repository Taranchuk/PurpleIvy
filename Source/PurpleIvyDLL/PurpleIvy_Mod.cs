using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace PurpleIvy.Settings
{
    public class PurpleIvy_Mod : Mod
    {
        public PurpleIvy_Mod(ModContentPack content) : base(content)
        {
            Harmony harmonyInstance = new Harmony("rimworld.PurpleIvy.org");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            base.GetSettings<PurpleIvySettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            PurpleIvySettings.DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Purple Ivy";
        }
    }
}

