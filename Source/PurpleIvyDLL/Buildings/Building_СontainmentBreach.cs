using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    public class Building_СontainmentBreach : Building_CryptosleepCasket
    {

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            if (this.Alien != null)
            {
                yield return new FloatMenuOption(Translator.Translate("ConductResearch"), delegate ()
                {
                    if (selPawn != null)
                    {
                        Job job = JobMaker.MakeJob(PurpleIvyDefOf.PI_ConductResearchOnAliens, this);
                        selPawn.jobs.TryTakeOrderedJob(job, 0);
                    }
                }, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else
            {
                yield return new FloatMenuOption("NoAliensToConductResearch".Translate(), null,
                    MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            yield break;
        }
        public Pawn Alien
        {
            get
            {
                for (int i = 0; i < this.innerContainer.Count; i++)
                {
                    Pawn alien = this.innerContainer[i] as Pawn;
                    if (alien != null)
                    {
                        return alien;
                    }
                }
                return null;
            }
        }
    }
}

