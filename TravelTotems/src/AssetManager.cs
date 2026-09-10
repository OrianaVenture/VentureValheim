using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VentureValheim.TravelTotems;

public class AssetManager
{
    public static string TravelTotemTrackerPrefabName = "VV_TravelTotemTracker";
    public static int TravelTotemTrackerHash = TravelTotemTrackerPrefabName.GetStableHashCode();

    private static string TravelTotem_PrefabName = "VV_TravelTotem";
    private static string TravelTotemMistlands_PrefabName = "VV_TravelTotemMistlands";
    private static string TravelTotemAshlands_PrefabName = "VV_TravelTotemAshlands";
    private static int TravelTotemHash = TravelTotem_PrefabName.GetStableHashCode();
    private static int TravelTotemMistlandsHash = TravelTotemMistlands_PrefabName.GetStableHashCode();
    private static int TravelTotemAshlandsHash = TravelTotemAshlands_PrefabName.GetStableHashCode();

    // Pieces
    private static string TravelTotemPiece_PrefabName = "VV_TravelTotemPiece";
    private static string TravelTotemPieceMistlands_PrefabName = "VV_TravelTotemPieceMistlands";
    private static string TravelTotemPieceAshlands_PrefabName = "VV_TravelTotemPieceAshlands";
    private static int TravelTotemPieceHash = TravelTotemPiece_PrefabName.GetStableHashCode();
    private static int TravelTotemPieceMistlandsHash = TravelTotemPieceMistlands_PrefabName.GetStableHashCode();
    private static int TravelTotemPieceAshlandsHash = TravelTotemPieceAshlands_PrefabName.GetStableHashCode();

    public static string TravelTotemPiece_Cost = "GreydwarfEye:10,Stone:20,SurtlingCore:5";
    public static string TravelTotemPieceMistlands_Cost = "GreydwarfEye:10,BlackMarble:20,SurtlingCore:5";
    public static string TravelTotemPieceAshlands_Cost = "GreydwarfEye:10,Grausten:20,SurtlingCore:5";

    // Items
    private static string TravelTotemArea_PrefabName = "VV_TravelTotemArea";
    public static string TravelTotemBomb_PrefabName = "VV_BombTotem";
    private static string TravelTotemBombSpawn_PrefabName = "VV_BombTotem_projectile";
    public static string TotemBombCraft_Cost = "GreydwarfEye:10,Resin:10,SurtlingCore:2";

    public static Material TotemMaterialDefault { get; private set; }
    public static Material TotemMaterialMistlands { get; private set; }
    public static Material TotemMaterialAshlands { get; private set; }

    private static string TotemMapSprite_Name = "VV_TravelTotemMapSprite";
    public static Sprite TotemMapSprite { get; private set; }

    public static bool IsTravelTotem(int prefabHash)
    {
        return prefabHash == TravelTotemHash ||
            prefabHash == TravelTotemMistlandsHash ||
            prefabHash == TravelTotemAshlandsHash ||
            prefabHash == TravelTotemPieceHash ||
            prefabHash == TravelTotemPieceMistlandsHash ||
            prefabHash == TravelTotemPieceAshlandsHash;
    }

    public static bool IsTravelTotemTracker(int prefabHash)
    {
        return prefabHash == TravelTotemTrackerHash;
    }

    public static void AddPrefabs()
    {
        CreateMaterials();

        AddPrefab(TravelTotem_PrefabName);
        AddPrefab(TravelTotemMistlands_PrefabName);
        AddPrefab(TravelTotemAshlands_PrefabName);
        AddPrefab(TravelTotemTrackerPrefabName);

        CreatePieces();
        CreateItems();

        TotemMapSprite = TravelTotemsPlugin.TotemBundle.LoadAsset<Sprite>(TotemMapSprite_Name);

        PrefabManager.OnVanillaPrefabsAvailable -= AddPrefabs;
    }

    private static void AddPrefab(string name)
    {
        CustomPrefab prefab = new CustomPrefab(TravelTotemsPlugin.TotemBundle, name, true);
        PrefabManager.Instance.AddPrefab(prefab);
    }

