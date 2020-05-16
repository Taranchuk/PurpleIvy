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
        public bool canJump = false;
        public bool canHaul = false;
        public bool canFight = false;
        public bool canSpawnNests = false;
        public bool canGuard = false;
        public LocalTargetInfo focus = null;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (this.Faction != PurpleIvyData.AlienFaction)
            {
                this.SetFaction(PurpleIvyData.AlienFaction);
            }
        }
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

        public void SetFocus()
        {
            var plants = this.Map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy);
            LocalTargetInfo choosen = null;
            if (plants.Count > 0)
            {
                choosen = plants.RandomElement();
            }
            else
            {
                choosen = new LocalTargetInfo(this.Position);
            }
            PawnDuty duty = new PawnDuty(DutyDefOf.DefendHiveAggressively);
            this.mindState.duty = duty;
            this.mindState.duty.focus = choosen;
            this.focus = choosen;
            Log.Message("Set focus to " + choosen, true);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.lastAttacked, "lastAttacked", 0);
            Scribe_Values.Look<bool>(ref this.canJump, "canJump", false);
            Scribe_Values.Look<bool>(ref this.canHaul, "canHaul", false);
            Scribe_Values.Look<bool>(ref this.canFight, "canFight", false);
            Scribe_Values.Look<bool>(ref this.canSpawnNests, "canSpawnNests", false);
            Scribe_Values.Look<bool>(ref this.canGuard, "canGuard", false);
            if (this.canGuard)
            {
                Scribe_TargetInfo.Look(ref this.focus, "focus");
                PawnDuty duty = new PawnDuty(DutyDefOf.DefendHiveAggressively);
                this.mindState.duty = duty;
                this.mindState.duty.focus = focus;
            }
        }
    }
}

