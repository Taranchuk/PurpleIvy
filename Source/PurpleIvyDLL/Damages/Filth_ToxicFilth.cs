using System;
using UnityEngine;
using Verse;
using Verse.Sound;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using RimWorld;

namespace PurpleIvy
{
    public class Filth_ToxicFilth : Filth
    {
        public override void Tick()
        {
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                List<Thing> list = new List<Thing>();
                try
                {
                    if (GenGrid.InBounds(this.Position, this.Map))
                    {
                        list = this.Map.thingGrid.ThingsListAt(this.Position);
                    }
                }
                catch
                {
                    return;
                }
                if (list == null || list.Count <= 0) return;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == null || list[i].Faction == PurpleIvyData.AlienFaction) continue;
                    switch (list[i])
                    {
                        case Pawn _:
                            {
                                try
                                {
                                    PurpleIvyMoteMaker.ThrowToxicSmoke(this.Position.ToVector3Shifted(), this.Map);
                                    Pawn pawn = (Pawn)list[i];
                                    if (!pawn.RaceProps.IsMechanoid)
                                    {
                                        HealthUtility.AdjustSeverity(pawn, HediffDefOf.ToxicBuildup, 0.005f);
                                        HealthUtility.AdjustSeverity(pawn, PurpleIvyDefOf.PI_VaporToxicFilth, 1f);
                                    }
                                }
                                catch { }
                                break;
                            }
                        case Plant _:
                            {
                                if (list[i].def != PurpleIvyDefOf.PurpleIvy && list[i].def != PurpleIvyDefOf.PI_Nest
                                        && list[i].def != PurpleIvyDefOf.PlantVenomousToothwort)
                                {
                                    PurpleIvyMoteMaker.ThrowToxicSmoke(this.Position.ToVector3Shifted(), this.Map);
                                    list[i].TakeDamage(new DamageInfo(PurpleIvyDefOf.PI_ToxicBurn, 1));
                                }
                                break;
                            }
                    }
                }
            }
        }
    }
}

