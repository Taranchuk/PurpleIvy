using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RaceAbilities
{
	public static class BlueFireUtility
	{
		public static bool CanEverAttachFire(this Thing t)
		{
			return !t.Destroyed && t.FlammableNow && t.def.category == ThingCategory.Pawn && ThingCompUtility.TryGetComp<CompAttachBase>(t) != null;
		}

		public static float ChanceToStartFireIn(IntVec3 c, Map map)
		{
			List<Thing> thingList = GridsUtility.GetThingList(c, map);
			float num = (!c.TerrainFlammableNow(map)) ? 0f : StatExtension.GetStatValueAbstract(GridsUtility.GetTerrain(c, map), StatDefOf.Flammability, null);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing is Fire)
				{
					return 0f;
				}
				if (thing.def.category != ThingCategory.Pawn && thingList[i].FlammableNow)
				{
					num = Mathf.Max(num, StatExtension.GetStatValue(thing, StatDefOf.Flammability, true));
				}
			}
			if (num > 0f)
			{
				Building edifice = GridsUtility.GetEdifice(c, map);
				if (edifice != null && edifice.def.passability == Traversability.Impassable && GenAdj.OccupiedRect(edifice).ContractedBy(1).Contains(c))
				{
					return 0f;
				}
				List<Thing> thingList2 = GridsUtility.GetThingList(c, map);
				for (int j = 0; j < thingList2.Count; j++)
				{
					if (thingList2[j].def.category == ThingCategory.Filth && !thingList2[j].def.filth.allowsFire)
					{
						return 0f;
					}
				}
			}
			return num;
		}

		public static bool TryStartFireIn(IntVec3 c, Map map, float fireSize)
		{
			if (BlueFireUtility.ChanceToStartFireIn(c, map) <= 0f)
			{
				return false;
			}
			Fire fire = (Fire)ThingMaker.MakeThing(ThingDefOf.BlueFire, null);
			fire.fireSize = fireSize;
			GenSpawn.Spawn(fire, c, map, Rot4.North, 0, false);
			return true;
		}

		public static void TryAttachFire(this Thing t, float fireSize)
		{
			if (t.Map == null)
			{
				return;
			}
			if (!t.Spawned)
			{
				return;
			}
			if (!t.CanEverAttachFire())
			{
				return;
			}
			if (AttachmentUtility.HasAttachment(t, ThingDefOf.BlueFire))
			{
				return;
			}
			Fire fire = (Fire)ThingMaker.MakeThing(ThingDefOf.BlueFire, null);
			fire.fireSize = fireSize;
			fire.AttachTo(t);
			GenSpawn.Spawn(fire, t.Position, t.Map, Rot4.North, 0, false);
			Pawn pawn = t as Pawn;
			if (pawn != null)
			{
				pawn.jobs.StopAll(false);
				pawn.records.Increment(RecordDefOf.TimesOnFire);
			}
		}

		public static bool IsBurning(this TargetInfo t)
		{
			if (t.HasThing)
			{
				return t.Thing.IsBurning();
			}
			return t.Cell.ContainsStaticFire(t.Map);
		}

		public static bool IsBurning(this Thing t)
		{
			if (t.Destroyed || !t.Spawned)
			{
				return false;
			}
			if (!(t.def.size == IntVec2.One))
			{
				CellRect.CellRectIterator iterator = GenAdj.OccupiedRect(t).GetIterator();
				while (!iterator.Done())
				{
					if (iterator.Current.ContainsStaticFire(t.Map))
					{
						return true;
					}
					iterator.MoveNext();
				}
				return false;
			}
			if (t is Pawn)
			{
				return AttachmentUtility.HasAttachment(t, ThingDefOf.BlueFire);
			}
			return t.Position.ContainsStaticFire(t.Map);
		}

		public static bool ContainsStaticFire(this IntVec3 c, Map map)
		{
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Fire fire = list[i] as Fire;
				if (fire != null && fire.parent == null)
				{
					return true;
				}
			}
			return false;
		}

		public static bool ContainsTrap(this IntVec3 c, Map map)
		{
			Building edifice = GridsUtility.GetEdifice(c, map);
			return edifice != null && edifice is Building_Trap;
		}

		public static bool Flammable(this TerrainDef terrain)
		{
			return StatExtension.GetStatValueAbstract(terrain, StatDefOf.Flammability, null) > 0.01f;
		}

		public static bool TerrainFlammableNow(this IntVec3 c, Map map)
		{
			if (!GridsUtility.GetTerrain(c, map).Flammable())
			{
				return false;
			}
			List<Thing> thingList = GridsUtility.GetThingList(c, map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].FireBulwark)
				{
					return false;
				}
			}
			return true;
		}
	}
}

