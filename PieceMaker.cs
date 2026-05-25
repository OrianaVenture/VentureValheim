using BepInEx.Logging;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VentureValheim;

public class PieceMaker
{
    internal ManualLogSource PieceMakerLogger;

    public PieceMaker(ManualLogSource logger)
    {
        PieceMakerLogger = logger;
    }

    public static string[] RoughStonePieces =
    {
        "stone_pillar",
        "stone_arch",
        "stone_wall_4x2"
    };

    public static string[] GraustenPieces =
    {
        "Piece_grausten_stone_ladder",
        "Piece_grausten_floor_1x1",
        "Piece_grausten_floor_2x2",
        "Piece_grausten_floor_4x4",
        "Piece_grausten_pillarbase_small",
        "Piece_grausten_pillarbase_medium",
        "Piece_grausten_pillarbase_tapered",
        "Piece_grausten_pillarbase_tapered_inverted",
        "Piece_grausten_pillarbeam_small",
        "Piece_grausten_pillarbeam_medium",
        "Piece_grausten_pillar_arch_small",
        "Piece_grausten_pillar_arch",
        "Piece_grausten_wall_arch",
        "Piece_grausten_wall_arch_inverted",
        "Piece_grausten_wall_1x2",
        "Piece_grausten_wall_2x2",
        "Piece_grausten_wall_4x2",
        "Piece_grausten_window_2x2",
        "Piece_grausten_window_4x2"
    };

    public static string[] GraustenRoofPieces =
    {
        "piece_grausten_roof_45",
        "piece_grausten_roof_45_corner",
        "piece_grausten_roof_45_corner2",
        "piece_grausten_roof_45_arch",
        "piece_grausten_roof_45_arch_corner",
        "piece_grausten_roof_45_arch_corner2"
    };

    public static string[] BlackMarblePieces =
    {
        "blackmarble_1x1",
        "blackmarble_2x1x1",
        "blackmarble_2x2x2",
        "blackmarble_floor",
        "blackmarble_floor_triangle",
        "blackmarble_stair",
        "blackmarble_tip",
        "blackmarble_base_1",
        "blackmarble_basecorner",
        "blackmarble_out_1",
        "blackmarble_outcorner",
        "blackmarble_arch",
        "blackmarble_column_1",
        "blackmarble_column_2"
    };

    public struct PieceMakerConfiguration
    {
        public string PrefabPrefix;
        public string NamePrefix;
        public List<PieceConfiguration> Pieces;
        public string PrefabReference;
        public string PieceTableCategory;
        public Material ReplacementMaterial;
        public Material ReplacementMaterialWorn;
        public Material ReplacementMaterialDestruction;
        public Material ReplacementMaterialSecondary;
    }

    public struct PieceConfiguration
    {
        public string Prefab;
        public int ResourceAmount;
        public Sprite Icon;
    }

    public static PieceTable GetPieceTable(GameObject item)
    {
        if (item == null)
        {
            return null;
        }

        ItemDrop itemDrop = item.GetComponent<ItemDrop>();
        if (itemDrop == null)
        {
            return null;
        }

        PieceTable pieceTable = itemDrop.m_itemData?.m_shared.m_buildPieces;

        return pieceTable;
    }

    /// <summary>
    /// Helper function to find build piece names
    /// </summary>
    public static List<string> FindVanillaPieces(PieceTable pieceTable, string resource, Piece.PieceCategory category)
    {
        List<string> pieces = new List<string>();

        foreach (GameObject pieceGO in pieceTable.m_pieces)
        {
            if (pieceGO.TryGetComponent<Piece>(out Piece piece) &&
                (category != Piece.PieceCategory.All && piece.m_category == category))
            {
                if (piece.m_resources.Length == 1 && piece.m_resources[0].m_resItem.name == resource)
                {
                    pieces.Add(Utils.GetPrefabName(pieceGO.name));
                }
            }
        }

        return pieces;
    }

    public List<PieceConfiguration> FindVanillaPieces(string[] prefabs, AssetBundle bundle, string iconPrefx)
    {
        List<PieceConfiguration> pieces = new List<PieceConfiguration>();

        foreach (string name in prefabs)
        {
            GameObject pieceGO = ZNetScene.instance.GetPrefab(name);
            if (pieceGO != null &&
                pieceGO.TryGetComponent<Piece>(out Piece piece))
            {
                // TODO: if more than one resource can be used modify this
                string iconName = $"{iconPrefx}{name}";
                Sprite icon = bundle.LoadAsset<Sprite>(iconName);
                if (icon == null)
                {
                    PieceMakerLogger.LogDebug($"Asset buundle did not contain icon: {iconName}");
                }

                pieces.Add(new PieceConfiguration
                {
                    Prefab = Utils.GetPrefabName(pieceGO.name),
                    ResourceAmount = piece.m_resources.Length > 0 ? piece.m_resources[0].m_amount : 0,
                    Icon = icon
                });
            }
        }

        return pieces;
    }

