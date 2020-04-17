﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PurpleIvy
{
    [StaticConstructorOnStartup]
    public static class BuildingsToxicDamageSectionLayerUtility
    {
        public static void Notify_BuildingHitPointsChanged(Building b, int oldHitPoints)
        {
            var comp = b.Map.GetComponent<MapComponent_MapEvents>();
            if (comp == null || !comp.ToxicDamages.ContainsKey(b) || comp.ToxicDamages[b] == oldHitPoints ||
                !b.Spawned
                || !b.def.drawDamagedOverlay)
            {
                return;
            }
            b.Map.mapDrawer.MapMeshDirty(b.Position, MapMeshFlag.BuildingsDamage);
        }

        public static bool UsesLinkableCornersAndEdges(Building b)
        {
            return b.def.size.x == 1 && b.def.size.z == 1 && b.def.Fillage == FillCategory.Full;
        }

        public static IList<Material> GetScratchMats(Building b)
        {
            IList<Material> result = BuildingsToxicDamageSectionLayerUtility.DefaultScratchMats;
            if (b.def.graphicData != null && b.def.graphicData.damageData != null && b.def.graphicData.damageData.scratchMats != null)
            {
                result = b.def.graphicData.damageData.scratchMats;
            }
            return result;
        }

        public static List<DamageOverlay> GetAvailableOverlays(Building b)
        {
            BuildingsToxicDamageSectionLayerUtility.availableOverlays.Clear();
            if (BuildingsToxicDamageSectionLayerUtility.GetScratchMats(b).Any<Material>())
            {
                int num = 3;
                Rect damageRect = BuildingsToxicDamageSectionLayerUtility.GetDamageRect(b);
                float num2 = damageRect.width * damageRect.height;
                if (num2 > 4f)
                {
                    num += Mathf.RoundToInt((num2 - 4f) * 0.54f);
                }
                for (int i = 0; i < num; i++)
                {
                    BuildingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.Scratch);
                }
            }
            if (BuildingsToxicDamageSectionLayerUtility.UsesLinkableCornersAndEdges(b))
            {
                if (b.def.graphicData != null && b.def.graphicData.damageData != null)
                {
                    IntVec3 position = b.Position;
                    DamageGraphicData damageData = b.def.graphicData.damageData;
                    if (damageData.edgeTopMat != null && BuildingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x, position.z + 1) && BuildingsToxicDamageSectionLayerUtility.SameAndDamagedAt(b, position.x + 1, position.z) && BuildingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x + 1, position.z + 1))
                    {
                        BuildingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.TopEdge);
                    }
                    if (damageData.edgeRightMat != null && BuildingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x + 1, position.z) && BuildingsToxicDamageSectionLayerUtility.SameAndDamagedAt(b, position.x, position.z + 1) && BuildingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x + 1, position.z + 1))
                    {
                        BuildingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.RightEdge);
                    }
                    if (damageData.edgeBotMat != null && BuildingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x, position.z - 1) && BuildingsToxicDamageSectionLayerUtility.SameAndDamagedAt(b, position.x + 1, position.z) && BuildingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x + 1, position.z - 1))
                    {
                        BuildingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.BotEdge);
                    }
                    if (damageData.edgeLeftMat != null && BuildingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x - 1, position.z) && BuildingsToxicDamageSectionLayerUtility.SameAndDamagedAt(b, position.x, position.z + 1) && BuildingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x - 1, position.z + 1))
                    {
                        BuildingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.LeftEdge);
                    }
                    if (damageData.cornerTLMat != null && BuildingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x - 1, position.z) && BuildingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x, position.z + 1))
                    {
                        BuildingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.TopLeftCorner);
                    }
                    if (damageData.cornerTRMat != null && BuildingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x + 1, position.z) && BuildingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x, position.z + 1))
                    {
                        BuildingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.TopRightCorner);
                    }
                    if (damageData.cornerBRMat != null && BuildingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x + 1, position.z) && BuildingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x, position.z - 1))
                    {
                        BuildingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.BotRightCorner);
                    }
                    if (damageData.cornerBLMat != null && BuildingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x - 1, position.z) && BuildingsToxicDamageSectionLayerUtility.DifferentAt(b, position.x, position.z - 1))
                    {
                        BuildingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.BotLeftCorner);
                    }
                }
            }
            else
            {
                Material x;
                Material x2;
                Material x3;
                Material x4;
                BuildingsToxicDamageSectionLayerUtility.GetCornerMats(out x, out x2, out x3, out x4, b);
                if (x != null)
                {
                    BuildingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.TopLeftCorner);
                }
                if (x2 != null)
                {
                    BuildingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.TopRightCorner);
                }
                if (x4 != null)
                {
                    BuildingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.BotLeftCorner);
                }
                if (x3 != null)
                {
                    BuildingsToxicDamageSectionLayerUtility.availableOverlays.Add(DamageOverlay.BotRightCorner);
                }
            }
            return BuildingsToxicDamageSectionLayerUtility.availableOverlays;
        }

        public static void GetCornerMats(out Material topLeft, out Material topRight, out Material botRight, out Material botLeft, Building b)
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

        public static List<DamageOverlay> GetOverlays(Building b)
        {
            BuildingsToxicDamageSectionLayerUtility.overlays.Clear();
            BuildingsToxicDamageSectionLayerUtility.overlaysWorkingList.Clear();
            BuildingsToxicDamageSectionLayerUtility.overlaysWorkingList.AddRange(BuildingsToxicDamageSectionLayerUtility.GetAvailableOverlays(b));
            if (!BuildingsToxicDamageSectionLayerUtility.overlaysWorkingList.Any<DamageOverlay>())
            {
                return BuildingsToxicDamageSectionLayerUtility.overlays;
            }
            Rand.PushState();
            Rand.Seed = Gen.HashCombineInt(b.thingIDNumber, 1958376471);
            int damageOverlaysCount = BuildingsToxicDamageSectionLayerUtility.GetDamageOverlaysCount(b, b.Map.GetComponent<MapComponent_MapEvents>().ToxicDamages[b]);
            int num = 0;
            while (num < damageOverlaysCount && BuildingsToxicDamageSectionLayerUtility.overlaysWorkingList.Any<DamageOverlay>())
            {
                DamageOverlay item = BuildingsToxicDamageSectionLayerUtility.overlaysWorkingList.RandomElement<DamageOverlay>();
                BuildingsToxicDamageSectionLayerUtility.overlaysWorkingList.Remove(item);
                BuildingsToxicDamageSectionLayerUtility.overlays.Add(item);
                num++;
            }
            Rand.PopState();
            return BuildingsToxicDamageSectionLayerUtility.overlays;
        }

        public static Rect GetDamageRect(Building b)
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

        private static int GetDamageOverlaysCount(Building b, int hp)
        {
            float num = (float)hp / (float)b.MaxHitPoints;
            int count = BuildingsToxicDamageSectionLayerUtility.GetAvailableOverlays(b).Count;
            return count - Mathf.FloorToInt((float)count * num);
        }

        private static bool DifferentAt(Building b, int x, int z)
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

        private static bool SameAndDamagedAt(Building b, int x, int z)
        {
            IntVec3 c = new IntVec3(x, 0, z);
            if (!c.InBounds(b.Map))
            {
                return false;
            }
            List<Thing> thingList = c.GetThingList(b.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                if (thingList[i].def == b.def && thingList[i].Map.GetComponent<MapComponent_MapEvents>()
                    .ToxicDamages[(Building)thingList[i]] < thingList[i].MaxHitPoints)
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
            Building building = Find.Selector.FirstSelectedObject as Building;
            if (building == null)
            {
                return;
            }
            Material material = DebugSolidColorMats.MaterialOf(Color.red);
            Rect damageRect = BuildingsToxicDamageSectionLayerUtility.GetDamageRect(building);
            float y = 14.99f;
            Vector3 pos = new Vector3(damageRect.x + damageRect.width / 2f, y, damageRect.y + damageRect.height / 2f);
            Vector3 s = new Vector3(damageRect.width, 1f, damageRect.height);
            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(pos, Quaternion.identity, s), material, 0);
        }

        private static readonly Material[] DefaultScratchMats = new Material[]
        {
            MaterialPool.MatFrom("Damage/ToxicScratch1"),
            MaterialPool.MatFrom("Damage/ToxicScratch2"),
            MaterialPool.MatFrom("Damage/ToxicScratch3")
        };

        private static List<DamageOverlay> availableOverlays = new List<DamageOverlay>();

        private static List<DamageOverlay> overlaysWorkingList = new List<DamageOverlay>();

        private static List<DamageOverlay> overlays = new List<DamageOverlay>();
    }
}

