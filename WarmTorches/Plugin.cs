using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace VentureValheim.WarmTorches
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class WarmTorchesPlugin : BaseUnityPlugin
    {
        private const string ModName = "WarmTorches";
        private const string ModVersion = "0.1.2";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource WarmTorchesLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        public void Awake()
        {
            WarmTorchesLogger.LogInfo("Feeling Hot Hot Hot!");

            Assembly assembly = Assembly.GetExecutingAssembly();
            HarmonyInstance.PatchAll(assembly);
        }

        /// <summary>
        /// Only called when the Player checks environment. Check for torch here.
        /// </summary>
        [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.IsCold))]
        public static class Patch_EnvMan_IsCold
        {
            private static void Postfix(ref bool __result)
            {
                if (!__result)
                {
                    return;
                }

                var player = Player.m_localPlayer;
                if (player != null)
                {
                    if (player.m_rightItem != null && player.m_rightItem.m_shared != null &&
                        player.m_rightItem.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Torch)
                    {
                        __result = false;
                    }
                    else if (player.m_leftItem != null && player.m_leftItem.m_shared != null &&
                        player.m_leftItem.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Torch)
                    {
                        __result = false;
                    }
                }
            }
        }
    }
}