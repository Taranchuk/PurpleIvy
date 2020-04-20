using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    public class Building_СontainmentBreach : Building_WorkTable, IThingHolder
    {

        public Building_СontainmentBreach()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
        }

        public override void PostMake()
        {
            base.PostMake();
            this.maxNumAliens = this.def.GetModExtension<DefModExtension_СontainmentBreach>().maxNumAliens;
            this.blackoutProtection = this.def.GetModExtension<DefModExtension_СontainmentBreach>().blackoutProtection;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.maxNumAliens = this.def.GetModExtension<DefModExtension_СontainmentBreach>().maxNumAliens;
            this.blackoutProtection = this.def.GetModExtension<DefModExtension_СontainmentBreach>().blackoutProtection;
        }

        public override string GetInspectString()
        {
            string aliens = "";
            if (Aliens.Count > 0)
            {
                foreach (var alien in Aliens)
                {
                    aliens += alien + "\n";
                }
            }
            return aliens.TrimEndNewlines() + base.GetInspectString();
        }
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption fmo in base.GetFloatMenuOptions(selPawn))
            {
                yield return fmo;
            }
            if (this.Aliens != null && this.Aliens.Count > 0)
            {
                yield return new FloatMenuOption(Translator.Translate("ConductResearch"), delegate ()
                {
                    if (selPawn != null)
                    {
                        Job job = JobMaker.MakeJob(PurpleIvyDefOf.PI_ConductResearchOnAliens, this);
                        selPawn.jobs.TryTakeOrderedJob(job, 0);
                    }
                }, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else
            {
                yield return new FloatMenuOption("NoAliensToConductResearch".Translate(), null,
                    MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            yield break;
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public List<Pawn> Aliens
        {
            get
            {
                List<Pawn> Aliens = new List<Pawn>();
                for (int i = 0; i < this.innerContainer.Count; i++)
                {
                    Aliens.Add((Pawn)this.innerContainer[i]);
                }
                return Aliens;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
            Scribe_Collections.Look<Pawn, int>(ref this.RecoveryBloodData, "RecoveryBloodData", 
                LookMode.Reference, LookMode.Value, ref this.RecoveryBloodDataKeys, ref this.RecoveryBloodDataValues);
        }

        public Dictionary<Pawn, int> RecoveryBloodData = new Dictionary<Pawn, int>();

        public List<Pawn> RecoveryBloodDataKeys = new List<Pawn>();

        public List<int> RecoveryBloodDataValues = new List<int>();

        public ThingOwner innerContainer;

        public int maxNumAliens;

        public bool blackoutProtection;
    }
}

