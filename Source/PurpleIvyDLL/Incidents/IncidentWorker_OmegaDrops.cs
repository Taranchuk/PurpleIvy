using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace PurpleIvy
{
    public class IncidentWorker_OmegaDrops : IncidentWorker_RaidEnemy
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms);
        }

        public List<Pawn> GeneratePawns(IncidentParms parms)
        {
            List<Pawn> pawns = new List<Pawn>();
            float points = parms.points;
            if (PurpleIvyData.combatPoints.Where(x => x.Value >= points) != null)
            {
                while (points >= 35f)
                {
                    var combatCandidat = PurpleIvyData.combatPoints[PurpleIvyData.Genny_ParasiteOmega];
                    if (points >= combatCandidat)
                    {
                        points -= combatCandidat;
                        Pawn alien = PurpleIvyUtils.GenerateParasite(PurpleIvyData.Genny_ParasiteOmega);
                        pawns.Add(alien);
                    }
                }
            }
            return pawns;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            bool result;
            Map map = (Map)parms.target;
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            parms.raidNeverFleeIndividual = true;
            this.ResolveRaidPoints(parms);
            PawnGroupKindDef combat = PawnGroupKindDefOf.Combat;
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            parms.faction = PurpleIvyData.AlienFaction;
            parms.points = StorytellerUtility.DefaultThreatPointsNow(map);
            parms.generateFightersOnly = true;
            parms.forced = true;
            IntVec3 spawncenter = CellFinder.RandomClosewalkCellNear(map.Center, map, 50, 
                (IntVec3 x) => GenGrid.Walkable(x, map) && !GridsUtility.Fogged(x, map));
            parms.spawnCenter = spawncenter;
            parms.raidArrivalMode = PawnsArrivalModeDefOf.CenterDrop;
            Lord lord = null;
            List<Pawn> list = new List<Pawn>();
            if (parms.pawnGroups == null)
            {
                Log.Message(parms.points + " points");
                list = this.GeneratePawns(parms);
            }
            else
            {
                foreach (var data in parms.pawnGroups)
                {
                    list.Add(data.Key);
                }
            }

            //foreach (Pawn pawn in list)
            //{
            //    if (map.mapPawns.SpawnedPawnsInFaction(PurpleIvyData.AlienFaction).Any((Pawn p) =>
            //    p != pawn))
            //    {
            //        lord = ((Pawn)GenClosest.ClosestThing_Global(pawn.Position,
            //            map.mapPawns.SpawnedPawnsInFaction(PurpleIvyData.AlienFaction), 99999f,
            //            (Thing p) => p != pawn && ((Pawn)p).GetLord() != null, null)).GetLord();
            //    }
            //    if (lord == null)
            //    {
            //        var lordJob = new LordJob_AssaultColony(Faction.OfPlayer, false, false, false, false, false);
            //        lord = LordMaker.MakeNewLord(PurpleIvyData.AlienFaction, lordJob, map, null);
            //    }
            //    lord.AddPawn(pawn);
            //}
            if (list.Count == 0)
            {
                result = false;
            }
            else
            {
                
                parms.raidArrivalMode.Worker.Arrive(list, parms);
                Find.LetterStack.ReceiveLetter("BiologicalWarfare".Translate(), "BiologicalWarfareDesc".Translate(), this.GetLetterDef(), list, parms.faction);
                result = true;
                Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
            }
            return result;
        }
    }
}

