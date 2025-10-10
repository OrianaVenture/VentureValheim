using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace VentureValheim.AdminTools;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class AdminToolsPlugin : BaseUnityPlugin
{
    private const string ModName = "AdminTools";
    private const string ModVersion = "0.1.0";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource AdminToolsLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    public void Awake()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);
    }
}