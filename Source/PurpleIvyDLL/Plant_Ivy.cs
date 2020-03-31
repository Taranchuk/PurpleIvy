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
        private int SpreadTick;
        private int OrigSpreadTick;
        private int TickCount;
        private bool MutateTry;
        Faction factionDirect = Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamed("Genny", true));
        DamageDef dmgdef = DefDatabase<DamageDef>.GetNamed("Scratch", true);

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            System.Random random = new System.Random();
            SpreadTick = random.Next(15, 25);
            OrigSpreadTick = SpreadTick;
            MutateTry = true;
        }

        public void SpawnIvy(IntVec3 dir)
        {
            if (!GenCollection.Any<Thing>(GridsUtility.GetThingList(dir, Map), (Thing t) =>          (t.def.IsBuildingArtificial || t.def.IsNonResourceNaturalRock | t.def.defName == "PurpleIvy")))
            {
                foreach (var t in GridsUtility.GetThingList(dir, this.Map))
                {
                    Log.Message(t.Label);
                }
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
            //List all things in that random direction cell

            //Loop over things if there are things
            if (list != null && list.Count > 0)
            {

                //iterate through things
                //for (int i = list.Count - 1; i >= 0; i--) -- backwards iteration, incase i need it later

                for (int i = 0; i < list.Count; i++)
                {

                    //If we find a corpse
                    if (list[i] != null && list[i].Faction != factionDirect && list[i].def.IsCorpse)
                    {
                        Corpse corpse = (Corpse)list[i];
                        if (corpse.TryGetComp<AlienInfection>() == null)
                        {
                            CompProperties_AlienInfection compProperties = new CompProperties_AlienInfection();
                            compProperties.compClass = typeof(AlienInfection);
                            compProperties.numberOfCreaturesPerSpawn = 1;
                            compProperties.typesOfCreatures = new List<string>()
                            {
                                "Genny_ParasiteOmega"
                            };
                            compProperties.maxNumberOfCreatures = 20;
                            AlienInfection infected = new AlienInfection
                            {
                                parent = corpse
                            };
                            corpse.AllComps.Add(infected);
                            infected.Initialize(compProperties);
                        }

                        //speedup the spread a little
                        SpreadTick--;
                        SpreadTick--;
                        SpreadTick--;
                    }
                    //If we find a pawn
                    if (list[i] != null && list[i] is Pawn)
                    {

                        //And its not a hatchling
                        if (list[i] != null && list[i].Faction != factionDirect)
                        {
                            //Could do some mutatey zombie stuff here, but for now save the pawn and injur it outside this loop
                            Thing stuckPawn = list[i];
                            DamageInfo damageInfo = new DamageInfo(this.dmgdef, 1, 0f, -1f, this, null, null);
                            stuckPawn.TakeDamage(damageInfo);
                        }
                    }
                }
            }
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
            foreach (Thing thing in this.Map.thingGrid.ThingsAt(this.Position))
            {
                if (thing.def.defName.Contains("Spores"))
                {
                    thing.Graphic.drawSize.x = this.Growth;
                    thing.Graphic.drawSize.y = this.Growth;
                    thing.Graphic.color.a = 100 - (this.Growth * 100);
                    //thing.Graphic.color.r = 0;// 100 - (this.Growth * 100);
                    //thing.Graphic.color.g = 0;// 100 - (this.Growth * 100);
                    //thing.Graphic.color.b = 0;// 100 - (this.Growth * 100);
                    return;
                }
            }
            if (GenGrid.InBounds(this.Position, this.Map))
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
            }
        }


        public override void Tick()
        {
            base.Tick();
            if (this.Growth >= 0.25f)
            {
                foreach (Thing thing in this.Map.thingGrid.ThingsListAt(this.Position))
                {
                    if (thing is Pawn && thing.Faction != factionDirect)
                    {
                        Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.PoisonousPurpleHediff,
                            (Pawn)thing, null);
                        hediff.Severity = 0.1f;
                        ((Pawn)thing).health.AddHediff(hediff, null, null, null);
                        Hediff hediff2 = HediffMaker.MakeHediff(HediffDefOf.HarmfulBacteriaHediff,
                        (Pawn)thing, null);
                        hediff2.Severity = 0.1f;
                        ((Pawn)thing).health.AddHediff(hediff2, null, null, null);
                    }
                }
                if (Find.TickManager.TicksGame % 250 == 0)
                {
                    foreach (Thing thing in this.Map.thingGrid.ThingsAt(this.Position))
                    {
                        Plant plant = thing as Plant;
                        if (plant != null && plant.def.defName != "PurpleIvy")
                        {
                            plant.TakeDamage(new DamageInfo(DamageDefOf.Flame, 5));
                        }
                    }
                }
                this.ThrowGasOrAdjustGasSize();
            }
            if (Find.TickManager.TicksGame % 350 == 0)
            {
                base.TickLong();
                this.SpreadTick--;
                if (this.growthInt >= 1)
                {
                    //check things in cell and react
                    CheckThings(Position);
                }
                if (this.SpreadTick == 0)
                {
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
                                    //if (dir.GetPlant(this.Map) == null)
                                    //{
                                    //    //no plant, move on
                                    //}
                                    //else
                                    //{
                                    //    //Found plant, Kill it
                                    //    Plant plant = dir.GetPlant(this.Map);
                                    //    plant.Destroy();
                                    //}
                                    //Spawn more Ivy
                                    SpawnIvy(dir);
                                }
                            }
                            //its water or something I dont know of
                            else
                            {

                            }
                        }
                    }
                    SpreadTick = OrigSpreadTick;
                }
                if (this.MutateTry == true)
                {
                    System.Random random = new System.Random();
                    int MutateRate = random.Next(1, 200);
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

