using System;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace PurpleIvy
{
	public class SymbolResolver_CrashedAlienShip : SymbolResolver
	{
		public override bool CanResolve(ResolveParams rp)
		{
			return base.CanResolve(rp);
		}

		public override void Resolve(ResolveParams rp)
		{
			if (rp.wallStuff == null)
			{
				rp.wallStuff = ThingDefOf.Plasteel;
			}
			if (rp.floorDef == null)
			{
				rp.floorDef = TerrainDef.Named("SilverTile");
			}
            ResolveParams resolveParams = rp;
            // battery room
            resolveParams.rect = new CellRect(rp.rect.minX, rp.rect.maxX / 2, 5, 13);

            BaseGen.symbolStack.Push("emptyRoom", resolveParams, null);

            // half way
            resolveParams.rect = new CellRect(rp.rect.minX + 4, rp.rect.maxX / 2 + 4, 24, 5);
            BaseGen.symbolStack.Push("emptyRoom", resolveParams, null);

            //side room
            resolveParams.rect = new CellRect(rp.rect.minX + 4, rp.rect.maxX / 2, 8, 5);
            BaseGen.symbolStack.Push("emptyRoom", resolveParams, null);

            //generators
            resolveParams.rect = new CellRect(rp.rect.minX + 4, rp.rect.maxX / 2 + 8, 8, 5);
            BaseGen.symbolStack.Push("emptyRoom", resolveParams, null);

            
            resolveParams.rect = new CellRect(rp.rect.minX + 10, rp.rect.maxX / 2, 18, 5);
            BaseGen.symbolStack.Push("emptyRoom", resolveParams, null);

            resolveParams.rect = new CellRect(rp.rect.minX + 10, rp.rect.maxX / 2 + 8, 18, 5);
            BaseGen.symbolStack.Push("emptyRoom", resolveParams, null);



        }
    }
}

