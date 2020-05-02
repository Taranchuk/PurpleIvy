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
    public class Filth_ToxicBlood : Filth
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
                        //If we find a pawn and its not a hatchling
                        case Pawn _:
                            {
                                try
                                {
                                    //PurpleIvyMoteMaker.ThrowToxicSmoke(this.Position.ToVector3Shifted(), this.Map);
                                    Pawn pawn = (Pawn)list[i];
                                    if (!pawn.RaceProps.IsMechanoid)
                                    {
                                        pawn.TakeDamage(new DamageInfo(PurpleIvyDefOf.PI_ToxicBurn, 1, 0, -1, this,
                                        pawn.health.hediffSet.GetNotMissingParts(0, 0, null, null)
                                        .Where(x => x.groups.Contains(BodyPartGroupDefOf.Legs))
                                        .FirstOrDefault()));
                                        HealthUtility.AdjustSeverity(pawn, HediffDefOf.ToxicBuildup, 0.01f);
                                        HealthUtility.AdjustSeverity(pawn, PurpleIvyDefOf.PI_AlienBlood, 1f);
                                    }
                                }
                                catch { }
                                break;
                            }
                        //If we find a plant
                        case Plant _:
                            {
                                if (list[i].def != PurpleIvyDefOf.PurpleIvy
                                    && list[i].def != PurpleIvyDefOf.PI_Nest
                                    && list[i].def != PurpleIvyDefOf.PlantVenomousToothwort
                                    && list[i].def != PurpleIvyDefOf.PI_CorruptedTree)
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

