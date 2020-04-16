using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RaceWeapons
{
	public class Projectile_HeatImpact : Projectile
	{
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.drawingTexture = this.def.DrawMatSingle;
			this.compED = base.GetComp<CompFutureDamage>();
		}

		public ThingDef_RaceWeapons Props
		{
			get
			{
				return this.def as ThingDef_RaceWeapons;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.tickCounter, "tickCounter", 0, false);
		}

		public override void Tick()
		{
			if (this.tickCounter == 0)
			{
				this.PerformPreFiringTreatment();
			}
			if (this.tickCounter < this.Props.TickOffset)
			{
				this.GetPreFiringDrawingParameters();
			}
			else
			{
				if (this.tickCounter == this.Props.TickOffset)
				{
					this.Fire();
				}
				this.GetPostFiringDrawingParameters();
			}
			if (this.tickCounter == this.Props.TickOffset + this.Props.TickOffsetSecond)
			{
				base.Tick();
				this.Destroy(0);
			}
			Pawn pawn = this.launcher as Pawn;
			if (pawn != null)
			{
				Pawn_StanceTracker stances = pawn.stances;
				if (stances != null && (!(stances.curStance is Stance_Busy) || pawn.Dead))
				{
					this.Destroy(0);
				}
			}
			this.tickCounter++;
		}

		public virtual void PerformPreFiringTreatment()
		{
			this.DetermineImpactExactPosition();
			Vector3 a = (this.destination - this.origin).normalized * 0.9f;
			this.drawingScale = new Vector3(1f, 1f, (this.destination - this.origin).magnitude - a.magnitude);
			this.drawingPosition = this.origin + a / 2f + (this.destination - this.origin) / 2f + Vector3.up * this.def.Altitude;
			this.drawingMatrix.SetTRS(this.drawingPosition, this.ExactRotation, this.drawingScale);
		}

		public virtual void GetPreFiringDrawingParameters()
		{
			if (this.Props.TickOffset != 0)
			{
				this.drawingIntensity = this.Props.DrawingOffset + (this.Props.DrawingOffsetSecond - this.Props.DrawingOffset) * (float)this.tickCounter / (float)this.Props.TickOffset;
			}
		}

		public virtual void GetPostFiringDrawingParameters()
		{
			if (this.Props.TickOffsetSecond != 0)
			{
				this.drawingIntensity = this.Props.DrawingOffsetThird + (this.Props.DrawingOffsetFourth - this.Props.DrawingOffsetThird) * (((float)this.tickCounter - (float)this.Props.TickOffset) / (float)this.Props.TickOffsetSecond);
			}
		}

		protected void DetermineImpactExactPosition()
		{
			Vector3 a = this.destination - this.origin;
			int num = (int)a.magnitude;
			Vector3 b = a / a.magnitude;
			Vector3 destination = this.origin;
			Vector3 vector = this.origin;
			IntVec3 intVec = IntVec3Utility.ToIntVec3(vector);
			for (int i = 1; i <= num; i++)
			{
				vector += b;
				intVec = IntVec3Utility.ToIntVec3(vector);
				if (!GenGrid.InBounds(vector, base.Map))
				{
					this.destination = destination;
					return;
				}
				if (!this.def.projectile.flyOverhead && this.def.projectile.alwaysFreeIntercept && i >= 5)
				{
					List<Thing> list = base.Map.thingGrid.ThingsListAt(base.Position);
					for (int j = 0; j < list.Count; j++)
					{
						Thing thing = list[j];
						if (thing.def.Fillage == 2)
						{
							this.destination = intVec.ToVector3Shifted() + new Vector3(Rand.Range(-0.3f, 0.3f), 0f, Rand.Range(-0.3f, 0.3f));
							this.hitThing = thing;
							break;
						}
						if (thing.def.category == 1)
						{
							Pawn pawn = thing as Pawn;
							float num2 = 0.45f;
							if (pawn.Downed)
							{
								num2 *= 0.1f;
							}
							float num3 = GenGeo.MagnitudeHorizontal(this.ExactPosition - this.origin);
							if (num3 < 4f)
							{
								num2 *= 0f;
							}
							else if (num3 < 7f)
							{
								num2 *= 0.5f;
							}
							else if (num3 < 10f)
							{
								num2 *= 0.75f;
							}
							num2 *= pawn.RaceProps.baseBodySize;
							if (Rand.Value < num2)
							{
								this.destination = intVec.ToVector3Shifted() + new Vector3(Rand.Range(-0.3f, 0.3f), 0f, Rand.Range(-0.3f, 0.3f));
								this.hitThing = pawn;
								break;
							}
						}
					}
				}
				destination = vector;
			}
		}

		public virtual void Fire()
		{
			this.ApplyDamage(this.hitThing);
		}

		protected void ApplyDamage(Thing hitThing)
		{
			if (hitThing != null)
			{
				this.Impact(hitThing);
				return;
			}
			this.ImpactSomething();
		}

		private void ImpactSomething()
		{
			if (this.def.projectile.flyOverhead)
			{
				RoofDef roofDef = base.Map.roofGrid.RoofAt(base.Position);
				if (roofDef != null)
				{
					if (roofDef.isThickRoof)
					{
						SoundStarter.PlayOneShot(this.def.projectile.soundHitThickRoof, new TargetInfo(base.Position, base.Map, false));
						this.Destroy(0);
						return;
					}
					if (GridsUtility.GetEdifice(base.Position, base.Map) == null || GridsUtility.GetEdifice(base.Position, base.Map).def.Fillage != 2)
					{
						RoofCollapserImmediate.DropRoofInCells(base.Position, base.Map, null);
					}
				}
			}
			if (!this.usedTarget.HasThing || !base.CanHit(this.usedTarget.Thing))
			{
				List<Thing> list = new List<Thing>();
				list.Clear();
				List<Thing> thingList = GridsUtility.GetThingList(base.Position, base.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing = thingList[i];
					if ((thing.def.category == 3 || thing.def.category == 1 || thing.def.category == 2 || thing.def.category == 4) && base.CanHit(thing))
					{
						list.Add(thing);
					}
				}
				GenList.Shuffle<Thing>(list);
				for (int j = 0; j < list.Count; j++)
				{
					Thing thing2 = list[j];
					Pawn pawn = thing2 as Pawn;
					float num;
					if (pawn != null)
					{
						num = 0.5f * Mathf.Clamp(pawn.BodySize, 0.1f, 2f);
						if (PawnUtility.GetPosture(pawn) != null && GenGeo.MagnitudeHorizontalSquared(this.origin - this.destination) >= 20.25f)
						{
							num *= 0.2f;
						}
						if (this.launcher != null && pawn.Faction != null && this.launcher.Faction != null && !FactionUtility.HostileTo(pawn.Faction, this.launcher.Faction))
						{
							num *= VerbUtility.InterceptChanceFactorFromDistance(this.origin, base.Position);
						}
					}
					else
					{
						num = 1.5f * thing2.def.fillPercent;
					}
					if (Rand.Chance(num))
					{
						this.Impact(GenCollection.RandomElement<Thing>(list));
						return;
					}
				}
				this.Impact(null);
				return;
			}
			Pawn pawn2 = this.usedTarget.Thing as Pawn;
			if (pawn2 != null && PawnUtility.GetPosture(pawn2) != null && GenGeo.MagnitudeHorizontalSquared(this.origin - this.destination) >= 20.25f && !Rand.Chance(0.2f))
			{
				this.Impact(null);
				return;
			}
			this.Impact(this.usedTarget.Thing);
		}

		protected override void Impact(Thing hitThing)
		{
			Map map = base.Map;
			base.Impact(hitThing);
			if (hitThing != null)
			{
				int damageAmount = this.def.projectile.GetDamageAmount((float)base.DamageAmount, null);
				ThingDef equipmentDef = this.equipmentDef;
				float armorPenetration = this.def.projectile.GetArmorPenetration(base.ArmorPenetration, null);
				DamageInfo damageInfo;
				damageInfo..ctor(this.def.projectile.damageDef, (float)damageAmount, armorPenetration, this.ExactRotation.eulerAngles.y, this.launcher, null, equipmentDef, 0, null);
				hitThing.TakeDamage(damageInfo);
				Pawn pawn = hitThing as Pawn;
				if (pawn != null && !pawn.Downed && Rand.Value < this.compED.chanceToProc)
				{
					MoteMaker.ThrowMicroSparks(this.destination, base.Map);
					hitThing.TakeDamage(new DamageInfo(DefDatabase<DamageDef>.GetNamed(this.compED.damageDef, true), (float)this.compED.damageAmount, armorPenetration, this.ExactRotation.eulerAngles.y, this.launcher, null, null, 0, null));
					return;
				}
			}
			else
			{
				SoundStarter.PlayOneShot(SoundDefOf.BulletImpact_Ground, new TargetInfo(base.Position, map, false));
				MoteMaker.MakeStaticMote(this.ExactPosition, map, ThingDefOf.LaserMoteWorker, 0.5f);
				Projectile_HeatImpact.ThrowMicroSparksRed(this.ExactPosition, base.Map);
			}
		}

		public override void Draw()
		{
			base.Comps_PostDraw();
			Graphics.DrawMesh(MeshPool.plane10, this.drawingMatrix, FadedMaterialPool.FadedVersionOf(this.drawingTexture, this.drawingIntensity), 0);
		}

		public static void ThrowMicroSparksRed(Vector3 loc, Map map)
		{
			if (!GenView.ShouldSpawnMotesAt(loc, map) || map.moteCounter.SaturatedLowPriority)
			{
				return;
			}
			Rand.PushState();
			MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("LaserImpactThing"), null);
			moteThrown.Scale = Rand.Range(1f, 1.2f);
			moteThrown.rotationRate = Rand.Range(-12f, 12f);
			moteThrown.exactPosition = loc;
			moteThrown.exactPosition -= new Vector3(0.5f, 0f, 0.5f);
			moteThrown.exactPosition += new Vector3(Rand.Value, 0f, Rand.Value);
			moteThrown.SetVelocity((float)Rand.Range(35, 45), 1.2f);
			GenSpawn.Spawn(moteThrown, IntVec3Utility.ToIntVec3(loc), map, 0);
			Rand.PopState();
		}

		public int tickCounter;

		public Thing hitThing;

		public CompFutureDamage compED;

		public Material preFiringTexture;

		public Material postFiringTexture;

		public Matrix4x4 drawingMatrix;

		public Vector3 drawingScale;

		public Vector3 drawingPosition;

		public float drawingIntensity;

		public Material drawingTexture;
	}
}
