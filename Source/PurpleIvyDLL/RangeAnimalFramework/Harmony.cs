using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using System.Reflection;

namespace PurpleIvy
{

	[StaticConstructorOnStartup]
	internal static class AnimalRangeAttack_Init
	{
		static AnimalRangeAttack_Init()
		{
			var harmony = new Harmony("com.github.rimworld.mod.AnimalRangeAttack");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}

	//Current effective verb influence target pick.
	[HarmonyPatch(typeof(Pawn), "TryGetAttackVerb")]
	public static class ARA__VerbCheck_Patch
	{
		static bool Prefix(ref Pawn __instance,ref Verb __result)
		{
			//If not animal don't bother
			if (!__instance.AnimalOrWildMan())
				return true;
			
			List<Verb> verbList = __instance.verbTracker.AllVerbs;
			for (int i = 0; i < verbList.Count; i++)
			{
				if (verbList[i].verbProps.range > 1.1f)
				{
					//found range verb return first one in the list
					__result = verbList[i];
					return false;
				}
			}
			return true;

		}
	}
	
	
	[HarmonyPatch(typeof(JobGiver_Manhunter), "TryGiveJob")]
	public static class ARA__ManHunter_Patch
	{
		
		static bool Prefix(ref JobGiver_Manhunter __instance, ref Job __result, ref Pawn pawn)
		{
            //Log.Warning("Man hunter detected");
            if (!pawn.RaceProps.Animal)
                return true;

            bool hasRangedVerb = false;

            List<Verb> verbList = pawn.verbTracker.AllVerbs;
            List<Verb> rangeList = new List<Verb>();
            for (int i = 0; i < verbList.Count; i++)
            {
                //Log.Warning("Checkity");
                //It corresponds with verbs anyway
                if (!verbList[i].verbProps.IsMeleeAttack)
                {
                    rangeList.Add(verbList[i]);
                    hasRangedVerb = true;
                    //Log.Warning("Added Ranged Verb");
                }
                
            }

            if (hasRangedVerb == false)
            {
               // Log.Warning("I don't have range verb");
                return true;
            }
            // this.SetCurMeleeVerb(updatedAvailableVerbsList.RandomElementByWeight((VerbEntry ve) => ve.SelectionWeight).verb);
            Verb rangeVerb = rangeList.RandomElementByWeight((Verb rangeItem) => rangeItem.verbProps.commonality);
            if (rangeVerb == null)
            {
                //Log.Warning("Can't get random range verb");
                return true;
            }

            //Seek enemy in conventional way.
            Thing enemyTarget = (Thing)ARA_AttackTargetFinder.BestAttackTarget((IAttackTargetSearcher)pawn, TargetScanFlags.NeedThreat | TargetScanFlags.NeedReachable, (Predicate<Thing>)(x =>
             x is Pawn || x is Building), 0.0f, 9999, new IntVec3(), float.MaxValue, false);

            //Seek thing hiding in embrasure.
            if (enemyTarget == null)
                enemyTarget = (Thing)ARA_AttackTargetFinder.BestAttackTarget((IAttackTargetSearcher)pawn, TargetScanFlags.NeedThreat, (Predicate<Thing>)(x =>
            x is Pawn || x is Building), 0.0f, 9999, new IntVec3(), float.MaxValue, false);

            if (enemyTarget == null)
            {
                //Log.Warning("I can't find anything to fight.");
                return true;
            }

            //Check if enemy directly next to pawn
            if (enemyTarget.Position.DistanceTo(pawn.Position) < rangeVerb.verbProps.minRange)
            {
                //If adjacent melee attack
                if (enemyTarget.Position.AdjacentTo8Way(pawn.Position))
                {
                    __result = new Job(JobDefOf.AttackMelee, enemyTarget)
                    {
                        maxNumMeleeAttacks = 1,
                        expiryInterval = Rand.Range(420, 900),
                        attackDoorIfTargetLost = false
                    };
                    return false;
                }
                return true;
            }

            //Log.Warning("got list of ranged verb");
            //Log.Warning("Attempting flag");
            bool flag1 = (double)CoverUtility.CalculateOverallBlockChance(pawn.Position, enemyTarget.Position, pawn.Map) > 0.00999999977648258;
            bool flag2 = pawn.Position.Standable(pawn.Map);
            bool flag3 = rangeVerb.CanHitTarget(enemyTarget);
            bool flag4 = (pawn.Position - enemyTarget.Position).LengthHorizontalSquared < 25;

            if (flag1 && flag2 && flag3 || flag4 && flag3)
            {
                //Log.Warning("Shooting");
                __result = new Job(DefDatabase<JobDef>.GetNamed("PI_AnimalRangeAttack"), enemyTarget, JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange, true)
                {
                    verbToUse = rangeVerb

                };
                return false;
            }
            IntVec3 dest;
            bool canShootCondition = false;

            canShootCondition = CastPositionFinder.TryFindCastPosition(new CastPositionRequest
            {
                caster = pawn,
                target = enemyTarget,
                verb = rangeVerb,
                maxRangeFromTarget = rangeVerb.verbProps.range,
                wantCoverFromTarget = false,
                maxRegions = 50
            }, out dest);

            if (!canShootCondition)
            {
                //Log.Warning("I can't move to shooting position");

                return true;
            }

            if (dest == pawn.Position)
            {
                //Log.Warning("I will stay here and attack");
                __result = new Job(DefDatabase<JobDef>.GetNamed("PI_AnimalRangeAttack"), enemyTarget, JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange, true)
                {
                    verbToUse = rangeVerb
                };
                return false;
            }
            //Log.Warning("Going to new place");
            __result = new Job(JobDefOf.Goto, (LocalTargetInfo)dest)
            {
                expiryInterval = JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange,
                checkOverrideOnExpire = true
            };
            return false;
        }

    }

