using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace VentureValheim.SeaOfNoodles;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class SeaOfNoodlesPlugin : BaseUnityPlugin
{
    private const string ModName = "SeaOfNoodles";
    private const string ModVersion = "0.2.0";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource SeaOfNoodlesLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    private static bool setup = false;

    public void Awake()
    {
        SeaOfNoodlesLogger.LogInfo("I shall call him squishy and he shall be mine and he shall be my squishy!");

        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);
    }

    /// <summary>
    /// Find the Serpent spawn entries and increase spawn rates
    /// </summary>
    [HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.Awake))]
    public static class Patch_SpawnSystem_Awake
    {
        private static void Postfix(SpawnSystem __instance)
        {
            if (setup)
            {
                return;
            }

            foreach (var spawn in __instance.m_spawnLists)
            {
                foreach (var entry in spawn.m_spawners)
                {
                    if (entry.m_prefab != null && entry.m_prefab.name == "Serpent")
                    {
                        SeaOfNoodlesLogger.LogDebug($"Rolling a Noodle to perfection.");
                        entry.m_spawnInterval = 500f; // original 1000
                        entry.m_spawnChance = 10f; // original 5
                        entry.m_spawnDistance = 40f; // original 50
                        entry.m_spawnAtDay = true;
                    }
                }
            }

            setup = true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
    public static class Patch_Player_Awake
    {
        private static void Postfix()
        {
            if (!SceneManager.GetActiveScene().name.Equals("main"))
            {
                // A starting menu is open, reset init in case of multi-server/game session
                setup = false;
            }
        }
    }
}