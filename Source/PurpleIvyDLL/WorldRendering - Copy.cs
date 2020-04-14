using System;
using System.Collections;
using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PurpleIvy
{
    public class WorldLayer_SingleTilePlus : WorldLayer
    {

        public override bool ShouldRegenerate
        {
            get
            {
                return base.ShouldRegenerate;
            }
        }

        public override IEnumerable Regenerate()
        {
            foreach (object obj in base.Regenerate())
            {
                yield return obj;
            }
            IEnumerator enumerator = null;
            if (PurpleIvyData.TotalPollutedBiomes.Count == 0)
            {
                yield break;
            }
            foreach (int tile in PurpleIvyData.BiomesToRenderNow)
            {
                Material material = MaterialPool.MatFrom(Find.WorldGrid[tile].biome.texture);
                LayerSubMesh subMesh = this.GetSubMesh(material);
                Log.Message("Rendering: " + Find.WorldGrid[tile].biome.defName + " - " + tile.ToString());
                Log.Message(Find.WorldGrid[tile].biome.texture);
                Find.WorldGrid.GetTileVertices(tile, this.verts);
                int count = subMesh.verts.Count;
                int i = 0;
                int count2 = this.verts.Count;
                while (i < count2)
                {
                    subMesh.verts.Add(this.verts[i] + this.verts[i].normalized * 0.012f);
                    subMesh.uvs.Add((GenGeo.RegularPolygonVertexPosition(count2, i) + Vector2.one) / 2f);
                    if (i < count2 - 2)
                    {
                        subMesh.tris.Add(count + i + 2);
                        subMesh.tris.Add(count + i + 1);
                        subMesh.tris.Add(count);
                    }
                    i++;
                }
                //base.FinalizeMesh(MeshParts.All);
                subMesh.FinalizeMesh(MeshParts.All);
            }
            //this.lastDrawnTile = tile;
            yield break;
            yield break;
        }

        private int lastDrawnTile = -1;

        private List<Vector3> verts = new List<Vector3>();
    }
}