    private static void CreateItems()
    {
        AddPrefab(TravelTotemArea_PrefabName);
        AddPrefab(TravelTotemBombSpawn_PrefabName);

        GameObject bombGO = TravelTotemsPlugin.TotemBundle.LoadAsset<GameObject>(TravelTotemBomb_PrefabName);
        ItemConfig itemConfig = new ItemConfig
        {
            Name = "Totem Bomb",
            Description = "An unstable burst energy that can temporarily access a teleportation network.",
            CraftingStation = CraftingStations.Workbench,
            Requirements = new RequirementConfig[]
            {
                new RequirementConfig
                {
                    Item = "GreydwarfEye",
                    Amount = 10,
                    Recover = true
                },
                new RequirementConfig
                {
                    Item = "Resin",
                    Amount = 10,
                    Recover = true
                },
                new RequirementConfig
                {
                    Item = "SurtlingCore",
                    Amount = 2,
                    Recover = true
                }
            }
        };
        CustomItem bomb = new CustomItem(bombGO, true, itemConfig);
        ItemManager.Instance.AddItem(bomb);
    }

    private static void CreatePieces()
    {
        GameObject totemGO = TravelTotemsPlugin.TotemBundle.LoadAsset<GameObject>(TravelTotemPiece_PrefabName);
        PieceConfig totemPC = new PieceConfig
        {
            PieceTable = PieceTables.Hammer,
            Category = PieceCategories.Misc,
            Name = "Travel Totem",
            Description = "A mystical relic that can form a teleportation network. CANNOT BE REMOVED ONCE PLACED!",
            CraftingStation = CraftingStations.Stonecutter,
            Requirements = new RequirementConfig[]
            {
                new RequirementConfig
                {
                    Item = "GreydwarfEye",
                    Amount = 10,
                    Recover = true
                },
                new RequirementConfig
                {
                    Item = "Stone",
                    Amount = 20,
                    Recover = true
                },
                new RequirementConfig
                {
                    Item = "SurtlingCore",
                    Amount = 5,
                    Recover = true
                }
            }
        };
        CustomPiece totem = new CustomPiece(totemGO, true, totemPC);
        PieceManager.Instance.AddPiece(totem);

        GameObject totemMistGO = TravelTotemsPlugin.TotemBundle.LoadAsset<GameObject>(TravelTotemPieceMistlands_PrefabName);
        PieceConfig totemMistPC = new PieceConfig
        {
            PieceTable = PieceTables.Hammer,
            Category = PieceCategories.Misc,
            Name = "Travel Totem BlackMarble",
            Description = "A mystical relic that can form a teleportation network. CANNOT BE REMOVED ONCE PLACED!",
            CraftingStation = CraftingStations.Stonecutter,
            Requirements = new RequirementConfig[]
            {
                new RequirementConfig
                {
                    Item = "GreydwarfEye",
                    Amount = 10,
                    Recover = true
                },
                new RequirementConfig
                {
                    Item = "BlackMarble",
                    Amount = 20,
                    Recover = true
                },
                new RequirementConfig
                {
                    Item = "SurtlingCore",
                    Amount = 5,
                    Recover = true
                }
            }
        };
        CustomPiece totemMist = new CustomPiece(totemMistGO, true, totemMistPC);
        PieceManager.Instance.AddPiece(totemMist);

        GameObject totemAshGO = TravelTotemsPlugin.TotemBundle.LoadAsset<GameObject>(TravelTotemPieceAshlands_PrefabName);
        PieceConfig totemAshPC = new PieceConfig
        {
            PieceTable = PieceTables.Hammer,
            Category = PieceCategories.Misc,
            Name = "Travel Totem Grausten",
            Description = "A mystical relic that can form a teleportation network. CANNOT BE REMOVED ONCE PLACED!",
            CraftingStation = CraftingStations.Stonecutter,
            Requirements = new RequirementConfig[]
            {
                new RequirementConfig
                {
                    Item = "GreydwarfEye",
                    Amount = 10,
                    Recover = true
                },
                new RequirementConfig
                {
                    Item = "Grausten",
                    Amount = 20,
                    Recover = true
                },
                new RequirementConfig
                {
                    Item = "SurtlingCore",
                    Amount = 5,
                    Recover = true
                }
            }
        };
        CustomPiece totemAsh = new CustomPiece(totemAshGO, true, totemAshPC);
        PieceManager.Instance.AddPiece(totemAsh);
    }

    public static void UpdateConfigurations()
    {
        UpdatePieceConfigurations();
        UpdateItemConfigurations();
    }

    public static void UpdateItemConfigurations()
    {
        if (TravelTotemsPlugin.GetTotemItemRecipesEnabled())
        {
            UpdateItemConfiguration(TravelTotemBomb_PrefabName,
                TravelTotemsPlugin.GetTotemBombItemCost());
        }
    }

