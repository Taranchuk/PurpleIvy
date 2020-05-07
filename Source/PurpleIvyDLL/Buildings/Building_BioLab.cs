using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace PurpleIvy
{
    public class Building_BioLab : Building_WorkTable
    {
        public Building_BioLab()
        {

        }

        public override void PostMake()
        {
            base.PostMake();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public bool HasJobOnRecipe(Job job, out JobDef jobDef)
        {
            jobDef = null;
            ThingDef def = job.targetQueueB.First().Thing.def;
            if (job.bill.recipe == PurpleIvyDefOf.PI_BiomaterialsStudyRecipe)
            {
                foreach (var data in PurpleIvyData.BioStudy[def])
                {
                    if (data.PrerequisitesCompleted &&
                        !data.IsFinished &&
                        this.Map.listerThings.ThingsOfDef
                        (ThingDef.Named("Techprint_" + data.defName)).Count == 0)
                    {
                        jobDef = PurpleIvyDefOf.PI_BiomaterialsStudy;
                        job.targetB = ThingMaker.MakeThing(ThingDef.Named("Techprint_" + data.defName));
                        return true;
                    }
                }
            }
            else
            {
                jobDef = JobDefOf.DoBill;
                return true;
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}

