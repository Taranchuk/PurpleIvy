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
    public class AlienQueen : Alien
    {
        private int spawnticks = 200;
        public override void PostMake()
        {
            base.PostMake();
        }
        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (base.Dead)
            {
                Log.Message("QUEEN DEAD");
                AlienInfectionHediff hediff = (AlienInfectionHediff)HediffMaker.MakeHediff
                (PurpleIvyDefOf.PI_AlienInfection, this);
                hediff.instigator = PawnKindDef.Named("Genny_ParasiteOmega");
                var range = new IntRange(30, 50);
                hediff.maxNumberOfCreatures = range.RandomInRange;
                hediff.ageTicks = new IntRange(40000, 50000).RandomInRange;
                this.health.AddHediff(hediff);
                var corpse = (Corpse)this.ParentHolder;
                PurpleIvyUtils.SpawnNests(this);
            }
            else if (!(dinfo.Instigator is MeteorIncoming))
            {
                var hediff = this.health.hediffSet.hediffs
                    .FirstOrDefault((Hediff h) => h.def == PurpleIvyDefOf.PI_CrashlandedDowned);
                if (hediff != null)
                {
                    this.health.hediffSet.hediffs.Remove(hediff);
                    RestUtility.Awake(this);
                }
            }
            base.PostApplyDamage(dinfo, totalDamageDealt);
        }
        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame > this.recoveryTick)
            {
                var hediff = this.health.hediffSet.hediffs
                .FirstOrDefault((Hediff h) => h.def == PurpleIvyDefOf.PI_CrashlandedDowned);
                if (hediff != null)
                {
                    this.health.hediffSet.hediffs.Remove(hediff);
                    RestUtility.Awake(this);
                }
            }
            spawnticks--;
            if (spawnticks <= 0)
            {
                PurpleIvyUtils.SpawnNests(this);
                spawnticks = new IntRange(15000, 30000).RandomInRange;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.recoveryTick, "recoveryTick", 0);
        }

        public int recoveryTick = 0;
    }
}

