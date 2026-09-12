using HarmonyLib;

namespace VentureValheim.Cornerstone;

public class Cornerstone
{
    [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.GetMaterialProperties))]
    public static class Patch_WearNTear_GetMaterialProperties
    {
        private static void Postfix(WearNTear __instance,
            ref float maxSupport, ref float minSupport, ref float horizontalLoss, ref float verticalLoss)
        {
            switch (__instance.m_materialType)
            {
                case WearNTear.MaterialType.Wood:
                    if (CornerstonePlugin.GetWoodEnabled())
                    {
                        maxSupport = CornerstonePlugin.GetWoodMaxStability();
                        minSupport = CornerstonePlugin.GetWoodMinStability();
                        verticalLoss = CornerstonePlugin.GetWoodVerticalLoss();
                        horizontalLoss = CornerstonePlugin.GetWoodHorizontalLoss();
                    }
                    break;
                case WearNTear.MaterialType.HardWood:
                    if (CornerstonePlugin.GetHardWoodEnabled())
                    {
                        maxSupport = CornerstonePlugin.GetHardWoodMaxStability();
                        minSupport = CornerstonePlugin.GetHardWoodMinStability();
                        verticalLoss = CornerstonePlugin.GetHardWoodVerticalLoss();
                        horizontalLoss = CornerstonePlugin.GetHardWoodHorizontalLoss();
                    }
                    break;
                case WearNTear.MaterialType.Stone:
                    if (CornerstonePlugin.GetStoneEnabled())
                    {
                        maxSupport = CornerstonePlugin.GetStoneMaxStability();
                        minSupport = CornerstonePlugin.GetStoneMinStability();
                        verticalLoss = CornerstonePlugin.GetStoneVerticalLoss();
                        horizontalLoss = CornerstonePlugin.GetStoneHorizontalLoss();
                    }
                    break;
                case WearNTear.MaterialType.Iron:
                    if (CornerstonePlugin.GetIronEnabled())
                    {
                        maxSupport = CornerstonePlugin.GetIronMaxStability();
                        minSupport = CornerstonePlugin.GetIronMinStability();
                        verticalLoss = CornerstonePlugin.GetIronVerticalLoss();
                        horizontalLoss = CornerstonePlugin.GetIronHorizontalLoss();
                    }
                    break;
                case WearNTear.MaterialType.Marble:
                    if (CornerstonePlugin.GetMarbleEnabled())
                    {
                        maxSupport = CornerstonePlugin.GetMarbleMaxStability();
                        minSupport = CornerstonePlugin.GetMarbleMinStability();
                        verticalLoss = CornerstonePlugin.GetMarbleVerticalLoss();
                        horizontalLoss = CornerstonePlugin.GetMarbleHorizontalLoss();
                    }
                    break;
                case WearNTear.MaterialType.Ashstone:
                    if (CornerstonePlugin.GetAshstoneEnabled())
                    {
                        maxSupport = CornerstonePlugin.GetAshstoneMaxStability();
                        minSupport = CornerstonePlugin.GetAshstoneMinStability();
                        verticalLoss = CornerstonePlugin.GetAshstoneVerticalLoss();
                        horizontalLoss = CornerstonePlugin.GetAshstoneHorizontalLoss();
                    }
                    break;
                case WearNTear.MaterialType.Ancient:
                    if (CornerstonePlugin.GetAncientEnabled())
                    {
                        maxSupport = CornerstonePlugin.GetAncientMaxStability();
                        minSupport = CornerstonePlugin.GetAncientMinStability();
                        verticalLoss = CornerstonePlugin.GetAncientVerticalLoss();
                        horizontalLoss = CornerstonePlugin.GetAncientHorizontalLoss();
                    }
                    break;
                case WearNTear.MaterialType.Ice:
                    if (CornerstonePlugin.GetIceEnabled())
                    {
                        maxSupport = CornerstonePlugin.GetIceMaxStability();
                        minSupport = CornerstonePlugin.GetIceMinStability();
                        verticalLoss = CornerstonePlugin.GetIceVerticalLoss();
                        horizontalLoss = CornerstonePlugin.GetIceHorizontalLoss();
                    }
                    break;
                case WearNTear.MaterialType.Timberwood:
                    if (CornerstonePlugin.GetTimberwoodEnabled())
                    {
                        maxSupport = CornerstonePlugin.GetTimberwoodMaxStability();
                        minSupport = CornerstonePlugin.GetTimberwoodMinStability();
                        verticalLoss = CornerstonePlugin.GetTimberwoodVerticalLoss();
                        horizontalLoss = CornerstonePlugin.GetTimberwoodHorizontalLoss();
                    }
                    break;
                default:
                    break;
            }
        }
    }
}