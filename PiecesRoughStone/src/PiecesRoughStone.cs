using System.Collections.Generic;
using UnityEngine;

namespace VentureValheim.PiecesRoughStone;

public class PiecesRoughStone
{
    public static void Initialize()
    {
        GameObject hammer = ObjectDB.instance.GetItemPrefab("Hammer".GetStableHashCode());
        PieceTable pieceTable = PieceMaker.GetPieceTable(hammer);

        if (pieceTable == null)
        {
            PiecesRoughStonePlugin.PiecesRoughStoneLogger.LogError(
                "Could not add new pieces! Original piece table cannot be found.");
        }
        else
        {
            List<PieceMaker.PieceConfiguration> pieces = PieceMaker.FindVanillaPieces(pieceTable);
            PieceMaker.PieceMakerConfiguration config = new PieceMaker.PieceMakerConfiguration
            {
                Pieces = pieces,
                PieceTableCategory = "VentureRoughStone",
                PrefabPrefix = "VV_RS_",
                NamePrefix = "VRS ",
                PrefabReference = "stone_wall_4x2",
                ReplacementMaterial = PiecesRoughStonePlugin.StoneTexture
            };

            PieceMaker.AddCopies(config, PiecesRoughStonePlugin.Root,
                PiecesRoughStonePlugin.PiecesRoughStoneLogger);
        }

        Jotunn.Managers.PrefabManager.OnPrefabsRegistered -= Initialize;
    }
}