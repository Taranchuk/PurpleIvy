using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace PurpleIvy
{
    public class IncidentWorker_RaidAliens : IncidentWorker_RaidEnemy
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Log.Message("CanFireNow2");
            return base.CanFireNowSub(parms);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            bool flag = true;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                Map map = (Map)parms.target;
                float points = parms.points;
                parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
                parms.raidNeverFleeIndividual = true;
                parms.points *= Rand.Range(1f, 1.6f);
                parms.faction = PurpleIvyData.factionDirect;
                this.ResolveRaidPoints(parms);
                PawnGroupKindDef combat = PawnGroupKindDefOf.Combat;
                this.ResolveRaidStrategy(parms, combat);
                this.ResolveRaidArriveMode(parms);
                parms.points = IncidentWorker_RaidAliens.AdjustedRaidPoints(parms.points, parms.raidArrivalMode, parms.raidStrategy, parms.faction, combat);
                PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(combat, parms, false);
                List<Pawn> list = Enumerable.ToList<Pawn>(PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms, true));
                parms.raidArrivalMode.Worker.Arrive(list, parms);
                Find.LetterStack.ReceiveLetter("AlienRaid".Translate(), "AlienRaidDesc".Translate(), this.GetLetterDef(), null, parms.faction, null, null, null);
                parms.raidStrategy.Worker.MakeLords(parms, list);
                result = true;
            }
            return result;
        }
    }
}