using BepInEx;
using System;
using UnityEngine;

namespace VentureValheim.TravelTotems;

public class TravelTotemPortal : MonoBehaviour, Hoverable, Interactable, TextReceiver
{
    public ZNetView ZNetView;

    public const string TOTEM_ID_BIN = "VV_TotemIDBin";
    public const string TOTEM_ID = "VV_TotemID";
    public const string TOTEM_NAME = "VV_TotemName";
    public const string TOTEM_TYPE = "VV_TotemType";

    public const string RPCNAME_SetupAccess = "VV_SetupAccess";

    // Effects
    public EffectFade EnabledEffects;
    public EffectFade DisabledEffects;
    public EffectFade PortalEffects;
    public float ActivationRange = 5f;

    // Emission
    public Light UnlockedLight;
    public MeshRenderer EmissionRenderer;
    public Color EmissionColor;
    private float EmissionAlpha = 0f;

    private float LastUseTime = -1f;

    private bool IsPlacementGhost = false;
    private bool IsCreator = false;
    private bool IsPublic = false;
    private bool IsPiece = false;

    private void Awake()
    {
        ZNetView = GetComponent<ZNetView>();

        ZNetView.Register(RPCNAME_SetupAccess, new Action<long, long>(RPC_SetupAccess));

        IsPiece = GetComponent<Piece>() != null;
    }

    private void Start()
    {
        if (Player.IsPlacementGhost(base.gameObject))
        {
            IsPlacementGhost = true;

            EnabledEffects.SetActive(false);
            DisabledEffects.SetActive(false);
            PortalEffects.SetActive(false);
            UnlockedLight.gameObject.SetActive(false);
            return;
        }

        int totemIDBin = ZNetView.GetZDO().GetInt(TOTEM_ID_BIN, -1);
        int totemID = ZNetView.GetZDO().GetInt(TOTEM_ID, -1);

        if (totemIDBin == -1 || totemID == -1)
        {
            TravelTotems.AssignTotemID(ZNetView.GetZDO());
        }

        int undefined = (int)TravelTotemType.Undefined;
        int type = ZNetView.GetZDO().GetInt(TOTEM_TYPE, undefined);
        if (type == undefined)
        {
            SetTotemType(TravelTotemType.ServerDefault);
        }

        SnapToGround snap = GetComponent<SnapToGround>();
        if (snap)
        {
            snap.Snap();
        }

        UpdateEffects();
        long creator = ZNetView.GetZDO().GetLong(ZDOVars.s_creator, 0L);
        SetupAccess(creator);

        TravelTotems.AddTotem(ZNetView.GetZDO());
        TravelTotemMap.RefreshMapPins();
    }

    private void Update()
    {
        if (!ZNetView.IsValid() || IsPlacementGhost)
        {
            return;
        }

        Player closestPlayer = Player.GetClosestPlayer(base.transform.position, ActivationRange);

        // TODO: teleport settings if doing that
        bool travelPossible = closestPlayer && closestPlayer.IsTeleportable(false);
        UpdateEffects(travelPossible);
    }

    private void SetupAccess(long creator)
    {
        IsPublic = creator == 0L;

        if (ZNet.instance.IsDedicated())
        {
            IsCreator = false;
        }
        else
        {
            IsCreator = creator == Game.instance.GetPlayerProfile().GetPlayerID();
        }
    }

    private void RPC_SetupAccess(long sender, long creator)
    {
        if (ZNetView.IsOwner())
        {
            ZNetView.GetZDO().Set(ZDOVars.s_creator, creator);
        }

        SetupAccess(creator);
    }

    private void UpdateEffects(bool travelPossible = false)
    {
        bool unlocked = Unlocked();
        bool enabled = unlocked && !travelPossible;
        bool disabled = !unlocked;
        bool portal = unlocked && travelPossible;

        EnabledEffects.SetActive(enabled);
        DisabledEffects.SetActive(disabled);
        PortalEffects.SetActive(portal);

        EmissionAlpha = Mathf.MoveTowards(EmissionAlpha, unlocked ? 1f : 0f, Time.deltaTime);
        EmissionRenderer.material.SetColor("_EmissionColor", Color.Lerp(Color.black, EmissionColor, EmissionAlpha));

        // TODO: find a way to get fading the light to work correctly
        UnlockedLight.gameObject.SetActive(unlocked);
    }

    public string GetHoverText()
    {
        if (!ZNetView.IsValid())
        {
            return "";
        }

        bool unlocked = Unlocked();
        string displayConnected = (unlocked ? "$piece_portal_connected" : "$piece_portal_unconnected");
        string portalText = $"{GetText()}[{ displayConnected }]";

        if (TravelTotemsPlugin.GetOverrideTotemAccess(false) || TravelTotemsPlugin.GetShowTotemID())
        {
            portalText += $"[<color=yellow>ID {GetTotemIDBin()}:{GetTotemID()}</color>]";
        }

        if (!unlocked && GetTotemType() == TravelTotemType.FindUnlock)
        {
            portalText += "\n[<color=yellow><b>$KEY_Use</b></color>] Activate";
            if (RequiresPayment())
            {
                portalText += $" [<color=green><b>{TravelTotemsPlugin.GetTotemUnlockRequirementsString()}</b></color>]";
            }
        }

        if (TravelTotemsPlugin.GetTotemNamingAccess(IsCreator, IsPublic))
        {
            bool gamepad = ZInput.IsNonClassicFunctionality() && ZInput.IsGamepadActive();
            string altKey = gamepad ? "$KEY_AltKeys" : "$KEY_AltPlace";
            portalText += $"\n[<color=yellow><b>{altKey} + $KEY_Use</b></color>] $piece_portal_settag";
        }

        if (TravelTotemsPlugin.GetOverrideTotemAccess(IsCreator))
        {
            portalText += $"\n[<color=yellow><b>Hold $KEY_Use</b></color>] Cycle Access: [{GetTotemType(false)}]";
        }

        return Localization.instance.Localize(portalText);
    }

