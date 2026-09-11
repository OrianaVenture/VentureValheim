using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace VentureValheim.AreaRepair;

public class AreaRepair
{
    private const float RADIUS = 20f;
    private static bool AltKeyPressed = false;
    private static Dictionary<string, bool> TempStationsInRange = new Dictionary<string, bool>();

    public static List<KeyValuePair<Piece, float>> GetAllPiecesInRadius(Vector3 position, float radius)
    {
        List<KeyValuePair<Piece, float>> pieces = new List<KeyValuePair<Piece, float>>();

        foreach (Piece piece in Piece.s_allPieces)
        {
            float distance = Vector3.Distance(position, piece.transform.position);
            if (piece.gameObject.layer != Piece.s_ghostLayer && distance <= radius)
            {
                pieces.Add(new KeyValuePair<Piece, float>(piece, distance));
            }
        }

        return pieces;
    }

    public static bool CheckCanRepair(Vector3 playerPosition, Piece piece, float radius, bool freePlacement)
    {
        if (!freePlacement && piece.m_craftingStation != null)
        {
            if (!TempStationsInRange.ContainsKey(piece.m_craftingStation.m_name))
            {
                TempStationsInRange.Add(piece.m_craftingStation.m_name,
                    CraftingStation.HaveBuildStationInRange(piece.m_craftingStation.m_name, playerPosition));
            }

            if (!TempStationsInRange[piece.m_craftingStation.m_name])
            {
                return false;
            }
        }

        return PrivateArea.CheckAccess(piece.transform.position);
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Repair))]
    private static class Patch_Player_Repair
    {
        private static void Postfix(Player __instance, ItemDrop.ItemData toolItem)
        {
            if (AltKeyPressed)
            {
                return;
            }

            ItemDrop.ItemData rightItem = __instance.GetRightItem();

            if (rightItem == null)
            {
                return;
            }

            float repairStamina = __instance.GetBuildStamina();
            if (!__instance.HaveStamina(repairStamina))
            {
                return;
            }

            Piece hoverPiece = __instance.GetHoveringPiece();
            Vector3 position = hoverPiece != null ? hoverPiece.transform.position : __instance.transform.position;

            int totalRepaired = 0;
            List<KeyValuePair<Piece, float>> pieces = GetAllPiecesInRadius(position, RADIUS);
            pieces = pieces.OrderBy(p => p.Value).ToList();

            bool freePlacement = __instance.m_noPlacementCost || ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoWorkbench);
            float durabilityDrain = toolItem.m_shared.m_useDurabilityDrain * Game.m_durabilityRate;

            for (int lcv = 0; lcv < pieces.Count; lcv++)
            {
                Piece piece = pieces[lcv].Key;
                WearNTear pieceWear = piece.GetComponent<WearNTear>();

                if (pieceWear == null)
                {
                    continue;
                }

                if (!CheckCanRepair(__instance.transform.position, piece, RADIUS, freePlacement))
                {
                    continue;
                }

                if (pieceWear.Repair())
                {
                    if (!__instance.m_noPlacementCost)
                    {
                        __instance.UseStamina(repairStamina);
                        // TODO: Evaluate if need additional checks are needed for player Eitr amount
                        __instance.UseEitr(toolItem.m_shared.m_attack.m_attackEitr);
                        if (toolItem.m_shared.m_useDurability)
                        {
                            toolItem.m_durability -= durabilityDrain;
                        }
                    }

                    piece.m_placeEffect.Create(piece.transform.position, piece.transform.rotation,
                        null, 1f, -1, __instance.GetZDOID());

                    totalRepaired++;
                }

                if (!__instance.m_noPlacementCost && (!__instance.HaveStamina(repairStamina) || toolItem.m_durability < durabilityDrain))
                {
                    break;
                }
            }

            if (totalRepaired > 0)
            {
                __instance.FaceLookDirection();
                __instance.m_zanim.SetTrigger(toolItem.m_shared.m_attack.m_attackAnimation);

                __instance.Message(MessageHud.MessageType.TopLeft,
                    Localization.instance.Localize("$msg_repaired", totalRepaired.ToString()));
            }

            TempStationsInRange.Clear();
        }
    }

    // Thank you Redsekio for the template!
    [HarmonyPatch(typeof(Player))]
    static class PlayerPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Player), nameof(Player.Update))]
        static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .Start()
                .MatchStartForward(
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Player), nameof(Player.UpdateHover))))
                .ThrowIfInvalid($"Could not patch Player.Update()!")
                .Advance(offset: 1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlayerPatch), nameof(UpdateInputDelegate))))
                .InstructionEnumeration();
        }

        static void UpdateInputDelegate(Player player, bool takeInput)
        {
            AltKeyPressed = takeInput && ZInput.GetKey(KeyCode.LeftAlt);
        }
    }
}