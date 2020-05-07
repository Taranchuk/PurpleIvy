using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    public class WorkGiver_DoBioBillBase : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.InteractionCell;
            }
        }

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Some;
        }

        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                if (this.def.fixedBillGiverDefs != null && this.def.fixedBillGiverDefs.Count == 1)
                {
                    return ThingRequest.ForDef(this.def.fixedBillGiverDefs[0]);
                }
                return ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver);
            }
        }

        public static void ResetStaticData()
        {
            WorkGiver_DoBioBillBase.MissingMaterialsTranslated = "MissingMaterials".Translate();
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.PotentialBillGiver);
            for (int i = 0; i < list.Count; i++)
            {
                IBillGiver billGiver;
                if ((billGiver = (list[i] as IBillGiver)) != null && this.ThingIsUsableBillGiver(list[i]) && billGiver.BillStack.AnyShouldDoNow)
                {
                    return false;
                }
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            IBillGiver billGiver = thing as IBillGiver;
            if (billGiver == null || !this.ThingIsUsableBillGiver(thing) || !billGiver.BillStack.AnyShouldDoNow || !billGiver.UsableForBillsAfterFueling() || !pawn.CanReserve(thing, 1, -1, null, forced) || thing.IsBurning() || thing.IsForbidden(pawn))
            {
                return null;
            }
            CompRefuelable compRefuelable = thing.TryGetComp<CompRefuelable>();
            if (compRefuelable == null || compRefuelable.HasFuel)
            {
                billGiver.BillStack.RemoveIncompletableBills();
                return this.StartOrResumeBillJob(pawn, billGiver);
            }
            if (!RefuelWorkGiverUtility.CanRefuel(pawn, thing, forced))
            {
                return null;
            }
            return RefuelWorkGiverUtility.RefuelJob(pawn, thing, forced, null, null);
        }

        private static UnfinishedThing ClosestUnfinishedThingForBill(Pawn pawn, Bill_ProductionWithUft bill)
        {
            Predicate<Thing> predicate = (Thing t) => !t.IsForbidden(pawn) && ((UnfinishedThing)t).Recipe == bill.recipe && ((UnfinishedThing)t).Creator == pawn && ((UnfinishedThing)t).ingredients.TrueForAll((Thing x) => bill.IsFixedOrAllowedIngredient(x.def)) && pawn.CanReserve(t, 1, -1, null, false);
            IntVec3 position = pawn.Position;
            Map map = pawn.Map;
            ThingRequest thingReq = ThingRequest.ForDef(bill.recipe.unfinishedThingDef);
            PathEndMode peMode = PathEndMode.InteractionCell;
            TraverseParms traverseParams = TraverseParms.For(pawn, pawn.NormalMaxDanger(), TraverseMode.ByPawn, false);
            Predicate<Thing> validator = predicate;
            return (UnfinishedThing)GenClosest.ClosestThingReachable(position, map, thingReq, peMode, traverseParams, 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
        }

        private static Job FinishUftJob(Pawn pawn, UnfinishedThing uft, Bill_ProductionWithUft bill)
        {
            if (uft.Creator != pawn)
            {
                Log.Error(string.Concat(new object[]
                {
                    "Tried to get FinishUftJob for ",
                    pawn,
                    " finishing ",
                    uft,
                    " but its creator is ",
                    uft.Creator
                }), false);
                return null;
            }
            Job job = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, bill.billStack.billGiver, uft);
            if (job != null && job.targetA.Thing != uft)
            {
                return job;
            }
            Job job2 = JobMaker.MakeJob(JobDefOf.DoBill, (Thing)bill.billStack.billGiver);
            job2.bill = bill;
            job2.targetQueueB = new List<LocalTargetInfo>
            {
                uft
            };
            job2.countQueue = new List<int>
            {
                1
            };
            job2.haulMode = HaulMode.ToCellNonStorage;
            return job2;
        }

        private Job StartOrResumeBillJob(Pawn pawn, IBillGiver giver)
        {
            for (int i = 0; i < giver.BillStack.Count; i++)
            {
                Bill bill = giver.BillStack[i];
                if ((bill.recipe.requiredGiverWorkType == null || bill.recipe.requiredGiverWorkType == this.def.workType) && (Find.TickManager.TicksGame >= bill.lastIngredientSearchFailTicks + WorkGiver_DoBioBillBase.ReCheckFailedBillTicksRange.RandomInRange || FloatMenuMakerMap.makingFor == pawn))
                {
                    bill.lastIngredientSearchFailTicks = 0;
                    if (bill.ShouldDoNow() && bill.PawnAllowedToStartAnew(pawn))
                    {
                        SkillRequirement skillRequirement = bill.recipe.FirstSkillRequirementPawnDoesntSatisfy(pawn);
                        if (skillRequirement != null)
                        {
                            JobFailReason.Is("UnderRequiredSkill".Translate(skillRequirement.minLevel), bill.Label);
                        }
                        else
                        {
                            Bill_ProductionWithUft bill_ProductionWithUft = bill as Bill_ProductionWithUft;
                            if (bill_ProductionWithUft != null)
                            {
                                if (bill_ProductionWithUft.BoundUft != null)
                                {
                                    if (bill_ProductionWithUft.BoundWorker == pawn && pawn.CanReserveAndReach(bill_ProductionWithUft.BoundUft, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false) && !bill_ProductionWithUft.BoundUft.IsForbidden(pawn))
                                    {
                                        return WorkGiver_DoBioBillBase.FinishUftJob(pawn, bill_ProductionWithUft.BoundUft, bill_ProductionWithUft);
                                    }
                                    goto IL_199;
                                }
                                else
                                {
                                    UnfinishedThing unfinishedThing = WorkGiver_DoBioBillBase.ClosestUnfinishedThingForBill(pawn, bill_ProductionWithUft);
                                    if (unfinishedThing != null)
                                    {
                                        return WorkGiver_DoBioBillBase.FinishUftJob(pawn, unfinishedThing, bill_ProductionWithUft);
                                    }
                                }
                            }
                            if (WorkGiver_DoBioBillBase.TryFindBestBillIngredients(bill, pawn, (Thing)giver, this.chosenIngThings))
                            {
                                Job job;
                                Job result = WorkGiver_DoBioBillBase.TryStartNewDoBillJob(pawn, bill, giver, this.chosenIngThings, out job, true);
                                this.chosenIngThings.Clear();
                                return result;
                            }
                            if (FloatMenuMakerMap.makingFor != pawn)
                            {
                                bill.lastIngredientSearchFailTicks = Find.TickManager.TicksGame;
                            }
                            else
                            {
                                JobFailReason.Is(WorkGiver_DoBioBillBase.MissingMaterialsTranslated, bill.Label);
                            }
                            this.chosenIngThings.Clear();
                        }
                    }
                }
            IL_199:;
            }
            this.chosenIngThings.Clear();
            return null;
        }

        public static Job TryStartNewDoBillJob(Pawn pawn, Bill bill, IBillGiver giver, List<ThingCount> chosenIngThings, out Job haulOffJob, bool dontCreateJobIfHaulOffRequired = true)
        {
            haulOffJob = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, giver, null);
            if (haulOffJob != null && dontCreateJobIfHaulOffRequired)
            {
                return haulOffJob;
            }
            Job job = JobMaker.MakeJob(JobDefOf.DoBill, (Thing)giver);
            job.targetQueueB = new List<LocalTargetInfo>(chosenIngThings.Count);
            job.countQueue = new List<int>(chosenIngThings.Count);
            for (int i = 0; i < chosenIngThings.Count; i++)
            {
                job.targetQueueB.Add(chosenIngThings[i].Thing);
                job.countQueue.Add(chosenIngThings[i].Count);
            }
            job.haulMode = HaulMode.ToCellNonStorage;
            job.bill = bill;
            return job;
        }

        public bool ThingIsUsableBillGiver(Thing thing)
        {
            Pawn pawn = thing as Pawn;
            Corpse corpse = thing as Corpse;
            Pawn pawn2 = null;
            if (corpse != null)
            {
                pawn2 = corpse.InnerPawn;
            }
            if (this.def.fixedBillGiverDefs != null && this.def.fixedBillGiverDefs.Contains(thing.def))
            {
                return true;
            }
            if (pawn != null)
            {
                if (this.def.billGiversAllHumanlikes && pawn.RaceProps.Humanlike)
                {
                    return true;
                }
                if (this.def.billGiversAllMechanoids && pawn.RaceProps.IsMechanoid)
                {
                    return true;
                }
                if (this.def.billGiversAllAnimals && pawn.RaceProps.Animal)
                {
                    return true;
                }
            }
            if (corpse != null && pawn2 != null)
            {
                if (this.def.billGiversAllHumanlikesCorpses && pawn2.RaceProps.Humanlike)
                {
                    return true;
                }
                if (this.def.billGiversAllMechanoidsCorpses && pawn2.RaceProps.IsMechanoid)
                {
                    return true;
                }
                if (this.def.billGiversAllAnimalsCorpses && pawn2.RaceProps.Animal)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool TryFindBestBillIngredients(Bill bill, Pawn pawn, Thing billGiver, List<ThingCount> chosen)
        {
            chosen.Clear();
            WorkGiver_DoBioBillBase.newRelevantThings.Clear();
            if (bill.recipe.ingredients.Count == 0)
            {
                return true;
            }
            IntVec3 rootCell = WorkGiver_DoBioBillBase.GetBillGiverRootCell(billGiver, pawn);
            Region rootReg = rootCell.GetRegion(pawn.Map, RegionType.Set_Passable);
            if (rootReg == null)
            {
                return false;
            }
            WorkGiver_DoBioBillBase.MakeIngredientsListInProcessingOrder(WorkGiver_DoBioBillBase.ingredientsOrdered, bill);
            WorkGiver_DoBioBillBase.relevantThings.Clear();
            WorkGiver_DoBioBillBase.processedThings.Clear();
            bool foundAll = false;
            Predicate<Thing> baseValidator = (Thing t) => t.Spawned && PurpleIvyData.BioStudy.ContainsKey(t.def)
            && PurpleIvyData.BioStudy[t.def].Where(x => x.PrerequisitesCompleted && !x.IsFinished
            && t.Map.listerThings.ThingsOfDef(ThingDef.Named("Techprint_" + x.defName)).Count == 0).Count() > 0
            && !t.IsForbidden(pawn) && (float)(t.Position - billGiver.Position).LengthHorizontalSquared
            < bill.ingredientSearchRadius * bill.ingredientSearchRadius && bill.IsFixedOrAllowedIngredient(t)
            && bill.recipe.ingredients.Any((IngredientCount ingNeed) => ingNeed.filter.Allows(t))
            && pawn.CanReserve(t, 1, -1, null, false);
            bool billGiverIsPawn = billGiver is Pawn;
            if (billGiverIsPawn)
            {
                WorkGiver_DoBioBillBase.AddEveryMedicineToRelevantThings(pawn, billGiver, WorkGiver_DoBioBillBase.relevantThings, baseValidator, pawn.Map);
                if (WorkGiver_DoBioBillBase.TryFindBestBillIngredientsInSet(WorkGiver_DoBioBillBase.relevantThings, bill, chosen, rootCell, billGiverIsPawn))
                {
                    WorkGiver_DoBioBillBase.relevantThings.Clear();
                    WorkGiver_DoBioBillBase.ingredientsOrdered.Clear();
                    return true;
                }
            }
            TraverseParms traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
            RegionEntryPredicate entryCondition = null;
            if (Math.Abs(999f - bill.ingredientSearchRadius) >= 1f)
            {
                float radiusSq = bill.ingredientSearchRadius * bill.ingredientSearchRadius;
                entryCondition = delegate (Region from, Region r)
                {
                    if (!r.Allows(traverseParams, false))
                    {
                        return false;
                    }
                    CellRect extentsClose = r.extentsClose;
                    int num = Math.Abs(billGiver.Position.x - Math.Max(extentsClose.minX, Math.Min(billGiver.Position.x, extentsClose.maxX)));
                    if ((float)num > bill.ingredientSearchRadius)
                    {
                        return false;
                    }
                    int num2 = Math.Abs(billGiver.Position.z - Math.Max(extentsClose.minZ, Math.Min(billGiver.Position.z, extentsClose.maxZ)));
                    return (float)num2 <= bill.ingredientSearchRadius && (float)(num * num + num2 * num2) <= radiusSq;
                };
            }
            else
            {
                entryCondition = ((Region from, Region r) => r.Allows(traverseParams, false));
            }
            int adjacentRegionsAvailable = rootReg.Neighbors.Count((Region region) => entryCondition(rootReg, region));
            int regionsProcessed = 0;
            WorkGiver_DoBioBillBase.processedThings.AddRange(WorkGiver_DoBioBillBase.relevantThings);
            RegionProcessor regionProcessor = delegate (Region r)
            {
                List<Thing> list = r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver));
                for (int i = 0; i < list.Count; i++)
                {
                    Thing thing = list[i];
                    if (!WorkGiver_DoBioBillBase.processedThings.Contains(thing) && ReachabilityWithinRegion.ThingFromRegionListerReachable(thing, r, PathEndMode.ClosestTouch, pawn) && baseValidator(thing) && !(thing.def.IsMedicine & billGiverIsPawn))
                    {
                        WorkGiver_DoBioBillBase.newRelevantThings.Add(thing);
                        WorkGiver_DoBioBillBase.processedThings.Add(thing);
                    }
                }
                regionsProcessed++;
                if (WorkGiver_DoBioBillBase.newRelevantThings.Count > 0 && regionsProcessed > adjacentRegionsAvailable)
                {
                    WorkGiver_DoBioBillBase.relevantThings.AddRange(WorkGiver_DoBioBillBase.newRelevantThings);
                    WorkGiver_DoBioBillBase.newRelevantThings.Clear();
                    if (WorkGiver_DoBioBillBase.TryFindBestBillIngredientsInSet(WorkGiver_DoBioBillBase.relevantThings, bill, chosen, rootCell, billGiverIsPawn))
                    {
                        foundAll = true;
                        return true;
                    }
                }
                return false;
            };
            RegionTraverser.BreadthFirstTraverse(rootReg, entryCondition, regionProcessor, 99999, RegionType.Set_Passable);
            WorkGiver_DoBioBillBase.relevantThings.Clear();
            WorkGiver_DoBioBillBase.newRelevantThings.Clear();
            WorkGiver_DoBioBillBase.processedThings.Clear();
            WorkGiver_DoBioBillBase.ingredientsOrdered.Clear();
            return foundAll;
        }

        private static IntVec3 GetBillGiverRootCell(Thing billGiver, Pawn forPawn)
        {
            Building building = billGiver as Building;
            if (building == null)
            {
                return billGiver.Position;
            }
            if (building.def.hasInteractionCell)
            {
                return building.InteractionCell;
            }
            Log.Error("Tried to find bill ingredients for " + billGiver + " which has no interaction cell.", false);
            return forPawn.Position;
        }

        private static void AddEveryMedicineToRelevantThings(Pawn pawn, Thing billGiver, List<Thing> relevantThings, Predicate<Thing> baseValidator, Map map)
        {
            MedicalCareCategory medicalCareCategory = WorkGiver_DoBioBillBase.GetMedicalCareCategory(billGiver);
            List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine);
            WorkGiver_DoBioBillBase.tmpMedicine.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                Thing thing = list[i];
                if (medicalCareCategory.AllowsMedicine(thing.def) && baseValidator(thing) && pawn.CanReach(thing, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
                {
                    WorkGiver_DoBioBillBase.tmpMedicine.Add(thing);
                }
            }
            WorkGiver_DoBioBillBase.tmpMedicine.SortBy((Thing x) => -x.GetStatValue(StatDefOf.MedicalPotency, true), (Thing x) => x.Position.DistanceToSquared(billGiver.Position));
            relevantThings.AddRange(WorkGiver_DoBioBillBase.tmpMedicine);
            WorkGiver_DoBioBillBase.tmpMedicine.Clear();
        }

        private static MedicalCareCategory GetMedicalCareCategory(Thing billGiver)
        {
            Pawn pawn = billGiver as Pawn;
            if (pawn != null && pawn.playerSettings != null)
            {
                return pawn.playerSettings.medCare;
            }
            return MedicalCareCategory.Best;
        }

        private static void MakeIngredientsListInProcessingOrder(List<IngredientCount> ingredientsOrdered, Bill bill)
        {
            ingredientsOrdered.Clear();
            if (bill.recipe.productHasIngredientStuff)
            {
                ingredientsOrdered.Add(bill.recipe.ingredients[0]);
            }
            for (int i = 0; i < bill.recipe.ingredients.Count; i++)
            {
                if (!bill.recipe.productHasIngredientStuff || i != 0)
                {
                    IngredientCount ingredientCount = bill.recipe.ingredients[i];
                    if (ingredientCount.IsFixedIngredient)
                    {
                        ingredientsOrdered.Add(ingredientCount);
                    }
                }
            }
            for (int j = 0; j < bill.recipe.ingredients.Count; j++)
            {
                IngredientCount item = bill.recipe.ingredients[j];
                if (!ingredientsOrdered.Contains(item))
                {
                    ingredientsOrdered.Add(item);
                }
            }
        }

        private static bool TryFindBestBillIngredientsInSet(List<Thing> availableThings, Bill bill, List<ThingCount> chosen, IntVec3 rootCell, bool alreadySorted)
        {
            if (bill.recipe.allowMixingIngredients)
            {
                return WorkGiver_DoBioBillBase.TryFindBestBillIngredientsInSet_AllowMix(availableThings, bill, chosen);
            }
            return WorkGiver_DoBioBillBase.TryFindBestBillIngredientsInSet_NoMix(availableThings, bill, chosen, rootCell, alreadySorted);
        }

        private static bool TryFindBestBillIngredientsInSet_NoMix(List<Thing> availableThings, Bill bill, List<ThingCount> chosen, IntVec3 rootCell, bool alreadySorted)
        {
            if (!alreadySorted)
            {
                Comparison<Thing> comparison = delegate (Thing t1, Thing t2)
                {
                    float num5 = (float)(t1.Position - rootCell).LengthHorizontalSquared;
                    float value = (float)(t2.Position - rootCell).LengthHorizontalSquared;
                    return num5.CompareTo(value);
                };
                availableThings.Sort(comparison);
            }
            RecipeDef recipe = bill.recipe;
            chosen.Clear();
            WorkGiver_DoBioBillBase.availableCounts.Clear();
            WorkGiver_DoBioBillBase.availableCounts.GenerateFrom(availableThings);
            for (int i = 0; i < WorkGiver_DoBioBillBase.ingredientsOrdered.Count; i++)
            {
                IngredientCount ingredientCount = recipe.ingredients[i];
                bool flag = false;
                for (int j = 0; j < WorkGiver_DoBioBillBase.availableCounts.Count; j++)
                {
                    float num = (float)ingredientCount.CountRequiredOfFor(WorkGiver_DoBioBillBase.availableCounts.GetDef(j), bill.recipe);
                    if ((recipe.ignoreIngredientCountTakeEntireStacks || num <= WorkGiver_DoBioBillBase.availableCounts.GetCount(j)) && ingredientCount.filter.Allows(WorkGiver_DoBioBillBase.availableCounts.GetDef(j)) && (ingredientCount.IsFixedIngredient || bill.ingredientFilter.Allows(WorkGiver_DoBioBillBase.availableCounts.GetDef(j))))
                    {
                        for (int k = 0; k < availableThings.Count; k++)
                        {
                            if (availableThings[k].def == WorkGiver_DoBioBillBase.availableCounts.GetDef(j))
                            {
                                int num2 = availableThings[k].stackCount - ThingCountUtility.CountOf(chosen, availableThings[k]);
                                if (num2 > 0)
                                {
                                    if (recipe.ignoreIngredientCountTakeEntireStacks)
                                    {
                                        ThingCountUtility.AddToList(chosen, availableThings[k], num2);
                                        return true;
                                    }
                                    int num3 = Mathf.Min(Mathf.FloorToInt(num), num2);
                                    ThingCountUtility.AddToList(chosen, availableThings[k], num3);
                                    num -= (float)num3;
                                    if (num < 0.001f)
                                    {
                                        flag = true;
                                        float num4 = WorkGiver_DoBioBillBase.availableCounts.GetCount(j);
                                        num4 -= (float)ingredientCount.CountRequiredOfFor(WorkGiver_DoBioBillBase.availableCounts.GetDef(j), bill.recipe);
                                        WorkGiver_DoBioBillBase.availableCounts.SetCount(j, num4);
                                        break;
                                    }
                                }
                            }
                        }
                        if (flag)
                        {
                            break;
                        }
                    }
                }
                if (!flag)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool TryFindBestBillIngredientsInSet_AllowMix(List<Thing> availableThings, Bill bill, List<ThingCount> chosen)
        {
            chosen.Clear();
            availableThings.Sort((Thing t, Thing t2) => bill.recipe.IngredientValueGetter.ValuePerUnitOf(t2.def).CompareTo(bill.recipe.IngredientValueGetter.ValuePerUnitOf(t.def)));
            for (int i = 0; i < bill.recipe.ingredients.Count; i++)
            {
                IngredientCount ingredientCount = bill.recipe.ingredients[i];
                float num = ingredientCount.GetBaseCount();
                for (int j = 0; j < availableThings.Count; j++)
                {
                    Thing thing = availableThings[j];
                    if (ingredientCount.filter.Allows(thing) && (ingredientCount.IsFixedIngredient || bill.ingredientFilter.Allows(thing)))
                    {
                        float num2 = bill.recipe.IngredientValueGetter.ValuePerUnitOf(thing.def);
                        int num3 = Mathf.Min(Mathf.CeilToInt(num / num2), thing.stackCount);
                        ThingCountUtility.AddToList(chosen, thing, num3);
                        num -= (float)num3 * num2;
                        if (num <= 0.0001f)
                        {
                            break;
                        }
                    }
                }
                if (num > 0.0001f)
                {
                    return false;
                }
            }
            return true;
        }

        private List<ThingCount> chosenIngThings = new List<ThingCount>();

        private static readonly IntRange ReCheckFailedBillTicksRange = new IntRange(500, 600);

        private static string MissingMaterialsTranslated;

        private static List<Thing> relevantThings = new List<Thing>();

        private static HashSet<Thing> processedThings = new HashSet<Thing>();

        private static List<Thing> newRelevantThings = new List<Thing>();

        private static List<IngredientCount> ingredientsOrdered = new List<IngredientCount>();

        private static List<Thing> tmpMedicine = new List<Thing>();

        private static WorkGiver_DoBioBillBase.DefCountList availableCounts = new WorkGiver_DoBioBillBase.DefCountList();

        private class DefCountList
        {
            public int Count
            {
                get
                {
                    return this.defs.Count;
                }
            }

            public float this[ThingDef def]
            {
                get
                {
                    int num = this.defs.IndexOf(def);
                    if (num < 0)
                    {
                        return 0f;
                    }
                    return this.counts[num];
                }
                set
                {
                    int num = this.defs.IndexOf(def);
                    if (num < 0)
                    {
                        this.defs.Add(def);
                        this.counts.Add(value);
                        num = this.defs.Count - 1;
                    }
                    else
                    {
                        this.counts[num] = value;
                    }
                    this.CheckRemove(num);
                }
            }

            public float GetCount(int index)
            {
                return this.counts[index];
            }

            public void SetCount(int index, float val)
            {
                this.counts[index] = val;
                this.CheckRemove(index);
            }

            public ThingDef GetDef(int index)
            {
                return this.defs[index];
            }

            private void CheckRemove(int index)
            {
                if (this.counts[index] == 0f)
                {
                    this.counts.RemoveAt(index);
                    this.defs.RemoveAt(index);
                }
            }

            public void Clear()
            {
                this.defs.Clear();
                this.counts.Clear();
            }

            public void GenerateFrom(List<Thing> things)
            {
                this.Clear();
                for (int i = 0; i < things.Count; i++)
                {
                    ThingDef def = things[i].def;
                    this[def] += (float)things[i].stackCount;
                }
            }

            private List<ThingDef> defs = new List<ThingDef>();

            private List<float> counts = new List<float>();
        }
    }
}

