using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    public class StickyGoo : ThingWithComps, IThingHolder, IOpenable
    {
        public StickyGoo()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
        }

        public bool HasAnyContents
        {
            get
            {
                return this.innerContainer.Count > 0;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            return base.GetGizmos();
        }
        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (this.innerContainer != null && this.innerContainer.Count > 0 && (this.ContainedThing is Pawn || this.ContainedThing is Corpse))
            {
                this.innerContainer[0].DrawAt(drawLoc, flip);
                base.DrawAt(drawLoc, flip);
                //Graphics.DrawMesh(MeshPool.plane10, drawLoc, GenMath.ToQuat(0f), this.def.DrawMatSingle, 0);
            }
            else
            {
                base.DrawAt(drawLoc, flip);
            }
        }

        public Thing ContainedThing
        {
            get
            {
                if (this.innerContainer.Count != 0)
                {
                    return this.innerContainer[0];
                }
                return null;
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            foreach (FloatMenuOption floatMenuOption2 in base.GetFloatMenuOptions(myPawn))
            {
                yield return floatMenuOption2;
            }
            IEnumerator<FloatMenuOption> enumerator = null;
            if (this.innerContainer.Count == 1)
            {
                    JobDef jobDef = PurpleIvyDefOf.PI_SavePawnFromStickyGoo;
                    string label = "SavePawnFromStickyGoo".Translate();
                    Action action = delegate ()
                    {
                        Job job = JobMaker.MakeJob(jobDef, this.ContainedThing, this);
                        job.count = 1;
                        myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    };
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn, this, "ReservedBy");
            }
            yield break;
            yield break;
        }
        public override string GetInspectString()
        {
            if (this.innerContainer != null && this.innerContainer.Count > 0 && this.innerContainer[0] is Pawn pawn)
            {
                return this.innerContainer.ContentsString;
            }
            else
            {
                return base.GetInspectString();
            }
        }

        public bool CanOpen
        {
            get
            {
                return this.HasAnyContents;
            }
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public override void TickRare()
        {
            base.TickRare();
            this.innerContainer.ThingOwnerTickRare(true);
        }

        public override void Tick()
        {
            base.Tick();
            this.innerContainer.ThingOwnerTick(true);
        }

        public virtual void Open()
        {
            if (!this.HasAnyContents)
            {
                return;
            }
            this.EjectContents();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
            Scribe_Values.Look<bool>(ref this.contentsKnown, "contentsKnown", false, false);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (base.Faction != null && base.Faction.IsPlayer)
            {
                this.contentsKnown = true;
            }
        }

        public virtual bool Accepts(Thing thing)
        {
            return this.innerContainer.CanAcceptAnyOf(thing, true);
        }

        public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (!this.Accepts(thing))
            {
                return false;
            }
            bool flag;
            if (thing.holdingOwner != null)
            {
                thing.holdingOwner.TryTransferToContainer(thing, this.innerContainer, thing.stackCount, true);
                flag = true;
            }
            else
            {
                flag = this.innerContainer.TryAdd(thing, true);
            }
            if (flag)
            {
                if (thing.Faction != null && thing.Faction.IsPlayer)
                {
                    this.contentsKnown = true;
                }
                return true;
            }
            return false;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (this.innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize))
            {
                if (mode != DestroyMode.Deconstruct)
                {
                    List<Pawn> list = new List<Pawn>();
                    foreach (Thing thing in ((IEnumerable<Thing>)this.innerContainer))
                    {
                        Pawn pawn = thing as Pawn;
                        if (pawn != null)
                        {
                            list.Add(pawn);
                        }
                    }
                    foreach (Pawn p in list)
                    {
                        HealthUtility.DamageUntilDowned(p, true);
                    }
                }
                this.EjectContents();
            }
            this.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
            base.Destroy(mode);
        }

        public virtual void EjectContents()
        {
            this.innerContainer.TryDropAll(this.InteractionCell, base.Map, ThingPlaceMode.Direct, null, null);
            this.contentsKnown = true;
        }

        public ThingOwner innerContainer;

        public bool contentsKnown;
    }
}

