using System;
using HarmonyLib;
using UnityEngine;

namespace VentureValheim.NPCSTests
{
    public class NPCSTests
    {
        private NPCSTests()
        {
        }
        private static readonly NPCSTests _instance = new NPCSTests();

        public static NPCSTests Instance
        {
            get => _instance;
        }

        public void Initialize()
        {
        }

        // TODO
        // Load different zdo configurations from file like in original mod?
        // Add commands to spawn the saved npcs.
        // Perhaps a master command to spawn test cases with npcs randomly spread out in an area
        // Add command to test upgrade data? The upgrade should happen on awake, might do this is harmony patches
    }
}