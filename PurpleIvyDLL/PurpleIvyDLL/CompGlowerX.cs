using System;
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
                    this.UpdateLit(this.parent.Map);
                    this.parent.Map.glowGrid.DeRegisterGlower(this);
				}
				else
				{
                    this.UpdateLit(this.parent.Map);
                    this.parent.Map.glowGrid.RegisterGlower(this);
				}
			}
		}
	}

	public int RadiusIntCeilingX
	{
		get
		{
			return (int)Math.Ceiling((double)this.Props.glowRadius);
		}
	}

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        this.UpdateLit(this.parent.Map);
        this.parent.Map.glowGrid.RegisterGlower(this);
        return;
    }

	public override void ReceiveCompSignal(string signal)
	{
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look<bool>(ref this.glowOnInt, "glowOn", false, false);
	}

	//public override void PostDestroy(DestroyMode mode = 0)
	//{
	//	base.PostDestroy(mode);
	//	base.Lit = false;
	//}

	private bool glowOnInt;
}
