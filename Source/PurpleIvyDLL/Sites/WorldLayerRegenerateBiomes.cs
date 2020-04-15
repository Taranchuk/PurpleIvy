using System;
using System.Collections;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PurpleIvy
{
    public class WorldLayerRegenerateBiomes : WorldLayer
    {

        public override IEnumerable Regenerate()
        {
            Log.Message("START");
            foreach (object obj in base.Regenerate())
            {
                yield return obj;
            }
            IEnumerator enumerator = null;
            if (PurpleIvyData.BiomesToRenderNow.Count == 0)
            {
                yield break;
            }
            foreach (object obj2 in this.CalculateInterpolatedVerticesParams())
            {
                yield return obj2;
            }
            foreach (int num in PurpleIvyData.BiomesToRenderNow)
            {
                if (num >= 0)
                {
                    Log.Message("RENDER: " + num.ToString());
                    int ind = 0;
                    Material material = Find.WorldGrid[num].biome.DrawMaterial;
                    LayerSubMesh subMesh = base.GetSubMesh(material);
                    subMesh.finalized = false; 
                    Find.World.grid.GetTileVertices(num, this._vertices);
                    int startVertIndex = subMesh.verts.Count;
                    int currentIndex = 0;
                    int maxCount = this._vertices.Count;
                    while (currentIndex < maxCount)
                    {
                        subMesh.verts.Add(this._vertices[currentIndex] + this._vertices[currentIndex].normalized * 0.012f);
                        subMesh.uvs.Add(this.elevationValues[ind]);
                        ind++;
                        if (currentIndex < maxCount - 2)
                        {
                            subMesh.tris.Add(startVertIndex + currentIndex + 2);
                            subMesh.tris.Add(startVertIndex + currentIndex + 1);
                            subMesh.tris.Add(startVertIndex);
                        }
                        currentIndex++;
                    }
                    //PurpleIvyData.BiomesToRenderNow.Remove(num);
                }
                base.FinalizeMesh(MeshParts.All);
            }
            List<int>.Enumerator enumerator2 = default(List<int>.Enumerator);
            this.elevationValues.Clear();
            this.elevationValues.TrimExcess();
            yield break;
            yield break;
        }

        private IEnumerable CalculateInterpolatedVerticesParams()
        {
            this.elevationValues.Clear();
            WorldGrid grid = Find.World.grid;
            int tilesCount = grid.TilesCount;
            List<Vector3> verts = grid.verts;
            List<int> tileIDToVerts_offsets = grid.tileIDToVerts_offsets;
            List<int> tileIDToNeighbors_offsets = grid.tileIDToNeighbors_offsets;
            List<int> tileIDToNeighbors_values = grid.tileIDToNeighbors_values;
            List<Tile> tiles = grid.tiles;
            int num4;
            for (int i = 0; i < tilesCount; i = num4 + 1)
            {
                if (PurpleIvyData.BiomesToRenderNow.Contains(i))
                {
                    Tile tile = tiles[i];
                    float elevation = tile.elevation;
                    int num = (i + 1 < tileIDToNeighbors_offsets.Count) ? tileIDToNeighbors_offsets[i + 1] : tileIDToNeighbors_values.Count;
                    int num2 = (i + 1 < tilesCount) ? tileIDToVerts_offsets[i + 1] : verts.Count;
                    for (int j = tileIDToVerts_offsets[i]; j < num2; j++)
                    {
                        Vector3 vector = default(Vector3);
                        vector.x = elevation;
                        bool flag = false;
                        for (int k = tileIDToNeighbors_offsets[i]; k < num; k++)
                        {
                            int num3 = (tileIDToNeighbors_values[k] + 1 < tileIDToVerts_offsets.Count) ? tileIDToVerts_offsets[tileIDToNeighbors_values[k] + 1] : verts.Count;
                            int l = tileIDToVerts_offsets[tileIDToNeighbors_values[k]];
                            while (l < num3)
                            {
                                if (verts[l] == verts[j])
                                {
                                    Tile tile2 = tiles[tileIDToNeighbors_values[k]];
                                    if (flag)
                                    {
                                        break;
                                    }
                                    if ((tile2.elevation >= 0f && elevation <= 0f) || (tile2.elevation <= 0f && elevation >= 0f))
                                    {
                                        flag = true;
                                        break;
                                    }
                                    if (tile2.elevation > vector.x)
                                    {
                                        vector.x = tile2.elevation;
                                        break;
                                    }
                                    break;
                                }
                                else
                                {
                                    l++;
                                }
                            }
                        }
                        if (flag)
                        {
                            vector.x = 0f;
                        }
                        if (tile.biome.DrawMaterial.shader != ShaderDatabase.WorldOcean && vector.x < 0f)
                        {
                            vector.x = 0f;
                        }
                        this.elevationValues.Add(vector);
                    }
                    if (i % 1000 == 0)
                    {
                        yield return null;
                    }
                }
                num4 = i;
            }
            yield break;
        }

        private List<Vector3> elevationValues = new List<Vector3>();

        private readonly List<Vector3> _vertices = new List<Vector3>();
    }
}
