using System;
using UnityEngine;
using Verse;
using Verse.Sound;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
namespace RimWorld
{
    public class Projectile_Seed : Projectile
    {
        public Thing gun
        {
            get
            {
                return ThingMaker.MakeThing(this.def.building.turretGunDef, null);
            }
        }

        public override void Tick()
        {
            base.Tick();
        }

        private bool IsValidTarget(Thing t)
        {
            Pawn pawn = t as Pawn;
            if (pawn != null)
            {
                if (this.def.projectile.flyOverhead)
                {
                    RoofDef roofDef = this.Map.roofGrid.RoofAt(t.Position);
                    if (roofDef != null && roofDef.isThickRoof)
                    {
                        return false;
                    }
                }
                return !GenAI.MachinesLike(base.Faction, pawn);
            }
            return true;
        }

        protected override void Impact(Thing hitThing)
        {
            if (hitThing == null)
            {

            }
            else
            {
                foreach (IntVec3 current in GenAdj.CellsAdjacent8WayAndInside(hitThing))
                {
                    MoteMaker.ThrowDustPuff(current, this.Map, 2f);

                    Thing t = GenClosest.ClosestThingReachable(hitThing.Position, hitThing.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.ClosestTouch, TraverseParms.For((Pawn)hitThing, Danger.Deadly, TraverseMode.ByPawn), 9999, new Predicate<Thing>(this.IsValidTarget), null, 0, -1, false, RegionType.Set_Passable, false);

                    //Thing t = GenAI.BestAttackTarget(hitThing.Position, this, new Predicate<Thing>(this.IsValidTarget), 2f, 0f, false, false, false, true);

                    Pawn pawn = t as Pawn;
                    pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Wander_Psychotic, null, false, false, null, false);

                    //pawn.thinker.mindState.Sanity.Equals(SanityState.Psychotic);
                }
            }
		}
    }
}
