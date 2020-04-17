using System;
using RimWorld;
using RimWorld.BaseGen;
using Verse;
using Verse.AI.Group;

namespace GenerationWorker
{
	public class SymbolResolver_AdvancedGeneration : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			Faction faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction(false, false, true, 0);
			int num = 0;
			int? edgeDefenseWidth = rp.edgeDefenseWidth;
			if (edgeDefenseWidth != null)
			{
				num = rp.edgeDefenseWidth.Value;
			}
			else if (rp.rect.Width >= 40 && rp.rect.Height >= 20 && (faction.def.techLevel >= TechLevel.Animal || Rand.Bool))
			{
				num = ((!Rand.Bool) ? 4 : 2);
			}
			float num2 = (float)rp.rect.Area / 144f * 0.17f;
			BaseGen.globalSettings.minEmptyNodes = ((num2 >= 1f) ? GenMath.RoundRandom(num2) : 0);
			Lord singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, rp.rect.CenterCell), map, null);
			TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false);
			ResolveParams resolveParams = rp;
			resolveParams.rect = rp.rect;
			resolveParams.faction = faction;
			resolveParams.singlePawnLord = singlePawnLord;
			resolveParams.pawnGroupKindDef = (rp.pawnGroupKindDef ?? PawnGroupKindDefOf.Settlement);
			resolveParams.singlePawnSpawnCellExtraPredicate = (rp.singlePawnSpawnCellExtraPredicate ?? ((IntVec3 x) => map.reachability.CanReachMapEdge(x, traverseParms)));
			if (resolveParams.pawnGroupMakerParams == null)
			{
				resolveParams.pawnGroupMakerParams = new PawnGroupMakerParms();
				resolveParams.pawnGroupMakerParams.tile = map.Tile;
				resolveParams.pawnGroupMakerParams.faction = faction;
				PawnGroupMakerParms pawnGroupMakerParams = resolveParams.pawnGroupMakerParams;
				float? settlementPawnGroupPoints = rp.settlementPawnGroupPoints;
				pawnGroupMakerParams.points = ((settlementPawnGroupPoints == null) ? SymbolResolver_AdvancedGeneration.DefaultPawnsPoints.RandomInRange : settlementPawnGroupPoints.Value);
				resolveParams.pawnGroupMakerParams.inhabitants = true;
				resolveParams.pawnGroupMakerParams.seed = rp.settlementPawnGroupSeed;
			}
			BaseGen.symbolStack.Push("hives", resolveParams);
			BaseGen.symbolStack.Push("hives", rp);
			if (faction.def.techLevel >= TechLevel.Animal)
			{
				int num3 = Rand.Chance(1f) ? GenMath.RoundRandom((float)rp.rect.Area / 400f) : 0;
				for (int i = 0; i < num3; i++)
				{
					ResolveParams resolveParams2 = rp;
					resolveParams2.faction = faction;
					BaseGen.symbolStack.Push("hives", resolveParams2);
				}
				ResolveParams resolveParams3 = rp;
				resolveParams3.hivesCount = new int?(Rand.RangeInclusive(4, 5));
				BaseGen.symbolStack.Push("hives", resolveParams3);
			}
			if (num > 0)
			{
				ResolveParams resolveParams4 = rp;
				resolveParams4.faction = faction;
				resolveParams4.edgeDefenseWidth = new int?(num);
				BaseGen.symbolStack.Push("edgeDefense", resolveParams4);
			}
			ResolveParams resolveParams5 = rp;
			resolveParams5.rect = rp.rect.ContractedBy(num);
			resolveParams5.faction = faction;
			BaseGen.symbolStack.Push("ensureCanReachMapEdge", resolveParams5);
			ResolveParams resolveParams6 = rp;
			resolveParams6.rect = rp.rect.ContractedBy(num);
			resolveParams6.faction = faction;
			BaseGen.symbolStack.Push("basePart_outdoors", resolveParams6);
			ResolveParams resolveParams7 = rp;
			resolveParams7.floorDef = TerrainDefOf.Bridge;
			bool? floorOnlyIfTerrainSupports = rp.floorOnlyIfTerrainSupports;
			resolveParams7.floorOnlyIfTerrainSupports = new bool?(floorOnlyIfTerrainSupports == null || floorOnlyIfTerrainSupports.Value);
			BaseGen.symbolStack.Push("floor", resolveParams7);
		}

		public static readonly FloatRange DefaultPawnsPoints = new FloatRange(1150f, 1600f);
	}
}

