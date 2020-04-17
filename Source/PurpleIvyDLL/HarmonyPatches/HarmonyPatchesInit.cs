using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace PurpleIvy
{
    [StaticConstructorOnStartup]
    internal static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmonyInstance = new Harmony("rimworld.PurpleIvy.org");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}