    public static void UpdatePieceConfigurations()
    {
        if (TravelTotemsPlugin.GetTravelTotemPieceRecipesEnabled())
        {
            UpdatePieceConfiguration(TravelTotemPiece_PrefabName,
                TravelTotemsPlugin.GetTravelTotemPieceCost());
            UpdatePieceConfiguration(TravelTotemPieceMistlands_PrefabName,
                TravelTotemsPlugin.GetTravelTotemPieceMistlandsCost());
            UpdatePieceConfiguration(TravelTotemPieceAshlands_PrefabName,
                TravelTotemsPlugin.GetTravelTotemPieceAshlandsCost());
        }
    }

    public static void UpdatePieceConfiguration(string name, string cost)
    {
        bool enabled = !cost.IsNullOrWhiteSpace();
        Piece.Requirement[] reqs = MakeRequirementFromConfig(name, cost).ToArray();

        GameObject piece = PrefabManager.Instance.GetPrefab(name);

        if (piece == null)
        {
            TravelTotemsPlugin.TravelTotemsLogger.LogError($"Could not find prefab for piece {name}.");
            return;
        }

        Piece pieceComp = piece.GetComponent<Piece>();

        if (pieceComp == null)
        {
            TravelTotemsPlugin.TravelTotemsLogger.LogError($"Could not find Piece for piece {name}.");
            return;
        }

        pieceComp.m_resources = reqs;

        // Add to Hammer
        PieceTable pieceTable = null;
        GameObject hammerPrefab = PrefabManager.Instance.GetPrefab("Hammer");

        if (hammerPrefab == null || !hammerPrefab.TryGetComponent<ItemDrop>(out ItemDrop itemdrop))
        {
            TravelTotemsPlugin.TravelTotemsLogger.LogWarning($"Hammer not found, will not add build piece.");
            return;
        }

        pieceTable = itemdrop.m_itemData.m_shared.m_buildPieces;
        GameObject pieceTablePiece = pieceTable.m_pieces.Find(x => x.name == Utils.GetPrefabName(pieceComp.name));

        if (!enabled && pieceTablePiece != null)
        {
            // Remove existing
            pieceTable.m_pieces.Remove(pieceTablePiece);
        }
        else if (enabled && pieceTablePiece == null)
        {
            // Add new
            pieceTable.m_pieces.Add(piece);
        }

        TravelTotemsPlugin.TravelTotemsLogger.LogDebug($"Updated piece {name}.");
    }

    public static void UpdateItemConfiguration(string name, string recipe)
    {
        bool enabled = !recipe.IsNullOrWhiteSpace();
        // Update Recipe
        Piece.Requirement[] reqs = MakeRequirementFromConfig(name, recipe).ToArray();
        Recipe gameRecipe = ObjectDB.instance.m_recipes.FirstOrDefault(x =>
            x.m_item != null &&
            x.m_item.m_itemData.m_dropPrefab != null &&
            x.m_item.m_itemData.m_dropPrefab.name == name);

        if (gameRecipe == null)
        {
            TravelTotemsPlugin.TravelTotemsLogger.LogError($"Could not find recipe for item {name}.");
        }
        else
        {
            gameRecipe.m_enabled = enabled;
            gameRecipe.m_resources = reqs;
        }
    }

    public static List<Piece.Requirement> MakeRequirementFromConfig(string itemName, string configString)
    {
        List<RequirementConfig> config = MakeRecipeFromConfig(configString);
        List<Piece.Requirement> reqs = new List<Piece.Requirement>();

        foreach (RequirementConfig req in config)
        {
            Piece.Requirement newReq = req.GetRequirement();
            GameObject resource = PrefabManager.Instance.GetPrefab(req.Item);

            if (resource == null)
            {
                TravelTotemsPlugin.TravelTotemsLogger.LogError(
                    $"Could not add requirement {req.Item}, for {itemName}. Prefab not found.");
                continue;
            }

            ItemDrop resourceItemDrop = resource.GetComponent<ItemDrop>();
            if (resourceItemDrop != null)
            {
                newReq.m_resItem = resourceItemDrop;
                reqs.Add(newReq);
            }
            else
            {
                TravelTotemsPlugin.TravelTotemsLogger.LogError(
                    $"Could not add requirement {req.Item}, for {itemName}.");
            }
        }

        return reqs;
    }

