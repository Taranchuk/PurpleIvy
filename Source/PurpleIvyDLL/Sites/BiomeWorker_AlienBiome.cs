using System;
using RimWorld;
using RimWorld.Planet;

namespace PurpleIvy
{
    public class BiomeWorker_AlienBiome : BiomeWorker
    {
        public override float GetScore(Tile tile, int tileID)
        {
            return -100f;
        }
    }
}
