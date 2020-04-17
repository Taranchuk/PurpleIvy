using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace EvaineQBionics
{
	public class JobDriver_BlueVomit : JobDriver
	{
		public override void SetInitialPosture()
		{
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.ticksLeft, "ticksLeft", 0, false);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = new Toil();
			toil.initAction = delegate()
			{
				this.ticksLeft = Rand.Range(300, 900);
				int num = 0;
				IntVec3 intVec;
				do
				{
					intVec = this.pawn.Position + GenAdj.AdjacentCellsAndInside[Rand.Range(0, 9)];
					num++;
					if (num > 12)
					{
						goto IL_6B;
					}
				}
				while (!GenGrid.InBounds(intVec, this.pawn.Map) || !GenGrid.Standable(intVec, this.pawn.Map));
				goto IL_77;
				IL_6B:
				intVec = this.pawn.Position;
				IL_77:
				this.job.targetA = intVec;
				this.pawn.pather.StopDead();
			};
			toil.tickAction = delegate()
			{
				if (this.ticksLeft % 150 == 149)
				{
					FilthMaker.TryMakeFilth(this.job.targetA.Cell, base.Map, ThingDefOf.Filth_BlueVomit, GenText.LabelIndefinite(this.pawn), 1);
					if (this.pawn.needs.food.CurLevelPercentage > 0.1f)
					{
						this.pawn.needs.food.CurLevel -= this.pawn.needs.food.MaxLevel * 0.04f;
					}
				}
				this.ticksLeft--;
				if (this.ticksLeft <= 0)
				{
					base.ReadyForNextToil();
					TaleRecorder.RecordTale(TaleDefOf.Vomited, new object[]
					{
						this.pawn
					});
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Never;
			ToilEffects.WithEffect(toil, EffecterDefOf.Blue_Vomit, TargetIndex.A);
			ToilEffects.PlaySustainerOrSound(toil, () => SoundDefOf.Vomit);
			yield return toil;
			yield break;
		}

		private int ticksLeft;
	}
}

