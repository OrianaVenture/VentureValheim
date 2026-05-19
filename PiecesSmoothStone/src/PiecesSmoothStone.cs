using System.Collections.Generic;
using UnityEngine;

namespace VentureValheim.PiecesSmoothStone;

public class PiecesSmoothStone
{
    public static void Initialize()
    {
        GameObject hammer = ObjectDB.instance.GetItemPrefab("Hammer".GetStableHashCode());
        PieceTable pieceTable = PieceMaker.GetPieceTable(hammer);

        if (pieceTable == null)
        {
            PiecesSmoothStonePlugin.PiecesSmoothStoneLogger.LogError(
                "Could not add new pieces! Original piece table cannot be found.");
        }
        else
        {
            List<PieceMaker.PieceConfiguration> pieces = PieceMaker.FindVanillaPieces(pieceTable);
            PieceMaker.PieceMakerConfiguration config = new PieceMaker.PieceMakerConfiguration
            {
                Pieces = pieces,
                PieceTableCategory = "VentureSmoothStone",
                PrefabPrefix = "VV_SS_",
                NamePrefix = "VSS ",
                PrefabReference = "stone_floor_2x2",
                ReplacementMaterial = PiecesSmoothStonePlugin.StoneTexture
            };

            PieceMaker.AddCopies(config, PiecesSmoothStonePlugin.Root,
                PiecesSmoothStonePlugin.PiecesSmoothStoneLogger);
        }

        Jotunn.Managers.PrefabManager.OnPrefabsRegistered -= Initialize;
    }
}