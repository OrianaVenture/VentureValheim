using Jotunn.Extensions;
using Jotunn.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace VentureValheim.TorchEmbers;

public class TorchEmbers
{
    public const string EMBER_PREFAB = "VV_TorchEmber";

    private struct EmberConfig
    {
        public EmberConfig(string prefab, Vector3 position)
        {
            Prefab = prefab;
            Position = position;
        }

        public string Prefab;
        public Vector3 Position;
    }

    private static readonly List<EmberConfig> Torches = new List<EmberConfig> {
        new EmberConfig("piece_groundtorch", new Vector3(0, 0.74f, 0)),
        new EmberConfig("piece_groundtorch_blue", new Vector3(0, 0.74f, 0)),
        new EmberConfig("piece_groundtorch_green", new Vector3(0, 0.74f, 0)),
        new EmberConfig("piece_groundtorch_wood", new Vector3(0, 0.78f, 0)),
        new EmberConfig("piece_walltorch", new Vector3(0.135f, 0.48f, 0f))
    };

    public static void ApplyChanges()
    {
        GameObject ember = PrefabManager.Cache.GetPrefab<GameObject>(EMBER_PREFAB);

        if (ember == null)
        {
            TorchEmbersPlugin.TorchEmbersLogger.LogWarning("Cannot apply ember configurations.");
            return;
        }

        foreach (EmberConfig torch in Torches)
        {
            GameObject torchGO = PrefabManager.Cache.GetPrefab<GameObject>(torch.Prefab);

            if (torchGO == null)
            {
                TorchEmbersPlugin.TorchEmbersLogger.LogDebug($"Cannot find object for {torch}.");
                continue;
            }

            if (torchGO.transform.FindDeepChild("VV_TorchEmber") != null)
            {
                continue;
            }

            GameObject emberObj = UnityEngine.Object.Instantiate(
                ember, torchGO.transform.position + torch.Position, Quaternion.identity);
            emberObj.transform.parent = torchGO.transform;
            emberObj.name = EMBER_PREFAB;

            TorchEmbersPlugin.TorchEmbersLogger.LogDebug($"Attached ember for {torchGO.name}");
        }

        TorchEmbersPlugin.TorchEmbersLogger.LogInfo($"Done applying ember configurations.");
    }
}