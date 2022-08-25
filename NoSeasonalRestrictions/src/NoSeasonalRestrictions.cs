using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VentureValheim.NoSeasonalRestrictions
{
    public class NoSeasonalRestrictions
    {
        private NoSeasonalRestrictions()
        {
        }
        private static readonly NoSeasonalRestrictions _instance = new NoSeasonalRestrictions();

        public static NoSeasonalRestrictions Instance
        {
            get => _instance;
        }

        public void Initialize()
        {
        }

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
        public static class Patch_ObjectDB_Awake
        {
            private static void Postfix()
            {
                NoSeasonalRestrictionsPlugin.NoSeasonalRestrictionsLogger.LogDebug("NoSeasonalRestrictions.Patch_ObjectDB_Awake called.");

                if (SceneManager.GetActiveScene().name.Equals("main"))
                {
                    EnableSeasonalItems();
                    NoSeasonalRestrictionsPlugin.NoSeasonalRestrictionsLogger.LogDebug("Done enabling seasonal items.");
                }
            }
        }

        private static void EnableSeasonalItems()
        {
            EnablePiece("piece_xmastree");
            EnablePiece("piece_maypole");
            EnablePiece("piece_jackoturnip");
            EnablePiece("piece_gift1");
            EnablePiece("piece_gift2");
            EnablePiece("piece_gift3");

            EnableRecipe("$item_helmet_midsummercrown");
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
            }
            catch (Exception e)
            {
                NoSeasonalRestrictionsPlugin.NoSeasonalRestrictionsLogger.LogDebug($"Skipping configuring Piece: {name}");
            }
        }

        /// <summary>
        /// Enables the given Recipe if found
        /// </summary>
        /// <param name="name">Name found in the prefab's ItemData</param>
        private static void EnableRecipe(string name)
        {
            try
            {
                var recipeData = new ItemDrop.ItemData();
                recipeData.m_shared.m_name = name;
                ObjectDB.instance.GetRecipe(recipeData).m_enabled = true;
            }
            catch (Exception e)
            {
                NoSeasonalRestrictionsPlugin.NoSeasonalRestrictionsLogger.LogDebug($"Skipping configuring Recipe: {name}");
            }
        }

        /// <summary>
        /// Helper method to identify diabled entities
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
                catch (Exception e)
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
                catch (Exception e)
                {
                    NoSeasonalRestrictionsPlugin.NoSeasonalRestrictionsLogger.LogDebug($"Error with ListDisabledItems: {obj.name}");
                }
            }
        }
    }
}