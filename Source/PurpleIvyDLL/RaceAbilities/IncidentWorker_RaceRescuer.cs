using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RaceAbilities
{
	public class IncidentWorker_RaceRescuer : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			Map map = (Map)parms.target;
			IntVec3 intVec;
			return this.TryFindEntryCell(map, out intVec);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 intVec;
			if (!this.TryFindEntryCell(map, out intVec))
			{
				return false;
			}
			Gender? gender = null;
			if (this.def.pawnFixedGender != null)
			{
				gender = new Gender?(this.def.pawnFixedGender);
			}
			PawnKindDef pawnKindDef = this.def.pawnKind;
			Faction ofPlayer = Faction.OfPlayer;
			List<PawnKindDef> list = (from def in DefDatabase<PawnKindDef>.AllDefs
			where def.race == ofPlayer.def.basicMemberKind.race && def.defName.Contains("BKRescuer")
			select def).ToList<PawnKindDef>();
			if (list.Count > 0)
			{
				pawnKindDef = GenCollection.RandomElement<PawnKindDef>(list);
			}
			else
			{
				list = (from def in DefDatabase<PawnKindDef>.AllDefs
				where def.defName.Contains("BKRescuer")
				select def).ToList<PawnKindDef>();
				pawnKindDef = GenCollection.RandomElement<PawnKindDef>(list);
			}
			pawnKindDef.defaultFactionType = ofPlayer.def;
			bool pawnMustBeCapableOfViolence = this.def.pawnMustBeCapableOfViolence;
			Gender? gender2 = gender;
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKindDef, ofPlayer, 2, -1, true, false, false, false, true, pawnMustBeCapableOfViolence, 20f, false, true, true, false, false, false, false, null, null, null, null, null, gender2, null, null));
			GenSpawn.Spawn(pawn, intVec, map, 0);
			string text = GenText.AdjustedFor(GrammarResolverSimpleStringExtensions.Formatted(this.def.letterText, NamedArgumentUtility.Named(pawn, "PAWN")), pawn, "PAWN");
			string text2 = GenText.AdjustedFor(GrammarResolverSimpleStringExtensions.Formatted(this.def.letterLabel, NamedArgumentUtility.Named(pawn, "PAWN")), pawn, "PAWN");
			PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref text2, pawn);
			Find.LetterStack.ReceiveLetter(text2, text, LetterDefOf.PositiveEvent, pawn, null, null);
			return true;
		}

		private bool TryFindEntryCell(Map map, out IntVec3 cell)
		{
			return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c) && !GridsUtility.Fogged(c, map), map, CellFinder.EdgeRoadChance_Neutral, ref cell);
		}

		private const float RelationWithColonistWeight = 20f;
	}
}
