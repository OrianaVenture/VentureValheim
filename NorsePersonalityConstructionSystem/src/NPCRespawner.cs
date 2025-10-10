using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VentureValheim.NPCS;

public class NPCRespawner : MonoBehaviour
{
    private static readonly Dictionary<uint, ZPackage> _npcs = new Dictionary<uint, ZPackage>();

    private static NPCRespawner _instance;

    public static NPCRespawner Instance
    {
        get => _instance;
    }

    public void AddZdo(string prefabName, ZDO zdo)
    {
        IEnumerator enumerator = WaitToRespawn(zdo.m_uid.ID);

        ZPackage package = new ZPackage();
        package.Clear();
        zdo.Serialize(package);
        _npcs.Add(zdo.m_uid.ID, package);

        StartCoroutine(enumerator);
    }

    private IEnumerator WaitToRespawn(uint zdoId)
    {
        yield return new WaitForSeconds(5);
        yield return null;

        RespawnNow(zdoId);
    }

    public void OnDestroy()
    {
        foreach (var package in _npcs.Values)
        {
            NPCFactory.RespawnNPC(package);
        }

        _npcs.Clear();
    }

    private void RespawnNow(uint zdoId)
    {
        if (_npcs.ContainsKey(zdoId))
        {
            NPCFactory.RespawnNPC(_npcs[zdoId]);
            _npcs.Remove(zdoId);
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.Start))]
    private static class Patch_Game_Start
    {
        private static void Postfix(Game __instance)
        {
            _instance = __instance.gameObject.AddComponent<NPCRespawner>();
        }
    }
}