    public void AddCopies(PieceMakerConfiguration config, GameObject root)
    {
        if (config.Pieces == null || config.Pieces.Count == 0)
        {
            PieceMakerLogger.LogError("No pieces found! Can not add new options.");
            return;
        }

        GameObject tierPiecePrefab = ZNetScene.instance.GetPrefab(config.PrefabReference);
        Piece tierPiece = null;
        WearNTear tierWearNTear = null;

        if (tierPiecePrefab == null)
        {
            PieceMakerLogger.LogWarning(
                "Could not find stone piece to copy effects. Using defaults.");
        }
        else
        {
            tierPiece = tierPiecePrefab.GetComponent<Piece>();
            tierWearNTear = tierPiecePrefab.GetComponent<WearNTear>();
        }

        // Copy the reference material if no replacement exists
        if (config.ReplacementMaterial == null)
        {
            MeshRenderer mesh = tierPiecePrefab.GetComponentInChildren<MeshRenderer>();

            if (mesh != null)
            {
                config.ReplacementMaterial = mesh.material;
            }

            if (config.ReplacementMaterial == null)
            {
                PieceMakerLogger.LogError(
                    $"Could not find material to copy from prefab {config.PrefabReference}!");
            }

            PieceMakerLogger.LogDebug(
                $"Secondary material null: {config.ReplacementMaterialSecondary == null}!");
        }

        int count = 0;

        foreach (PieceConfiguration pieceConfig in config.Pieces)
        {
            GameObject original = ZNetScene.instance.GetPrefab(pieceConfig.Prefab);
            GameObject copy = GameObject.Instantiate(original, root.transform, false);
            copy.name = config.PrefabPrefix + Utils.GetPrefabName(copy);

            // Change piece
            Piece pieceComp = copy.GetComponent<Piece>();
            if (pieceComp != null)
            {
                Piece originalPiece = original.GetComponent<Piece>();
                pieceComp.m_name = $"{config.NamePrefix}{originalPiece.m_name}";
                pieceComp.m_icon = pieceConfig.Icon == null ? originalPiece.m_icon : pieceConfig.Icon;

                pieceComp.m_placeEffect = tierPiece.m_placeEffect;
            }
            else
            {
                PieceMakerLogger.LogDebug($"{copy.name} is not a piece, skipping.");
                break;
            }

            // Change wear and tear
            WearNTear wearComp = copy.GetComponent<WearNTear>();
            if (wearComp != null)
            {
                wearComp.m_health = tierWearNTear.m_health;
                wearComp.m_destroyedEffect = tierWearNTear.m_destroyedEffect;
                wearComp.m_hitEffect = tierWearNTear.m_hitEffect;
                wearComp.m_materialType = tierWearNTear.m_materialType;
                wearComp.m_damages = tierWearNTear.m_damages;
                wearComp.m_ashDamageImmune = tierWearNTear.m_ashDamageImmune;
                wearComp.m_ashDamageResist = tierWearNTear.m_ashDamageResist;
                wearComp.m_burnable = tierWearNTear.m_burnable;
            }

            // Change textures
            ReplaceMaterials(ref copy, new string[] { "new", "New" },
                config.ReplacementMaterial, config.ReplacementMaterialSecondary);
            ReplaceMaterials(ref copy, new string[] { "worn", "Worn" },
                config.ReplacementMaterialWorn, config.ReplacementMaterialSecondary);
            ReplaceMaterials(ref copy, new string[] { "broken", "Broken" },
                config.ReplacementMaterialWorn, config.ReplacementMaterialSecondary);
            ReplaceMaterials(ref copy, new string[] { "destruction", "Destruction" },
                config.ReplacementMaterialDestruction, config.ReplacementMaterialSecondary);

            // Add to game
            PieceConfig customPiece = new PieceConfig
            {
                Name = pieceComp.m_name,
                Description = pieceComp.m_description,
                PieceTable = PieceTables.Hammer,
                Category = config.PieceTableCategory,
                Requirements = new RequirementConfig[]
                {
                    new RequirementConfig
                    {
                        Item = "Stone",
                        Amount = pieceConfig.ResourceAmount,
                        Recover = true
                    }
                }
            };

            Jotunn.Managers.PieceManager.Instance.AddPiece(new CustomPiece(copy, true, customPiece));
            count++;
        }

        PieceMakerLogger.LogInfo($"Done adding {count} additional pieces.");
    }

    internal void ReplaceMaterials(ref GameObject piece, string[] hierarchyNames,
        Material material, Material secondaryMaterial)
    {
        bool found = false;
        foreach (string name in hierarchyNames)
        {
            Transform transform = piece.transform.FindDeepChild(name);

            if (transform == null)
            {
                continue;
            }

            MeshRenderer[] renders = transform.GetComponentsInChildren<MeshRenderer>(true);

            if (renders == null)
            {
                continue;
            }

            found = true;
            foreach (MeshRenderer render in renders)
            {
                if (secondaryMaterial != null && render.materials.Length > 1)
                {
                    Material[] newMats = new Material[]
                    {
                        material,
                        secondaryMaterial
                    };

                    render.SetMaterials(newMats.ToList());
                }
                else
                {
                    render.material = material;
                }
            }
        }

        if (!found)
        {
            PieceMakerLogger.LogDebug($"Could not replace materials for {piece.name}: {hierarchyNames[0]}");
        }
    }
}