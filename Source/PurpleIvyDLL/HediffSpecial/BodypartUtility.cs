using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace HediffSpecial
{
	public static class BodypartUtility
	{
		public static IEnumerable<BodyPartRecord> GetFirstMatchingBodyparts(this Pawn pawn, BodyPartRecord startingPart, HediffDef hediffDef)
		{
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			List<BodyPartRecord> currentSet = new List<BodyPartRecord>();
			List<BodyPartRecord> nextSet = new List<BodyPartRecord>();
			nextSet.Add(startingPart);
			do
			{
				currentSet.AddRange(nextSet);
				nextSet.Clear();
				foreach (BodyPartRecord part in currentSet)
				{
					bool matchingPart = false;
					int num;
					for (int i = hediffs.Count - 1; i >= 0; i = num - 1)
					{
						Hediff hediff = hediffs[i];
						if (hediff.Part == part && hediff.def == hediffDef)
						{
							matchingPart = true;
							yield return part;
						}
						num = i;
					}
					if (!matchingPart)
					{
						for (int j = 0; j < part.parts.Count; j = num + 1)
						{
							nextSet.Add(part.parts[j]);
							num = j;
						}
					}
				}
				List<BodyPartRecord>.Enumerator enumerator = default(List<BodyPartRecord>.Enumerator);
				currentSet.Clear();
			}
			while (nextSet.Count > 0);
			yield break;
			yield break;
		}

		public static IEnumerable<BodyPartRecord> GetFirstMatchingBodyparts(this Pawn pawn, BodyPartRecord startingPart, HediffDef hediffDef, HediffDef hediffExceptionDef)
		{
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			List<BodyPartRecord> currentSet = new List<BodyPartRecord>();
			List<BodyPartRecord> nextSet = new List<BodyPartRecord>();
			nextSet.Add(startingPart);
			do
			{
				currentSet.AddRange(nextSet);
				nextSet.Clear();
				foreach (BodyPartRecord part in currentSet)
				{
					bool matchingPart = false;
					int num;
					for (int i = hediffs.Count - 1; i >= 0; i = num - 1)
					{
						Hediff hediff = hediffs[i];
						if (hediff.Part == part)
						{
							if (hediff.def == hediffExceptionDef)
							{
								matchingPart = true;
								break;
							}
							if (hediff.def == hediffDef)
							{
								matchingPart = true;
								yield return part;
								break;
							}
						}
						num = i;
					}
					if (!matchingPart)
					{
						for (int j = 0; j < part.parts.Count; j = num + 1)
						{
							nextSet.Add(part.parts[j]);
							num = j;
						}
					}
				}
				List<BodyPartRecord>.Enumerator enumerator = default(List<BodyPartRecord>.Enumerator);
				currentSet.Clear();
			}
			while (nextSet.Count > 0);
			yield break;
			yield break;
		}

		public static IEnumerable<BodyPartRecord> GetFirstMatchingBodyparts(this Pawn pawn, BodyPartRecord startingPart, HediffDef hediffDef, HediffDef hediffExceptionDef, Predicate<Hediff> extraExceptionPredicate)
		{
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			List<BodyPartRecord> currentSet = new List<BodyPartRecord>();
			List<BodyPartRecord> nextSet = new List<BodyPartRecord>();
			nextSet.Add(startingPart);
			do
			{
				currentSet.AddRange(nextSet);
				nextSet.Clear();
				foreach (BodyPartRecord part in currentSet)
				{
					bool matchingPart = false;
					int num;
					for (int i = hediffs.Count - 1; i >= 0; i = num - 1)
					{
						Hediff hediff = hediffs[i];
						if (hediff.Part == part)
						{
							if (hediff.def == hediffExceptionDef || extraExceptionPredicate(hediff))
							{
								matchingPart = true;
								break;
							}
							if (hediff.def == hediffDef)
							{
								matchingPart = true;
								yield return part;
								break;
							}
						}
						num = i;
					}
					if (!matchingPart)
					{
						for (int j = 0; j < part.parts.Count; j = num + 1)
						{
							nextSet.Add(part.parts[j]);
							num = j;
						}
					}
				}
				List<BodyPartRecord>.Enumerator enumerator = default(List<BodyPartRecord>.Enumerator);
				currentSet.Clear();
			}
			while (nextSet.Count > 0);
			yield break;
			yield break;
		}

		public static IEnumerable<BodyPartRecord> GetFirstMatchingBodyparts(this Pawn pawn, BodyPartRecord startingPart, HediffDef hediffDef, HediffDef[] hediffExceptionDefs)
		{
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			List<BodyPartRecord> currentSet = new List<BodyPartRecord>();
			List<BodyPartRecord> nextSet = new List<BodyPartRecord>();
			nextSet.Add(startingPart);
			do
			{
				currentSet.AddRange(nextSet);
				nextSet.Clear();
				foreach (BodyPartRecord part in currentSet)
				{
					bool matchingPart = false;
					int num;
					for (int i = hediffs.Count - 1; i >= 0; i = num - 1)
					{
						Hediff hediff = hediffs[i];
						if (hediff.Part == part)
						{
							if (hediffExceptionDefs.Contains(hediff.def))
							{
								matchingPart = true;
								break;
							}
							if (hediff.def == hediffDef)
							{
								matchingPart = true;
								yield return part;
								break;
							}
						}
						num = i;
					}
					if (!matchingPart)
					{
						for (int j = 0; j < part.parts.Count; j = num + 1)
						{
							nextSet.Add(part.parts[j]);
							num = j;
						}
					}
				}
				List<BodyPartRecord>.Enumerator enumerator = default(List<BodyPartRecord>.Enumerator);
				currentSet.Clear();
			}
			while (nextSet.Count > 0);
			yield break;
			yield break;
		}

		public static IEnumerable<BodyPartRecord> GetFirstMatchingBodyparts(this Pawn pawn, BodyPartRecord startingPart, HediffDef[] hediffDefs)
		{
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			List<BodyPartRecord> currentSet = new List<BodyPartRecord>();
			List<BodyPartRecord> nextSet = new List<BodyPartRecord>();
			nextSet.Add(startingPart);
			do
			{
				currentSet.AddRange(nextSet);
				nextSet.Clear();
				foreach (BodyPartRecord part in currentSet)
				{
					bool matchingPart = false;
					int num;
					for (int i = hediffs.Count - 1; i >= 0; i = num - 1)
					{
						Hediff hediff = hediffs[i];
						if (hediff.Part == part && hediffDefs.Contains(hediff.def))
						{
							matchingPart = true;
							yield return part;
							break;
						}
						num = i;
					}
					if (!matchingPart)
					{
						for (int j = 0; j < part.parts.Count; j = num + 1)
						{
							nextSet.Add(part.parts[j]);
							num = j;
						}
					}
				}
				List<BodyPartRecord>.Enumerator enumerator = default(List<BodyPartRecord>.Enumerator);
				currentSet.Clear();
			}
			while (nextSet.Count > 0);
			yield break;
			yield break;
		}

		public static void ReplaceHediffFromBodypart(this Pawn pawn, BodyPartRecord startingPart, HediffDef hediffDef, HediffDef replaceWithDef)
		{
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			List<BodyPartRecord> list = new List<BodyPartRecord>();
			List<BodyPartRecord> list2 = new List<BodyPartRecord>();
			list2.Add(startingPart);
			do
			{
				list.AddRange(list2);
				list2.Clear();
				foreach (BodyPartRecord bodyPartRecord in list)
				{
					for (int i = hediffs.Count - 1; i >= 0; i--)
					{
						Hediff hediff = hediffs[i];
						if (hediff.Part == bodyPartRecord && hediff.def == hediffDef)
						{
							Hediff hediff2 = hediffs[i];
							hediffs.RemoveAt(i);
							hediff2.PostRemoved();
							Hediff item = HediffMaker.MakeHediff(replaceWithDef, pawn, bodyPartRecord);
							hediffs.Insert(i, item);
						}
					}
					for (int j = 0; j < bodyPartRecord.parts.Count; j++)
					{
						list2.Add(bodyPartRecord.parts[j]);
					}
				}
				list.Clear();
			}
			while (list2.Count > 0);
		}
	}
}

