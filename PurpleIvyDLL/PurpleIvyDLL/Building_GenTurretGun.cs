﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
    class Building_GenTurretGun : Building_TurretGun
    {
        private int chance;
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            Random random = new Random();
            chance = random.Next(1, 50);
            Thing weaponDrop = (Thing)ThingMaker.MakeThing(ThingDef.Named("MeleeWeapon_GenTurretTentacle"));
            if(chance == 25)
            {
                GenPlace.TryPlaceThing(weaponDrop, Position, this.Map, ThingPlaceMode.Near);
            }
        }
    }
}
