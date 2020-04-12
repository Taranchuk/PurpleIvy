using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;

namespace PurpleIvy
{
    public class Plant_Ivy : Plant
    {
        private int SpreadTick;
        private int OrigSpreadTick;
        private int mutateChance;
        private int mutateRate;
        public bool MutateTry;

        ThingDef sporesThingDef = ThingDef.Named("Spores");
        private Thing Spores = null;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!(this.Growth < 1)) return;
            var random = new System.Random();
            SpreadTick = random.Next(1, 3);
            OrigSpreadTick = SpreadTick;
            MutateTry = true;
        }
        public override void PostMapInit()
        {
            base.PostMapInit();
            if (this.Growth >= 0.25f)
            {
                this.ThrowGasOrAdjustGasSize();
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            try
            {
                if (this.Spores != null)
                {
                    this.Spores.Destroy(DestroyMode.Vanish);
                }
            }
            catch
            {
                ;
            }

        }

        public void SpawnIvy(IntVec3 dir)
        {
            if (!GenCollection.Any<Thing>(GridsUtility.GetThingList(dir, Map), (Thing t) => (t.def.IsBuildingArtificial || t.def.IsNonResourceNaturalRock | t.def.defName == "PurpleIvy")))
            {
                Plant newivy = new Plant();
                newivy = (Plant)ThingMaker.MakeThing(ThingDef.Named("PurpleIvy"));
                GenSpawn.Spawn(newivy, dir, this.Map);
            }
        }

        public bool IvyInCell(IntVec3 dir)
        {
            //List all things in that random direction cell
            List<Thing> list = this.Map.thingGrid.ThingsListAt(dir);
            return list.Count > 0 && list.OfType<Plant>().Any(t => t.def.defName == "PurpleIvy");
        }

        public bool IsSurroundedByIvy(IntVec3 dir)
        {
            return GenAdj.CellsAdjacent8Way(new TargetInfo(dir, this.Map, false)).All(IvyInCell);
        }

        public bool HasNoBuildings(IntVec3 dir)
        {
            return GenAdj.CellsAdjacent8Way(new TargetInfo(dir, this.Map, false)).All(current => current.Standable(this.Map));
        }

        public void DoDamageToThings(IntVec3 pos)
        {
            List<Thing> list = new List<Thing>();
            try
            {
                list = this.Map.thingGrid.ThingsListAt(pos);
            }
            catch
            {
                return;
            }
            //Loop over things if there are things
            if (list != null && list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != null && list[i].Faction != PurpleIvyData.factionDirect)
                    {
                        //If we find a corpse
                        if (list[i].def.IsCorpse)
                        {
                            Corpse corpse = (Corpse)list[i];
                            if (corpse.TryGetComp<AlienInfection>() == null)
                            {
                                Thing dummyCorpse = ThingMaker.MakeThing(PurpleIvyDefOf.InfectedCorpseDummy);
                                var comp = dummyCorpse.TryGetComp<AlienInfection>();
                                comp.parent = corpse;
                                IntRange range = new IntRange(1, 5);
                                comp.Props.maxNumberOfCreatures = range;
                                comp.maxNumberOfCreatures = range.RandomInRange;
                                comp.Props.typesOfCreatures = new List<string>()
                                {
                                    "Genny_ParasiteOmega"
                                };
                                corpse.AllComps.Add(comp);
                                Log.Message("Adding infected comp to " + corpse);
                            }
                            //speedup the spread a little
                            SpreadTick--;
                            SpreadTick--;
                            SpreadTick--;
                        }
                        //If we find a pawn and its not a hatchling
                        else if (list[i] is Pawn)
                        {
                            Pawn stuckPawn = (Pawn)list[i];
                            DamageInfo damageInfo = new DamageInfo(DamageDefOf.Scratch, 1, 0f, -1f, this, null, null);
                            stuckPawn.TakeDamage(damageInfo);
                            Hediff hediff = HediffMaker.MakeHediff(PurpleIvyDefOf.PoisonousPurpleHediff,
                            stuckPawn, null);
                            hediff.Severity = 0.1f;
                            (stuckPawn).health.AddHediff(hediff, null, null, null);
                            Hediff hediff2 = HediffMaker.MakeHediff(PurpleIvyDefOf.HarmfulBacteriaHediff,
                            stuckPawn, null);
                            hediff2.Severity = 0.1f;
                            (stuckPawn).health.AddHediff(hediff2, null, null, null);

                        }
                        //If we find a plant
                        else if (list[i] is Plant)
                        {
                            if (list[i].def.defName != "PurpleIvy")
                            {
                                list[i].TakeDamage(new DamageInfo(PurpleIvyDefOf.AlienToxicSting, 1));
                            }
                        }
                    }
                }
            }
        }

        public void SpreadBuildings()
        {
            if (this.MutateTry == true && HasNoBuildings(Position))
            {
                System.Random random = new System.Random(this.Position.GetHashCode());
                mutateChance = random.Next(1, 100);
                if (30 >= mutateChance)
                {
                    random = new System.Random(this.ThingID.GetHashCode());
                    mutateRate = random.Next(1, 100);
                    if (mutateRate >= 0 && mutateRate <= 5)
                    {
                        Building_GasPump GasPump = (Building_GasPump)ThingMaker.MakeThing(PurpleIvyDefOf.GasPump);
                        GasPump.SetFactionDirect(PurpleIvyData.factionDirect);
                        GenSpawn.Spawn(GasPump, Position, this.Map);
                        this.MutateTry = false;
                    }
                    else if (mutateRate >= 6 && mutateRate <= 10)
                    {
                        Building_Turret GenMortar = (Building_Turret)ThingMaker.MakeThing(PurpleIvyDefOf.Turret_GenMortarSeed);
                        GenMortar.SetFactionDirect(PurpleIvyData.factionDirect);
                        GenSpawn.Spawn(GenMortar, Position, this.Map);
                        this.MutateTry = false;
                    }
                    else if (mutateRate >= 11 && mutateRate <= 15)
                    {
                        Building_Turret GenTurret = (Building_Turret)ThingMaker.MakeThing(PurpleIvyDefOf.GenTurretBase);
                        GenTurret.SetFactionDirect(PurpleIvyData.factionDirect);
                        GenSpawn.Spawn(GenTurret, Position, this.Map);
                        this.MutateTry = false;
                    }
                    else if (mutateRate >= 16 && mutateRate <= 17)
                    {
                        Building_EggSac EggSac = (Building_EggSac)ThingMaker.MakeThing(PurpleIvyDefOf.EggSac);
                        EggSac.SetFactionDirect(PurpleIvyData.factionDirect);
                        GenSpawn.Spawn(EggSac, Position, this.Map);
                        this.MutateTry = false;
                    }
                    else if (mutateRate >= 18 && mutateRate <= 23)
                    {
                        Building_ParasiteEgg ParasiteEgg = (Building_ParasiteEgg)ThingMaker.MakeThing(PurpleIvyDefOf.ParasiteEgg);
                        ParasiteEgg.SetFactionDirect(PurpleIvyData.factionDirect);
                        GenSpawn.Spawn(ParasiteEgg, Position, this.Map);
                        this.MutateTry = false;
                    }
                    else
                    {
                        this.MutateTry = false;
                    }
                }
            }
        }
        public void SpreadPlants()
        {
            this.SpreadTick--;
            if (this.SpreadTick <= 0)
            {
                //Pick a random direction cell
                IntVec3 dir = new IntVec3();
                dir = GenAdj.RandomAdjacentCellCardinal(Position);
                //If in bounds
                try
                {
                    if (dir.InBounds(this.Map))
                    {
                        TerrainDef terrain = dir.GetTerrain(this.Map);
                        if (terrain != null)
                        {
                            if (terrain.defName != "WaterDeep" &&
                                     terrain.defName != "WaterShallow" &&
                                     terrain.defName != "MarshyTerrain")
                            {
                                //if theres no ivy here
                                if (!IvyInCell(dir))
                                {
                                    SpawnIvy(dir);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR:" + ex.Message);
                }

                SpreadTick = OrigSpreadTick;
            }
        }

        public void ThrowGasOrAdjustGasSize()
        {
            if (this.Spores != null)
            {
                this.Spores.Graphic.drawSize.x = (this.Growth * 4f) - 1f;
                this.Spores.Graphic.drawSize.y = (this.Growth * 4f) - 1f;
                this.Spores.Graphic.color.a = this.Growth;
            }
            else if (this.Position.InBounds(this.Map))
            {
                ThingDef thingDef = new ThingDef
                {
                    defName = "Spores" + this.ThingID,
                    thingClass = typeof(Gas),
                    category = ThingCategory.Gas,
                    altitudeLayer = AltitudeLayer.Gas,
                    useHitPoints = false,
                    tickerType = TickerType.Normal,
                    graphicData = new GraphicData
                    {
                        texPath = sporesThingDef.graphicData.texPath,
                        graphicClass = typeof(Graphic_Gas),
                        shaderType = ShaderTypeDefOf.Transparent,
                        drawSize = new Vector2((this.Growth * 4f) - 1f, (this.Growth * 4f) - 1f),
                        color = new ColorInt(sporesThingDef.graphicData.color).ToColor
                    },
                    gas = new GasProperties
                    {
                        expireSeconds = new FloatRange(29000f, 31000f),
                        blockTurretTracking = true,
                        accuracyPenalty = 0.7f,
                        rotationSpeed = 10f
                    }
                };
                Thing thing = ThingMaker.MakeThing(thingDef, null);
                GenSpawn.Spawn(thing, this.Position, this.Map, 0);
                this.Spores = thing;
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % 2000 == 0)
            {
                base.TickLong();
                if (this.Growth >= 0.25f)
                {
                    this.ThrowGasOrAdjustGasSize();
                }
            }
            if (Find.TickManager.TicksGame % 350 == 0)
            {
                if (this.Growth >= 0.1f)
                {
                    this.SpreadPlants();
                }
                if (this.Growth >= 0.25f)
                {
                    this.DoDamageToThings(Position);
                    //Log.Message("Spread buildings");
                    this.SpreadBuildings();
                }

            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.SpreadTick, "SpreadTick", 0, true);
            Scribe_Values.Look<int>(ref this.OrigSpreadTick, "OrigSpreadTick", 0, true);
            Scribe_Values.Look<bool>(ref this.MutateTry, "MutateTry", true, true);
        }
    }
}

