using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace PurpleIvy
{
    public class IncidentWorker_MutantRaid : IncidentWorker_RaidEnemy
    {
        protected override bool TryResolveRaidFaction(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            parms.faction = PurpleIvyData.AlienFaction;
            return true;
        }

        protected override void ResolveRaidPoints(IncidentParms parms)
        {
            parms.points /= 1.5f;
            bool flag = parms.points <= 0f;
            if (flag)
            {
                Log.Error("RaidEnemy is resolving raid points. They should always be set before initiating the incident.", false);
                parms.points = StorytellerUtility.DefaultThreatPointsNow(parms.target) / 1.5f;
            }
        }

        public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
        }

        public override void ResolveRaidArriveMode(IncidentParms parms)
        {
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            this.ResolveRaidPoints(parms);
            if (!this.TryResolveRaidFaction(parms))
            {
                return false;
            }
            PawnGroupKindDef combat = PawnGroupKindDefOf.Combat;
            this.ResolveRaidStrategy(parms, combat);
            this.ResolveRaidArriveMode(parms);
            parms.raidStrategy.Worker.TryGenerateThreats(parms);
            if (!parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
            {
                return false;
            }
            parms.points = IncidentWorker_Raid.AdjustedRaidPoints(parms.points, parms.raidArrivalMode, parms.raidStrategy, parms.faction, combat);
            List<Pawn> list = parms.raidStrategy.Worker.SpawnThreats(parms);
            if (list == null)
            {
                list = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(combat, parms, false), true).ToList<Pawn>();
                if (list.Count == 0)
                {
                    Log.Error("Got no pawns spawning raid from parms " + parms, false);
                    return false;
                }
                parms.raidArrivalMode.Worker.Arrive(list, parms);
            }
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Points = " + parms.points.ToString("F0"));
            foreach (Pawn pawn in list)
            {
                string str = (pawn.equipment != null && pawn.equipment.Primary != null) ? pawn.equipment.Primary.LabelCap : "unarmed";
                stringBuilder.AppendLine(pawn.KindLabel + " - " + str);
            }
            TaggedString baseLetterLabel = this.GetLetterLabel(parms);
            TaggedString baseLetterText = this.GetLetterText(parms, list);
            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(list, ref baseLetterLabel, ref baseLetterText, this.GetRelatedPawnsInfoLetterText(parms), true, true);
            List<TargetInfo> list2 = new List<TargetInfo>();
            if (parms.pawnGroups != null)
            {
                List<List<Pawn>> list3 = IncidentParmsUtility.SplitIntoGroups(list, parms.pawnGroups);
                List<Pawn> list4 = list3.MaxBy((List<Pawn> x) => x.Count);
                if (list4.Any<Pawn>())
                {
                    list2.Add(list4[0]);
                }
                for (int i = 0; i < list3.Count; i++)
                {
                    if (list3[i] != list4 && list3[i].Any<Pawn>())
                    {
                        list2.Add(list3[i][0]);
                    }
                }
            }
            else if (list.Any<Pawn>())
            {
                foreach (Pawn t in list)
                {
                    list2.Add(t);
                }
            }
            base.SendStandardLetter(baseLetterLabel, baseLetterText, this.GetLetterDef(), parms, list2, Array.Empty<NamedArgument>());
            parms.raidStrategy.Worker.MakeLords(parms, list);
            foreach (var pawn in list)
            {
                var hediff = (AlienMutationHediff)HediffMaker.MakeHediff(PurpleIvyDefOf.PI_AlienMutation, pawn, null);
                hediff.Mutate();
                pawn.health.AddHediff(hediff, null, null, null);
            }
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.EquippingWeapons, OpportunityType.Critical);
            if (!PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.ShieldBelts))
            {
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j].apparel.WornApparel.Any((Apparel ap) => ap is ShieldBelt))
                    {
                        LessonAutoActivator.TeachOpportunity(ConceptDefOf.ShieldBelts, OpportunityType.Critical);
                        break;
                    }
                }
            }
            return true;
        }

        protected override string GetLetterLabel(IncidentParms parms)
        {
            return parms.raidStrategy.letterLabelEnemy;
        }

        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
        {
            string text = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural, parms.faction.Name);
            text += "\n\n";
            text += parms.raidStrategy.arrivalTextEnemy;
            Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
            bool flag = pawn != null;
            if (flag)
            {
                text += "\n\n";
                text += TranslatorFormattedStringExtensions.Translate("EnemyRaidLeaderPresent", pawn.Faction.def.pawnsPlural, pawn.LabelShort, NamedArgumentUtility.Named(pawn, "LEADER"));
            }
            return text;
        }

        protected override LetterDef GetLetterDef()
        {
            return LetterDefOf.ThreatBig;
        }

        protected override string GetRelatedPawnsInfoLetterText(IncidentParms parms)
        {
            return TranslatorFormattedStringExtensions.Translate("LetterRelatedPawnsRaidEnemy", Faction.OfPlayer.def.pawnsPlural, parms.faction.def.pawnsPlural);
        }
    }
}

