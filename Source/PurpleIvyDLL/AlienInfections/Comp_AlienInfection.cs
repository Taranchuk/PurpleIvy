﻿using UnityEngine;
using RimWorld;
using Verse;
using System.Linq;
using System.Text;
using Verse.AI;

namespace PurpleIvy
{

    public class AlienInfection : ThingComp
    {
        public int currentCountOfCreatures = 0;
        public int startOfIncubation = 0;
        public int maxNumberOfCreatures = 0;
        public bool prevAngle = true;
        public int tickStartHediff = 0;
        public bool stopSpawning = false;

        public CompProperties_AlienInfection Props => this.props as CompProperties_AlienInfection;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            this.startOfIncubation = Find.TickManager.TicksGame;
        }

        public void DoWiggle()
        {
            if (this.currentCountOfCreatures < this.maxNumberOfCreatures)
            {
                if (this.parent is Corpse || this.parent.def.IsCorpse)
                {
                    var corpse = (Corpse)this.parent;
                    if (this.prevAngle == true)
                    {
                        corpse.InnerPawn.Drawer.renderer.wiggler.downedAngle += 5f;
                        this.prevAngle = false;
                    }
                    else
                    {
                        corpse.InnerPawn.Drawer.renderer.wiggler.downedAngle -= 5f;
                        this.prevAngle = true;
                    }
                    corpse.InnerPawn.Drawer.renderer.wiggler.WigglerTick();
                }
                else if (this.parent is Pawn pawn)
                {
                    if (pawn.Dead)
                    {
                        var corpse = (Corpse)this.parent.ParentHolder;
                        if (this.prevAngle == true)
                        {
                            corpse.InnerPawn.Drawer.renderer.wiggler.downedAngle += 5f;
                            this.prevAngle = false;
                        }
                        else
                        {
                            corpse.InnerPawn.Drawer.renderer.wiggler.downedAngle -= 5f;
                            this.prevAngle = true;
                        }
                        corpse.InnerPawn.Drawer.renderer.wiggler.WigglerTick();
                    }
                    else
                    {
                        Log.Message(this.parent + " not corpse 1");
                    }
                }
                else if (!(this.parent is Building))
                {
                    Log.Message(this.parent + " not corpse 2");
                }
            }
        }

        public bool CheckIfShouldSpawn()
        {
            if (this.parent.AmbientTemperature < -21f) return false;
            if (this.stopSpawning == true) return false;
            if (this.Props.maxNumberOfCreaturesOnMap > 0)
            {
                int count = 0;
                foreach (var type in this.Props.typesOfCreatures)
                {
                    count += this.parent.Map.mapPawns.AllPawns.Where(x => x.kindDef.defName == type).ToList().Count;
                }
                if (this.Props.maxNumberOfCreaturesOnMap <= count) return false;
            }
            return true;
        }

