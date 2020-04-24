using System;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace PurpleIvy
{
    public class GenStep_AlienLairs : GenStep_Scatterer
    {
        public override int SeedPart
        {
            get
            {
                return 1806208471;
            }
        }

        protected override bool CanScatterAt(IntVec3 c, Map map)
        {
            if (!base.CanScatterAt(c, map))
            {
                return false;
            }
            if (!c.Standable(map))
            {
                return false;
            }
            if (c.Roofed(map))
            {
                return false;
            }
            if (!map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)))
            {
                return false;
            }
            int min = GenStep_AlienLairs.SettlementSizeRange.min;
            CellRect cellRect = new CellRect(c.x - min / 2, c.z - min / 2, min, min);
            return cellRect.FullyContainedWithin(new CellRect(0, 0, map.Size.x, map.Size.z));
        }

        protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
        {
            int randomInRange = GenStep_AlienLairs.SettlementSizeRange.RandomInRange;
            int randomInRange2 = GenStep_AlienLairs.SettlementSizeRange.RandomInRange;
            CellRect rect = new CellRect(c.x - randomInRange / 2, c.z - randomInRange2 / 2, randomInRange, randomInRange2);
            Faction faction;
            if (map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer)
            {
                faction = Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
            }
            else
            {
                faction = map.ParentFaction;
            }
            rect.ClipInsideMap(map);
            ResolveParams resolveParams = default(ResolveParams);
            resolveParams.rect = rect;
            resolveParams.faction = faction;
            BaseGen.globalSettings.map = map;
            BaseGen.globalSettings.minBuildings = 1;
            BaseGen.globalSettings.minBarracks = 1;
            BaseGen.symbolStack.Push("settlement", resolveParams, null);
            BaseGen.Generate();
        }

        private static readonly IntRange SettlementSizeRange = new IntRange(34, 38);
    }
}
