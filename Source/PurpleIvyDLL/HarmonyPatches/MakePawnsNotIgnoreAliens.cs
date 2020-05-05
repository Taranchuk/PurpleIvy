using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PurpleIvy
{

    [HarmonyPatch(typeof(AttackTargetFinder), "GetShootingTargetScore")]
    internal class AttackTargetFinder_GetShootingTargetScore
    {
        [HarmonyPrefix]
        public static bool GetShootingTargetScoreFix(ref float __result, IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
        {
            bool result;
            if (target is Pawn pawn && pawn?.Faction?.def == PurpleIvyDefOf.Genny)
            {
                __result = AttackTargetFinder_GetShootingTargetScore.GetShootingTargetScore(target, searcher, verb);
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }

        private static float GetShootingTargetScore(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
        {
            float num = 60f;
            num -= Mathf.Min((target.Thing.Position - searcher.Thing.Position).LengthHorizontal, 40f);
            if (target.TargetCurrentlyAimingAt == searcher.Thing)
            {
                num += 10f;
            }
            if (searcher.LastAttackedTarget == target.Thing && Find.TickManager.TicksGame - searcher.LastAttackTargetTick <= 300)
            {
                num += 40f;
            }
            num -= CoverUtility.CalculateOverallBlockChance(target.Thing.Position, searcher.Thing.Position, searcher.Thing.Map) * 10f;
            Pawn pawn = target as Pawn;
            if (pawn != null && pawn.RaceProps.Animal && pawn.Faction != PurpleIvyData.AlienFaction && !pawn.IsFighting())
            {
                num -= 50f;
            }
            num += AttackTargetFinder_GetShootingTargetScore.FriendlyFireBlastRadiusTargetScoreOffset(target, searcher, verb);
            num += AttackTargetFinder_GetShootingTargetScore.FriendlyFireConeTargetScoreOffset(target, searcher, verb);
            return num * target.TargetPriorityFactor;
        }

        private static float FriendlyFireBlastRadiusTargetScoreOffset(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
        {
            if (verb.verbProps.ai_AvoidFriendlyFireRadius <= 0f)
            {
                return 0f;
            }
            Map map = target.Thing.Map;
            IntVec3 position = target.Thing.Position;
            int num = GenRadial.NumCellsInRadius(verb.verbProps.ai_AvoidFriendlyFireRadius);
            float num2 = 0f;
            for (int i = 0; i < num; i++)
            {
                IntVec3 intVec = position + GenRadial.RadialPattern[i];
                if (intVec.InBounds(map))
                {
                    bool flag = true;
                    List<Thing> thingList = intVec.GetThingList(map);
                    for (int j = 0; j < thingList.Count; j++)
                    {
                        if (thingList[j] is IAttackTarget && thingList[j] != target)
                        {
                            if (flag)
                            {
                                if (!GenSight.LineOfSight(position, intVec, map, true, null, 0, 0))
                                {
                                    break;
                                }
                                flag = false;
                            }
                            float num3;
                            if (thingList[j] == searcher)
                            {
                                num3 = 40f;
                            }
                            else if (thingList[j] is Pawn)
                            {
                                num3 = 18f;
                            }
                            else
                            {
                                num3 = 10f;
                            }
                            if (searcher.Thing.HostileTo(thingList[j]))
                            {
                                num2 += num3 * 0.6f;
                            }
                            else
                            {
                                num2 -= num3;
                            }
                        }
                    }
                }
            }
            return num2;
        }

        private static float FriendlyFireConeTargetScoreOffset(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
        {
            Pawn pawn = searcher.Thing as Pawn;
            if (pawn == null)
            {
                return 0f;
            }
            if (pawn.RaceProps.intelligence < Intelligence.ToolUser)
            {
                return 0f;
            }
            if (pawn.RaceProps.IsMechanoid)
            {
                return 0f;
            }
            Verb_Shoot verb_Shoot = verb as Verb_Shoot;
            if (verb_Shoot == null)
            {
                return 0f;
            }
            ThingDef defaultProjectile = verb_Shoot.verbProps.defaultProjectile;
            if (defaultProjectile == null)
            {
                return 0f;
            }
            if (defaultProjectile.projectile.flyOverhead)
            {
                return 0f;
            }
            Map map = pawn.Map;
            ShotReport report = ShotReport.HitReportFor(pawn, verb, (Thing)target);
            float a = VerbUtility.CalculateAdjustedForcedMiss(verb.verbProps.forcedMissRadius, report.ShootLine.Dest - report.ShootLine.Source);
            float radius = Mathf.Max(a, 1.5f);
            IntVec3 dest2 = report.ShootLine.Dest;
            IEnumerable<IntVec3> source = from dest in GenRadial.RadialCellsAround(dest2, radius, true)
                                          where dest.InBounds(map)
                                          select dest;
            IEnumerable<ShootLine> source2 = from dest in source
                                             select new ShootLine(report.ShootLine.Source, dest);
            IEnumerable<IntVec3> source3 = source2.SelectMany((ShootLine line) => line.Points().Concat(line.Dest).TakeWhile((IntVec3 pos) => pos.CanBeSeenOverFast(map)));
            IEnumerable<IntVec3> enumerable = source3.Distinct<IntVec3>();
            float num = 0f;
            foreach (IntVec3 c in enumerable)
            {
                float num2 = VerbUtility.InterceptChanceFactorFromDistance(report.ShootLine.Source.ToVector3Shifted(), c);
                if (num2 > 0f)
                {
                    List<Thing> thingList = c.GetThingList(map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        Thing thing = thingList[i];
                        if (thing is IAttackTarget && thing != target)
                        {
                            float num3;
                            if (thing == searcher)
                            {
                                num3 = 40f;
                            }
                            else if (thing is Pawn)
                            {
                                num3 = 18f;
                            }
                            else
                            {
                                num3 = 10f;
                            }
                            num3 *= num2;
                            if (searcher.Thing.HostileTo(thing))
                            {
                                num3 *= 0.6f;
                            }
                            else
                            {
                                num3 *= -1f;
                            }
                            num += num3;
                        }
                    }
                }
            }
            return num;
        }
    }
}

