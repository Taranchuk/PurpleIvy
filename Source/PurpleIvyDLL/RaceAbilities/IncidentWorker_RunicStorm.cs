using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace RaceAbilities
{
	public class IncidentWorker_RunicStorm : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			return !((Map)parms.target).gameConditionManager.ConditionIsActive(GameConditionDefOf.RunicStorm);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			int num = Mathf.RoundToInt(this.def.durationDays.RandomInRange * 60000f);
			GameCondition_RunicStorm gameCondition_RunicStorm = (GameCondition_RunicStorm)GameConditionMaker.MakeCondition(GameConditionDefOf.RunicStorm, num);
			map.gameConditionManager.RegisterCondition(gameCondition_RunicStorm);
			base.SendStandardLetter(parms, new TargetInfo(gameCondition_RunicStorm.centerLocation.ToIntVec3, map, false));
			if (map.weatherManager.curWeather.rainRate > 0.1f)
			{
				map.weatherDecider.StartNextWeather();
			}
			return true;
		}
	}
}
