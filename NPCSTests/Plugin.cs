using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Managers;

namespace VentureValheim.NPCSTests;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class NPCSTestsPlugin : BaseUnityPlugin
{
    private const string ModName = "NPCSTests";
    private const string ModVersion = "0.1.0";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource NPCSTestsLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    public void Awake()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);
    }
}