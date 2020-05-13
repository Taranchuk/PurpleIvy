using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PurpleIvy
{
    [StaticConstructorOnStartup]
    public static class ThingsToxicDamageSectionLayerUtility
    {
        public static void Notify_ThingHitPointsChanged(MapComponent_MapEvents comp, Thing b, int oldHitPoints)
        {
            if (comp == null || comp.ToxicDamages == null || !comp.ToxicDamages.ContainsKey(b) || 
                comp.ToxicDamages[b] == oldHitPoints || !b.Spawned)
            {
                return;
            }
            b.Map.mapDrawer.MapMeshDirty(b.Position, MapMeshFlag.Things);
        }

        public static bool UsesLinkableCornersAndEdges(Thing b)
        {
            return b.def.size.x == 1 && b.def.size.z == 1 && b.def.Fillage == FillCategory.Full;
        }

        public static IList<Material> GetScratchMats(Thing b)
        {
            IList<Material> result = ThingsToxicDamageSectionLayerUtility.DefaultScratchMats;
            if (b.def.graphicData != null && b.def.graphicData.damageData != null && b.def.graphicData.damageData.scratchMats != null)
            {
                result = b.def.graphicData.damageData.scratchMats;
            }
            return result;
        }

        public static List<DamageOverlay> GetAvailableOverlays(Thing b)
        {
            ThingsToxicDamageSectionLayerUtility.availableOverlays.Clear();
            if (ThingsToxicDamageSectionLayerUtility.GetScratchMats(b).Any<Material>())
            {
                int num = 3;
                Rect damageRect = ThingsToxicDamageSectionLayerUtility.GetDamageRect(b);
                float num2 = damageRect.width * damageRect.height;
                if (num2 > 4f)
                {
                    num += Mathf.RoundToInt((num2 - 4f) * 0.54f);
                }
                for (int i = 0; i < num; i++)
                {
                    ThingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.Scratch);
                }
            }
            if (ThingsToxicDamageSectionLayerUtility.UsesLinkableCornersAndEdges(b))
            {
                if (b.def.graphicData != null && b.def.graphicData.damageData != null)
                {
                    IntVec3 position = b.Position;
                    DamageGraphicData damageData = b.def.graphicData.damageData;
                    if (damageData.edgeTopMat != null && ThingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x, position.z + 1) && ThingsToxicDamageSectionLayerUtility.SameAndDamagedAt(b, position.x + 1, position.z) && ThingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x + 1, position.z + 1))
                    {
                        ThingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.TopEdge);
                    }
                    if (damageData.edgeRightMat != null && ThingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x + 1, position.z) && ThingsToxicDamageSectionLayerUtility.SameAndDamagedAt(b, position.x, position.z + 1) && ThingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x + 1, position.z + 1))
                    {
                        ThingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.RightEdge);
                    }
                    if (damageData.edgeBotMat != null && ThingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x, position.z - 1) && ThingsToxicDamageSectionLayerUtility.SameAndDamagedAt(b, position.x + 1, position.z) && ThingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x + 1, position.z - 1))
                    {
                        ThingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.BotEdge);
                    }
                    if (damageData.edgeLeftMat != null && ThingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x - 1, position.z) && ThingsToxicDamageSectionLayerUtility.SameAndDamagedAt(b, position.x, position.z + 1) && ThingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x - 1, position.z + 1))
                    {
                        ThingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.LeftEdge);
                    }
                    if (damageData.cornerTLMat != null && ThingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x - 1, position.z) && ThingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x, position.z + 1))
                    {
                        ThingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.TopLeftCorner);
                    }
                    if (damageData.cornerTRMat != null && ThingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x + 1, position.z) && ThingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x, position.z + 1))
                    {
                        ThingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.TopRightCorner);
                    }
                    if (damageData.cornerBRMat != null && ThingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x + 1, position.z) && ThingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x, position.z - 1))
                    {
                        ThingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.BotRightCorner);
                    }
                    if (damageData.cornerBLMat != null && ThingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x - 1, position.z) && ThingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x, position.z - 1))
                    {
                        ThingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.BotLeftCorner);
                    }
                }
            }
            else
            {
                Material x;
                Material x2;
                Material x3;
                Material x4;
                ThingsToxicDamageSectionLayerUtility.GetCornerMats(out x, out x2, out x3, out x4, b);
                if (x != null)
                {
                    ThingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.TopLeftCorner);
                }
                if (x2 != null)
                {
                    ThingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.TopRightCorner);
                }
                if (x4 != null)
                {
                    ThingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.BotLeftCorner);
                }
                if (x3 != null)
                {
                    ThingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.BotRightCorner);
                }
            }
            return ThingsToxicDamageSectionLayerUtility.availableOverlays;
        }

        public static void GetCornerMats(out Material topLeft, out Material topRight, out Material botRight, out Material botLeft, Thing b)
        {
            if (b.def.graphicData == null || b.def.graphicData.damageData == null)
            {
                topLeft = null;
                topRight = null;
                botRight = null;
                botLeft = null;
                return;
            }
            DamageGraphicData damageData = b.def.graphicData.damageData;
            if (b.Rotation == Rot4.North)
            {
                topLeft = damageData.cornerTLMat;
                topRight = damageData.cornerTRMat;
                botRight = damageData.cornerBRMat;
                botLeft = damageData.cornerBLMat;
                return;
            }
            if (b.Rotation == Rot4.East)
            {
                topLeft = damageData.cornerBLMat;
                topRight = damageData.cornerTLMat;
                botRight = damageData.cornerTRMat;
                botLeft = damageData.cornerBRMat;
                return;
            }
            if (b.Rotation == Rot4.South)
            {
                topLeft = damageData.cornerBRMat;
                topRight = damageData.cornerBLMat;
                botRight = damageData.cornerTLMat;
                botLeft = damageData.cornerTRMat;
                return;
            }
            topLeft = damageData.cornerTRMat;
            topRight = damageData.cornerBRMat;
            botRight = damageData.cornerBLMat;
            botLeft = damageData.cornerTLMat;
        }

        public static List<DamageOverlay> GetOverlays(Thing b)
        {
            ThingsToxicDamageSectionLayerUtility.overlays.Clear();
            ThingsToxicDamageSectionLayerUtility.overlaysWorkingList.Clear();
            ThingsToxicDamageSectionLayerUtility.overlaysWorkingList.AddRange(ThingsToxicDamageSectionLayerUtility.GetAvailableOverlays(b));
            if (!ThingsToxicDamageSectionLayerUtility.overlaysWorkingList.Any<DamageOverlay>())
            {
                return ThingsToxicDamageSectionLayerUtility.overlays;
            }
            Rand.PushState();
            Rand.Seed = Gen.HashCombineInt(b.thingIDNumber, 1958376471);
            int damageOverlaysCount = ThingsToxicDamageSectionLayerUtility.GetDamageOverlaysCount(b, b.Map.GetComponent<MapComponent_MapEvents>().ToxicDamages[b]);
            int num = 0;
            while (num < damageOverlaysCount && ThingsToxicDamageSectionLayerUtility.overlaysWorkingList.Any<DamageOverlay>())
            {
                DamageOverlay item = ThingsToxicDamageSectionLayerUtility.overlaysWorkingList.RandomElement<DamageOverlay>();
                ThingsToxicDamageSectionLayerUtility.overlaysWorkingList.Remove(item);
                ThingsToxicDamageSectionLayerUtility.overlays.Add(item);
                num++;
            }
            Rand.PopState();
            return ThingsToxicDamageSectionLayerUtility.overlays;
        }

        public static Rect GetDamageRect(Thing b)
        {
            DamageGraphicData damageGraphicData = null;
            if (b.def.graphicData != null)
            {
                damageGraphicData = b.def.graphicData.damageData;
            }
            CellRect cellRect = b.OccupiedRect();
            Rect result = new Rect((float)cellRect.minX, (float)cellRect.minZ, (float)cellRect.Width, (float)cellRect.Height);
            if (damageGraphicData != null)
            {
                if (b.Rotation == Rot4.North && damageGraphicData.rectN != default(Rect))
                {
                    result.position += damageGraphicData.rectN.position;
                    result.size = damageGraphicData.rectN.size;
                }
                else if (b.Rotation == Rot4.East && damageGraphicData.rectE != default(Rect))
                {
                    result.position += damageGraphicData.rectE.position;
                    result.size = damageGraphicData.rectE.size;
                }
                else if (b.Rotation == Rot4.South && damageGraphicData.rectS != default(Rect))
                {
                    result.position += damageGraphicData.rectS.position;
                    result.size = damageGraphicData.rectS.size;
                }
                else if (b.Rotation == Rot4.West && damageGraphicData.rectW != default(Rect))
                {
                    result.position += damageGraphicData.rectW.position;
                    result.size = damageGraphicData.rectW.size;
                }
                else if (damageGraphicData.rect != default(Rect))
                {
                    Rect rect = damageGraphicData.rect;
                    if (b.Rotation == Rot4.North)
                    {
                        result.x += rect.x;
                        result.y += rect.y;
                        result.width = rect.width;
                        result.height = rect.height;
                    }
                    else if (b.Rotation == Rot4.South)
                    {
                        result.x += (float)cellRect.Width - rect.x - rect.width;
                        result.y += (float)cellRect.Height - rect.y - rect.height;
                        result.width = rect.width;
                        result.height = rect.height;
                    }
                    else if (b.Rotation == Rot4.West)
                    {
                        result.x += (float)cellRect.Width - rect.y - rect.height;
                        result.y += rect.x;
                        result.width = rect.height;
                        result.height = rect.width;
                    }
                    else if (b.Rotation == Rot4.East)
                    {
                        result.x += rect.y;
                        result.y += (float)cellRect.Height - rect.x - rect.width;
                        result.width = rect.height;
                        result.height = rect.width;
                    }
                }
            }
            return result;
        }

        private static int GetDamageOverlaysCount(Thing b, int hp)
        {
            float num = (float)hp / (float)b.MaxHitPoints;
            int count = ThingsToxicDamageSectionLayerUtility.GetAvailableOverlays(b).Count;
            return count - Mathf.FloorToInt((float)count * num);
        }

        private static bool DifferentAt(Thing b, int x, int z)
        {
            IntVec3 c = new IntVec3(x, 0, z);
            if (!c.InBounds(b.Map))
            {
                return true;
            }
            List<Thing> thingList = c.GetThingList(b.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                if (thingList[i].def == b.def)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool SameAndDamagedAt(Thing b, int x, int z)
        {
            IntVec3 c = new IntVec3(x, 0, z);
            if (!c.InBounds(b.Map))
            {
                return false;
            }
            List<Thing> thingList = c.GetThingList(b.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                var comp = thingList[i].Map.GetComponent<MapComponent_MapEvents>();
                if (thingList[i].def == b.def && comp != null && comp.ToxicDamages != null
                    && comp.ToxicDamages.ContainsKey((Thing)thingList[i])
                    && comp.ToxicDamages[(Thing)thingList[i]] < thingList[i].MaxHitPoints)
                {
                    return true;
                }
            }
            return false;
        }

        public static void DebugDraw()
        {
            if (!Prefs.DevMode || !DebugViewSettings.drawDamageRects || Find.CurrentMap == null)
            {
                return;
            }
            Thing Thing = Find.Selector.FirstSelectedObject as Thing;
            if (Thing == null)
            {
                return;
            }
            Material material = DebugSolidColorMats.MaterialOf(Color.red);
            Rect damageRect = ThingsToxicDamageSectionLayerUtility.GetDamageRect(Thing);
            float y = 14.99f;
            Vector3 pos = new Vector3(damageRect.x + damageRect.width / 2f, y, damageRect.y + damageRect.height / 2f);
            Vector3 s = new Vector3(damageRect.width, 1f, damageRect.height);
            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(pos, Quaternion.identity, s), material, 0);
        }

        private static readonly Material[] DefaultScratchMats = new Material[]
        {
            MaterialPool.MatFrom("Things/Veins/VeinsA"),
            MaterialPool.MatFrom("Things/Veins/VeinsB"),
            MaterialPool.MatFrom("Things/Veins/VeinsC"),
            MaterialPool.MatFrom("Things/Veins/VeinsD"),
            MaterialPool.MatFrom("Things/Veins/VeinsE")
        };

        private static List<DamageOverlay> availableOverlays = new List<DamageOverlay>();

        private static List<DamageOverlay> overlaysWorkingList = new List<DamageOverlay>();

        private static List<DamageOverlay> overlays = new List<DamageOverlay>();
    }
}

