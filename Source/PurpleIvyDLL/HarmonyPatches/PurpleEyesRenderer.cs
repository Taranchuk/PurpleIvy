using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PurpleIvy
{
    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal")]
    [HarmonyPatch(new Type[]
    {
        typeof(Vector3),
        typeof(float),
        typeof(bool),
        typeof(Rot4),
        typeof(Rot4),
        typeof(RotDrawMode),
        typeof(bool),
        typeof(bool),
        typeof(bool)
    })]
    internal static class PurpleEyesRenderer
    {
        [HarmonyPostfix]
        private static void Postfix(PawnRenderer __instance, ref Vector3 rootLoc, ref float angle, 
            ref bool renderBody, ref Rot4 bodyFacing, ref Rot4 headFacing, ref RotDrawMode bodyDrawType,
            ref bool portrait, ref bool headStump, ref bool invisible, Pawn ___pawn)
        {
            if (invisible)
            {
                return;
            }

            if (__instance != null)
            {
                Pawn pawn = ___pawn;
                var comp = pawn.TryGetComp<AlienMutation>();
                if (pawn != null && !pawn.Dead && comp != null && comp.mutationActive
                    && !headStump && ((!portrait && pawn?.jobs?.curDriver != null
                    ? !pawn.jobs.curDriver.asleep : portrait) || portrait))
                {
                    Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);

                    //Get base offset.
                    Vector3 baseHeadOffset = rootLoc;
                    if (bodyFacing != Rot4.North)
                    {
                        baseHeadOffset.y += 0.0281250011f;
                        rootLoc.y += 0.0234375f;
                    }
                    else
                    {
                        baseHeadOffset.y += 0.0234375f;
                        rootLoc.y += 0.0281250011f;
                    }

                    Vector3 headOffset = quat * __instance.BaseHeadOffsetAt(headFacing);

                    //Finalize offset.
                    Vector3 eyeOffset = baseHeadOffset + headOffset + new Vector3(0f, 0.01f, 0f);

                    //Render eyes.
                    if (headFacing != Rot4.North)
                    {
                        //Is not the back.
                        Mesh headMesh = MeshPool.humanlikeHeadSet.MeshAt(headFacing);
                        Log.Message("Glow eyes");
                        if (headFacing.IsHorizontal)
                        {
                            //Side
                            GenDraw.DrawMeshNowOrLater(headMesh, eyeOffset, quat, PurpleEyes.GetEyeGraphic(false, new Color(0.368f, 0f, 1f)).MatSingle, portrait);
                        }
                        else
                        {
                            //Front
                            GenDraw.DrawMeshNowOrLater(headMesh, eyeOffset, quat, PurpleEyes.GetEyeGraphic(true, new Color(0.368f, 0f, 1f)).MatSingle, portrait);
                        }
                    }
                }
            }
        }
    }
}
