using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VentureValheim.TerrainReset;

public class TerrainReset
{
    private TerrainReset()
    {
    }
    private static readonly TerrainReset _instance = new TerrainReset();

    public static TerrainReset Instance
    {
        get => _instance;
    }

    public static bool IgnoreKeyPresses()
    {
        return ZNetScene.instance == null || Player.m_localPlayer == null || 
            Minimap.IsOpen() || Console.IsVisible() || TextInput.IsVisible() || 
            ZNet.instance.InPasswordDialog() || 
            (Chat.instance != null && Chat.instance.HasFocus() == true) || 
            StoreGui.IsVisible() || InventoryGui.IsVisible() || Menu.IsVisible() || 
            (TextViewer.instance != null && TextViewer.instance.IsVisible() == true);
    }

    public static bool CheckKeyDown(KeyCode value)
    {
        return ZInput.GetKeyDown(value);
    }

    public static bool CheckKeyHeld(KeyCode value)
    {
        return ZInput.GetKey(value);
    }

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

    /// <summary>
    /// Replace the performed operation with the reset when modifier key is held.
    /// </summary>
    [HarmonyPatch(typeof(TerrainComp), nameof(TerrainComp.ApplyOperation))]
    private static class Patch_TerrainComp_ApplyOperation
    {
        private static bool Prefix(TerrainOp modifier)
        {
            if (TerrainResetPlugin.GetModEnabled() &&
                CheckKeyHeld(TerrainResetPlugin.GetToolModKey()))
            {
                // Reset, do not send RPC
                var pos = modifier.transform.position;
                ResetTerrain(pos, TerrainResetPlugin.GetToolRadius() > 0 ?
                    TerrainResetPlugin.GetToolRadius() : modifier.GetRadius());
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Prevent original TerrainOp from applying when resetting.
    /// </summary>
    [HarmonyPatch(typeof(TerrainOp), nameof(TerrainOp.OnPlaced))]
    private static class Patch_TerrainOp_OnPlaced
    {
        private static bool Prefix()
        {
            if (TerrainResetPlugin.GetModEnabled() &&
                CheckKeyHeld(TerrainResetPlugin.GetToolModKey()))
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Adds command for resetting terrain.
    /// </summary>
    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
    private static class Patch_Terminal_InitTerminal
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(out bool __state)
        {
            __state = Terminal.m_terminalInitialized;
        }

        private static void Postfix(bool __state)
        {
            if (__state || !TerrainResetPlugin.GetModEnabled())
            {
                return;
            }

            TerrainResetPlugin.TerrainResetLogger.LogInfo("Adding Terminal Command \"resetterrain\".");

            try
            {
                new Terminal.ConsoleCommand("resetterrain", "[name] [radius]", delegate (Terminal.ConsoleEventArgs args)
                {
                    if (args.Length < 2)
                    {
                        args.Context.AddString("Syntax: resetterrain [radius]");
                        return;
                    }

                    if (float.TryParse(args[1], out float radius))
                    {
                        int resets = ResetTerrain(Player.m_localPlayer.transform.position, radius);
                        args.Context.AddString(string.Format("{0} edits reset.", resets));
                    }
                    else
                    {
                        args.Context.AddString("Invalid command, syntax: resetterrain [radius as a float]");
                    }
                }, isCheat: false, isNetwork: false, onlyServer: false);
            }
            catch (Exception e)
            {
                TerrainResetPlugin.TerrainResetLogger.LogWarning("Error, could not add terminal command. " +
                    "This can happen when two mods add the same command. " +
                    "The rest of this mod should work as expected.");
                TerrainResetPlugin.TerrainResetLogger.LogWarning(e);
            }
        }
    }
}