        public void TryStartSpawn()
        {
            if (CheckIfShouldSpawn() != true)
            {
                this.startOfIncubation += 250;
                return;
            }
            if (tickStartHediff > 0 && this.startOfIncubation + 
                this.Props.IncubationData.tickStartHediff.RandomInRange < Find.TickManager.TicksGame)
            {
                if (this.parent is Pawn && this.Props.IncubationData.hediff != null)
                {
                    var hediff = HediffMaker.MakeHediff(HediffDef.Named(this.Props.IncubationData.hediff),
                    (Pawn)this.parent, null);
                    ((Pawn)this.parent).health.AddHediff(hediff, null, null, null);
                    this.tickStartHediff = 0;
                }
            }

            DoWiggle();

            if (this.Props.incubationPeriod.min > 0)
            {
                if (this.startOfIncubation + this.Props.incubationPeriod.RandomInRange
                    < Find.TickManager.TicksGame)
                {
                    if (this.maxNumberOfCreatures <= this.currentCountOfCreatures &&
                        this.Props.resetIncubation == true)
                    {
                        this.startOfIncubation = Find.TickManager.TicksGame;
                        this.currentCountOfCreatures = 0;
                        this.maxNumberOfCreatures = this.Props.maxNumberOfCreatures.RandomInRange;
                    }

                    if (this.startOfIncubation + this.Props.incubationPeriod.RandomInRange
                        + this.Props.ticksPerSpawn.RandomInRange
                        > Find.TickManager.TicksGame)
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            if (this.Props.typesOfCreatures == null) return;
            if (this.maxNumberOfCreatures != 0 && currentCountOfCreatures >= this.maxNumberOfCreatures) return;
            foreach (var newPawn in from defName in this.Props.typesOfCreatures
                                    let numberOfSpawn = this.Props.numberOfCreaturesPerSpawn.RandomInRange
                                    where numberOfSpawn > 0 from i in Enumerable.Range(0, numberOfSpawn)
                                    select PawnKindDef.Named(defName) into pawnKindDef
                                    select PawnGenerator.GeneratePawn(pawnKindDef, null))
            {

                Log.Message(this.parent + " produces " + newPawn, true);
                if (this.Props.ageTick.RandomInRange > 0)
                {
                    var ageTick = this.Props.ageTick.RandomInRange;
                    newPawn.ageTracker.AgeBiologicalTicks = ageTick;
                    newPawn.ageTracker.AgeChronologicalTicks = ageTick;
                }
                else
                {
                    newPawn.ageTracker.AgeBiologicalTicks = 0;
                    newPawn.ageTracker.AgeChronologicalTicks = 0;
                }
                Alien alien = newPawn as Alien;
                switch (this.parent)
                {
                    case Corpse _:
                        HatchFromCorpse(newPawn);
                        break;
                    case Pawn _:
                        HatchFromPawn(newPawn);
                        break;
                    case Building _:
                        GenSpawn.Spawn(newPawn, this.parent.Position, this.parent.Map);
                        if (alien.def == PurpleIvyDefOf.Genny_ParasiteOmega)
                        {
                            alien.canHaul = true;
                        }
                        break;
                    default:
                        Log.Error("Unknown parent. Cant spawn. Parent: " + this.parent);
                        break;
                }
                if (alien.def == PurpleIvyDefOf.Genny_ParasiteNestGuard)
                {
                    alien.canGuard = true;
                    alien.SetFocus();
                    alien.health.AddHediff(PurpleIvyDefOf.PI_Regen);
                }
                currentCountOfCreatures++;
            }
        }
        public override void PostPostMake()
        {
            base.PostPostMake();
            this.maxNumberOfCreatures = Props.maxNumberOfCreatures.RandomInRange;
        }
        public void HatchFromCorpse(Pawn newPawn)
        {
            if (newPawn is Alien alien && alien.def == PurpleIvyDefOf.Genny_ParasiteOmega)
            {
                alien.canHaul = false;
            }
            var corpse = (Corpse)this.parent;
            var compRottable = corpse.TryGetComp<CompRottable>();
            if (compRottable != null && compRottable.Stage < RotStage.Dessicated)
            {
                compRottable.RotProgress += this.Props.rotProgressPerSpawn.RandomInRange;
                FilthMaker.TryMakeFilth(corpse.Position, corpse.Map, corpse.InnerPawn.def.race.BloodDef);
            }
            GenSpawn.Spawn(newPawn, corpse.Position, corpse.Map);
        }

        public void HatchFromPawn(Pawn newPawn)
        {
            if (newPawn is Alien alien && alien.def == PurpleIvyDefOf.Genny_ParasiteOmega)
            {
                alien.canHaul = false;
            }
            var host = (Pawn)this.parent;
            host.TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, 25f));
            if (host.Dead)
            {
                var corpse = (Corpse)this.parent.ParentHolder;
                var compRottable = corpse.TryGetComp<CompRottable>();
                if (compRottable != null && compRottable.Stage < RotStage.Dessicated)
                {
                    compRottable.RotProgress += this.Props.rotProgressPerSpawn.RandomInRange;
                    FilthMaker.TryMakeFilth(corpse.Position, corpse.Map, corpse.InnerPawn.def.race.BloodDef);
                }
                GenSpawn.Spawn(newPawn, corpse.Position, corpse.Map);
            }
            else if (this.Props.IncubationData.deathChance >= Rand.RangeInclusive(0, 100))
            {
                this.parent.Kill();
                var corpse = (Corpse)this.parent.ParentHolder;
                Log.Message("3 Adding infected comp to " + corpse);
                corpse.AllComps.Add(this);
                this.parent.AllComps.Remove(this);
                FilthMaker.TryMakeFilth(corpse.Position, corpse.Map, corpse.InnerPawn.def.race.BloodDef);
                GenSpawn.Spawn(newPawn, corpse.Position, corpse.Map);
            }
            else
            {
                FilthMaker.TryMakeFilth(host.Position, host.Map, host.def.race.BloodDef);
                GenSpawn.Spawn(newPawn, host.Position, host.Map);
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (Find.TickManager.TicksGame % 250 == 0)
            {
                this.TryStartSpawn();
            }
        }
        public override void CompTickRare()
        {
            base.CompTickRare();
            this.TryStartSpawn();
        }

        public override string CompInspectStringExtra()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Concat(new string[]
            {
                "birthTime".Translate(),
                ": ", (this.startOfIncubation + this.Props.incubationPeriod.min
                    - Find.TickManager.TicksGame).ToString(), "~",
                (this.startOfIncubation + this.Props.incubationPeriod.max
                    - Find.TickManager.TicksGame).ToString()
            }));

