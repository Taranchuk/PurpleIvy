using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PurpleIvy
{
    public class AlienProjectile : Projectile
    {
        protected override void Impact(Thing hitThing)
        {
            Map map = base.Map;
            base.Impact(hitThing);
            BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = 
                new BattleLogEntry_RangedImpact(this.launcher, hitThing, 
                this.intendedTarget.Thing, ThingDef.Named("Gun_Autopistol"), this.def, this.targetCoverDef);
            Find.BattleLog.Add(battleLogEntry_RangedImpact);
            bool flag = hitThing != null;
            if (flag)
            {
                DamageDef damageDef = this.def.projectile.damageDef;
                float num = (float)base.DamageAmount;
                float armorPenetration = base.ArmorPenetration;
                float y = this.ExactRotation.eulerAngles.y;
                Thing launcher = this.launcher;
                ThingDef equipmentDef = this.equipmentDef;
                DamageInfo damageInfo = new DamageInfo(damageDef, num, armorPenetration, y, launcher, null, null, 0, this.intendedTarget.Thing);;
                hitThing.TakeDamage(damageInfo).AssociateWithLog(battleLogEntry_RangedImpact);
                Pawn pawn = hitThing as Pawn;
                bool flag2 = pawn != null && pawn.stances != null && 
                    pawn.BodySize <= this.def.projectile.StoppingPower + 0.001f;
                if (flag2)
                {
                    pawn.stances.StaggerFor(95);
                }
            }
            else
            {
                SoundStarter.PlayOneShot(SoundDefOf.BulletImpact_Ground, new TargetInfo(base.Position, map, false));
                Log.Message("2 MoteMaker");

                MoteMaker.MakeStaticMote(this.ExactPosition, map, ThingDefOf.Mote_ShotHit_Dirt, 1f);
                bool takeSplashes = GridsUtility.GetTerrain(base.Position, map).takeSplashes;
                if (takeSplashes)
                {
                    Log.Message("3 MoteMaker");

                    MoteMaker.MakeWaterSplash(this.ExactPosition, map, Mathf.Sqrt((float)base.DamageAmount) * 1f, 4f);
                }
            }
        }
    }
}