    [HarmonyPatch(typeof(JobGiver_AIDefendPawn), "TryGiveJob")]
    internal static class ARA_FightAI_Patch
    {
        [HarmonyPrefix]
        private static bool Prefix(ref JobGiver_AIFightEnemy __instance, ref Job __result, ref Pawn pawn)
        {
            bool flag = !pawn.RaceProps.Animal;
            bool result;
            if (flag)
            {
                result = true;
            }
            else
            {
                bool flag2 = false;
                List<Verb> allVerbs = pawn.verbTracker.AllVerbs;
                List<Verb> list = new List<Verb>();
                for (int i = 0; i < allVerbs.Count; i++)
                {
                    bool flag3 = allVerbs[i].verbProps.range > 1.1f;
                    if (flag3)
                    {
                        list.Add(allVerbs[i]);
                        flag2 = true;
                    }
                }
                bool flag4 = !flag2;
                if (flag4)
                {
                    result = true;
                }
                else
                {
                    Verb verb = GenCollection.RandomElementByWeight<Verb>(list, (Verb rangeItem) => rangeItem.verbProps.commonality);
                    bool flag5 = verb == null;
                    if (flag5)
                    {
                        result = true;
                    }
                    else
                    {
                        Thing thing = (Thing)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedThreat, (Thing x) => x is Pawn || x is Building, 0f, verb.verbProps.range, default(IntVec3), float.MaxValue, false, true);
                        bool flag6 = thing == null;
                        if (flag6)
                        {
                            result = true;
                        }
                        else
                        {
                            bool flag7 = IntVec3Utility.DistanceTo(thing.Position, pawn.Position) < verb.verbProps.minRange;
                            if (flag7)
                            {
                                bool flag8 = GenAdj.AdjacentTo8Way(thing.Position, pawn.Position);
                                if (flag8)
                                {
                                    __result = new Job(JobDefOf.AttackMelee, thing)
                                    {
                                        maxNumMeleeAttacks = 1,
                                        expiryInterval = Rand.Range(420, 900),
                                        attackDoorIfTargetLost = false
                                    };
                                    return false;
                                }
                                bool flag9 = pawn.Faction != null && !pawn.Faction.def.isPlayer;
                                if (!flag9)
                                {
                                    bool flag10 = ReachabilityUtility.CanReach(pawn, thing, PathEndMode.Touch, Danger.Deadly, false, 0) && pawn.playerSettings.Master.playerSettings.animalsReleased;
                                    if (flag10)
                                    {
                                        __result = new Job(JobDefOf.AttackMelee, thing)
                                        {
                                            maxNumMeleeAttacks = 1,
                                            expiryInterval = Rand.Range(420, 900),
                                            attackDoorIfTargetLost = false
                                        };
                                        return false;
                                    }
                                    return true;
                                }
                            }
                            bool flag11 = (double)CoverUtility.CalculateOverallBlockChance(pawn.Position, thing.Position, pawn.Map) > 0.00999999977648258;
                            bool flag12 = GenGrid.Standable(pawn.Position, pawn.Map);
                            bool flag13 = verb.CanHitTarget(thing);
                            bool flag14 = (pawn.Position - thing.Position).LengthHorizontalSquared < 25;
                            bool flag15 = (flag11 && flag12 && flag13) || (flag14 && flag13);
                            if (flag15)
                            {
                                JobDef named = DefDatabase<JobDef>.GetNamed("PI_AnimalRangeAttack", true);
                                LocalTargetInfo localTargetInfo = thing;
                                IntRange expiryInterval_ShooterSucceeded = JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded;
                                __result = new Job(named, localTargetInfo, expiryInterval_ShooterSucceeded.RandomInRange, true)
                                {
                                    verbToUse = verb
                                };
                                result = false;
                            }
                            else
                            {
                                CastPositionRequest castPositionRequest = default(CastPositionRequest);
                                castPositionRequest.caster = pawn;
                                castPositionRequest.target = thing;
                                castPositionRequest.verb = verb;
                                castPositionRequest.maxRangeFromTarget = verb.verbProps.range;
                                castPositionRequest.wantCoverFromTarget = (pawn.training.HasLearned(TrainableDefOf.Release) && (double)verb.verbProps.range > 7.0);
                                castPositionRequest.locus = pawn.playerSettings.Master.Position;
                                castPositionRequest.maxRangeFromLocus = Traverse.Create(__instance).Method("GetFlagRadius", new object[]
                                {
                                    pawn
                                }).GetValue<float>();
                                castPositionRequest.maxRegions = 50;
                                IntVec3 intVec = new IntVec3();
                                bool flag16 = CastPositionFinder.TryFindCastPosition(castPositionRequest, out intVec);
                                bool flag17 = !flag16;
                                if (flag17)
                                {
                                    result = true;
                                }
                                else
                                {
                                    bool flag18 = intVec == pawn.Position;
                                    if (flag18)
                                    {
                                        JobDef named2 = DefDatabase<JobDef>.GetNamed("PI_AnimalRangeAttack", true);
                                        LocalTargetInfo localTargetInfo2 = thing;
                                        IntRange expiryInterval_ShooterSucceeded = JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded;
                                        __result = new Job(named2, localTargetInfo2, expiryInterval_ShooterSucceeded.RandomInRange, true)
                                        {
                                            verbToUse = verb
                                        };
                                        result = false;
                                    }
                                    else
                                    {
                                        Job job = new Job(JobDefOf.Goto, intVec);
                                        IntRange expiryInterval_ShooterSucceeded = JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded;
                                        job.expiryInterval = expiryInterval_ShooterSucceeded.RandomInRange;
                                        job.checkOverrideOnExpire = true;
                                        __result = job;
                                        result = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }
    }

    ////Because of the opportunity of the patching, we can tamed animal are savvy and smarter at shooting than wild one
    //[HarmonyPatch(typeof(JobGiver_AIDefendMaster),"TryGiveJob")]
	//static class ARA_FightAI_Patch
	//{
	//	static bool Prefix(ref JobGiver_AIFightEnemy __instance, ref Job __result, ref Pawn pawn)
	//	{
	//		//Log.Warning("Tame animal job detected");
	//		if (!pawn.RaceProps.Animal)
	//			return true;
	//		
	//		bool hasRangedVerb = false;
    //
    //
	//		List<Verb> verbList = pawn.verbTracker.AllVerbs;
	//		List<Verb> rangeList = new List<Verb>();
	//		for (int i = 0; i < verbList.Count; i++)
	//		{
	//			//Log.Warning("Checkity");
	//			//It corresponds with verbs anyway
	//			if (!verbList[i].verbProps.IsMeleeAttack)
	//			{
	//				rangeList.Add(verbList[i]);
	//				hasRangedVerb = true;
	//			}
	//			//Log.Warning("Added Ranged Verb");
	//		}
    //
	//		if (hasRangedVerb == false)
	//		{
	//			//Log.Warning("I don't have range verb");
	//			return true;
	//		}
	//		// this.SetCurMeleeVerb(updatedAvailableVerbsList.RandomElementByWeight((VerbEntry ve) => ve.SelectionWeight).verb);
	//		Verb rangeVerb = rangeList.RandomElementByWeight((Verb rangeItem) => rangeItem.verbProps.commonality);
	//		if (rangeVerb == null)
	//		{
	//			//Log.Warning("Can't get random range verb");
	//			return true;
	//		}
    //
	//		
	//		 Thing enemyTarget = (Thing)AttackTargetFinder.BestAttackTarget((IAttackTargetSearcher)pawn, TargetScanFlags.NeedThreat, (Predicate<Thing>)(x =>
    //         x is Pawn || x is Building), 0.0f, rangeVerb.verbProps.range, new IntVec3(), float.MaxValue, false);
    //
	//		
	//		if (enemyTarget == null)
	//		{
	//			//Log.Warning("I can't find anything to fight.");
	//			return true;
	//		}
    //
	//		//Check if enemy directly next to pawn
	//		if (enemyTarget.Position.DistanceTo(pawn.Position) < rangeVerb.verbProps.minRange )
	//		{
	//			//If adjacent melee attack
	//			if (enemyTarget.Position.AdjacentTo8Way(pawn.Position))
	//			{
	//				__result = new Job(JobDefOf.AttackMelee,enemyTarget)
	//				{
	//					maxNumMeleeAttacks = 1,
	//					expiryInterval = Rand.Range(420, 900),
	//					attackDoorIfTargetLost = false
	//				};
	//				return false;
	//			}
	//			//Only go if I am to be released. This prevent animal running off.
	//			if (pawn.CanReach(enemyTarget, PathEndMode.Touch, Danger.Deadly, false) && pawn.playerSettings.Master.playerSettings.animalsReleased)
	//			{
	//				//Log.Warning("Melee Attack");
	//				__result = new Job(JobDefOf.AttackMelee,enemyTarget)
	//				{
	//					maxNumMeleeAttacks = 1,
	//					expiryInterval = Rand.Range(420, 900),
	//					attackDoorIfTargetLost = false
	//				};
	//				return false;
	//			}
	//			else
	//			{
	//				return true;
	//			}
	//		}
	//		
	//		//Log.Warning("got list of ranged verb");
	//		//Log.Warning("Attempting flag");
	//		bool flag1 = (double)CoverUtility.CalculateOverallBlockChance(pawn.Position, enemyTarget.Position, pawn.Map) > 0.00999999977648258;
	//		bool flag2 = pawn.Position.Standable(pawn.Map);
	//		bool flag3 = rangeVerb.CanHitTarget(enemyTarget);
	//		bool flag4 = (pawn.Position - enemyTarget.Position).LengthHorizontalSquared < 25;
	//		
	//		
	//		
	//		if (flag1 && flag2 && flag3 || flag4 && flag3)
	//		{
	//			//Log.Warning("Shooting");
	//			__result = new Job(DefDatabase<JobDef>.GetNamed("PI_AnimalRangeAttack"), enemyTarget, JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange, true)
	//			{
	//				verbToUse = rangeVerb
    //
	//			};
	//			return false;
	//		}
	//		IntVec3 dest;
	//		bool canShootCondition = false;
	//		//Log.Warning("Try casting");
    //
	//		//Animals with training seek cover
	//		/*
	//			if (pawn.training.IsCompleted(TrainableDefOf.Release) && (double)verb.verbProps.range > 7.0)
	//				Log.Warning("Attempting cover");
	//			Log.Warning("Try get flag radius :" + Traverse.Create(__instance).Method("GetFlagRadius", pawn).GetValue<float>());
	//			Log.Warning("Checking cast condition");
	//			*/
    //
	//		//Don't find new position if animal not released.
	//		
	//		
	//		canShootCondition = CastPositionFinder.TryFindCastPosition(new CastPositionRequest
	//		{
	//			caster = pawn,
	//			target = enemyTarget,
	//			verb = rangeVerb,
	//			maxRangeFromTarget = rangeVerb.verbProps.range,
	//			wantCoverFromTarget = pawn.training.HasLearned(TrainableDefOf.Release) && (double)rangeVerb.verbProps.range > 7.0,
	//			locus = pawn.playerSettings.Master.Position,
	//			maxRangeFromLocus = Traverse.Create(__instance).Method("GetFlagRadius", pawn).GetValue<float>(),
	//			maxRegions = 50
	//		}, out dest);
    //
	//		if (!canShootCondition)
	//		{
	//			//Log.Warning("I can't move to shooting position");
	//			
	//			
	//			return true;
	//		}
	//		
	//		if (dest == pawn.Position)
	//		{
	//			//Log.Warning("I will stay here and attack");
	//			__result = new Job(DefDatabase<JobDef>.GetNamed("PI_AnimalRangeAttack"), enemyTarget, JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange, true)
	//			{
	//				verbToUse = rangeVerb
	//			};
	//			return false;
	//		}
	//		//Log.Warning("Going to new place");
	//		__result =  new Job(JobDefOf.Goto, (LocalTargetInfo)dest)
	//		{
	//			expiryInterval = JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange,
	//			checkOverrideOnExpire = true
	//		};
	//		return false;
	//		}
    //
	//}
	

}