    private static List<RequirementConfig> MakeRecipeFromConfig(string configString)
    {
        string[] entries = configString.Replace(" ", "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        List<RequirementConfig> config = new List<RequirementConfig>();
        foreach (string entry in entries)
        {
            string[] parts = entry.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                PrintRecipeErrorMessage(configString);
                continue;
            }

            string item = parts[0];
            string amountString = parts[1];
            if (!int.TryParse(amountString, out int amount))
            {
                PrintRecipeErrorMessage(configString);
                continue;
            }

            config.Add(new RequirementConfig
            {
                Item = item,
                Amount = amount,
                Recover = true
            });
        }

        return config;
    }

    public static void PrintRecipeErrorMessage(string entry)
    {
        TravelTotemsPlugin.TravelTotemsLogger.LogError($"Incorrectly formatted recipe: {entry}! " +
            $"Should be 'ITEM:QUANITY,ITEM2:QUANTITY' etc.");
    }

    public static void AddTotemLocations()
    {
        CreateMaterials();
        int spacing = TravelTotemsPlugin.GetLocationSpacing();
        AddTotemLocation("VV_TravelTotemLocation_Default", Heightmap.Biome.All,
            TravelTotemsPlugin.GetLocationAmountDefault(), spacing);
        AddTotemLocation("VV_TravelTotemLocation_Meadows", Heightmap.Biome.Meadows,
            TravelTotemsPlugin.GetLocationAmountMeadows(), spacing);
        AddTotemLocation("VV_TravelTotemLocation_BlackForest", Heightmap.Biome.BlackForest,
            TravelTotemsPlugin.GetLocationAmountBlackForest(), spacing);
        AddTotemLocation("VV_TravelTotemLocation_Swamp", Heightmap.Biome.Swamp,
            TravelTotemsPlugin.GetLocationAmountSwamp(), spacing);
        AddTotemLocation("VV_TravelTotemLocation_Mountain", Heightmap.Biome.Mountain,
            TravelTotemsPlugin.GetLocationAmountMountain(), spacing);
        AddTotemLocation("VV_TravelTotemLocation_Plains", Heightmap.Biome.Plains,
            TravelTotemsPlugin.GetLocationAmountPlains(), spacing);
        AddTotemLocation("VV_TravelTotemLocation_Mistlands", Heightmap.Biome.Mistlands,
            TravelTotemsPlugin.GetLocationAmountMistlands(), spacing, 15);
        AddTotemLocation("VV_TravelTotemLocation_Ashlands", Heightmap.Biome.AshLands,
            TravelTotemsPlugin.GetLocationAmountAshlands(), spacing, 15, 5);

        ZoneManager.OnVanillaLocationsAvailable -= AddTotemLocations;
    }

    private static void AddTotemLocation(string locationName, Heightmap.Biome biome, int quantity, int distance, int radius = 7, int minAltitude = 3)
    {
        GameObject location = ZoneManager.Instance.CreateLocationContainer(TravelTotemsPlugin.TotemBundle, locationName);
        LocationConfig locationConfig = new LocationConfig();
        locationConfig.Biome = biome;
        locationConfig.Quantity = quantity;
        locationConfig.ExteriorRadius = radius;
        locationConfig.HasInterior = false;
        locationConfig.InteriorRadius = 5;
        locationConfig.MinAltitude = minAltitude;
        locationConfig.MaxAltitude = 1000;
        locationConfig.Priotized = true;
        locationConfig.MinDistanceFromSimilar = distance;
        locationConfig.ClearArea = true;

        CustomLocation customLoc = new CustomLocation(location, true, locationConfig);
        ZoneManager.Instance.AddCustomLocation(customLoc);
    }

