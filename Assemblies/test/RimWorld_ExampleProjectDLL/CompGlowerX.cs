﻿using System;
using Verse;

public class CompGlowerX : CompGlower
{
	public bool LitX
	{
		get
		{
			return this.glowOnInt;
		}
		set
		{
			if (this.glowOnInt != value)
			{
				this.glowOnInt = value;
				if (!value)
				{
					Find.MapDrawer.MapChanged(this.parent.Position, 2);
					Find.GlowGrid.DeRegisterGlower(this);
				}
				else
				{
					Find.MapDrawer.MapChanged(this.parent.Position, 2);
					Find.GlowGrid.RegisterGlower(this);
				}
			}
		}
	}

	public int RadiusIntCeilingX
	{
		get
		{
			return (int)Math.Ceiling((double)this.props.glowRadius);
		}
	}

	public override void PostSpawnSetup()
	{
		if (base.Lit)
		{
			Find.GlowGrid.RegisterGlower(this);
		}
		base.Lit = true;
	}

	public override void ReceiveCompSignal(string signal)
	{
	}

	public override void PostExposeData()
	{
		Scribe_Values.LookValue<bool>(ref this.glowOnInt, "glowOn", false, false);
	}

	public override void PostDestroy(DestroyMode mode = 0)
	{
		base.PostDestroy(mode);
		base.Lit = false;
	}

	private bool glowOnInt;
}