    public string GetHoverName()
    {
        return "TravelTotem";
    }

    public float GetHoverOffset()
    {
        return 0f;
    }

    public bool Interact(Humanoid human, bool hold, bool alt)
    {
        if (!ZNetView.IsValid())
        {
            return false;
        }

        if (hold)
        {
            if (TravelTotemsPlugin.GetOverrideTotemAccess(IsCreator))
            {
                if (Time.time - LastUseTime > 0.5f)
                {
                    CycleTotemAccess();
                    LastUseTime = Time.time;
                    return true;
                }
            }

            return false;
        }

        LastUseTime = Time.time;

        if (alt)
        {
            if (TravelTotemsPlugin.GetTotemNamingAccess(IsCreator, IsPublic))
            {
                TextInput.instance.RequestText(this, "$piece_sign_input", 50);
            }

            return true;
        }

        if (!TryTrackTotem(true))
        {
            return false;
        }

        return true;
    }

    private void CycleTotemAccess()
    {
        switch (GetTotemType(false))
        {
            case TravelTotemType.AlwaysUnlock:
                SetTotemType(TravelTotemType.FindUnlock);
                break;
            case TravelTotemType.FindUnlock:
                SetTotemType(TravelTotemType.AlwaysLock);
                break;
            case TravelTotemType.AlwaysLock:
                SetTotemType(TravelTotemType.ServerDefault);
                break;
            case TravelTotemType.ServerDefault:
                SetTotemType(TravelTotemType.AlwaysUnlock);
                break;
            default:
                SetTotemType(TravelTotemType.ServerDefault);
                break;
        }
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        return false;
    }

    public string GetText()
    {
        string name = ZNetView.GetZDO().GetString(TOTEM_NAME);
        if (!name.IsNullOrWhiteSpace())
        {
            name += " ";
        }

        return name;
    }

    public void SetText(string name)
    {
        ZNetView.ClaimOwnership();
        ZNetView.GetZDO().Set(TOTEM_NAME, name);
    }

    public void SetTotemType(TravelTotemType type)
    {
        ZNetView.ClaimOwnership();
        ZNetView.GetZDO().Set(TOTEM_TYPE, (int)type);
    }

    public bool Unlocked()
    {
        return TravelTotemPlayer.TotemUnlocked(IsCreator, GetTotemType(), GetTotemIDBin(), GetTotemID());
    }

    /// <summary>
    /// Totems built or claimed by players do not require a payment to unlock.
    /// </summary>
    public bool RequiresPayment()
    {
        return !IsPiece && IsPublic && TravelTotems.HasUnlockRequirements();
    }

    public int GetTotemIDBin()
    {
        return ZNetView.GetZDO().GetInt(TOTEM_ID_BIN, -1);
    }

    public int GetTotemID()
    {
        return ZNetView.GetZDO().GetInt(TOTEM_ID, -1);
    }

    public static TravelTotemType GetTotemType(ZDO zdo)
    {
        TravelTotemType type = (TravelTotemType)zdo.GetInt(TOTEM_TYPE);
        return ResolveTotemType(type);
    }

    public TravelTotemType GetTotemType(bool resolveDefault = true)
    {
        TravelTotemType type = (TravelTotemType)ZNetView.GetZDO().GetInt(TOTEM_TYPE);
        if (resolveDefault)
        {
            type = ResolveTotemType(type);
        }

        return type;
    }

    private static TravelTotemType ResolveTotemType(TravelTotemType type)
    {
        if (type == TravelTotemType.ServerDefault)
        {
            type = TravelTotemsPlugin.GetTravelTotemType();
        }
        
        if (type == TravelTotemType.Undefined)
        {
            // Fallback
            return TravelTotemType.FindUnlock;
        }

        return type;
    }

    /// <summary>
    /// Try to track the totem if it can be used.
    /// If admin is overriding access only adds to known personal totem list if meets regular play requirements.
    /// </summary>
    public bool TryTrackTotem(bool consumeResources)
    {
        if (Unlocked())
        {
            TryClaimTotem();
        }
        else
        {
            TravelTotemType type = GetTotemType();
            bool adminOverride = TravelTotemsPlugin.GetOverrideTotemAccess(false);

            if (type == TravelTotemType.AlwaysLock)
            {
                if (!adminOverride)
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$piece_noaccess");
                }

                return adminOverride;
            }
            else if (type == TravelTotemType.FindUnlock)
            {
                if (RequiresPayment())
                {
                    if (!consumeResources)
                    {
                        // Do not try to unlock
                        return adminOverride;
                    }

                    if (!TravelTotems.TryConsumeUnlockRequirements())
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_incompleteoffering");
                        return false;
                    }
                }

                // Items were consumed
                TryClaimTotem();
            }
        }

        TravelTotemPlayer.AddKnownTotem(GetTotemIDBin(), GetTotemID());
        return true;
    }

    public void TryClaimTotem()
    {
        if (!IsPiece && IsPublic && TravelTotemsPlugin.GetAllowTotemClaiming())
        {
            long creator = Game.instance.GetPlayerProfile().GetPlayerID();

            // Only sends to owner when using this
            // ZNetView.InvokeRPC(RPCNAME_SetupAccess, creator);

            // Sends to all players
            RPC_SetupAccess(creator, creator);
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, RPCNAME_SetupAccess, creator);
        }
    }
}