using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    public static class FloatMenuMakerMap_Patch
    {
        [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
        public static class AddHumanlikeOrders_Patch
        {
            [HarmonyPostfix]
            public static void AddHumanlikeOrdersPostfix(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> opts)
            {
                foreach (LocalTargetInfo localTargetInfo in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), true))
                {
                    Pawn target = (Pawn)localTargetInfo.Thing;
                    if (target.Faction == PurpleIvyData.AlienFaction && target.Downed &&
                        ReservationUtility.CanReserveAndReach(pawn, target, PathEndMode.OnCell,
                        Danger.Deadly, 1, -1, null, true))
                    {
                        var containers = target.Map.listerBuildings.AllBuildingsColonistOfClass
                            <Building_СontainmentBreach>().Where(x => x.maxNumAliens > x.innerContainer.Count);
                        var containmentBreach = (Building_СontainmentBreach)GenClosest.ClosestThing_Global
                            (target.Position, containers, 9999f);
                        if (containmentBreach != null)
                        {
                            JobDef jobDef = PurpleIvyDefOf.PI_TakeAlienToContainmentBreach;
                            Action action = delegate ()
                            {
                                Job job = JobMaker.MakeJob(jobDef, target, containmentBreach);
                                job.count = 1;
                                pawn.jobs.TryTakeOrderedJob(job, 0);
                            };
                            string text = TranslatorFormattedStringExtensions.Translate("TakeAlienToContainmentBreach", target.LabelCap, target);
                            FloatMenuOption opt = new FloatMenuOption
                                (text, action, MenuOptionPriority.RescueOrCapture, null, target, 0f, null, null);
                            if (opts.Where(x => x.Label == text).Count() == 0)
                            {
                                opts.Add(opt);
                            }
                        }
                        else
                        {
                            string text = "NoContainersToTake".Translate();
                            if (opts.Where(x => x.Label == text).Count() == 0)
                            {
                                opts.Add(new FloatMenuOption(text, null, MenuOptionPriority.Default, null, null,
                                    0f, null, null));
                            }
                        }
                    }
                }
            }
        }
    }
}

