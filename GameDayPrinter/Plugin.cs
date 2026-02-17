using System.Collections;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace VentureValheim.GameDayPrinter;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class GameDayPrinterPlugin : BaseUnityPlugin
{
    private const string ModName = "GameDayPrinter";
    private const string ModVersion = "0.1.0";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource GameDayPrinterLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    public void Awake()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Start))]
    public static class Patch_ZNet_Start
    {
        private static void Postfix(ZNet __instance)
        {
            __instance.StartCoroutine(PrintDayDelayed());
        }
    }

    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.OnMorning))]
    public static class Patch_EnvMan_OnMorning
    {
        private static void Postfix()
        {
            PrintDay();
        }
    }

    private static IEnumerator PrintDayDelayed()
    {
        yield return new WaitForSeconds(3);
        PrintDay();
    }

    public static void PrintDay()
    {
        if (EnvMan.instance != null)
        {
            GameDayPrinterLogger.LogInfo($"Day {EnvMan.instance.GetCurrentDay()}");
        }
    }
}