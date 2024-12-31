using System.Collections.Generic;
using UnityEngine;

namespace VentureValheim.LocationReset
{
    /// <summary>
    /// Terrain Resetting code from terrain reset mod.
    /// </summary>
    public class TerrainReset
    {
        public static int ResetTerrain(Vector3 center, float radius)
        {
            int resets = 0;
            List<Heightmap> list = new List<Heightmap>();

            Heightmap.FindHeightmap(center, radius + 100, list);

            // Reset terrain modifiers
            List<TerrainModifier> allInstances = TerrainModifier.GetAllInstances();
            foreach (TerrainModifier terrainModifier in allInstances)
            {
                Vector3 position = terrainModifier.transform.position;
                ZNetView nview = terrainModifier.GetComponent<ZNetView>();

                if (nview == null || !nview.IsValid())
                {
                    continue;
                }

                if (Utils.DistanceXZ(position, center) <= radius)
                {
                    if (!nview.IsOwner())
                    {
                        nview.ClaimOwnership();
                    }

                    resets++;
                    foreach (Heightmap heightmap in list)
                    {
                        if (heightmap.TerrainVSModifier(terrainModifier))
                            heightmap.Poke(true);
                    }
                    nview.Destroy();
                }
            }

            // Reset Heightmaps & Terrain Paints
            using (List<Heightmap>.Enumerator enumerator = list.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    TerrainComp terrainComp = TerrainComp.FindTerrainCompiler(enumerator.Current.transform.position);
                    if (!terrainComp)
                        continue;

                    if (!terrainComp.m_nview.IsOwner())
                    {
                        terrainComp.m_nview.ClaimOwnership();
                    }

                    if (!terrainComp.m_initialized)
                    {
                        continue;
                    }

                    // These should always result in the same x, y. But will leave both for now.
                    enumerator.Current.WorldToVertex(center, out int VertexX, out int VertexY);
                    enumerator.Current.WorldToVertexMask(center, out int MaskX, out int MaskY);

                    bool thisReset = false;
                    int num = terrainComp.m_width + 1;
                    for (int h = 0; h < num; h++)
                    {
                        for (int w = 0; w < num; w++)
                        {
                            int idx = h * num + w;

                            // Reset heights
                            float disVetex = CoordDistance(VertexX, VertexY, w, h);

                            if (disVetex <= radius && terrainComp.m_modifiedHeight[idx])
                            {
                                resets++;
                                thisReset = true;
                                terrainComp.m_modifiedHeight[idx] = false;
                                terrainComp.m_levelDelta[idx] = 0;
                                terrainComp.m_smoothDelta[idx] = 0;
                            }

                            // Reset paint
                            float disMask = CoordDistance(MaskX, MaskY, w, h);

                            if (disMask <= radius && terrainComp.m_modifiedPaint[idx])
                            {
                                thisReset = true;
                                terrainComp.m_modifiedPaint[idx] = false;
                                terrainComp.m_paintMask[idx] = Color.clear;
                            }
                        }
                    }

                    if (thisReset)
                    {
                        terrainComp.Save();
                        enumerator.Current.Poke(true);
                    }
                }
            }

            // Reset Grass
            if (ClutterSystem.instance != null)
            {
                ClutterSystem.instance.ResetGrass(center, radius);
            }

            return resets;
        }

        private static float CoordDistance(float x, float y, float rx, float ry)
        {
            float deltaX = x - rx;
            float deltaY = y - ry;
            return Mathf.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
        }
    }
}