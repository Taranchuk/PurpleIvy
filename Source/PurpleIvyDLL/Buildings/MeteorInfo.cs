﻿using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace PurpleIvy
{
    public class MeteorInfo : IExposable
    {
        public const int DefaultOpenDelay = 110;
        public List<Thing> containedThings = new List<Thing>();
        public int openDelay = 110;
        public bool leaveSlag;
        public Thing SingleContainedThing
        {
            get
            {
                if (this.containedThings.Count == 0)
                {
                    return null;
                }
                if (this.containedThings.Count > 1)
                {
                    Log.Error("ContainedThing used on a DropPodInfo holding > 1 thing.");
                }
                return this.containedThings[0];
            }
            set
            {
                this.containedThings.Clear();
                this.containedThings.Add(value);
            }
        }
        public void ExposeData()
        {
            Scribe_Collections.Look<Thing>(ref this.containedThings, "containedThings", LookMode.Deep, null);
            Scribe_Values.Look<int>(ref this.openDelay, "openDelay", 110, false);
            Scribe_Values.Look<bool>(ref this.leaveSlag, "leaveSlag", false, false);
        }
    }
}