    public static void CreateMaterials()
    {
        if (TotemMaterialDefault != null)
        {
            return;
        }

        Material originalPlatform = PrefabManager.Cache.GetPrefab<Material>("startplatform");

        if (originalPlatform == null)
        {
            TravelTotemsPlugin.TravelTotemsLogger.LogWarning("Material \"startplatform\" could not be found. Could not replace.");
            return;
        }

        Texture emission = TravelTotemsPlugin.TotemBundle.LoadAsset<Texture>("VV_startstone_emission");

        if (emission == null)
        {
            TravelTotemsPlugin.TravelTotemsLogger.LogWarning("Could not load emission texture data.");
        }

        Vector4 emissionColor = new Vector4(0, 0, 0, 1f);
        Material originalMistlands = PrefabManager.Cache.GetPrefab<Material>("rock_mistlands");
        Material originalAshlands = PrefabManager.Cache.GetPrefab<Material>("AshlandsRockTest_mat");

        CreateDefaultMaterial(originalPlatform, emission, emissionColor);

        if (originalMistlands == null)
        {
            TotemMaterialMistlands = new Material(TotemMaterialDefault);
            TotemMaterialMistlands.name = "VV_TotemMaterialMistlands";
            TravelTotemsPlugin.TravelTotemsLogger.LogWarning("Material \"rock_mistlands\" could not be found. Could not replace.");
        }
        else
        {
            CreateMistlandsMaterial(originalPlatform, originalMistlands, emission, emissionColor);
        }

        if (originalAshlands == null)
        {
            TotemMaterialAshlands = new Material(TotemMaterialDefault);
            TotemMaterialAshlands.name = "VV_TotemMaterialAshlands";
            TravelTotemsPlugin.TravelTotemsLogger.LogWarning("Material \"AshlandsRockTest_mat\" could not be found. Could not replace.");
        }
        else
        {
            CreateAshlandsMaterial(originalPlatform, originalAshlands, emission, emissionColor);
        }

        Jotunn.Managers.AssetManager.Instance.AddAsset(TotemMaterialDefault);
        Jotunn.Managers.AssetManager.Instance.AddAsset(TotemMaterialMistlands);
        Jotunn.Managers.AssetManager.Instance.AddAsset(TotemMaterialAshlands);
    }

    private static void CreateDefaultMaterial(
        Material originalPlatform, Texture emission, Vector3 emissionColor)
    {
        TotemMaterialDefault = new Material(originalPlatform);
        TotemMaterialDefault.name = "VV_TotemMaterialDefault";
        TotemMaterialDefault.SetTexture("_EmissiveTex", emission);
        TotemMaterialDefault.SetVector("_EmissionColor", emissionColor);
        TotemMaterialDefault.SetFloat("_MossBlend", 5f);
    }

    private static void CreateMistlandsMaterial(
        Material originalPlatform, Material originalMaterial, Texture emission, Vector3 emissionColor)
    {
        TotemMaterialMistlands = new Material(originalMaterial);
        TotemMaterialMistlands.name = "VV_TotemMaterialMistlands";
        CopyBasics(originalPlatform, TotemMaterialMistlands, emission, emissionColor);
        CopyMoss(originalPlatform, TotemMaterialMistlands);
        TotemMaterialMistlands.SetVector("_Color", new Vector4(0.254717f, 0.254717f, 0.254717f, 1f));
    }

    private static void CreateAshlandsMaterial(
        Material originalPlatform, Material originalMaterial, Texture emission, Vector3 emissionColor)
    {
        TotemMaterialAshlands = new Material(originalMaterial);
        TotemMaterialAshlands.name = "VV_TotemMaterialAshlands";
        CopyBasics(originalPlatform, TotemMaterialAshlands, emission, emissionColor);
        TotemMaterialAshlands.SetVector("_Color", new Vector4(0.02f, 0.04f, 0.06f, 1f));
    }

    private static void CopyBasics(Material original, Material newMaterial, Texture emission, Vector4 emissionColor)
    {
        newMaterial.SetTexture("_MainTex", original.GetTexture("_MainTex"));
        newMaterial.SetTextureScale("_MainTex", original.GetTextureScale("_MainTex"));
        newMaterial.SetTextureOffset("_MainTex", original.GetTextureOffset("_MainTex"));

        newMaterial.SetTexture("_BumpMap", original.GetTexture("_BumpMap"));
        newMaterial.SetTextureScale("_BumpMap", original.GetTextureScale("_BumpMap"));
        newMaterial.SetTextureOffset("_BumpMap", original.GetTextureOffset("_BumpMap"));
        newMaterial.SetFloat("_BumpScale", original.GetFloat("_BumpScale"));

        newMaterial.SetTexture("_EmissiveTex", emission);
        newMaterial.SetVector("_EmissionColor", emissionColor);
    }

    private static void CopyMoss(Material original, Material material)
    {
        material.SetTextureScale("_MossTex", original.GetTextureScale("_MossTex"));
        material.SetTextureOffset("_MossTex", original.GetTextureOffset("_MossTex"));
        material.SetFloat("_MossBlend", 5f);
        material.SetFloat("_MossNormal", original.GetFloat("_MossNormal"));
        material.SetFloat("_MossTransition", original.GetFloat("_MossTransition"));
    }
}
