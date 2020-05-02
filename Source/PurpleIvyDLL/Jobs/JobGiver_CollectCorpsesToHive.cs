using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    public class JobGiver_CollectCorpsesToHive : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Predicate<Thing> validator = delegate (Thing t)
            {
                List<Thing> list = pawn.Map.thingGrid.ThingsListAt(t.Position);
                return !(list.Count > 0 && list.OfType<Plant>().Any(x =>
                x.def == PurpleIvyDefOf.PurpleIvy || x.def == PurpleIvyDefOf.PI_Nest
                || x.def == PurpleIvyDefOf.PlantVenomousToothwort));
            };

            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.Corpse), PathEndMode.ClosestTouch,
                TraverseParms.For(pawn, Danger.None, TraverseMode.NoPassClosedDoors, false), 50f, validator, null);
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
                    Log.Message(pawn + " hauls " + thing + " - " + plantToHaul.Position + " position");
                    Job job = JobMaker.MakeJob(PurpleIvyDefOf.PI_HaulToCell, thing, plantToHaul.Position);
                    job.attackDoorIfTargetLost = true;
                    //Job job = JobMaker.MakeJob(JobDefOf.HaulToCell, thing, plant.Position);

                    // Job job = HaulAIUtility.HaulToCellStorageJob(pawn, thing, plant.Position, false);
                    if (job != null && job.TryMakePreToilReservations(pawn, false))
                    {
                        ReservationUtility.Reserve(pawn, thing, job);
                        return job;
                    }
                }

            }
            return null;
        }
    }
}

