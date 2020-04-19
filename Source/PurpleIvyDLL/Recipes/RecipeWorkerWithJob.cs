using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    internal class RecipeWorkerWithJob : RecipeWorker
    {
        public JobDef AlienStudy
        {
            get
            {
                return PurpleIvyDefOf.PI_ConductResearchOnAliens;
            }
        }

        public JobDef DrawAlienBlood
        {
            get
            {
                return PurpleIvyDefOf.PI_DrawAlienBlood;
            }
        }

        //public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        //{
        //    //var building = (Building_СontainmentBreach)bill.billStack.billGiver;
        //    //Log.Message(building.Label);
        //    //IntVec3 position = billDoer.Position;
        //    //Map map = billDoer.Map;
        //    GenSpawn.Spawn(PurpleIvyDefOf.PI_AlphaBlood, ingredient.Position, map, 0);
        //    base.ConsumeIngredient(ingredient, recipe, map);
        //}
    }
}

