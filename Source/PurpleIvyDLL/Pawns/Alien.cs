using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using System.Linq;
using Verse.AI;

namespace PurpleIvy
{
    public class Alien : Pawn
    {
        public int lastAttacked = 0;
        public Pawn lastInstigator = null;
        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (!base.Dead)
            {
                if (dinfo.Instigator is Pawn pawn)
                {
                    lastAttacked = Find.TickManager.TicksGame;
                    lastInstigator = pawn;
                }
            }
            base.PostApplyDamage(dinfo, totalDamageDealt);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.lastAttacked, "lastAttacked", 0);
        }
    }
}

