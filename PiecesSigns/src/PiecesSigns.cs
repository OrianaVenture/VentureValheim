using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using TMPro;
using UnityEngine;

namespace VentureValheim.VentureSigns;

public class PiecesSigns
{
    public static void Initialize()
    {
        MakeSign("VV_SignLeft");
        MakeSign("VV_SignRight");
        MakePole("VV_SignPole3m");

        Jotunn.Managers.PrefabManager.OnPrefabsRegistered -= Initialize;
    }

    private static void MakeSign(string assetName)
    {
        GameObject go = VentureSignsPlugin.SignBundle.LoadAsset<GameObject>(assetName);
        string name = go.name;
        go.name = go.name + "_old";
        GameObject clone = PrefabManager.Instance.CreateClonedPrefab(name, go);
        PieceConfig config = new PieceConfig()
        {
            PieceTable = PieceTables.Hammer,
            Category = PieceCategories.Furniture,
            Usage = new string[] { PieceUsages.Decor },
            Name = "$piece_sign",
            Description = "$piece_sign_description",
            CraftingStation = CraftingStations.Workbench,
            Requirements = new RequirementConfig[]
            {
                new RequirementConfig
                {
                    Item = "Wood",
                    Amount = 2,
                    Recover = true
                },
                new RequirementConfig
                {
                    Item = "Coal",
                    Amount = 1,
                    Recover = true
                }
            }
        };

        PieceManager.Instance.AddPiece(new CustomPiece(clone, true, config));
    }

    private static void MakePole(string assetName)
    {
        GameObject go = VentureSignsPlugin.SignBundle.LoadAsset<GameObject>(assetName);
        string name = go.name;
        go.name = go.name + "_old";
        GameObject clone = PrefabManager.Instance.CreateClonedPrefab(name, go);
        PieceConfig config = new PieceConfig()
        {
            PieceTable = PieceTables.Hammer,
            Category = PieceCategories.Furniture,
            Usage = new string[] { PieceUsages.Decor },
            Name = "$piece_woodpole 3m",
            Description = "$piece_woodpoles_description",
            CraftingStation = CraftingStations.Workbench,
            Requirements = new RequirementConfig[]
            {
                new RequirementConfig
                {
                    Item = "Wood",
                    Amount = 2,
                    Recover = true
                }
            }
        };

        PieceManager.Instance.AddPiece(new CustomPiece(clone, true, config));
    }

    /// <summary>
    /// Fix up references due to mocking sign canvas.
    /// </summary>
    [HarmonyPatch(typeof(Sign), nameof(Sign.Awake))]
    public static class Patch_Sign_Awake
    {
        private static void Prefix(Sign __instance)
        {
            if (__instance.m_textWidget == null)
            {
                __instance.m_textWidget = __instance.GetComponentInChildren<TextMeshProUGUI>();
            }
        }
    }
}