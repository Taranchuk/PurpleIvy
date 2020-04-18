using System;
using RimWorld;
using RimWorld.Planet;
using Verse;

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

