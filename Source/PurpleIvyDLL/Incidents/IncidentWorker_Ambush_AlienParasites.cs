﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace PurpleIvy
{
    public class IncidentWorker_Ambush_AlienParasites : IncidentWorker_Ambush
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Faction faction = PurpleIvyData.factionDirect;
            Log.Message("CanFireNow");
            return base.CanFireNowSub(parms) && PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(parms.points, out faction, null, false, false, false, true);
        }

        protected override List<Pawn> GeneratePawns(IncidentParms parms)
        {
            if (!PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(parms.points, out parms.faction, null, false, false, false, true))
            {
                Log.Error("Could not find any valid faction for " + this.def + " incident.", false);
                return new List<Pawn>();
            }
            PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, parms, false);
            defaultPawnGroupMakerParms.generateFightersOnly = true;
            defaultPawnGroupMakerParms.dontUseSingleUseRocketLaunchers = true;
            return PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms, true).ToList<Pawn>();
        }
        protected override LordJob CreateLordJob(List<Pawn> generatedPawns, IncidentParms parms)
        {
            return new LordJob_AssaultColony(parms.faction, true, false, false, false, true);
        }

        protected override string GetLetterText(Pawn anyPawn, IncidentParms parms)
        {
            Caravan caravan = parms.target as Caravan;
            return string.Format(this.def.letterText, (caravan != null) ? caravan.Name : "yourCaravan".TranslateSimple(), parms.faction.def.pawnsPlural, parms.faction.Name).CapitalizeFirst();
        }
    }
}
