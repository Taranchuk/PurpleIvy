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
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKindDef, ofPlayer, PawnGenerationContext.NonPlayer, -1, true, false, false, false, true, pawnMustBeCapableOfViolence, 20f, false, true, true, false, false, false, false));
			GenSpawn.Spawn(pawn, intVec, map, 0);

            var taggedString1 = this.def.letterLabel.Translate(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN", true);
            var taggedString2 = this.def.letterText.Translate(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN", true);

			PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref taggedString1, ref taggedString2, pawn);
			Find.LetterStack.ReceiveLetter(taggedString1, taggedString2, LetterDefOf.PositiveEvent, pawn, null, null);
			return true;
		}

		private bool TryFindEntryCell(Map map, out IntVec3 cell)
		{
			return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c) && !GridsUtility.Fogged(c, map), map, CellFinder.EdgeRoadChance_Neutral, out cell);
		}

		private const float RelationWithColonistWeight = 20f;
	}
}