            stringBuilder.AppendLine(string.Concat(new string[]
            {
                "TypesOfCreatures".Translate(),
                ":"
            }));
            foreach (var type in this.Props.typesOfCreatures)
            {
                stringBuilder.AppendLine(string.Concat(new string[]
                {
                        "\t", type
                }));
            }
            stringBuilder.AppendLine(string.Concat(new string[]
            {
                    "MaxNumberOfCreatures".Translate(),
                    ": ", this.Props.maxNumberOfCreatures.ToString()
            }));
            stringBuilder.AppendLine(string.Concat(new string[]
            {
                    "numberOfCreaturesPerSpawn".Translate(),
                    ": ", this.Props.numberOfCreaturesPerSpawn.min.ToString(), "~",
                    this.Props.numberOfCreaturesPerSpawn.max.ToString()
            }));

            stringBuilder.AppendLine(string.Concat(new string[]
            {
                "incubationPeriod".Translate(),
                ": ", this.Props.incubationPeriod.min.ToString(), "~",
                this.Props.incubationPeriod.max.ToString()
            }));
            stringBuilder.AppendLine(string.Concat(new string[]
            {
                "ageTick".Translate(),
                ": ", this.Props.ageTick.min.ToString(), "~",
                this.Props.ageTick.max.ToString()
            }));
            stringBuilder.AppendLine(string.Concat(new string[]
            {
                "ticksPerSpawn".Translate(),
                ": ", this.Props.ticksPerSpawn.min.ToString(), "~",
                this.Props.ticksPerSpawn.max.ToString()
            }));
            stringBuilder.AppendLine(string.Concat(new string[]
            {
               "rotProgressPerSpawn".Translate(),
               ": ", this.Props.rotProgressPerSpawn.min.ToString(), "~",
               this.Props.rotProgressPerSpawn.max.ToString()
            }));
            stringBuilder.AppendLine(string.Concat(new string[]
            {
               "resetIncubation".Translate(),
               ": ", this.Props.resetIncubation.ToString()
            }));
            stringBuilder.AppendLine(string.Concat(new string[]
            {
               "currentCountOfCreatures".Translate(),
               ": ", this.currentCountOfCreatures.ToString()
            }));
            stringBuilder.AppendLine(string.Concat(new string[]
            {
                "totalNumberOfCreatures".Translate(),
                ": ", this.maxNumberOfCreatures.ToString()
            }));
            stringBuilder.AppendLine(string.Concat(new string[]
            {
               "startOfIncubation".Translate(),
               ": ", this.startOfIncubation.ToString()
            }));
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.currentCountOfCreatures, "currentCountOfCreatures", 0, false);
            Scribe_Values.Look<int>(ref this.startOfIncubation, "startOfIncubation", 0, false);
            Scribe_Values.Look<int>(ref this.maxNumberOfCreatures, "maxNumberOfCreatures", 0, false);
            Scribe_Values.Look<int>(ref this.tickStartHediff, "tickStartHediff", 0, false);
            Scribe_Values.Look<bool>(ref this.stopSpawning, "stopSpawning", false, false);
        }
    }
}

