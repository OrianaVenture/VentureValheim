using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VentureValheim.MutedSails;

public class MutedSails
{
    public class MutedSailTracker : MonoBehaviour
    {
        public Material TransparentSailMaterial;
        public Material OriginalSailMaterial;

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
        Cloth sailCloth = ship.GetComponentInChildren<Cloth>();

        if (sailCloth == null)
        {
            return;
        }

        SkinnedMeshRenderer sail = sailCloth.GetComponent<SkinnedMeshRenderer>();

        if (sail == null || sail.materials == null || sail.materials.Length < 1)
        {
            return;
        }

        Material originalMaterial = sail.materials[0];
        Texture2D texture = originalMaterial.mainTexture as Texture2D;

        Shader shader = PrefabManager.Cache.GetPrefab<Shader>("Custom/LitParticles");
        Material material = new Material(shader);
        material.SetTexture("_MainTex", texture);
        material.SetFloat("_Cutoff", 0.1f);
        material.SetVector("_Color", new Vector4(1, 1, 1, 0.2f));

        MutedSailTracker mutedSail = ship.gameObject.AddComponent<MutedSailTracker>();
        mutedSail.TransparentSailMaterial = material;
        mutedSail.OriginalSailMaterial = originalMaterial;
    }

    [HarmonyPatch(typeof(Ship), nameof(Ship.Awake))]
    public static class Patch_Ship_Awake
    {
        private static void Postfix(Ship __instance)
        {
            if (!__instance.TryGetComponent<MutedSailTracker>(out MutedSailTracker sails))
            {
                AddMutedSailTracker(ref __instance);
                MutedSailsPlugin.MutedSailsLogger.LogWarning($"Added late sail information to {__instance.name}! This should not happen!");
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
            if (__state || __instance.m_sailCloth == null || Player.m_localPlayer == null)
            {
                return;
            }

            SkinnedMeshRenderer sail = __instance.m_sailCloth.GetComponent<SkinnedMeshRenderer>();
            MutedSailTracker mutedSail = __instance.gameObject.GetComponent<MutedSailTracker>();

            if (sail == null || mutedSail == null)
            {
                return;
            }

            if (__instance.HasPlayerOnboard())
            {
                sail.material = mutedSail.TransparentSailMaterial;
            }
            else
            {
                sail.material = mutedSail.OriginalSailMaterial;
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
}