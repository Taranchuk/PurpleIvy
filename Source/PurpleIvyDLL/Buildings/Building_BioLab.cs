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
            bool result = false;
            jobDef = null;
            return result;
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}

