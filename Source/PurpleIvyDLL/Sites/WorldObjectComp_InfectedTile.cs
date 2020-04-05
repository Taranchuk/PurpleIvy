using System;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PurpleIvy
{
    public class WorldObjectComp_InfectedTile : WorldObjectComp
    {
        public bool Active
        {
            get
            {
                return this.active;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.active)
            {
                MapParent mapParent = this.parent as MapParent;
                if (mapParent != null && mapParent.Map != null)
                {
                    if (mapParent.Map.listerThings.ThingsOfDef(PurpleIvyDefOf.PurpleIvy).Count <= 0)
                    {
                         this.StopQuest();
                    }
                }
            }
        }

        public override void PostMapGenerate()
        {
            base.PostMapGenerate();
            Log.Message("Spawning plants:");
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.active, "active", false, false);
            Scribe_Values.Look<int>(ref this.worldTileAffected, "worldTileAffected", 0, false);
            Scribe_Defs.Look<GameConditionDef>(ref this.gameConditionCaused, "gameConditionCaused");
        }

        public void StartQuest()
        {
            this.active = true;
        }

        public void StopQuest()
        {
            this.active = false;
            Settlement settlement = Find.World.worldObjects.SettlementAt(this.worldTileAffected);
            bool flag = settlement != null && settlement.HasMap;
            if (flag)
            {
                GameConditionManager gameConditionManager = settlement.Map.gameConditionManager;
                if (gameConditionManager.ConditionIsActive(this.gameConditionCaused))
                {
                    gameConditionManager.ActiveConditions.Remove(gameConditionManager.GetActiveCondition(this.gameConditionCaused));
                }
            }
        }

        public override void PostPostRemove()
        {
            this.StopQuest();
            base.PostPostRemove();
        }

        private bool active;

        public int worldTileAffected;

        public GameConditionDef gameConditionCaused;


    }
}
