using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace PurpleIvy
{

    [HarmonyPatch(typeof(ResearchManager))]
    [HarmonyPatch("ApplyTechprint")]
    public static class InstantUnlockAlienTechnologies
    {
        [HarmonyPrefix]
        public static bool ApplyTechprint(ResearchProjectDef proj, Pawn applyingPawn)
        {
            if (proj.defName.StartsWith("PI_"))
            {
                Find.ResearchManager.FinishProject(proj, false, applyingPawn);
                applyingPawn.skills.Learn(SkillDefOf.Intellectual, 2000f, false);
                Find.LetterStack.ReceiveLetter("LetterTechprintResearchedLabel".Translate(proj.Named("PROJECT")), "LetterTechprintResearchedDesc".Translate(proj.Named("PROJECT")), LetterDefOf.PositiveEvent, null);
                return false;
            }
            return true;
        }
    }
}

