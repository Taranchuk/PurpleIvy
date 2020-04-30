using System;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace PurpleIvy
{
    public class GenStep_CrashedAlienShip : GenStep
    {
        public override int SeedPart
        {
            get
            {
                return 12346547;
            }
        }
        public override void Generate(Map map, GenStepParams parms)
        {
            int num = map.Size.x / 5;
            int num2 = 8 * map.Size.x / 5;
            int num3 = map.Size.z / 5;
            int num4 = 8 * map.Size.z / 5;
            this.adventureRegion = new CellRect(num, num3, num2, num4);
            this.adventureRegion.ClipInsideMap(map);
            IntVec3 playerStartSpot;
            CellFinder.TryFindRandomEdgeCellWith((IntVec3 v) =>
            GenGrid.Standable(v, map), map, 0f, out playerStartSpot);
            MapGenerator.PlayerStartSpot = playerStartSpot;
            this.baseResolveParams = default(ResolveParams);
            this.baseResolveParams.rect = this.adventureRegion;
            BaseGen.globalSettings.map = map;
            BaseGen.globalSettings.minBuildings = 0;
            BaseGen.globalSettings.minBarracks = 0;
            BaseGen.symbolStack.Push("crashedShip", this.baseResolveParams, null);
            BaseGen.Generate();
        }

        protected CellRect adventureRegion;

        protected ResolveParams baseResolveParams;
    }
}

