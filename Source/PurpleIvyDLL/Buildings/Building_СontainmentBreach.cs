using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace PurpleIvy
{
    public class Building_СontainmentBreach : Building_WorkTable, IThingHolder
    {
        public Building_СontainmentBreach()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
        }

        public override void PostMake()
        {
            base.PostMake();
            if (this.def.GetModExtension<DefModExtension_СontainmentBreach>() != null)
            {
                this.maxNumAliens = this.def.GetModExtension<DefModExtension_СontainmentBreach>().maxNumAliens;
                this.blackoutProtection = this.def.GetModExtension<DefModExtension_СontainmentBreach>().blackoutProtection;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = this.TryGetComp<CompPowerTrader>();
            if (this.def.GetModExtension<DefModExtension_СontainmentBreach>() != null)
            {
                this.maxNumAliens = this.def.GetModExtension<DefModExtension_СontainmentBreach>().maxNumAliens;
                this.blackoutProtection = this.def.GetModExtension<DefModExtension_СontainmentBreach>().blackoutProtection;
            }
        }

        public bool HasBloodInAlien(Thing alien)
        {
            bool result = false;
            if (this.RecoveryBloodData != null && this.RecoveryBloodData.ContainsKey(alien))
            {
                if (this.RecoveryBloodData[alien] == 0
                    || this.RecoveryBloodData[alien] < Find.TickManager.TicksGame)
                {
                    result = true;
                }
            }
            else
            {
                result = true;
            }
            return result;
        }

        public bool HasToxinInAlien(Thing alien)
        {
            bool result = false;
            if (this.RecoveryToxinData != null && this.RecoveryToxinData.ContainsKey(alien))
            {
                if (this.RecoveryToxinData[alien] == 0
                    || this.RecoveryToxinData[alien] < Find.TickManager.TicksGame)
                {
                    result = true;
                }
            }
            else
            {
                result = true;
            }
            return result;
        }

        public Thing FindCorpse(ThingCategoryDef category)
        {
            Thing corpse = null;
            var corpses = this.Map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse).
            Where(x => x != null && x.def.thingCategories.Contains(category)).ToList();
            if (corpses != null && corpses.Count > 0)
            {
                corpse = corpses[0];
            }
            return corpse;
        }
        public bool HasJobOnRecipe(Job job, out JobDef jobDef)
        {
            bool result = false;
            jobDef = null;
            foreach (var alien in this.Aliens)
            {
                if (alien is Pawn)
                {
                    if (job.bill.recipe == PurpleIvyDefOf.PI_AlienStudyRecipe &&
                        PurpleIvyData.AlienStudy.Where(x => x.PrerequisitesCompleted && x.TechprintsApplied == 0
                        && this.Map.listerThings.ThingsOfDef(ThingDef.Named("Techprint_" + x.defName))
                        .Count == 0).Count() > 0
                        )
                    {
                        result = true;
                        jobDef = PurpleIvyDefOf.PI_ConductResearchOnAliens;
                        if (result == true) break;
                    }
                    if (job.bill.recipe == PurpleIvyDefOf.DrawAlienBlood)
                    {
                        result = this.HasBloodInAlien(alien);
                        job.targetB = alien;
                        jobDef = PurpleIvyDefOf.PI_DrawAlienBlood;
                        if (result == true) break;
                    }
                    else if (job.bill.recipe == PurpleIvyDefOf.DrawAlphaAlienBlood &&
                        "Genny_ParasiteAlpha" == alien.def.defName)
                    {
                        result = this.HasBloodInAlien(alien);
                        job.targetB = alien;
                        jobDef = PurpleIvyDefOf.PI_DrawAlienBlood;
                        if (result == true) break;
                    }
                    else if (job.bill.recipe == PurpleIvyDefOf.DrawBetaAlienBlood
                        && "Genny_ParasiteBeta" == alien.def.defName)
                    {
                        result = this.HasBloodInAlien(alien);
                        job.targetB = alien;
                        jobDef = PurpleIvyDefOf.PI_DrawAlienBlood;
                        if (result == true) break;
                    }
                    else if (job.bill.recipe == PurpleIvyDefOf.DrawGammaAlienBlood
                        && "Genny_ParasiteGamma" == alien.def.defName)
                    {
                        result = this.HasBloodInAlien(alien);
                        job.targetB = alien;
                        jobDef = PurpleIvyDefOf.PI_DrawAlienBlood;
                        if (result == true) break;
                    }
                    else if (job.bill.recipe == PurpleIvyDefOf.DrawOmegaAlienBlood && "Genny_ParasiteOmega" == alien.def.defName)
                    {
                        result = this.HasBloodInAlien(alien);
                        job.targetB = alien;
                        jobDef = PurpleIvyDefOf.PI_DrawAlienBlood;
                        if (result == true) break;
                    }
                    else if (job.bill.recipe == PurpleIvyDefOf.DrawGuardAlienBlood && "Genny_ParasiteNestGuard" == alien.def.defName)
                    {
                        result = this.HasBloodInAlien(alien);
                        job.targetB = alien;
                        jobDef = PurpleIvyDefOf.PI_DrawAlienBlood;
                        if (result == true) break;
                    }
                    else if (job.bill.recipe == PurpleIvyDefOf.DrawKorsolianToxin)
                    {
                        result = this.HasToxinInAlien(alien);
                        job.targetB = alien;
                        jobDef = PurpleIvyDefOf.PI_DrawKorsolianToxin;
                        if (result == true) break;
                    }
                    else if (job.bill.recipe == PurpleIvyDefOf.PreciseVivisectionAlpha
                        && "Genny_ParasiteAlpha" == alien.def.defName)
                    {
                        result = true;
                        job.targetB = alien;
                        jobDef = PurpleIvyDefOf.PI_PreciseVivisection;
                        break;
                    }
                    else if (job.bill.recipe == PurpleIvyDefOf.PreciseVivisectionBeta &&
                        "Genny_ParasiteBeta" == alien.def.defName)
                    {
                        result = true;
                        job.targetB = alien;
                        jobDef = PurpleIvyDefOf.PI_PreciseVivisection;
                        break;
                    }
                    else if (job.bill.recipe == PurpleIvyDefOf.PreciseVivisectionGamma &&
                        "Genny_ParasiteGamma" == alien.def.defName)
                    {
                        result = true;
                        job.targetB = alien;
                        jobDef = PurpleIvyDefOf.PI_PreciseVivisection;
                        break;
                    }
                    else if (job.bill.recipe == PurpleIvyDefOf.PreciseVivisectionOmega
                        && "Genny_ParasiteOmega" == alien.def.defName)
                    {
                        result = true;
                        job.targetB = alien;
                        jobDef = PurpleIvyDefOf.PI_PreciseVivisection;
                        break;
                    }
                    else if (job.bill.recipe == PurpleIvyDefOf.PreciseVivisectionGuard
                         && "Genny_ParasiteNestGuard" == alien.def.defName)
                    {
                        result = true;
                        job.targetB = alien;
                        jobDef = PurpleIvyDefOf.PI_PreciseVivisection;
                        break;
                    }
                    else if (job.bill.recipe == PurpleIvyDefOf.PreciseVivisectionQueen
                        && "Genny_Queen" == alien.def.defName)
                    {
                        result = true;
                        job.targetB = alien;
                        jobDef = PurpleIvyDefOf.PI_PreciseVivisection;
                        break;
                    }
                }
            }
            if (result == false)
            {
                if (job.bill.recipe == PurpleIvyDefOf.PreciseVivisectionAlpha)
                {
                    var corpse = this.FindCorpse(PurpleIvyDefOf.CorpsesAlienParasiteAlpha);
                    if (corpse != null)
                    {
                        result = true;
                        jobDef = PurpleIvyDefOf.PI_PreciseVivisection;
                        job.targetB = corpse;
                    }
                }
                else if (job.bill.recipe == PurpleIvyDefOf.PreciseVivisectionBeta)
                {
                    var corpse = this.FindCorpse(PurpleIvyDefOf.CorpsesAlienParasiteBeta);
                    if (corpse != null)
                    {
                        result = true;
                        jobDef = PurpleIvyDefOf.PI_PreciseVivisection;
                        job.targetB = corpse;
                    }
                }
                else if (job.bill.recipe == PurpleIvyDefOf.PreciseVivisectionGamma)
                {
                    var corpse = this.FindCorpse(PurpleIvyDefOf.CorpsesAlienParasiteGamma);
                    if (corpse != null)
                    {
                        result = true;
                        jobDef = PurpleIvyDefOf.PI_PreciseVivisection;
                        job.targetB = corpse;
                    }
                }
                else if (job.bill.recipe == PurpleIvyDefOf.PreciseVivisectionOmega)
                {
                    var corpse = this.FindCorpse(PurpleIvyDefOf.CorpsesAlienParasiteOmega);
                    if (corpse != null)
                    {
                        result = true;
                        jobDef = PurpleIvyDefOf.PI_PreciseVivisection;
                        job.targetB = corpse;
                    }
                }

                else if (job.bill.recipe == PurpleIvyDefOf.PreciseVivisectionGuard)
                {
                    var corpse = this.FindCorpse(PurpleIvyDefOf.CorpsesAlienParasiteGuard);
                    if (corpse != null)
                    {
                        result = true;
                        jobDef = PurpleIvyDefOf.PI_PreciseVivisection;
                        job.targetB = corpse;
                    }
                }

                else if (job.bill.recipe == PurpleIvyDefOf.PreciseVivisectionQueen)
                {
                    var corpse = this.FindCorpse(PurpleIvyDefOf.CorpsesAlienParasiteQueen);
                    if (corpse != null)
                    {
                        result = true;
                        jobDef = PurpleIvyDefOf.PI_PreciseVivisection;
                        job.targetB = corpse;
                    }
                }
            }
            return result;
        }

        public bool AlienHasJobOnRecipe(Thing alien, RecipeDef recipe)
        {
            bool result = false;
            if (recipe == PurpleIvyDefOf.DrawAlienBlood)
            {
                result = this.HasBloodInAlien(alien);
            }
            else if (recipe == PurpleIvyDefOf.DrawAlphaAlienBlood)
            {
                if ("Genny_ParasiteAlpha" == alien.def.defName)
                {
                    result = this.HasBloodInAlien(alien);
                }
            }
            else if (recipe == PurpleIvyDefOf.DrawBetaAlienBlood)
            {
                if ("Genny_ParasiteBeta" == alien.def.defName)
                {
                    result = this.HasBloodInAlien(alien);
                }
            }
            else if (recipe == PurpleIvyDefOf.DrawGammaAlienBlood)
            {
                if ("Genny_ParasiteGamma" == alien.def.defName)
                {
                    result = this.HasBloodInAlien(alien);
                }
            }
            else if (recipe == PurpleIvyDefOf.DrawOmegaAlienBlood)
            {
                if ("Genny_ParasiteOmega" == alien.def.defName)
                {
                    result = this.HasBloodInAlien(alien);
                }
            }
            else if (recipe == PurpleIvyDefOf.DrawGuardAlienBlood)
            {
                if ("Genny_ParasiteNestGuard" == alien.def.defName)
                {
                    result = this.HasBloodInAlien(alien);
                }
            }
            else if (recipe == PurpleIvyDefOf.DrawKorsolianToxin)
            {
                result = this.HasToxinInAlien(alien);
            }
            return result;
        }

        public ThingDef GetAlienBloodByRecipe(RecipeDef recipe)
        {
            ThingDef AlienBlood = null;
            foreach (var alien in this.Aliens)
            {
                if (this.AlienHasJobOnRecipe(alien, recipe))
                {
                    string bloodType = "PI_" + alien.def.defName.Replace("Genny_Parasite", "") + "Blood";
                    AlienBlood = DefDatabase<ThingDef>.GetNamed(bloodType, false);
                    if (this.RecoveryBloodData == null)
                    {
                        this.RecoveryBloodData = new Dictionary<Thing, int>();
                    }
                    this.RecoveryBloodData[alien] = new IntRange(30000, 60000).RandomInRange + Find.TickManager.TicksGame;
                    break;
                }
            }
            return AlienBlood;
        }

        public ThingDef GetKorsolianToxin(RecipeDef recipe)
        {
            ThingDef KorsolianToxin = null;
            foreach (var alien in this.Aliens)
            {
                if (this.AlienHasJobOnRecipe(alien, recipe))
                {
                    KorsolianToxin = PurpleIvyDefOf.PI_KorsolianToxin;
                    if (this.RecoveryToxinData == null)
                    {
                        this.RecoveryToxinData = new Dictionary<Thing, int>();
                    }
                    this.RecoveryToxinData[alien] = new IntRange(30000, 60000).RandomInRange + Find.TickManager.TicksGame;
                    break;
                }
            }
            return KorsolianToxin;
        }
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (Aliens.Count > 0)
            {
                foreach (var alien in Aliens)
                {
                    var str = alien.Label;
                    if (this.RecoveryBloodData != null && this.RecoveryBloodData.ContainsKey(alien))
                    {
                        str += " - " + (this.RecoveryBloodData[alien] - Find.TickManager.TicksGame);
                    }
                    else
                    {
                        str += " - 0";
                    }
                    if (this.RecoveryToxinData != null && this.RecoveryToxinData.ContainsKey(alien))
                    {
                        str += " - " + (this.RecoveryToxinData[alien] - Find.TickManager.TicksGame);
                    }
                    else
                    {
                        str += " - 0";
                    }
                    stringBuilder.Append(str + "\n");
                }
            }
            stringBuilder.Append(base.GetInspectString());
            return stringBuilder.ToString();
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public List<Thing> Aliens
        {
            get
            {
                List<Thing> Aliens = new List<Thing>();
                for (int i = 0; i < this.innerContainer.Count; i++)
                {

                    Aliens.Add(this.innerContainer[i]);
                }
                return Aliens;
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (this.startOpenContainer == 0 && this.blackoutProtection.min > 0 && this.powerComp != null && !this.powerComp.PowerOn)
            {
                this.startOpenContainer = Find.TickManager.TicksGame + this.blackoutProtection.RandomInRange;
            }
            else if (this.startOpenContainer > 0 && Find.TickManager.TicksGame > this.startOpenContainer)
            {
                foreach (var pawn in this.Aliens)
                {
                    if (pawn is Pawn alien)
                    {
                        Log.Message("Gained");
                        alien.health.AddHediff(PurpleIvyDefOf.PI_GainConsciousness);
                    }
                }
                this.innerContainer.TryDropAll(this.Position, this.Map, ThingPlaceMode.Near);
                this.startOpenContainer = 0;
                this.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 50f));
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            IEnumerator<Gizmo> enumerator = null;

            if (base.Faction == Faction.OfPlayer && this.innerContainer.Count > 0)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.action = new Action(this.EjectContents);
                command_Action.defaultLabel = "AlienEject".Translate();
                command_Action.defaultDesc = "AlienEjectDesc".Translate();
                if (this.innerContainer.Count == 0)
                {
                    command_Action.Disable("CommandPodEjectFailEmpty".Translate());
                }
                command_Action.hotKey = KeyBindingDefOf.Misc8;
                command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject", true);
                yield return command_Action;
            }
            yield break;
        }

        public virtual void EjectContents()
        {
            this.innerContainer.TryDropAll(this.InteractionCell, base.Map, ThingPlaceMode.Direct, null, null);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.startOpenContainer, "startOpenContainer", 0);
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
            Scribe_Collections.Look<Thing, int>(ref this.RecoveryBloodData, "RecoveryBloodData",
                LookMode.Reference, LookMode.Value, ref this.RecoveryBloodDataKeys, ref this.RecoveryBloodDataValues);
            Scribe_Collections.Look<Thing, int>(ref this.RecoveryToxinData, "RecoveryToxinData",
                LookMode.Reference, LookMode.Value, ref this.RecoveryToxinDataKeys, ref this.RecoveryToxinDataValues);
        }

        public Dictionary<Thing, int> RecoveryBloodData = new Dictionary<Thing, int>();

        public List<Thing> RecoveryBloodDataKeys = new List<Thing>();

        public List<int> RecoveryBloodDataValues = new List<int>();

        public Dictionary<Thing, int> RecoveryToxinData = new Dictionary<Thing, int>();

        public List<Thing> RecoveryToxinDataKeys = new List<Thing>();

        public List<int> RecoveryToxinDataValues = new List<int>();

        public ThingOwner innerContainer;

        public int maxNumAliens;

        public int startOpenContainer;

        public IntRange blackoutProtection;

        public CompPowerTrader powerComp;
    }
}

