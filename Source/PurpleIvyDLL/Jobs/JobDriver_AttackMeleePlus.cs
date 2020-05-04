using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    public class JobDriver_AttackMeleePlus : JobDriver
    {
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.numMeleeAttacksMade, "numMeleeAttacksMade", 0, false);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_General.DoAtomic(delegate
            {
                if (this.job.targetA.Thing is Pawn pawn1 && pawn1.Downed && this.pawn.mindState.duty != null && this.pawn.mindState.duty.attackDownedIfStarving && this.pawn.Starving())
                {
                    this.job.killIncappedTarget = true;
                }
            });
            yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
            yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, delegate
            {
                var thing = this.job.GetTarget(TargetIndex.A).Thing;
                if (this.pawn.meleeVerbs.TryMeleeAttack(thing, this.job.verbToUse, false))
                {
                    if (this.pawn.CurJob == null || this.pawn.jobs.curDriver != this)
                    {
                        return;
                    }
                    this.numMeleeAttacksMade++;
                    if (thing is Pawn && 1f >= Rand.Range(0f, 100f))
                    {
                        var victim = (Pawn)thing;
                        if (!victim.RaceProps.IsMechanoid && PurpleIvyData.maxNumberOfCreatures.ContainsKey(this.pawn.def.defName) &&
                        thing.TryGetComp<AlienInfection>() == null)
                        {
                            AlienInfectionHediff hediff = (AlienInfectionHediff)HediffMaker.MakeHediff
                            (PurpleIvyDefOf.PI_AlienInfection, victim);
                            hediff.instigator = this.pawn.kindDef;
                            victim.health.AddHediff(hediff);
                        }
                    }
                    if (this.numMeleeAttacksMade < this.job.maxNumMeleeAttacks) return;
                    base.EndJobWith(JobCondition.Succeeded);
                }
                return;
            }).FailOnDespawnedOrNull(TargetIndex.A);
            yield break;
        }

        public override void Notify_PatherFailed()
        {
            if (this.job.attackDoorIfTargetLost)
            {
                Thing thing;
                using (var pawnPath = base.Map.pathFinder.FindPath(this.pawn.Position, base.TargetA.Cell, TraverseParms.For(this.pawn, Danger.Deadly, TraverseMode.PassDoors, false), PathEndMode.OnCell))
                {
                    if (!pawnPath.Found)
                    {
                        return;
                    }
                    thing = pawnPath.FirstBlockingBuilding(out var position, this.pawn);
                }
                if (thing != null)
                {
                    var position = thing.Position;
                    if (position.InHorDistOf(this.pawn.Position, 6f))
                    {
                        this.job.targetA = thing;
                        this.job.maxNumMeleeAttacks = Rand.RangeInclusive(2, 5);
                        this.job.expiryInterval = Rand.Range(2000, 4000);
                        return;
                    }
                }
            }
            base.Notify_PatherFailed();
        }

        public override bool IsContinuation(Job j)
        {
            return this.job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
        }

        private int numMeleeAttacksMade;
    }
}

