using System;
using System.Collections;
using System.Collections.Generic;
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
            foreach (int num in PurpleIvyData.BiomesToRenderNow)
            {
                if (num >= 0)
                {
                    Log.Message("RENDER: " + num.ToString());
                    Material material = Find.WorldGrid[num].biome.DrawMaterial;
                    LayerSubMesh subMesh = base.GetSubMesh(material);
                    subMesh.finalized = false; 
                    Find.World.grid.GetTileVertices(num, this._vertices);
                    int startVertIndex = subMesh.verts.Count;
                    int currentIndex = 0;
                    int maxCount = this._vertices.Count;
                    while (currentIndex < maxCount)
                    {
                        if (currentIndex % 1000 == 0)
                        {
                            yield return null;
                        }
                        if (subMesh.verts.Count > 60000)
                        {
                            subMesh = base.GetSubMesh(material);
                        }
                        subMesh.verts.Add(this._vertices[currentIndex] + this._vertices[currentIndex].normalized * 0.012f);
                        if (currentIndex < maxCount - 2)
                        {
                            subMesh.tris.Add(startVertIndex + currentIndex + 2);
                            subMesh.tris.Add(startVertIndex + currentIndex + 1);
                            subMesh.tris.Add(startVertIndex);
                        }
                        int num2 = currentIndex;
                        currentIndex = num2 + 1;
                    }
                    subMesh.FinalizeMesh(MeshParts.All);
                    material = null;
                    subMesh = null;
                    //PurpleIvyData.BiomesToRenderNow.Remove(num);
                }
            }
            List<int>.Enumerator enumerator2 = default(List<int>.Enumerator);
            yield break;
            yield break;
        }

        private readonly List<Vector3> _vertices = new List<Vector3>();
    }
}
