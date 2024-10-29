using BepInEx;
using HarmonyLib;

namespace VentureValheim.Progression
{
    public partial class KeyManager
    {
        /// <summary>
        /// Only increase taming if the player has the private key.
        /// </summary>
        [HarmonyPatch(typeof(Tameable), nameof(Tameable.DecreaseRemainingTime))]
        public static class Patch_Tameable_DecreaseRemainingTime
        {
            [HarmonyPriority(Priority.Low)]
            private static void Prefix(Tameable __instance, ref float time)
            {
                if (ProgressionConfiguration.Instance.GetLockTaming())
                {
                    if (__instance.m_character == null ||
                        !Instance.HasTamingKey(Utils.GetPrefabName(__instance.m_character.gameObject)))
                    {
                        time = 0f;
                    }
                }
            }
        }

        /// <summary>
        /// Block getting guardian powers without the key.
        /// </summary>
        [HarmonyPatch(typeof(ItemStand), nameof(ItemStand.DelayedPowerActivation))]
        public static class Patch_ItemStand_DelayedPowerActivation
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(ItemStand __instance)
            {
                if (ProgressionConfiguration.Instance.GetLockGuardianPower())
                {
                    if (!Instance.HasGuardianKey(__instance.m_guardianPower?.name))
                    {
                        Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                        return false; // Skip giving power
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Block activating guardian powers without the key if a Player already has one.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.ActivateGuardianPower))]
        public static class Patch_Player_ActivateGuardianPower
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(Player __instance, ref bool __result, ref bool __runOriginal)
            {
                // Check for other mods skipping first
                if (__runOriginal == false)
                {
                    return false;
                }

                if (!__instance.m_guardianPower.IsNullOrWhiteSpace() && ProgressionConfiguration.Instance.GetLockGuardianPower())
                {
                    if (!Instance.HasGuardianKey(__instance.m_guardianPower))
                    {
                        Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                        __result = false; // Not sure why they have a return type on this, watch for game changes
                        return false; // Skip giving power
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Block the boss spawn when the player has not defeated the previous boss
        /// </summary>
        [HarmonyPatch(typeof(OfferingBowl), nameof(OfferingBowl.InitiateSpawnBoss))]
        public static class Patch_OfferingBowl_InitiateSpawnBoss
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(OfferingBowl __instance)
            {
                if (ProgressionConfiguration.Instance.GetLockBossSummons() && __instance.m_bossPrefab != null)
                {
                    if (!Instance.HasSummoningKey(Utils.GetPrefabName(__instance.m_bossPrefab.gameObject)))
                    {
                        Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                        return false; // Skip summoning
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Block equipping items without the proper keys.
        /// </summary>
        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
        public static class Patch_Humanoid_EquipItem
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(Humanoid __instance, ref bool __result, ItemDrop.ItemData item)
            {
                if (__instance != Player.m_localPlayer)
                {
                    return true;
                }

                if (ProgressionConfiguration.Instance.GetLockEquipment())
                {
                    if (Instance.IsActionBlocked(item, item.m_quality, true, true, false))
                    {
                        Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                        __result = false;
                        return false; // Skip equipping item
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Block using ammo without the proper keys.
        /// </summary>
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.GetAmmoItem))]
        public static class Patch_Inventory_GetAmmoItem
        {
            private static void Postfix(Inventory __instance, ref ItemDrop.ItemData __result)
            {
                if (__instance != Player.m_localPlayer.GetInventory() || __result == null)
                {
                    return;
                }

                if (ProgressionConfiguration.Instance.GetLockEquipment())
                {
                    if (Instance.IsActionBlocked(__result, __result.m_quality, true, true, false))
                    {
                        Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                        __result = null;
                    }
                }
            }
        }

        /// <summary>
        /// Block opening doors without the proper keys.
        /// </summary>
        [HarmonyPatch(typeof(Door), nameof(Door.HaveKey))]
        public static class Patch_Door_HaveKey
        {
            [HarmonyPriority(Priority.Low)]
            private static void Postfix(Door __instance, ref bool __result)
            {
                if (__result && ProgressionConfiguration.Instance.GetLockEquipment() && __instance.m_keyItem != null &&
                    !Instance.HasItemKey(Utils.GetPrefabName(__instance.m_keyItem.gameObject), true, false, false))
                {
                    Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                    __result = false;
                }
            }
        }

        /// <summary>
        /// Block crafting items without the proper keys.
        /// </summary>
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.DoCrafting))]
        public static class Patch_InventoryGui_DoCrafting
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(InventoryGui __instance)
            {
                bool cookingStation = false;
                if (__instance.m_craftRecipe?.m_craftingStation != null)
                {
                    string station = Utils.GetPrefabName(__instance.m_craftRecipe.m_craftingStation.gameObject);
                    cookingStation = station.Equals("piece_cauldron");
                }

                var lockCrafting = ProgressionConfiguration.Instance.GetLockCrafting() && !cookingStation;
                var lockCooking = ProgressionConfiguration.Instance.GetLockCooking() && cookingStation;

                int quality = ProgressionAPI.GetQualityLevel(__instance.m_craftUpgradeItem);

                if ((lockCrafting || lockCooking) && Instance.IsActionBlocked(
                    __instance.m_craftRecipe, quality, lockCrafting, lockCrafting, lockCooking))
                {
                    Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                    return false; // Skip crafting or cooking
                }

                return true;
            }
        }

        /// <summary>
        /// Block placing items without the proper keys.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.TryPlacePiece))]
        public static class Patch_Player_TryPlacePiece
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(ref bool __result, Piece piece)
            {
                if (ProgressionConfiguration.Instance.GetLockBuilding() && piece?.m_resources != null)
                {
                    for (int lcv = 0; lcv < piece.m_resources.Length; lcv++)
                    {
                        if (piece.m_resources[lcv]?.m_resItem != null &&
                            !Instance.HasItemKey(Utils.GetPrefabName(piece.m_resources[lcv].m_resItem.gameObject), true, true, false))
                        {
                            Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                            __result = false;
                            return false; // Skip placing
                        }
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Block cooking items without the proper keys.
        /// </summary>
        [HarmonyPatch(typeof(CookingStation), nameof(CookingStation.OnUseItem))]
        public static class Patch_CookingStation_OnUseItem
        {
            [HarmonyPriority(Priority.Low)]
            private static bool Prefix(ItemDrop.ItemData item, ref bool __result)
            {
                if (ProgressionConfiguration.Instance.GetLockCooking() &&
                    Instance.IsActionBlocked(item, item.m_quality, false, false, true))
                {
                    Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                    __result = false;
                    return false; // Skip cooking
                }

                return true;
            }
        }

        /// <summary>
        /// Block portal usage without the proper keys.
        /// </summary>
        [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Teleport))]
        public static class Patch_TeleportWorld_Teleport
        {
            private static bool Prefix(Player player)
            {
                if (player == Player.m_localPlayer && !Instance.HasKey(ProgressionConfiguration.Instance.GetLockPortalsKey()))
                {
                    Instance.ApplyBlockedActionEffects(Player.m_localPlayer);
                    return false; // Skip portaling
                }

                return true;
            }
        }

        /// <summary>
        /// Unblock portal usage for metals with proper keys.
        /// </summary>
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.IsTeleportable))]
        public static class Patch_Inventory_IsTeleportable
        {
            private static bool Prefix(Inventory __instance, ref bool __result)
            {
                // TODO: test
                if (ZoneSystem.instance.GetGlobalKey(GlobalKeys.TeleportAll))
                {
                    __result = true;
                    return false;
                }

                foreach (ItemDrop.ItemData item in __instance.m_inventory)
                {
                    if (!item.m_shared.m_teleportable && !Instance.IsTeleportable(item.m_dropPrefab.name))
                    {
                        __result = false;
                        return false;
                    }
                }

                __result = true;
                return false;
            }
        }
    }
}
