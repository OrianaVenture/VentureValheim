using System.Collections.Generic;

namespace VentureValheim.PiecesRoughStone;

public class PiecesRoughStone
{
    private const string CATEGORY = "Venture Stone";
    private const string PREFAB_PREFIX = "VV_RS_";
    private const string NAME_PREFIX = "VRS ";
    private const string ICON_PREFIX = "VV_RSicon_";

    public static void Initialize()
    {
        PieceMaker maker = new PieceMaker(PiecesRoughStonePlugin.PiecesRoughStoneLogger);

        string prefabReference = "stone_wall_4x2";

        List<PieceMaker.PieceConfiguration> pieces1 = maker.FindVanillaPieces(
            PieceMaker.BlackMarblePieces, PiecesRoughStonePlugin.StoneBundle, ICON_PREFIX);

        PieceMaker.PieceMakerConfiguration config1 = new PieceMaker.PieceMakerConfiguration
        {
            Pieces = pieces1,
            PieceTableCategory = CATEGORY,
            PrefabPrefix = PREFAB_PREFIX,
            NamePrefix = NAME_PREFIX,
            PrefabReference = prefabReference,
            ReplacementMaterial = PiecesRoughStonePlugin.StoneTexture,
            ReplacementMaterialWorn = PiecesRoughStonePlugin.StoneTextureWorn,
            ReplacementMaterialDestruction = PiecesRoughStonePlugin.StoneTextureDestruction,
            ReplacementMaterialSecondary = null
        };

        List<PieceMaker.PieceConfiguration> pieces2 = maker.FindVanillaPieces(
            PieceMaker.GraustenPieces, PiecesRoughStonePlugin.StoneBundle, ICON_PREFIX);

        PieceMaker.PieceMakerConfiguration config2 = new PieceMaker.PieceMakerConfiguration
        {
            Pieces = pieces2,
            PieceTableCategory = CATEGORY,
            PrefabPrefix = PREFAB_PREFIX,
            NamePrefix = NAME_PREFIX,
            PrefabReference = prefabReference,
            ReplacementMaterial = PiecesRoughStonePlugin.StoneTextureNoNoise,
            ReplacementMaterialWorn = PiecesRoughStonePlugin.StoneTextureNoNoise,
            ReplacementMaterialDestruction = PiecesRoughStonePlugin.StoneTextureDestruction,
            ReplacementMaterialSecondary = null
        };

        List<PieceMaker.PieceConfiguration> pieces3 = maker.FindVanillaPieces(
            PieceMaker.GraustenRoofPieces, PiecesRoughStonePlugin.StoneBundle, ICON_PREFIX);

        PieceMaker.PieceMakerConfiguration config3 = new PieceMaker.PieceMakerConfiguration
        {
            Pieces = pieces3,
            PieceTableCategory = CATEGORY,
            PrefabPrefix = PREFAB_PREFIX,
            NamePrefix = NAME_PREFIX,
            PrefabReference = prefabReference,
            ReplacementMaterial = PiecesRoughStonePlugin.StoneTextureNoNoise,
            ReplacementMaterialWorn = PiecesRoughStonePlugin.StoneTextureNoNoise,
            ReplacementMaterialDestruction = PiecesRoughStonePlugin.StoneTextureSecondary,
            ReplacementMaterialSecondary = PiecesRoughStonePlugin.StoneTextureSecondary
        };

        maker.AddCopies(config1, PiecesRoughStonePlugin.Root);
        maker.AddCopies(config2, PiecesRoughStonePlugin.Root);
        maker.AddCopies(config3, PiecesRoughStonePlugin.Root);

        Jotunn.Managers.PrefabManager.OnPrefabsRegistered -= Initialize;
    }
}