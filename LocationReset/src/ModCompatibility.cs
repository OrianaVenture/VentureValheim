namespace VentureValheim.LocationReset
{
    internal static class ModCompatibility
    {

        internal const string DungeonSplitterName = "dungeon_splitter";
        private static bool? _DungeonSplitterInstalled = null;

        /// <summary>
        /// Flag indicating if DungeonSplitter is installed.
        /// </summary>
        public static bool DungeonSplitterInstalled
        {
            get
            {
                _DungeonSplitterInstalled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(DungeonSplitterName);
                return _DungeonSplitterInstalled.Value;
            }
        }

        internal const string MVBPName = "Searica.Valheim.MoreVanillaBuildPrefabs";
        private static bool? _MVBPInstalled = null;

        /// <summary>
        /// Flag indicating if More Vanilla Build Prefabs is installed.
        /// </summary>
        public static bool MVBPInstalled
        {
            get
            {
                _MVBPInstalled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(MVBPName);
                return _MVBPInstalled.Value;
            }
        }
    }
}
