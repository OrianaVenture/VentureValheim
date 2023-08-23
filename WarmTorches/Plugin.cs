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
        private const string ModVersion = "0.1.0";
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
                if (__result)
                {
                    var player = Player.m_localPlayer;
                    if (player != null)
                    {
                        var rightItem = player.m_rightItem?.m_shared?.m_itemType ?? null;
                        var leftItem = player.m_leftItem?.m_shared?.m_itemType ?? null;

                        if ((rightItem != null && rightItem == ItemDrop.ItemData.ItemType.Torch) ||
                            (leftItem != null && leftItem == ItemDrop.ItemData.ItemType.Torch))
                        {
                            __result = false;
                        }
                    }
                }
            }
        }
    }
}