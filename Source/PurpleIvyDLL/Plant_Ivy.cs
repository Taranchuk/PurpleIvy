using System;
using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;

namespace PurpleIvy
{
    public class Plant_Ivy : Plant
    {
        private bool MutateTry;
        Faction factionDirect = Find.FactionManager.FirstFactionOfDef(PurpleIvyDefOf.Genny);
        private Thing Spores = null;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            System.Random random = new System.Random();
            MutateTry = true;
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
            if (!GenCollection.Any<Thing>(GridsUtility.GetThingList(dir, Map), (Thing t) =>          (t.def.IsBuildingArtificial || t.def.IsNonResourceNaturalRock | t.def.defName == "PurpleIvy")))
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
            if (list.Count > 0)
            {
                //Loop over things
                for (int i = 0; i < list.Count; i++)
                {
                    //If we find a plant
                    if (list[i] is Plant)
                    {
                        //If the plant is Ivy
                        if (list[i].def.defName == "PurpleIvy")
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool isSurroundedByIvy(IntVec3 dir)
        {
            foreach (IntVec3 current in GenAdj.CellsAdjacent8Way(new TargetInfo(dir, this.Map, false)))
			{
				if(!IvyInCell(current))
                {
                    return false;
                }
			}
            return true;
        }

        public bool hasNoBuildings(IntVec3 dir)
        {
            foreach (IntVec3 current in GenAdj.CellsAdjacent8Way(new TargetInfo(dir, this.Map, false)))
            {
                if (!current.Standable(this.Map))
                {
                    return false;
                }
            }
            return true;
        }

        public void ThrowGasOrAdjustGasSize()
        {
            if (this.Spores != null)
            {
                this.Spores.Graphic.drawSize.x = this.Growth + 0.25f;
                this.Spores.Graphic.drawSize.y = this.Growth + 0.25f;
                this.Spores.Graphic.color.a = this.Growth;

                //thing.Graphic.color.r = 0;// 100 - (this.Growth * 100);
                //thing.Graphic.color.g = 0;// 100 - (this.Growth * 100);
                //thing.Graphic.color.b = 0;// 100 - (this.Growth * 100);
            }
            else if (GenGrid.InBounds(this.Position, this.Map))
            {
                ThingDef oldThingDef = ThingDef.Named("Spores");
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
                        texPath = oldThingDef.graphicData.texPath,
                        graphicClass = typeof(Graphic_Gas),
                        shaderType = ShaderTypeDefOf.Transparent,
                        drawSize = new Vector2(oldThingDef.graphicData.drawSize.x,
                        oldThingDef.graphicData.drawSize.y),
                        color = new ColorInt(oldThingDef.graphicData.color).ToColor
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
                thing.Graphic.drawSize.x = this.Growth;
                thing.Graphic.drawSize.y = this.Growth;
                this.Spores = thing;
            }
        }

        public void CheckThings(IntVec3 pos)
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
                    //If we find a corpse
                    if (list[i] != null && list[i].Faction != factionDirect && list[i].def.IsCorpse)
                    {
                        Corpse corpse = (Corpse)list[i];
                        if (corpse.TryGetComp<AlienInfection>() == null)
                        {
                            ThingDef dummyCorpse = PurpleIvyDefOf.InfectedCorpseDummy;
                            AlienInfection comp = new AlienInfection();
                            comp.Initialize(dummyCorpse.GetCompProperties<CompProperties_AlienInfection>());
                            comp.parent = corpse;
                            comp.Props.typesOfCreatures = new List<string>()
                            {
                                "Genny_ParasiteOmega"
                            };
                            IntRange range = new IntRange(1, 10);
                            comp.totalNumberOfCreatures = range.RandomInRange;
                            comp.Props.maxNumberOfCreatures = range;
                            corpse.AllComps.Add(comp);
                            Log.Message("Adding infected comp to " + corpse.Label);
                        }
                    }
                    //If we find a pawn
                    else if (list[i] != null && list[i] is Pawn)
                    {
                        //And its not a hatchling
                        if (list[i] != null && list[i].Faction != factionDirect)
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
                    }
                    //If we find a plant
                    else if (list[i] != null && list[i] is Plant && list[i].def.defName != "PurpleIvy")
                    {
                        list[i].TakeDamage(new DamageInfo(PurpleIvyDefOf.AlienToxicSting, 5));
                    }
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (this.Growth >= 0.25f)
            {
                if (Find.TickManager.TicksGame % 2500 == 0)
                {
                    this.ThrowGasOrAdjustGasSize();
                    CheckThings(Position);
                }
            }
            if (Find.TickManager.TicksGame % 350 == 0)
            {
                base.TickLong();
                //Pick a random direction cell
                IntVec3 dir = new IntVec3();
                dir = GenAdj.RandomAdjacentCellCardinal(Position);
                //If in bounds
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
                if (this.MutateTry == true)
                {
                    System.Random random = new System.Random();
                    int MutateRate = random.Next(1, 100);
                    if (MutateRate == 3 || MutateRate == 23)
                    {
                        Building_GasPump GasPump = (Building_GasPump)ThingMaker.MakeThing(ThingDef.Named("GasPump"));
                        GasPump.SetFactionDirect(factionDirect);
                        if (hasNoBuildings(Position))
                        {
                            GenSpawn.Spawn(GasPump, Position, this.Map);
                        }
                        this.MutateTry = false;
                    }
                    else if (MutateRate == 4 || MutateRate == 24)
                    {
                        Building_EggSac EggSac = (Building_EggSac)ThingMaker.MakeThing(ThingDef.Named("EggSac"));
                        EggSac.SetFactionDirect(factionDirect);
                        if (hasNoBuildings(Position))
                        {
                            GenSpawn.Spawn(EggSac, Position, this.Map);
                        }
                        this.MutateTry = false;
                    }
                    else if (MutateRate == 8 || MutateRate == 16)
                    {
                        Building_ParasiteEgg ParasiteEgg = (Building_ParasiteEgg)ThingMaker.MakeThing(ThingDef.Named("ParasiteEgg"));
                        ParasiteEgg.SetFactionDirect(factionDirect);
                        ParasiteEgg.InitializeComps();
                        if (hasNoBuildings(Position))
                        {
                            GenSpawn.Spawn(ParasiteEgg, Position, this.Map);
                        }
                        this.MutateTry = false;
                    }
                    else if (MutateRate == 5)
                    {
                        Building_Turret GenMortar = (Building_Turret)ThingMaker.MakeThing(ThingDef.Named("Turret_GenMortarSeed"));
                        GenMortar.SetFactionDirect(factionDirect);
                        if (hasNoBuildings(Position))
                        {
                            GenSpawn.Spawn(GenMortar, Position, this.Map);
                        }
                        this.MutateTry = false;
                    }
                    else if (MutateRate == 6)
                    {
                        Building_Turret GenTurret = (Building_Turret)ThingMaker.MakeThing(ThingDef.Named("GenTurretBase"));
                        GenTurret.SetFactionDirect(factionDirect);
                        if (hasNoBuildings(Position))
                        {
                            GenSpawn.Spawn(GenTurret, Position, this.Map);
                        }
                        this.MutateTry = false;
                    }
                    else
                    {
                        this.MutateTry = false;
                    }
                }
        }
        }
    }
}

