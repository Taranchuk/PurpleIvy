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
                        //If we find a pawn and its not a hatchling
                        case Pawn _:
                            {
                                try
                                {
                                    MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_Smoke, null);
                                    moteThrown.Scale = Rand.Range(0.5f, 0.9f);
                                    moteThrown.rotationRate = Rand.Range(-30f, 30f);
                                    moteThrown.exactPosition = this.Position.ToVector3Shifted();
                                    moteThrown.airTimeLeft = Rand.Range(0.1f, 0.4f);
                                    moteThrown.Speed = 0.3f;
                                    moteThrown.SetVelocity((float)Rand.Range(-20, 20), Rand.Range(0.5f, 0.7f));
                                    GenSpawn.Spawn(moteThrown, this.Position, this.Map, WipeMode.Vanish);
                                    moteThrown.instanceColor = new Color(0f, 0.0862f, 0.094117f);
                                    list[i].TakeDamage(new DamageInfo(PurpleIvyDefOf.PI_ToxicBurn, 1));
                                    HealthUtility.AdjustSeverity((Pawn)list[i], HediffDefOf.ToxicBuildup, 0.005f);
                                    HealthUtility.AdjustSeverity((Pawn)list[i], PurpleIvyDefOf.PI_VaporToxicFilth, 1f);
                                }
                                catch { }
                                break;
                            }
                        //If we find a plant
                        case Plant _:
                            {
                                if (list[i].def != PurpleIvyDefOf.PurpleIvy && list[i].def != PurpleIvyDefOf.PI_Nest
                                        && list[i].def != PurpleIvyDefOf.PlantVenomousToothwort)
                                {
                                    MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_Smoke, null);
                                    moteThrown.Scale = Rand.Range(0.5f, 0.9f);
                                    moteThrown.rotationRate = Rand.Range(-30f, 30f);
                                    moteThrown.exactPosition = this.Position.ToVector3Shifted();
                                    moteThrown.airTimeLeft = Rand.Range(0.1f, 0.4f);
                                    moteThrown.Speed = 0.3f;
                                    moteThrown.SetVelocity((float)Rand.Range(-20, 20), Rand.Range(0.5f, 0.7f));
                                    GenSpawn.Spawn(moteThrown, this.Position, this.Map, WipeMode.Vanish);
                                    moteThrown.instanceColor = new Color(0f, 0.0862f, 0.094117f);
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

