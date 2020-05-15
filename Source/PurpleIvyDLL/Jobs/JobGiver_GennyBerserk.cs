using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace PurpleIvy
{
    public class JobGiver_GennyBerserk : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (Find.TickManager.TicksGame < PrevTick + 10)
            {
                return null;
            }
            PrevTick = Find.TickManager.TicksGame;
            if (GridsUtility.Fogged(pawn.Position, pawn.Map))
            {
                return JobMaker.MakeJob(JobDefOf.LayDown);
            }

            Alien alien = pawn as Alien;
            // Some optimizations here...
            //if (pawn.TryGetAttackVerb(null, false) == null)
            //{
            //    return null;
            //}
            if (pawn.CurJob != null)
            {
                Log.Message(pawn + " - " + pawn.CurJob.def.defName);
            }
            Pawn pawn2 = null;
            if ((Find.TickManager.TicksGame - alien.lastAttacked) < 1000)
            {
                pawn2 = alien.lastInstigator;
            }
            else if ((Find.TickManager.TicksGame - pawn.Map.GetComponent<MapComponent_MapEvents>().LastAttacked) < 1000)
            {
                pawn2 = FindPawnTargetNearPlants(pawn);
            }
            else
            {
                pawn2 = FindPawnTarget(pawn);
            }
            if (pawn2 == null)
            {
                if (pawn.GetRoom() != null && !pawn.GetRoom().PsychologicallyOutdoors)
                {
                    Predicate<Thing> validator = delegate (Thing t)
                    {
                        return t.def.defName.ToLower().Contains("door");
                    };
                    var door = GenClosest.ClosestThingReachable(pawn.Position,
                    pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly,
                    TraverseMode.ByPawn, false)
                    , 5f, validator);
                    if (door != null)
                    {
                        return PurpleIvyUtils.MeleeAttackJob(pawn, door);
                    }
                    else
                    {
                        Predicate<Thing> validator2 = delegate (Thing t)
                        {
                            return t.def.defName.ToLower().Contains("wall");
                        };
                        var wall = GenClosest.ClosestThingReachable(pawn.Position,
                        pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly,
                        TraverseMode.ByPawn, false)
                        , 5f, validator2);
                        if (wall != null)
                        {
                            return PurpleIvyUtils.MeleeAttackJob(pawn, wall);
                        }
                    }
                }
            }
            else if (!pawn2.Downed)
            {
                var verb = pawn.VerbTracker.AllVerbs.Where(x => x.IsMeleeAttack != true).FirstOrDefault();
                if (pawn.def == PurpleIvyDefOf.Genny_ParasiteOmega && pawn.Position.InHorDistOf(pawn2.Position, 15) && Rand.Chance(0.7f))
                {
                    //Log.Message(Find.TickManager.TicksGame.ToString() + " - " + pawn + " - " + pawn.jobs?.curJob?.def?.defName + " - JUMP");
                    return PurpleIvyUtils.JumpOnTargetJob(pawn, pawn2);
                }
                else if (pawn.def == PurpleIvyDefOf.Genny_ParasiteBeta && pawn.Position.InHorDistOf(pawn2.Position, 2) && Rand.Chance(0.1f))
                {
                    //Log.Message(Find.TickManager.TicksGame.ToString() + " - " + pawn + " - " + pawn.jobs?.curJob?.def?.defName + " - SMOKE");
                    return PurpleIvyUtils.SmokeAttackJob(pawn, pawn2);
                }
                else if (verb != null && Rand.Chance(0.8f) && pawn.Position.InHorDistOf(pawn2.Position, verb.verbProps.range))
                {
                    //Log.Message(Find.TickManager.TicksGame.ToString() + " - " + pawn + " - " + pawn.jobs?.curJob?.def?.defName + " - SHOOT");
                    return PurpleIvyUtils.RangeAttackJob(pawn, pawn2);
                }
                else
                {
                    //Log.Message(Find.TickManager.TicksGame.ToString() + " - " + pawn + " - " + pawn.jobs?.curJob?.def?.defName + " - MELEE");
                    return PurpleIvyUtils.MeleeAttackJob(pawn, pawn2);
                }
            }
            else if (pawn2.Downed)
            {
                if (pawn2.BodySize >= 0.5f && pawn.def != PurpleIvyDefOf.Genny_ParasiteOmega && 
                    pawn.def != PurpleIvyDefOf.Genny_ParasiteGamma &&
                    pawn.kindDef != PurpleIvyDefOf.Genny_Queen 
                    && pawn.needs.food.CurCategory < HungerCategory.Hungry)
                {
                    //Log.Message(Find.TickManager.TicksGame.ToString() + " - " + pawn + " - " + pawn.jobs?.curJob?.def?.defName + " - Goo");
                    return PurpleIvyUtils.EntagleWithGooJob(pawn, pawn2);
                }
                else
                {
                    //Log.Message(Find.TickManager.TicksGame.ToString() + " - " + pawn + " - " + pawn.jobs?.curJob?.def?.defName + " - KILL");
                    return PurpleIvyUtils.KillAttackJob(pawn, pawn2);
                }
            }

            if (alien.canHaul)
            {
                Predicate<Thing> validator = delegate (Thing t)
                {
                    List<Thing> list = pawn.Map.thingGrid.ThingsListAt(t.Position);
                    return !(list.Count > 0 && list.OfType<Plant>().Any(x =>
                    x.def == PurpleIvyDefOf.PurpleIvy || x.def == PurpleIvyDefOf.PI_Nest
                    || x.def == PurpleIvyDefOf.PlantVenomousToothwort));
                };

                Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                    ThingRequest.ForDef(PurpleIvyDefOf.PI_StickyGoo), PathEndMode.ClosestTouch,
                    TraverseParms.For(pawn, Danger.None, TraverseMode.NoPassClosedDoors, false), 9999f, validator, null);
                if (thing == null)
                {
                    thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                        ThingRequest.ForGroup(ThingRequestGroup.Corpse), PathEndMode.ClosestTouch,
                        TraverseParms.For(pawn, Danger.None, TraverseMode.NoPassClosedDoors, false), 50f, validator, null);
                }
                if (thing != null && ReservationUtility.CanReserveAndReach(pawn, thing, PathEndMode.ClosestTouch, Danger.None))
                {
                    var plants = pawn.Map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy);
                    if (plants == null || plants.Count <= 0)
                    {
                        return null;
                    }
                    var plantToHaul = plants.RandomElement();
                    if (ReachabilityUtility.CanReach(pawn, plantToHaul, PathEndMode.ClosestTouch, Danger.None,
                        true, TraverseMode.NoPassClosedDoors))
                    {
                        Job job = JobMaker.MakeJob(PurpleIvyDefOf.PI_HaulToCell, thing, plantToHaul.Position);
                        job.attackDoorIfTargetLost = true;
                        if (job != null && job.TryMakePreToilReservations(pawn, false))
                        {
                            ReservationUtility.Reserve(pawn, thing, job);
                            return job;
                        }
                    }
                }
            }

            //Log.Message(Find.TickManager.TicksGame.ToString() + " - " + pawn + " - " + pawn.jobs?.curJob?.def?.defName + " - NULL 1");
            //Building building = this.FindTurretTarget(pawn);
            //if (building != null)
            //{
            //    return MeleeAttackJob(pawn, building);
            //}
            //if (pawn2 != null)
            //{
            //    using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, pawn2.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors, false), PathEndMode.OnCell))
            //    {
            //        if (!pawnPath.Found)
            //        {
            //            return null;
            //        }
            //        IntVec3 loc;
            //        if (!pawnPath.TryFindLastCellBeforeBlockingDoor(pawn, out loc))
            //        {
            //            Log.Error(pawn + " did TryFindLastCellBeforeDoor but found none when it should have been one. Target: " + pawn2.LabelCap, false);
            //            return null;
            //        }
            //        IntVec3 randomCell = CellFinder.RandomRegionNear(loc.GetRegion(pawn.Map, RegionType.Set_Passable), 9, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), null, null, RegionType.Set_Passable).RandomCell;
            //        if (randomCell == pawn.Position)
            //        {
            //            return JobMaker.MakeJob(JobDefOf.Wait, 30, false);
            //        }
            //        return JobMaker.MakeJob(JobDefOf.Goto, randomCell);
            //    }
            //}
            return null;
        }

        private Building FindTurretTarget(Pawn pawn)
        {
            return (Building)AttackTargetFinder.BestAttackTarget(pawn,
                TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns |
                TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat |
                TargetScanFlags.NeedAutoTargetable, (Thing t) => t is Building, 0f, 70f,
                default(IntVec3), float.MaxValue, false, true);
        }

        private static Pawn FindPawnTarget(Pawn pawn)
        {
            Pawn victim = null;
            bool Predicate(Thing p) => p != null && p != pawn
            && p.Faction != pawn.Faction && 
            ((Pawn)p).health.hediffSet.GetFirstHediffOfDef(PurpleIvyDefOf.PI_MaskingSprayHigh) == null;
            const float distance = 25f;
            victim = (Pawn)GenClosest.ClosestThingReachable(pawn.Position,
                pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly,
                TraverseMode.PassDoors, false)
                , distance, Predicate);
            return victim;
        }

        private static Pawn FindPawnTargetNearPlants(Pawn pawn)
        {
            Pawn victim = null;
            bool Predicate(Thing p) => p != null && p != pawn
            && p.Faction != pawn.Faction && 
                        ((Pawn)p).health.hediffSet.GetFirstHediffOfDef(PurpleIvyDefOf.PI_MaskingSprayHigh) == null;
            const float distance = 9999f;
            var plants = pawn.Map.listerThings.ThingsOfDef(PurpleIvyDefOf.PI_Nest);
            if (plants.Count == 0)
            {
                plants = pawn.Map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy);
            }
            if (plants.Count > 0)
            {
                victim = (Pawn)GenClosest.ClosestThingReachable(plants.RandomElement().Position,
                    pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly,
                    TraverseMode.PassDoors, false)
                    , distance, Predicate);
            }
            return victim;
        }

        private const float MaxAttackDistance = 40f;

        private const float WaitChance = 0.5f;

        private const int WaitTicks = 90;

        private const int MinMeleeChaseTicks = 420;

        private const int MaxMeleeChaseTicks = 900;

        private int PrevTick = 0;
    }
}

