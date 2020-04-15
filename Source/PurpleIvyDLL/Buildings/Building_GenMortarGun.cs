using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace PurpleIvy
{
    class Building_GenMortarGun : Building_TurretGun
    {
        private int chance;

        public int plantToSpawn = 0;

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode); 
            Random random = new Random();
            chance = random.Next(1, 50);
            Thing weaponDrop = (Thing)ThingMaker.MakeThing(ThingDef.Named("MeleeWeapon_GenMortarTentacle"));
            if(chance == 28)
            {
                GenPlace.TryPlaceThing(weaponDrop, Position, this.Map, ThingPlaceMode.Near);
            }
        }
    }
}

