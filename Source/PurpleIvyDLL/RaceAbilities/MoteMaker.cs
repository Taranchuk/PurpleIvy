using System;
using UnityEngine;
using Verse;

namespace RaceAbilities
{
	public static class MoteMaker
	{
		public static void ThrowAvianMicroSparks(Vector3 loc, Map map)
		{
			if (!GenView.ShouldSpawnMotesAt(loc, map) || map.moteCounter.SaturatedLowPriority)
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Sparks_BlueLightning, null);
			moteThrown.Scale = Rand.Range(5f, 9f);
			moteThrown.rotationRate = Rand.Range(-12f, 12f);
			moteThrown.exactPosition = loc;
			moteThrown.exactPosition -= new Vector3(0.5f, 0f, 0.5f);
			moteThrown.exactPosition += new Vector3(Rand.Value, 0f, Rand.Value);
			moteThrown.SetVelocity((float)Rand.Range(35, 45), 1.2f);
			GenSpawn.Spawn(moteThrown, IntVec3Utility.ToIntVec3(loc), map, 0);
		}

		public static void ThrowAvianLightningGlow(Vector3 loc, Map map, float size)
		{
			if (!GenView.ShouldSpawnMotesAt(loc, map) || map.moteCounter.SaturatedLowPriority)
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.AbilityBlueGlow, null);
			moteThrown.Scale = Rand.Range(6f, 8f) * size;
			moteThrown.rotationRate = Rand.Range(-3f, 3f);
			moteThrown.exactPosition = loc + size * new Vector3(Rand.Value - 0.5f, 0f, Rand.Value - 0.5f);
			moteThrown.SetVelocity((float)Rand.Range(0, 360), 1.2f);
			GenSpawn.Spawn(moteThrown, IntVec3Utility.ToIntVec3(loc), map, 0);
		}

		public static void ThrowAvianSmoke(Vector3 loc, Map map, float size)
		{
			if (!GenView.ShouldSpawnMotesAt(loc, map) || map.moteCounter.SaturatedLowPriority)
			{
				return;
			}
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_BlueSmoke, null);
			moteThrown.Scale = Rand.Range(1.5f, 2.5f) * size;
			moteThrown.rotationRate = Rand.Range(-30f, 30f);
			moteThrown.exactPosition = loc;
			moteThrown.SetVelocity((float)Rand.Range(30, 40), Rand.Range(0.5f, 0.7f));
			GenSpawn.Spawn(moteThrown, IntVec3Utility.ToIntVec3(loc), map, 0);
		}
	}
}
