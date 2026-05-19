using BepInEx.Logging;
using Jotunn.Configs;
using Jotunn.Entities;
using System.Collections.Generic;
using UnityEngine;

namespace VentureValheim;

public class PieceMaker
{
    public struct PieceMakerConfiguration
    {
        public string PrefabPrefix;
        public string NamePrefix;
        public List<PieceConfiguration> Pieces;
        public string PrefabReference;
        public string PieceTableCategory;
        public Material ReplacementMaterial;
    }

    public struct PieceConfiguration
    {
        public string Prefab;
        public int ResourceAmount;
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

    public static List<PieceConfiguration> FindVanillaPieces(PieceTable pieceTable)
    {
        List<PieceConfiguration> pieces = new List<PieceConfiguration>();

        foreach (GameObject pieceGO in pieceTable.m_pieces)
        {
            if (pieceGO.TryGetComponent<Piece>(out Piece piece) && piece.m_category == Piece.PieceCategory.BuildingStonecutter)
            {
                if (piece.m_resources.Length == 1 && piece.m_resources[0].m_resItem.name == "BlackMarble")
                {
                    // Found a black marble piece
                    pieces.Add(new PieceConfiguration
                    {
                        Prefab = Utils.GetPrefabName(pieceGO.name),
                        ResourceAmount = piece.m_resources[0].m_amount
                    });
                }
            }
        }

        return pieces;
    }

    public static void AddCopies(PieceMakerConfiguration config, GameObject root, ManualLogSource logger)
    {
        if (config.Pieces == null || config.Pieces.Count == 0)
        {
            logger.LogError("No pieces found! Can not add new options.");
            return;
        }

        GameObject stonePiecePrefab = ZNetScene.instance.GetPrefab(config.PrefabReference);
        Piece stonePiece = null;
        WearNTear stoneWearNTear = null;

        if (stonePiecePrefab == null)
        {
            logger.LogWarning(
                "Could not find stone piece to copy effects. Using defaults.");
        }
        else
        {
            stonePiece = stonePiecePrefab.GetComponent<Piece>();
            stoneWearNTear = stonePiecePrefab.GetComponent<WearNTear>();
        }

        // Copy the reference material if no replacement exists
        if (config.ReplacementMaterial == null)
        {
            MeshRenderer mesh = stonePiecePrefab.GetComponentInChildren<MeshRenderer>();

            if (mesh != null)
            {
                config.ReplacementMaterial = mesh.material;
            }

            if (config.ReplacementMaterial == null)
            {
                logger.LogError(
                    $"Could not find material to copy from prefab {config.PrefabReference}!");
            }
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
                pieceComp.m_icon = originalPiece.m_icon;

                pieceComp.m_placeEffect = stonePiece.m_placeEffect;
            }
            else
            {
                logger.LogDebug($"{copy.name} is not a piece, skipping.");
                break;
            }

            // Change wear and tear
            WearNTear wearComp = copy.GetComponent<WearNTear>();
            if (wearComp != null)
            {
                wearComp.m_health = stoneWearNTear.m_health;
                wearComp.m_destroyedEffect = stoneWearNTear.m_destroyedEffect;
                wearComp.m_hitEffect = stoneWearNTear.m_hitEffect;
                wearComp.m_materialType = stoneWearNTear.m_materialType;
                wearComp.m_damages = stoneWearNTear.m_damages;
            }

            // Change textures
            MeshRenderer[] renders = copy.GetComponentsInChildren<MeshRenderer>(true);

            if (renders != null)
            {
                foreach (MeshRenderer render in renders)
                {
                    render.material = config.ReplacementMaterial;
                }
            }

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

        logger.LogInfo($"Done adding {count} additional pieces.");
    }
}