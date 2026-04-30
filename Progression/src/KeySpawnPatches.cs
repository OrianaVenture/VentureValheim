using BepInEx;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VentureValheim.Progression;

public partial class KeyManager
{
    public static Dictionary<string, string> SpawnSystemListWithKeys = new Dictionary<string, string>();

    /// <summary>
    /// Destroy night spawns if the hosting player does not have the appropriate keys.
    /// </summary>
    [HarmonyPatch(typeof(Character), nameof(Character.Awake))]
    private static class Patch_Character_Awake
    {
        private static void Postfix(Character __instance)
        {
            if (!ProgressionAPI.IsInTheMainScene())
            {
                return;
            }

            __instance.gameObject.AddComponent<ProgressionSpawnWatcher>();
        }
    }

    [HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.Awake))]
    public static class Patch_SpawnSystem_Awake
    {
        private static void Postfix(SpawnSystem __instance)
        {
            foreach (SpawnSystemList list in __instance.m_spawnLists)
            {
                foreach (SpawnSystem.SpawnData data in list.m_spawners)
                {
                    if (data != null &&
                        data.m_spawnAtNight &&
                        !data.m_requiredGlobalKey.IsNullOrWhiteSpace() &&
                        !SpawnSystemListWithKeys.ContainsKey(data.m_prefab.name))
                    {
                        ProgressionPlugin.VentureProgressionLogger.LogDebug(
                            $"Tracking night spawn table entry: {data.m_prefab.name} with key {data.m_requiredGlobalKey}");
                        SpawnSystemListWithKeys.Add(data.m_prefab.name, data.m_requiredGlobalKey);
                    }
                }
            }
        }
    }
}

public class ProgressionSpawnWatcher : MonoBehaviour
{
    IEnumerator checkerCoroutine;

    public void Start()
    {
        checkerCoroutine = WaitForCheck();
        StartCoroutine(checkerCoroutine);
    }

    public IEnumerator WaitForCheck()
    {
        yield return new WaitForSeconds(5);
        yield return null;

        Character character = gameObject.GetComponent<Character>();
        if (character == null || !character.m_nview.IsValid())
        {
            yield break;
        }

        int tries = 0;

        while (!character.m_nview.IsOwner())
        {
            tries++;

            if (tries > 100)
            {
                yield break;
            }

            yield return new WaitForSeconds(1);
        }

        string prefabName = Utils.GetPrefabName(character.name);
        if (!KeyManager.SpawnSystemListWithKeys.ContainsKey(prefabName) ||
            !character.TryGetComponent<MonsterAI>(out MonsterAI ai))
        {
            yield break;
        }

        if (ai.DespawnInDay() && !KeyManager.Instance.HasKey(KeyManager.SpawnSystemListWithKeys[prefabName]))
        {
            ProgressionPlugin.VentureProgressionLogger.LogDebug(
                $"Destroying high level night spawn {prefabName}");

            ai.MoveAwayAndDespawn(0f, false);
        }

        yield return null;
        Destroy(this);
    }

    public void OnDestroy()
    {
        if (checkerCoroutine != null)
        {
            StopCoroutine(checkerCoroutine);
        }
    }
}
