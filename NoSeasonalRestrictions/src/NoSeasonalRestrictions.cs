using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VentureValheim.NoSeasonalRestrictions
{
    public class NoSeasonalRestrictions
    {
        private NoSeasonalRestrictions() {}
        private static readonly NoSeasonalRestrictions _instance = new NoSeasonalRestrictions();

        public static NoSeasonalRestrictions Instance
        {
            get => _instance;
        }

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
        public static class Patch_ObjectDB_Awake
        {
            private static void Postfix()
            {
                if (SceneManager.GetActiveScene().name.Equals("main"))
                {
                    EnableSeasonalItems();
                    NoSeasonalRestrictionsPlugin.NoSeasonalRestrictionsLogger.LogInfo("Done enabling seasonal items.");
                }
            }
        }

        private static void EnableSeasonalItems()
        {
            EnablePiece("piece_maypole");
            EnablePiece("piece_jackoturnip");
            EnablePiece("piece_gift1");
            EnablePiece("piece_gift2");
            EnablePiece("piece_gift3");
            EnablePiece("piece_mistletoe");
            EnablePiece("piece_xmascrown");
            EnablePiece("piece_xmasgarland");
            EnablePiece("piece_xmastree");

            EnableRecipe("Recipe_HelmetMidsummerCrown");
        }

        /// <summary>
        /// Enables the given Piece if found
        /// </summary>
        /// <param name="name">The Prefab's name</param>
        private static void EnablePiece(string name)
        {
            try
            {
                var obj = ZNetScene.m_instance.GetPrefab(name);
                obj.GetComponent<Piece>().m_enabled = true;
                return;
            }
            catch
            {
                NoSeasonalRestrictionsPlugin.NoSeasonalRestrictionsLogger.LogWarning($"Error, skipping configuring Piece: {name}");
            }

            NoSeasonalRestrictionsPlugin.NoSeasonalRestrictionsLogger.LogWarning($"Not found, skipping configuring Piece: {name}");
        }

        /// <summary>
        /// Enables the given Recipe if found
        /// </summary>
        /// <param name="name">The Item's shared name for identifying the recipe</param>
        private static void EnableRecipe(string name)
        {
            try
            {
                for (int lcv = 0; lcv < ObjectDB.instance.m_recipes.Count; lcv++)
                {
                    var recipe = ObjectDB.instance.m_recipes[lcv];
                    if (recipe != null && recipe.name.Equals(name))
                    {
                        ObjectDB.instance.m_recipes[lcv].m_enabled = true;
                        return;
                    }
                }
            }
            catch
            {
                NoSeasonalRestrictionsPlugin.NoSeasonalRestrictionsLogger.LogWarning($"Error, skipping configuring Recipe: {name}");
            }

            NoSeasonalRestrictionsPlugin.NoSeasonalRestrictionsLogger.LogWarning($"Not found, skipping configuring Recipe: {name}");
        }

        /// <summary>
        /// Helper method to identify disabled entities
        /// </summary>
        private static void ListDisabledItems()
        {
            foreach (GameObject obj in ZNetScene.m_instance.m_prefabs)
            {
                try
                {
                    var component = obj.GetComponent<Piece>();
                    if (component != null)
                    {
                        if (!component.m_enabled)
                        {
                            NoSeasonalRestrictionsPlugin.NoSeasonalRestrictionsLogger.LogDebug($"Found Disabled Piece: {obj.name}");
                        }
                    }
                }
                catch
                {
                    NoSeasonalRestrictionsPlugin.NoSeasonalRestrictionsLogger.LogDebug($"Error with ListDisabledItems: {obj.name}");
                }
            }

            foreach (Recipe obj in ObjectDB.instance.m_recipes)
            {
                try
                {
                    if (!obj.m_enabled)
                    {
                        NoSeasonalRestrictionsPlugin.NoSeasonalRestrictionsLogger.LogDebug($"Found Disabled Piece: {obj.name}");
                    }
                }
                catch
                {
                    NoSeasonalRestrictionsPlugin.NoSeasonalRestrictionsLogger.LogDebug($"Error with ListDisabledItems: {obj.name}");
                }
            }
        }
    }
}