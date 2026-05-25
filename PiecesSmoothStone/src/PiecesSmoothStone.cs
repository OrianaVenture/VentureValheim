using System.Collections.Generic;
using System.Linq;
namespace VentureValheim.PiecesSmoothStone;

public class PiecesSmoothStone
{
    private const string CATEGORY = "Venture Stone";
    private const string PREFAB_PREFIX = "VV_SS_";
    private const string NAME_PREFIX = "VSS ";
    private const string ICON_PREFIX = "VV_SSicon_";

    public static void Initialize()
    {
        PieceMaker maker = new PieceMaker(PiecesSmoothStonePlugin.PiecesSmoothStoneLogger);

        string prefabReference = "stone_floor_2x2";

        List<PieceMaker.PieceConfiguration> pieces1 = maker.FindVanillaPieces(
            PieceMaker.RoughStonePieces.Union(PieceMaker.BlackMarblePieces).ToArray(), PiecesSmoothStonePlugin.StoneBundle, ICON_PREFIX);

        PieceMaker.PieceMakerConfiguration config1 = new PieceMaker.PieceMakerConfiguration
        {
            Pieces = pieces1,
            PieceTableCategory = CATEGORY,
            PrefabPrefix = PREFAB_PREFIX,
            NamePrefix = NAME_PREFIX,
            PrefabReference = prefabReference,
            ReplacementMaterial = PiecesSmoothStonePlugin.StoneTexture,
            ReplacementMaterialWorn = PiecesSmoothStonePlugin.StoneTextureWorn,
            ReplacementMaterialDestruction = PiecesSmoothStonePlugin.StoneTextureDestruction,
            ReplacementMaterialSecondary = null
        };

        List<PieceMaker.PieceConfiguration> pieces2 = maker.FindVanillaPieces(
            PieceMaker.GraustenPieces, PiecesSmoothStonePlugin.StoneBundle, ICON_PREFIX);

        PieceMaker.PieceMakerConfiguration config2 = new PieceMaker.PieceMakerConfiguration
        {
            Pieces = pieces2,
            PieceTableCategory = CATEGORY,
            PrefabPrefix = PREFAB_PREFIX,
            NamePrefix = NAME_PREFIX,
            PrefabReference = prefabReference,
            ReplacementMaterial = PiecesSmoothStonePlugin.StoneTextureNoNoise,
            ReplacementMaterialWorn = PiecesSmoothStonePlugin.StoneTextureNoNoise,
            ReplacementMaterialDestruction = PiecesSmoothStonePlugin.StoneTextureDestruction,
            ReplacementMaterialSecondary = null
        };

        List<PieceMaker.PieceConfiguration> pieces3 = maker.FindVanillaPieces(
            PieceMaker.GraustenRoofPieces, PiecesSmoothStonePlugin.StoneBundle, ICON_PREFIX);

        PieceMaker.PieceMakerConfiguration config3 = new PieceMaker.PieceMakerConfiguration
        {
            Pieces = pieces3,
            PieceTableCategory = CATEGORY,
            PrefabPrefix = PREFAB_PREFIX,
            NamePrefix = NAME_PREFIX,
            PrefabReference = prefabReference,
            ReplacementMaterial = PiecesSmoothStonePlugin.StoneTextureNoNoise,
            ReplacementMaterialWorn = PiecesSmoothStonePlugin.StoneTextureNoNoise,
            ReplacementMaterialDestruction = PiecesSmoothStonePlugin.StoneTextureSecondary,
            ReplacementMaterialSecondary = PiecesSmoothStonePlugin.StoneTextureSecondary
        };

        maker.AddCopies(config1, PiecesSmoothStonePlugin.Root);
        maker.AddCopies(config2, PiecesSmoothStonePlugin.Root);
        maker.AddCopies(config3, PiecesSmoothStonePlugin.Root);

        Jotunn.Managers.PrefabManager.OnPrefabsRegistered -= Initialize;
    }
}