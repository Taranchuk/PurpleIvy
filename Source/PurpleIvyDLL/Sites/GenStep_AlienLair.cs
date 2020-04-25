using System;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace PurpleIvy
{
    public class GenStep_AlienLair : GenStep_Scatterer
    {
        public override int SeedPart
        {
            get
            {
                return 1806208513;
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
            int min = GenStep_AlienLair.SettlementSizeRange.min;
            CellRect cellRect = new CellRect(c.x - min / 2, c.z - min / 2, min, min);
            return cellRect.FullyContainedWithin(new CellRect(0, 0, map.Size.x, map.Size.z));
        }

        protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
        {
            int randomInRange = SettlementSizeRange.RandomInRange;
            int randomInRange2 = SettlementSizeRange.RandomInRange;
            CellRect rect = new CellRect(c.x - randomInRange / 2, c.z - randomInRange2 / 2, randomInRange, randomInRange2);
            rect.ClipInsideMap(map);
            Faction faction = PurpleIvyData.AlienFaction;
            ResolveParams rp = default(ResolveParams);
            rp.rect = rect;
            TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false);
            ResolveParams resolveParams = rp;
            resolveParams.rect = rp.rect;
            resolveParams.faction = faction;
            resolveParams.pawnGroupKindDef = (rp.pawnGroupKindDef ?? PawnGroupKindDefOf.Settlement);
            resolveParams.singlePawnSpawnCellExtraPredicate = (rp.singlePawnSpawnCellExtraPredicate ?? ((IntVec3 x) => map.reachability.CanReachMapEdge(x, traverseParms)));
            if (resolveParams.pawnGroupMakerParams == null)
            {
                resolveParams.pawnGroupMakerParams = new PawnGroupMakerParms();
                resolveParams.pawnGroupMakerParams.tile = map.Tile;
                resolveParams.pawnGroupMakerParams.faction = faction;
                resolveParams.pawnGroupMakerParams.points = (rp.settlementPawnGroupPoints ?? SymbolResolver_Settlement.DefaultPawnsPoints.RandomInRange);
                resolveParams.pawnGroupMakerParams.inhabitants = true;
                resolveParams.pawnGroupMakerParams.seed = rp.settlementPawnGroupSeed;
            }
            BaseGen.symbolStack.Push("pawnGroup", resolveParams, null);
            BaseGen.globalSettings.map = map;
            BaseGen.globalSettings.minBuildings = 0;
            BaseGen.globalSettings.minBarracks = 0;
            BaseGen.Generate();
        }

        private static readonly IntRange SettlementSizeRange = new IntRange(34, 38);
    }
}

