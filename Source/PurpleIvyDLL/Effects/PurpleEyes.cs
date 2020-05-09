using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace PurpleIvy
{
    [StaticConstructorOnStartup]
    public static class PurpleEyes
    {
        public static Graphic GetEyeGraphic(bool isFront, Color color)
        {
            Pair<bool, Color> key = new Pair<bool, Color>(isFront, color);
            bool flag = PurpleEyes.eyeCache.ContainsKey(key);
            Graphic result;
            if (flag)
            {
                result = PurpleEyes.eyeCache[key];
            }
            else
            {
                if (isFront)
                {
                    PurpleEyes.eyeCache[key] = GraphicDatabase.Get<Graphic_Single>(PurpleEyes.Eyeglow_Front_Path,
                        ShaderDatabase.MoteGlow, Vector2.one, color);
                }
                else
                {
                    PurpleEyes.eyeCache[key] = GraphicDatabase.Get<Graphic_Single>(PurpleEyes.Eyeglow_Side_Path,
                        ShaderDatabase.MoteGlow, Vector2.one, color);
                }
                result = PurpleEyes.eyeCache[key];
            }
            return result;
        }

        public static string Eyeglow_Front_Path = "Effects/Eyeglow_front";

        public static string Eyeglow_Side_Path = "Effects/Eyeglow_side";

        public static Dictionary<Pair<bool, Color>, Graphic> eyeCache = 
            new Dictionary<Pair<bool, Color>, Graphic>();
    }
}

