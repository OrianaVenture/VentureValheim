using HarmonyLib;
using Jotunn.Managers;
using MagicaCloth2;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VentureValheim.MutedSails;

public class MutedSails
{
    public static bool ConfigurationDirty = false;

    public class MutedSailTracker : MonoBehaviour
    {
        public Material TransparentSailMaterial;
        public Material OriginalSailMaterial;
        public SkinnedMeshRenderer SkinnedMeshRenderer;

        public bool IsTransparent = false;
    }

    /// <summary>
    /// Finds all ships and attaches MutedSail material trackers.
    /// </summary>
    public static void UpdateSails()
    {
        foreach (GameObject obj in ZNetScene.instance.m_prefabs)
        {
            if (obj!= null && obj.TryGetComponent<Ship>(out Ship ship))
            {
                AddMutedSailTracker(ref ship);
                MutedSailsPlugin.MutedSailsLogger.LogInfo($"Tracking sail information for {obj.name}!");
            }
        }
    }

    public static void AddMutedSailTracker(ref Ship ship)
    {
        MagicaCloth sailCloth = ship.GetComponentInChildren<MagicaCloth>();

        if (sailCloth == null)
        {
            return;
        }

        SkinnedMeshRenderer sail = sailCloth.GetComponentInChildren<SkinnedMeshRenderer>();

        if (sail == null || sail.materials == null || sail.materials.Length < 1)
        {
            return;
        }

        Material originalMaterial = sail.materials[0];
        Texture2D texture = originalMaterial.mainTexture as Texture2D;

        Shader shader = PrefabManager.Cache.GetPrefab<Shader>("Custom/LitParticles");
        Material material = new Material(shader);
        material.SetTexture("_MainTex", texture);

        material.SetTexture("_MainTex", texture);
        material.SetTextureScale("_MainTex", originalMaterial.GetTextureScale("_MainTex"));
        material.SetTextureOffset("_MainTex", originalMaterial.GetTextureOffset("_MainTex"));
        material.SetFloat("_Cutoff", 0.1f);
        material.SetVector("_Color", new Vector4(1, 1, 1, 0.2f));

        MutedSailTracker mutedSail = ship.gameObject.AddComponent<MutedSailTracker>();
        mutedSail.TransparentSailMaterial = material;
        mutedSail.OriginalSailMaterial = originalMaterial;
        mutedSail.SkinnedMeshRenderer = sail;
    }

    [HarmonyPatch(typeof(Ship), nameof(Ship.Awake))]
    public static class Patch_Ship_Awake
    {
        private static void Postfix(Ship __instance)
        {
            if (!Player.IsPlacementGhost(__instance.gameObject) &&
                !__instance.TryGetComponent<MutedSailTracker>(out MutedSailTracker sails))
            {
                AddMutedSailTracker(ref __instance);
                MutedSailsPlugin.MutedSailsLogger.LogWarning(
                    $"Added late sail information to {__instance.name}! This should not happen!");
            }
        }
    }

    [HarmonyPatch(typeof(Ship), nameof(Ship.UpdateSailSize))]
    public static class Patch_Ship_UpdateSailSize
    {
        private static void Prefix(Ship __instance, out bool __state)
        {
            __state = __instance.m_sailWasInPosition;
        }

        private static void Postfix(Ship __instance, bool __state)
        {
            if (__instance.m_sailCloth == null)
            {
                return;
            }

            MutedSailTracker mutedSail = __instance.gameObject.GetComponent<MutedSailTracker>();

            if (mutedSail == null || mutedSail.SkinnedMeshRenderer == null)
            {
                return;
            }

            bool shouldBeTransparent = MutedSailsPlugin.GetTransparencyEnabled() && __instance.HasPlayerOnboard();

            if (shouldBeTransparent && !mutedSail.IsTransparent)
            {
                mutedSail.SkinnedMeshRenderer.material = mutedSail.TransparentSailMaterial;
                mutedSail.IsTransparent = true;
                __instance.m_sailCloth.SetParameterChange();
            }
            else if (!shouldBeTransparent && mutedSail.IsTransparent)
            {
                mutedSail.SkinnedMeshRenderer.material = mutedSail.OriginalSailMaterial;
                mutedSail.IsTransparent = false;
                __instance.m_sailCloth.SetParameterChange();
            }
        }
    }

    [HarmonyPriority(Priority.VeryLow)]
    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    public static class Patch_ObjectDB_Awake
    {
        private static void Postfix()
        {
            if (SceneManager.GetActiveScene().name.Equals("main"))
            {
                UpdateSails();
            }
        }
    }

    // Thank you Redsekio for the template!
    [HarmonyPatch(typeof(Player))]
    static class PlayerPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Player), nameof(Player.Update))]
        static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .Start()
                .MatchStartForward(
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Player), nameof(Player.UpdateHover))))
                .ThrowIfInvalid($"Could not patch Player.Update()!")
                .Advance(offset: 1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlayerPatch), nameof(UpdateInputDelegate))))
                .InstructionEnumeration();
        }

        static void UpdateInputDelegate(Player player, bool takeInput)
        {
            if (takeInput && ZInput.GetKeyDown(MutedSailsPlugin.GetToggleKey()))
            {
                MutedSailsPlugin.CE_TransparencyEnabled.BoxedValue = !MutedSailsPlugin.CE_TransparencyEnabled.Value;
                ConfigurationDirty = true;
            }
        }
    }
}