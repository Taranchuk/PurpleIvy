using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PurpleIvy
{
    public class WorldUpdater
    {
        public WorldUpdater()
        {
            FieldInfo fieldlayers = typeof(WorldRenderer).GetField("layers", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fieldMeshes = typeof(WorldLayer).GetField("subMeshes", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static);
            var tempLayers = fieldlayers.GetValue(Find.World.renderer) as List<WorldLayer>;
            Layers = new Dictionary<string, WorldLayer>(tempLayers.Count);
            LayersSubMeshes = new Dictionary<string, List<LayerSubMesh>>(tempLayers.Count);
            foreach (var layer in tempLayers)
            {
                Layers.Add(layer.GetType().Name, layer);
                var meshes = fieldMeshes.GetValue(layer) as List<LayerSubMesh>;
                LayersSubMeshes.Add(layer.GetType().Name, meshes);
            }
        }
        public void UpdateLayer(WorldLayer layer)
        {
            layer.RegenerateNow();
        }

        public void UpdateLayer(WorldLayer layer, Material material, List<LayerSubMesh> subMeshes, BiomeDef b)
        {
            int i;
            LayerSubMesh subMesh = this.GetSubMesh(material, subMeshes, out i);
            List<int> list = new List<int>();
            List<Tile> list2 = Find.WorldGrid.tiles.Where<Tile>((Tile tile) => tile.biome == b).ToList<Tile>();
            for (int j = 0; j < Find.WorldGrid.TilesCount; j++)
            {
                Tile tile2 = Find.WorldGrid[j];
                if (list2.Contains(tile2))
                {
                    list.Add(j);
                }
            }
            WorldGrid grid = Find.World.grid;
            List<int> tileIDToVerts_offsets = grid.tileIDToVerts_offsets;
            List<Vector3> verts = grid.verts;
            List<List<int>> list3 = new List<List<int>>();
            List<Vector3> list4 = new List<Vector3>();
            list3.Clear();
            subMesh.Clear(MeshParts.All);
            foreach (int num in list)
            {
                int num2 = 0;
                BiomeDef biome = list2[num].biome;
                while (i >= list3.Count)
                {
                    list3.Add(new List<int>());
                }
                int count = subMesh.verts.Count;
                int num3 = 0;
                int num4 = (num + 1 >= tileIDToVerts_offsets.Count) ? verts.Count : tileIDToVerts_offsets[num + 1];
                for (int k = tileIDToVerts_offsets[num]; k < num4; k++)
                {
                    subMesh.verts.Add(verts[k]);
                    subMesh.uvs.Add(list4[num2]);
                    num2++;
                    if (k < num4 - 2)
                    {
                        subMesh.tris.Add(count + num3 + 2);
                        subMesh.tris.Add(count + num3 + 1);
                        subMesh.tris.Add(count);
                        list3[i].Add(num);
                    }
                    num3++;
                }
            }
            this.FinalizeMesh(MeshParts.All, subMesh);
        }

        public void RenderSingleTile(int tileID, Material drawMaterial, string subMeshesKey)
        {
            Log.Message("RenderSingleTile: " + tileID.ToString());
            var subMeshes = this.LayersSubMeshes[subMeshesKey];
            LayerSubMesh subMesh = this.GetSubMesh(drawMaterial, subMeshes);
            subMesh.finalized = false;
            List<Vector3> list = new List<Vector3>();
            Find.WorldGrid.GetTileVertices(tileID, list);
            int count = subMesh.verts.Count;
            int i = 0;
            int count2 = list.Count;
            while (i < count2)
            {
                subMesh.verts.Add(list[i] + list[i].normalized * 0.012f);
                subMesh.uvs.Add((GenGeo.RegularPolygonVertexPosition(count2, i) + Vector2.one) / 2f);
                if (i < count2 - 2)
                {
                    subMesh.tris.Add(count + i + 2);
                    subMesh.tris.Add(count + i + 1);
                    subMesh.tris.Add(count);
                }
                i++;
            }
            this.FinalizeMesh(MeshParts.All, subMesh);
        }

        public void FinalizeMesh(MeshParts tags, List<LayerSubMesh> subMeshes)
        {
            foreach (var t in subMeshes.Where(t => t.verts.Count > 0))
            {
                t.FinalizeMesh(tags);
            }
        }

        public void FinalizeMesh(MeshParts tags, LayerSubMesh subMesh)
        {
            if (subMesh.verts.Count > 0)
            {
                subMesh.FinalizeMesh(tags);
            }
        }

        private LayerSubMesh GetSubMesh(Material material, List<LayerSubMesh> subMeshes)
        {
            int num;
            return this.GetSubMesh(material, subMeshes, out num);
        }

        private LayerSubMesh GetSubMesh(Material material, List<LayerSubMesh> subMeshes, out int subMeshIndex)
        {
            for (int i = 0; i < subMeshes.Count; i++)
            {
                LayerSubMesh layerSubMesh = subMeshes[i];
                if (layerSubMesh.material == material && layerSubMesh.verts.Count < 40000)
                {
                    subMeshIndex = i;
                    return layerSubMesh;
                }
            }
            Mesh mesh = new Mesh();
            LayerSubMesh layerSubMesh2 = new LayerSubMesh(mesh, material);
            subMeshIndex = subMeshes.Count;
            subMeshes.Add(layerSubMesh2);
            return layerSubMesh2;


            //for (int i = 0; i < subMeshes.Count; i++)
            //{
            //    LayerSubMesh layerSubMesh = subMeshes[i];
            //    if (layerSubMesh.material != material || layerSubMesh.verts.Count >= 40000) continue;
            //    subMeshIndex = i;
            //    return layerSubMesh;
            //}
            //LayerSubMesh layerSubMesh2 = new LayerSubMesh(new Mesh(), material);
            //subMeshIndex = subMeshes.Count;
            //subMeshes.Add(layerSubMesh2);
            //return layerSubMesh2;
        }

        public void RenderSingleHill(int tileID, List<LayerSubMesh> subMeshes)
        {
            WorldGrid worldGrid = Find.WorldGrid;
            int tilesCount = worldGrid.TilesCount;
            Tile tile = worldGrid[tileID];
            Material material = WorldMaterials.SmallHills;
            FloatRange floatRange = this.BasePosOffsetRange_SmallHills;
            switch (tile.hilliness)
            {
                case Hilliness.SmallHills:
                    material = WorldMaterials.SmallHills;
                    floatRange = this.BasePosOffsetRange_SmallHills;
                    break;
                case Hilliness.LargeHills:
                    material = WorldMaterials.LargeHills;
                    floatRange = this.BasePosOffsetRange_LargeHills;
                    break;
                case Hilliness.Mountainous:
                    material = WorldMaterials.Mountains;
                    floatRange = this.BasePosOffsetRange_Mountains;
                    break;
                case Hilliness.Impassable:
                    material = WorldMaterials.ImpassableMountains;
                    floatRange = this.BasePosOffsetRange_ImpassableMountains;
                    break;
                case Hilliness.Undefined:
                    break;
                case Hilliness.Flat:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            LayerSubMesh subMesh = this.GetSubMesh(material, subMeshes);
            Vector3 vector = worldGrid.GetTileCenter(tileID);
            Vector3 vector2 = vector;
            float magnitude = vector.magnitude;
            vector = (vector + Rand.UnitVector3 * floatRange.RandomInRange * worldGrid.averageTileSize).normalized * magnitude;
            WorldRendererUtility.PrintQuadTangentialToPlanet(vector, vector2, this.BaseSizeRange.RandomInRange * worldGrid.averageTileSize, 0.005f, subMesh, false, true, false);
            IntVec2 texturesInAtlas = this.TexturesInAtlas;
            int num = Rand.Range(0, texturesInAtlas.x);
            IntVec2 texturesInAtlas2 = this.TexturesInAtlas;
            int num2 = Rand.Range(0, texturesInAtlas2.z);
            int x = this.TexturesInAtlas.x;
            IntVec2 texturesInAtlas3 = this.TexturesInAtlas;
            WorldRendererUtility.PrintTextureAtlasUVs(num, num2, x, texturesInAtlas3.z, subMesh);
            this.FinalizeMesh(MeshParts.All, subMeshes);
        }

        public void RenderSingleHill(List<int> tileIDs, List<LayerSubMesh> subMeshes)
        {
            WorldGrid worldGrid = Find.WorldGrid;
            foreach (int num in tileIDs)
            {
                Tile tile = worldGrid[num];
                Material material = WorldMaterials.SmallHills;
                FloatRange floatRange = this.BasePosOffsetRange_SmallHills;
                switch (tile.hilliness)
                {
                    case Hilliness.SmallHills:
                        material = WorldMaterials.SmallHills;
                        floatRange = this.BasePosOffsetRange_SmallHills;
                        break;
                    case Hilliness.LargeHills:
                        material = WorldMaterials.LargeHills;
                        floatRange = this.BasePosOffsetRange_LargeHills;
                        break;
                    case Hilliness.Mountainous:
                        material = WorldMaterials.Mountains;
                        floatRange = this.BasePosOffsetRange_Mountains;
                        break;
                    case Hilliness.Impassable:
                        material = WorldMaterials.ImpassableMountains;
                        floatRange = this.BasePosOffsetRange_ImpassableMountains;
                        break;
                    case Hilliness.Undefined:
                        break;
                    case Hilliness.Flat:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                LayerSubMesh subMesh = this.GetSubMesh(material, subMeshes);
                Vector3 vector = worldGrid.GetTileCenter(num);
                Vector3 vector2 = vector;
                float magnitude = vector.magnitude;
                vector = (vector + Rand.UnitVector3 * floatRange.RandomInRange * worldGrid.averageTileSize).normalized * magnitude;
                WorldRendererUtility.PrintQuadTangentialToPlanet(vector, vector2, this.BaseSizeRange.RandomInRange * worldGrid.averageTileSize, 0.005f, subMesh, false, true, false);
                IntVec2 texturesInAtlas = this.TexturesInAtlas;
                int num2 = Rand.Range(0, texturesInAtlas.x);
                IntVec2 texturesInAtlas2 = this.TexturesInAtlas;
                int num3 = Rand.Range(0, texturesInAtlas2.z);
                int x = this.TexturesInAtlas.x;
                IntVec2 texturesInAtlas3 = this.TexturesInAtlas;
                WorldRendererUtility.PrintTextureAtlasUVs(num2, num3, x, texturesInAtlas3.z, subMesh);
            }
            this.FinalizeMesh(MeshParts.All, subMeshes);
        }

        private void ClearSubMeshes(MeshParts parts, List<LayerSubMesh> subMeshes)
        {
            foreach (var t in subMeshes)
            {
                t.Clear(parts);
            }
        }
        public Dictionary<string, WorldLayer> Layers;

        public Dictionary<string, List<LayerSubMesh>> LayersSubMeshes = new Dictionary<string, List<LayerSubMesh>>();

        private readonly FloatRange BaseSizeRange = new FloatRange(0.9f, 1.1f);

        private readonly IntVec2 TexturesInAtlas = new IntVec2(2, 2);

        private readonly FloatRange BasePosOffsetRange_SmallHills = new FloatRange(0f, 0.37f);

        private readonly FloatRange BasePosOffsetRange_LargeHills = new FloatRange(0f, 0.2f);

        private readonly FloatRange BasePosOffsetRange_Mountains = new FloatRange(0f, 0.08f);

        private readonly FloatRange BasePosOffsetRange_ImpassableMountains = new FloatRange(0f, 0.08f);
    }
}
