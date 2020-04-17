using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace PurpleIvy
{
    public class IncidentWorker_AlienRaid : IncidentWorker_RaidEnemy
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            bool result;
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            parms.raidNeverFleeIndividual = true;
            this.ResolveRaidPoints(parms);
            PawnGroupKindDef combat = PawnGroupKindDefOf.Combat;
            this.ResolveRaidStrategy(parms, combat);
            this.ResolveRaidArriveMode(parms);
            List<Pawn> list = new List<Pawn>();
            foreach (var data in parms.pawnGroups)
            {
                list.Add(data.Key);
            }
            if (list.Count == 0)
            {
                result = false;
            }
            else
            {
                parms.raidArrivalMode.Worker.Arrive(list, parms);
                parms.raidStrategy.Worker.MakeLords(parms, list);
                Find.LetterStack.ReceiveLetter("AlienRaid", "AlienRaidDesc", this.GetLetterDef(), list, parms.faction);
                result = true;
                Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
            }
            return result;
        }
    }
}

