using System;
using HarmonyLib;
using UnityEngine;

namespace VentureValheim.Template
{
    public class Template
    {
        private Template()
        {
        }
        private static readonly Template _instance = new Template();

        public static Template Instance
        {
            get => _instance;
        }

        public void Initialize()
        {
        }
    }
}