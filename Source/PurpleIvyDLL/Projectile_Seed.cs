using System;
using UnityEngine;
using Verse;
using Verse.Sound;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using RimWorld;

namespace PurpleIvy
{
    public class Projectile_Seed : Projectile
    {
        public Thing Gun => ThingMaker.MakeThing(this.def.building.turretGunDef, null);

        public override void Tick()
        {
            base.Tick();
        }

        private bool IsValidTarget(Thing t)
        {
            if (!(t is Pawn pawn)) return true;
            if (!this.def.projectile.flyOverhead) return !GenAI.MachinesLike(base.Faction, pawn);
            var roofDef = this.Map.roofGrid.RoofAt(t.Position);
            if (roofDef != null && roofDef.isThickRoof)
            {
                return false;
            }
            return !GenAI.MachinesLike(base.Faction, pawn);
        }

        protected override void Impact(Thing hitThing)
        {
            if (hitThing == null)
            {

            }
            else
            {
                foreach (IntVec3 current in hitThing.CellsAdjacent8WayAndInside())
                {
                    MoteMaker.ThrowDustPuff(current, this.Map, 2f);

                    var t = GenClosest.ClosestThingReachable(hitThing.Position, hitThing.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.ClosestTouch, TraverseParms.For((Pawn)hitThing, Danger.Deadly, TraverseMode.ByPawn), 9999, new Predicate<Thing>(this.IsValidTarget), null, 0, -1, false, RegionType.Set_Passable, false);

                    //Thing t = GenAI.BestAttackTarget(hitThing.Position, this, new Predicate<Thing>(this.IsValidTarget), 2f, 0f, false, false, false, true);

                    var pawn = t as Pawn;
                    pawn?.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Wander_Psychotic, null,
                        false, false, null, false);

                    //pawn.thinker.mindState.Sanity.Equals(SanityState.Psychotic);
                }
            }
		}
    }
}

