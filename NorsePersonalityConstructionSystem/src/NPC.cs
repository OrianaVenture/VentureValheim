using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;

namespace VentureValheim.NPCS;

// TODO localization
public class NPC : Humanoid, Interactable, Hoverable
{
    public enum NPCType
    {
        None = 0,
        Information = 1,
        Reward = 2,
        Sellsword = 3,
        SlayTarget = 4,
        Trader = 5
    }

    private const string DUMMY = "attach_dummy";

    public bool HasAttach = false; // TODO fix zdo syncing for this?
    public Transform AttachPoint;
    public string AttachAnimation;
    public GameObject AttachRoot;
    public Collider[] AttachColliders;

    private readonly Color32 _hairColorMin = new Color32(0xFF, 0xED, 0xB4, 0xFF);
    private readonly Color32 _hairColorMax = new Color32(0xFF, 0x7C, 0x47, 0xFF);
    private readonly Color32 _skinColorMin = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
    private readonly Color32 _skinColorMax = new Color32(0x4C, 0x4C, 0x4C, 0xFF);

    #region ZDO Values

    public string TamedName => m_nview.GetZDO().GetString(ZDOVars.s_tamedName);

    public const string ZDOVar_NPCType = "VV_NPCType";
    public int Type => m_nview.GetZDO().GetInt(ZDOVar_NPCType);
    // TODO support all kinds of attachment, probably by enum type
    public const string ZDOVar_SITTING = "VV_Sitting";
    public bool WasSitting => m_nview.GetZDO().GetBool(ZDOVar_SITTING);
    public const string ZDOVar_ATTACHED = "VV_Attached";
    public bool Attached => m_nview.GetZDO().GetBool(ZDOVar_ATTACHED);
    public const string ZDOVar_SPAWNPOINT = "VV_SpawnPoint";
    public Vector3 SpawnPoint => m_nview.GetZDO().GetVec3(ZDOVar_SPAWNPOINT, Vector3.zero);

    // Main Text used when all else fails
    public const string ZDOVar_DEFAULTTEXT = "VV_DefaultText";
    public string NPCDefaultText => m_nview.GetZDO().GetString(ZDOVar_DEFAULTTEXT);
    // Interact Text
    public const string ZDOVar_INTERACTTEXT = "VV_InteractText";
    public string NPCInteractText => m_nview.GetZDO().GetString(ZDOVar_INTERACTTEXT);
    // Use Item
    public const string ZDOVar_GIVEITEM = "VV_GiveItem";
    public string NPCGiveItem => m_nview.GetZDO().GetString(ZDOVar_GIVEITEM);
    public const string ZDOVar_GIVEITEMQUALITY = "VV_UseItemQuality";
    public int NPCGiveItemQuality => m_nview.GetZDO().GetInt(ZDOVar_GIVEITEMQUALITY);
    public const string ZDOVar_GIVEITEMAMOUNT = "VV_UseItemAmount";
    public int NPCGiveItemAmount => m_nview.GetZDO().GetInt(ZDOVar_GIVEITEMAMOUNT);
    // Reward
    public const string ZDOVar_REWARDTEXT = "VV_RewardText";
    public string NPCRewardText => m_nview.GetZDO().GetString(ZDOVar_REWARDTEXT);
    public const string ZDOVar_REWARDITEM = "VV_RewardItem";
    public string NPCRewardItem => m_nview.GetZDO().GetString(ZDOVar_REWARDITEM);
    public const string ZDOVar_REWARDITEMQUALITY = "VV_RewardItemQualtiy";
    public int NPCRewardItemQuality => m_nview.GetZDO().GetInt(ZDOVar_REWARDITEMQUALITY);
    public const string ZDOVar_REWARDITEMAMOUNT = "VV_RewardItemAmount";
    public int NPCRewardItemAmount => m_nview.GetZDO().GetInt(ZDOVar_REWARDITEMAMOUNT);
    public const string ZDOVar_REWARDLIMIT = "VV_RewardLimit";
    public int NPCRewardLimit => m_nview.GetZDO().GetInt(ZDOVar_REWARDLIMIT);
    // TODO add cooldown option
    // Keys
    public const string ZDOVar_REQUIREDKEYS = "VV_RequiredKeys";
    protected string NPCRequiredKeys => m_nview.GetZDO().GetString(ZDOVar_REQUIREDKEYS);
    public HashSet<string> NPCRequiredKeysSet = new HashSet<string>();
    public const string ZDOVar_NOTREQUIREDKEYS = "VV_NotRequiredKeys";
    protected string NPCNotRequiredKeys => m_nview.GetZDO().GetString(ZDOVar_NOTREQUIREDKEYS);
    public HashSet<string> NPCNotRequiredKeysSet = new HashSet<string>();
    public const string ZDOVar_INTERACTKEY = "VV_InteractKey";
    public string NPCInteractKey => m_nview.GetZDO().GetString(ZDOVar_INTERACTKEY);
    public const string ZDOVar_REWARDKEY = "VV_RewardKey";
    public string NPCRewardKey => m_nview.GetZDO().GetString(ZDOVar_REWARDKEY);
    public const string ZDOVar_DEFEATKEY = "VV_DefeatKey";
    public string NPCDefeatKey => m_nview.GetZDO().GetString(ZDOVar_DEFEATKEY);

    public const string ZDOVar_TRUEDEATH = "VV_TrueDeath";
    public bool TrueDeath => m_nview.GetZDO().GetBool(ZDOVar_TRUEDEATH);

    protected static List<int> ZdoVisEquipment = new List<int>
    {
        ZDOVars.s_beardItem,
        ZDOVars.s_hairItem,
        ZDOVars.s_helmetItem,
        ZDOVars.s_chestItem,
        ZDOVars.s_legItem,
        ZDOVars.s_shoulderItem,
        ZDOVars.s_shoulderItemVariant,
        ZDOVars.s_rightItem,
        ZDOVars.s_leftItem,
        ZDOVars.s_leftItemVariant,
        ZDOVars.s_rightBackItem,
        ZDOVars.s_leftBackItem,
        ZDOVars.s_leftBackItemVariant
    };

    #endregion

    #region Humanoid and Components

    public override void Awake()
    {
        base.Awake();

        var startCoroutine = SetUp();
        StartCoroutine(startCoroutine);
    }

    public override void OnDestroy()
    {
        if (AttachPoint != null && AttachPoint.name.Contains(DUMMY))
        {
            Destroy(AttachPoint.gameObject);
        }

        base.OnDestroy();
    }

    public IEnumerator SetUp()
    {
        yield return null;
        yield return null;

        m_name = TamedName;

        if (WasSitting)
        {
            var chair = Utility.GetClosestChair(base.transform.position, base.transform.localScale / 2);
            if (chair != null)
            {
                AttachStart(chair);
            }
        }
        else if (Attached)
        {
            AttachStart();
        }

        NPCRequiredKeysSet = Utility.StringToSet(NPCRequiredKeys);
        NPCNotRequiredKeysSet = Utility.StringToSet(NPCNotRequiredKeys);

        m_defeatSetGlobalKey = NPCDefeatKey;

        //var tamed = m_nview.GetZDO().GetBool(ZDOVars.s_tamed);
        //NPCSPlugin.NPCSLogger.LogDebug($"{m_name}: {m_faction}, {m_tamed}, {m_group}. ZDO: {tamed}");
    }

    public override void CustomFixedUpdate(float fixedDeltaTime)
    {
        if (!m_nview.IsValid() || !m_nview.IsOwner())
        {
            return;
        }

        base.CustomFixedUpdate(fixedDeltaTime);

        if (HasAttach)
        {
            if (AttachPoint != null)
            {
                base.transform.position = AttachPoint.position;
                base.transform.rotation = AttachPoint.rotation;
                Rigidbody componentInParent = AttachPoint.GetComponentInParent<Rigidbody>();
                m_body.useGravity = false;
                m_body.velocity = (componentInParent ?
                    componentInParent.GetPointVelocity(base.transform.position) : Vector3.zero);
                m_body.angularVelocity = Vector3.zero;
                m_maxAirAltitude = base.transform.position.y;
            }
            else
            {
                AttachStop();
            }
        }
    }

    public override void OnDeath()
    {
        if (m_nview == null || !m_nview.IsOwner())
        {
            return;
        }

        GameObject[] effects = m_deathEffects.Create(base.transform.position, base.transform.rotation, base.transform);
        for (int lcv = 0; lcv < effects.Length; lcv++)
        {
            Ragdoll ragdoll = effects[lcv].GetComponent<Ragdoll>();

            if (ragdoll != null)
            {
                Vector3 velocity = m_body.velocity;
                if (m_pushForce.magnitude * 0.5f > velocity.magnitude)
                {
                    velocity = m_pushForce * 0.5f;
                }

                ragdoll.Setup(velocity, 0f, 0f, 0f, null);

                VisEquipment visEquip = effects[lcv].GetComponent<VisEquipment>();
                if (visEquip != null)
                {
                    CopyVisEquipment(ref visEquip, m_visEquipment);
                }

                ragdoll.m_nview.GetZDO().Set(ZDOVars.s_tamedName, m_name);
                ragdoll.m_nview.GetZDO().Set(ZDOVar_TRUEDEATH, TrueDeath);
            }
        }

        SetKey(m_defeatSetGlobalKey);

        if (m_onDeath != null)
        {
            m_onDeath();
        }

        if (!TrueDeath)
        {
            NPCFactory.RespawnNPC(this.transform.root.gameObject);
        }

        ZNetScene.instance.Destroy(base.gameObject);
    }

    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        if (m_baseAI.m_aggravated)
        {
            return false;
        }

        var text = NPCDefaultText;

        if (Type == (int)NPCType.Reward)
        {
            if (NPCRewardLimit != 0)
            {
                // This is unlocked
                if (NPCGiveItem.IsNullOrWhiteSpace())
                {
                    // I give my reward on interact, change key requirement text unlocks
                    if (HasCorrectNotReqiuredKeys())
                    {
                        if (HasCorrectReqiuredKeys())
                        {
                            GiveReward();
                            if (!NPCRewardText.IsNullOrWhiteSpace())
                            {
                                text = NPCRewardText;
                            }
                        }
                        else
                        {
                            text = NPCInteractText;
                        }
                    }
                }
                else if (HasCorrectKeys())
                {
                    // I give my reward on give item
                    text = NPCInteractText;
                }
            }
        }
        else if (HasCorrectKeys())
        {
            text = NPCInteractText;
        }

        if (!NPCInteractKey.IsNullOrWhiteSpace())
        {
            SetKey(NPCInteractKey);
        }

        Talk(text);

        return false;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        if (Type != (int)NPCType.Reward || !HasCorrectKeys() || NPCRewardLimit == 0 || m_baseAI.m_aggravated)
        {
            return false;
        }

        if (!NPCGiveItem.IsNullOrWhiteSpace())
        {
            if (item != null && item.m_dropPrefab.name.Equals(NPCGiveItem) && Player.m_localPlayer != null)
            {
                var player = Player.m_localPlayer;
                var count = player.GetInventory().CountItems(item.m_shared.m_name, NPCGiveItemQuality);
                if (count >= NPCGiveItemAmount)
                {
                    player.UnequipItem(item);
                    player.GetInventory().RemoveItem(item.m_shared.m_name, NPCGiveItemAmount, NPCGiveItemQuality);
                }
                else
                {
                    Talk("Hmmm that's not enough... " + NPCInteractText);
                    return false;
                }
            }
            else
            {
                Talk("Hmmm that's not right... " + NPCInteractText);
                return false;
            }
        }

        GiveReward();

        Talk(NPCRewardText);

        return true;
    }

    public override string GetHoverText()
    {
        if (m_baseAI.m_aggravated)
        {
            return "";
        }
        
        var type = m_nview.GetZDO().GetInt(ZDOVar_NPCType);
        string text = "";
        if (type != (int)NPCType.None)
        {
            text = Localization.instance.Localize(
                "[<color=yellow><b>$KEY_Use</b></color>] Talk");
        }

        if (type == (int)NPCType.Reward)
        {
            text += Localization.instance.Localize(
                "\n[<color=yellow><b>1-8</b></color>] Give");
        }

        return text;
    }

    public override string GetHoverName()
    {
        return m_name;
    }

    public override void SetupVisEquipment(VisEquipment visEq, bool isRagdoll)
    {
        // TODO find best pattern for saving equipment data.
        if (!transform.gameObject.name.Equals(NPCSPlugin.MOD_PREFIX + "Player"))
        {
            // Do nothing for Player npcs, the values are not populated
            base.SetupVisEquipment(visEq, isRagdoll);
        }
    }

    public override bool IsAttached()
    {
        return HasAttach || base.IsAttached();
    }

    public void Attach(bool attach, Chair chair = null)
    {
        ClaimOwnership();
        if (attach)
        {
            AttachStop();
            if (chair != null)
            {
                AttachStart(chair);
            }
            else
            {
                AttachStart();
            }
        }
        else
        {
            AttachStop();
        }
    }

    public void AttachStart()
    {
        HasAttach = true;
        GameObject dummy = new GameObject();
        dummy.transform.SetPositionAndRotation(transform.position, transform.rotation);
        dummy.name = DUMMY;
        AttachPoint = dummy.transform;
        if (m_nview.IsOwner())
        {
            m_nview.GetZDO().Set(ZDOVar_ATTACHED, value: true);
        }

        m_body.mass = 1000f;

        m_body.useGravity = false;
        m_body.velocity = Vector3.zero;
        m_body.angularVelocity = Vector3.zero;
        m_maxAirAltitude = base.transform.position.y;
    }

    public void AttachStart(Chair chair)
    {
        HasAttach = true;
        AttachPoint = chair.m_attachPoint;
        AttachAnimation = chair.m_attachAnimation;
        AttachRoot = chair.transform.root.gameObject;

        base.transform.position = AttachPoint.position;
        base.transform.rotation = AttachPoint.rotation;

        if (m_nview.IsOwner())
        {
            m_zanim.SetBool(AttachAnimation, value: true);
            m_nview.GetZDO().Set(ZDOVar_SITTING, value: true);
        }

        m_body.mass = 1000f;

        Rigidbody componentInParent = AttachPoint.GetComponentInParent<Rigidbody>();
        m_body.useGravity = false;
        m_body.velocity = (componentInParent ?
            componentInParent.GetPointVelocity(base.transform.position) : Vector3.zero);
        m_body.angularVelocity = Vector3.zero;
        m_maxAirAltitude = base.transform.position.y;

        AttachColliders = AttachRoot.GetComponentsInChildren<Collider>();

        Collider[] attachColliders = AttachColliders;
        foreach (Collider collider in attachColliders)
        {
            Physics.IgnoreCollision(m_collider, collider, ignore: true);
        }

        HideHandItems();
        ResetCloth();
    }

    public override void AttachStop()
    {
        if (!HasAttach)
        {
            return;
        }

        if (m_nview.IsOwner())
        {
            m_nview.GetZDO().Set(ZDOVar_ATTACHED, value: false);
            m_nview.GetZDO().Set(ZDOVar_SITTING, value: false);
        }

        if (m_zanim.IsOwner() && AttachAnimation != null)
        {
            m_zanim.SetBool(AttachAnimation, value: false);
        }

        if (AttachColliders != null)
        {
            Collider[] attachColliders = AttachColliders;
            foreach (Collider collider in attachColliders)
            {
                Physics.IgnoreCollision(m_collider, collider, ignore: false);
            }
        }

        if (AttachPoint.name.Contains(DUMMY))
        {
            Destroy(AttachPoint.gameObject);
        }

        HasAttach = false;
        AttachAnimation = "";
        AttachPoint = null;
        m_body.useGravity = true;
        m_body.mass = m_originalMass;
        AttachColliders = null;
        AttachRoot = null;

        ShowHandItems();
        ResetCloth();
    }

    #endregion

    #region Helper Functions

    private void SetKey(string key)
    {
        // TODO handle player keys
        /*if (!string.IsNullOrEmpty(key))
        {
            Player.m_addUniqueKeyQueue.Add(key);
        }*/
        if (!string.IsNullOrEmpty(key))
        {
            ZoneSystem.instance.SetGlobalKey(key);
        }
    }

    private string SetupText(string text)
    {
        string giveItem = NPCGiveItem;
        if (Utility.GetItemDrop(giveItem, out var requirement))
        {
            giveItem = requirement.m_itemData.m_shared.m_name;
        }

        string giveItemText = $"{NPCGiveItemAmount} {giveItem}";
        if (NPCGiveItemQuality > 1)
        {
            giveItemText += $" *{NPCGiveItemQuality}";
        }

        string rewardItem = NPCRewardItem;
        if (Utility.GetItemDrop(rewardItem, out var reward))
        {
            rewardItem = reward.m_itemData.m_shared.m_name;
        }

        string rewardItemText = $"{NPCRewardItemAmount} {rewardItem}";
        if (NPCRewardItemQuality > 1)
        {
            rewardItemText += $" *{NPCRewardItemQuality}";
        }

        text = text.Replace("{giveitem}", giveItemText).Replace("{reward}", rewardItemText);
        return Localization.instance.Localize(text);
    }

    public static void CopyZDO(ref ZDO copy, ZDO original)
    {
        copy.Set(ZDOVars.s_tamedName, original.GetString(ZDOVars.s_tamedName));
        copy.Set(ZDOVar_NPCType, original.GetInt(ZDOVar_NPCType));
        copy.Set(ZDOVar_SITTING, original.GetBool(ZDOVar_SITTING));
        copy.Set(ZDOVar_ATTACHED, original.GetBool(ZDOVar_ATTACHED));
        copy.Set(ZDOVar_SPAWNPOINT, original.GetVec3(ZDOVar_SPAWNPOINT, Vector3.zero));
        copy.Set(ZDOVar_DEFAULTTEXT, original.GetString(ZDOVar_DEFAULTTEXT));
        copy.Set(ZDOVar_INTERACTTEXT, original.GetString(ZDOVar_INTERACTTEXT));

        copy.Set(ZDOVar_GIVEITEM, original.GetString(ZDOVar_GIVEITEM));
        copy.Set(ZDOVar_GIVEITEMQUALITY, original.GetString(ZDOVar_GIVEITEMQUALITY));
        copy.Set(ZDOVar_GIVEITEMAMOUNT, original.GetInt(ZDOVar_GIVEITEMAMOUNT));

        copy.Set(ZDOVar_REWARDTEXT, original.GetString(ZDOVar_REWARDTEXT));
        copy.Set(ZDOVar_REWARDITEM, original.GetString(ZDOVar_REWARDITEM));
        copy.Set(ZDOVar_REWARDITEMQUALITY, original.GetString(ZDOVar_REWARDITEMQUALITY));
        copy.Set(ZDOVar_REWARDITEMAMOUNT, original.GetInt(ZDOVar_REWARDITEMAMOUNT));
        copy.Set(ZDOVar_REWARDLIMIT, original.GetInt(ZDOVar_REWARDLIMIT, -1)); // -1 is unlimited

        copy.Set(ZDOVar_REQUIREDKEYS, original.GetString(ZDOVar_REQUIREDKEYS));
        copy.Set(ZDOVar_NOTREQUIREDKEYS, original.GetString(ZDOVar_NOTREQUIREDKEYS));
        copy.Set(ZDOVar_INTERACTKEY, original.GetString(ZDOVar_INTERACTKEY));
        copy.Set(ZDOVar_REWARDKEY, original.GetString(ZDOVar_REWARDKEY));
        copy.Set(ZDOVar_DEFEATKEY, original.GetString(ZDOVar_DEFEATKEY));

        copy.Set(ZDOVar_TRUEDEATH, original.GetBool(ZDOVar_TRUEDEATH));
    }

    public static void CopyVisEquipment(ref VisEquipment copy, VisEquipment original)
    {
        copy.SetModel(original.m_nview.GetZDO().GetInt(ZDOVars.s_modelIndex));
        copy.SetSkinColor(original.m_nview.GetZDO().GetVec3(ZDOVars.s_skinColor, Vector3.zero));
        copy.SetHairColor(original.m_nview.GetZDO().GetVec3(ZDOVars.s_hairColor, Vector3.zero));

        foreach (int item in ZdoVisEquipment)
        {
            copy.m_nview.GetZDO().Set(item, original.m_nview.GetZDO().GetInt(item));
        }
    }

    private void Talk(string text)
    {
        if (Player.m_localPlayer != null && !text.IsNullOrWhiteSpace())
        {
            Chat.instance.SetNpcText(base.gameObject, Vector3.up * 2f, 15f, 30f, "", SetupText(text), true);
        }
    }

    private bool HasCorrectReqiuredKeys()
    {
        foreach (var key in NPCRequiredKeysSet)
        {
            if (!key.IsNullOrWhiteSpace() && !ZoneSystem.instance.GetGlobalKey(key))
            {
                return false;
            }
        }

        return true;
    }
    private bool HasCorrectNotReqiuredKeys()
    {
        foreach (var key in NPCNotRequiredKeysSet)
        {
            if (!key.IsNullOrWhiteSpace() && ZoneSystem.instance.GetGlobalKey(key))
            {
                return false;
            }
        }

        return true;
    }

    private bool HasCorrectKeys()
    {
        return HasCorrectReqiuredKeys() && HasCorrectNotReqiuredKeys();
    }

    private void GiveReward()
    {
        if (!NPCRewardItem.IsNullOrWhiteSpace())
        {
            var reward = ObjectDB.instance.GetItemPrefab(NPCRewardItem);

            if (reward != null)
            {
                var go = GameObject.Instantiate(reward,
                    transform.position + (transform.rotation * Vector3.forward),
                    transform.rotation);

                var itemdrop = go.GetComponent<ItemDrop>();
                itemdrop.SetStack(NPCRewardItemAmount);
                itemdrop.SetQuality(NPCRewardItemQuality);
            }
        }

        if (NPCRewardLimit > 0)
        {
            ClaimOwnership();
            m_nview.GetZDO().Set(ZDOVar_REWARDLIMIT, NPCRewardLimit - 1);
        }

        if (!NPCRewardKey.IsNullOrWhiteSpace())
        {
            SetKey(NPCRewardKey);
        }
    }

    #endregion

    #region Style and Setup

    private void ClaimOwnership()
    {
        if (!m_nview.IsOwner())
        {
            m_nview.ClaimOwnership();
        }
    }

    public void SetFromConfig(NPCConfiguration.NPCConfig config)
    {
        if (config == null)
        {
            return;
        }

        ClaimOwnership();

        try
        {
            UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
            SetName(config.Name);
            SetTrueDeath(config.TrueDeath);

            if (config.StandStill)
            {
                NPCSPlugin.NPCSLogger.LogDebug($"{config.Name} has no random movement.");
                AttachStart();
            }

            m_nview.GetZDO().Set(ZDOVar_NPCType, (int)config.Type);
            m_nview.GetZDO().Set(ZDOVar_DEFAULTTEXT, config.DefaultText);
            m_nview.GetZDO().Set(ZDOVar_INTERACTTEXT, config.InteractText);

            m_nview.GetZDO().Set(ZDOVar_GIVEITEM, config.GiveItem);
            m_nview.GetZDO().Set(ZDOVar_GIVEITEMQUALITY, config.GiveItemQuality.Value);
            m_nview.GetZDO().Set(ZDOVar_GIVEITEMAMOUNT, config.GiveItemAmount.Value);

            m_nview.GetZDO().Set(ZDOVar_REWARDTEXT, config.RewardText);
            m_nview.GetZDO().Set(ZDOVar_REWARDITEM, config.RewardItem);
            m_nview.GetZDO().Set(ZDOVar_REWARDITEMQUALITY, config.RewardItemQuality.Value);
            m_nview.GetZDO().Set(ZDOVar_REWARDITEMAMOUNT, config.RewardItemAmount.Value);
            m_nview.GetZDO().Set(ZDOVar_REWARDLIMIT, config.RewardLimit.Value);

            NPCRequiredKeysSet = Utility.StringToSet(config.RequiredKeys);
            m_nview.GetZDO().Set(ZDOVar_REQUIREDKEYS, config.RequiredKeys);
            NPCNotRequiredKeysSet = Utility.StringToSet(config.NotRequiredKeys);
            m_nview.GetZDO().Set(ZDOVar_NOTREQUIREDKEYS, config.NotRequiredKeys);
            m_nview.GetZDO().Set(ZDOVar_INTERACTKEY, config.InteractKey);
            m_nview.GetZDO().Set(ZDOVar_REWARDKEY, config.RewardKey);
            m_nview.GetZDO().Set(ZDOVar_DEFEATKEY, config.DefeatKey);

            // Style

            if (config.ModelIndex.HasValue)
            {
                SetModel(config.ModelIndex.Value);
            }

            // TODO: check setting random skin/hair color here works for other models

            if (config.SkinColorR.HasValue &&
                config.SkinColorG.HasValue &&
                config.SkinColorB.HasValue)
            {
                SetSkinColor(config.SkinColorR.Value, config.SkinColorG.Value, config.SkinColorB.Value);
            }
            else
            {
                SetSkinColor(GetRandomSkinColor());
            }

            if (config.HairColorR.HasValue &&
                config.HairColorG.HasValue &&
                config.HairColorB.HasValue)
            {
                SetHairColor(config.HairColorR.Value, config.HairColorG.Value, config.HairColorB.Value);
            }
            else
            {
                SetHairColor(GetRandomHairColor());
            }

            SetNPCHair(config.Hair);
            SetNPCBeard(config.Beard);
            SetHelmet(config.Helmet);
            SetChest(config.Chest);
            SetLegs(config.Legs);
            SetShoulder(config.Shoulder, config.ShoulderVariant.Value);
            SetLeftHand(config.LeftHand, config.LeftHandVariant.Value);
            SetRightHand(config.RightHand);
            SetBackLeft(config.LeftBack, config.LeftBackVariant.Value);
            SetBackRight(config.RightBack);
        }
        catch (Exception e)
        {
            NPCSPlugin.NPCSLogger.LogError("There was an issue spawing the npc from configurations, " +
                "did you forget to reload the file?");
            NPCSPlugin.NPCSLogger.LogWarning(e);
        }
    }

    public void SetName(string name)
    {
        ClaimOwnership();
        m_nview.GetZDO().Set(ZDOVars.s_tamedName, name);
        m_name = name;
    }

    public void SetSpawnPoint(Vector3 position)
    {
        ClaimOwnership();
        m_nview.GetZDO().Set(ZDOVar_SPAWNPOINT, position);
    }

    public void SetTrueDeath(bool death)
    {
        ClaimOwnership();
        m_nview.GetZDO().Set(ZDOVar_TRUEDEATH, death);
    }

    public void SetRandom()
    {
        ClaimOwnership();
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        // Model
        var index = UnityEngine.Random.Range(0, 2);
        SetModel(index);

        // Skin
        SetSkinColor(GetRandomSkinColor());

        // Hair
        var hair = UnityEngine.Random.Range(1, 32);
        SetNPCHair("Hair" + hair);

        SetHairColor(GetRandomHairColor());

        if (index == 0)
        {
            // Male
            SetRandomMale();
        }
        else if (index == 1)
        {
            // Female
            SetRandomFemale();
        }

        m_visEquipment.UpdateEquipmentVisuals();
    }

    protected void SetRandomMale()
    {
        var beard = UnityEngine.Random.Range(1, 30);
        if (beard <= 21)
        {
            SetNPCBeard("Beard" + beard);
        }

        var helmet = UnityEngine.Random.Range(1, 25);
        if (helmet <= 10)
        {
            SetHelmet("HelmetHat" + helmet);
        }
        else if (helmet < 16)
        {
            SetHelmet("HelmetLeather");
        }

        var chest = UnityEngine.Random.Range(1, 15);
        if (chest > 10)
        {
            SetChest("ArmorLeatherChest");
        }
        else
        {
            SetChest("ArmorTunic" + chest);
        }

        var legs = UnityEngine.Random.Range(0, 15);
        if (legs < 10)
        {
            SetLegs("ArmorLeatherLegs");
        }
        else
        {
            SetLegs("ArmorRagsLegs");
        }
    }

    protected void SetRandomFemale()
    {
        SetNPCBeard("");
        var helmet = UnityEngine.Random.Range(1, 25);
        if (helmet <= 10)
        {
            SetHelmet("HelmetHat" + helmet);
        }
        else if (helmet < 17)
        {
            SetHelmet("HelmetMidsummerCrown");
        }

        var chest = UnityEngine.Random.Range(1, 15);
        if (chest <= 10)
        {
            SetChest("ArmorDress" + chest);
        }
        else
        {
            SetChest("ArmorLeatherChest");
        }

        var legs = UnityEngine.Random.Range(0, 15);
        if (legs < 5)
        {
            SetLegs("ArmorLeatherLegs");
        }
        else if (legs < 8)
        {
            SetLegs("ArmorRagsLegs");
        }
    }

    protected void SetNPCHair(string name)
    {
        m_hairItem = name;
        m_visEquipment.SetHairItem(name);
    }

    protected void SetHairColor(Color color)
    {
        SetHairColor(color.r, color.g, color.b);
    }

    protected void SetHairColor(float r, float g, float b)
    {
        var color = new Vector3(r, g, b);
        m_visEquipment.SetHairColor(color);
    }

    protected void SetSkinColor(Color color)
    {
        SetSkinColor(color.r, color.g, color.b);
    }

    protected void SetSkinColor(float r, float g, float b)
    {
        var color = new Vector3(r, g, b);
        m_visEquipment.SetSkinColor(color);
    }

    protected void SetNPCBeard(string name)
    {
        m_beardItem = name;
        m_visEquipment.SetBeardItem(name);
    }

    protected void SetModel(int index)
    {
        m_visEquipment.SetModel(index);
    }

    protected void SetHelmet(string name)
    {
        m_visEquipment.SetHelmetItem(name);
        if (Utility.GetItemDrop(name, out var item) && !m_inventory.ContainsItem(item.m_itemData))
        {
            m_helmetItem = item.m_itemData;
            m_inventory.AddItem(item.m_itemData);
        }
    }

    protected void SetChest(string name)
    {
        m_visEquipment.SetChestItem(name);
        if (Utility.GetItemDrop(name, out var item) && !m_inventory.ContainsItem(item.m_itemData))
        {
            m_chestItem = item.m_itemData;
            m_inventory.AddItem(item.m_itemData);
        }
    }

    protected void SetLegs(string name)
    {
        m_visEquipment.SetLegItem(name);
        if (Utility.GetItemDrop(name, out var item) && !m_inventory.ContainsItem(item.m_itemData))
        {
            m_legItem = item.m_itemData;
            m_inventory.AddItem(item.m_itemData);
        }
    }

    protected void SetShoulder(string name, int variant = 0)
    {
        m_visEquipment.SetShoulderItem(name, variant);
        if (Utility.GetItemDrop(name, out var item) && !m_inventory.ContainsItem(item.m_itemData))
        {
            var itemdata = item.m_itemData;
            itemdata.m_variant = variant;
            m_shoulderItem = itemdata;
            m_inventory.AddItem(itemdata);
        }
    }

    protected void SetUtility(string name)
    {
        m_visEquipment.SetUtilityItem(name);
        if (Utility.GetItemDrop(name, out var item) && !m_inventory.ContainsItem(item.m_itemData))
        {
            m_utilityItem = item.m_itemData;
            m_inventory.AddItem(item.m_itemData);
        }
    }

    protected void SetRightHand(string name)
    {
        m_visEquipment.SetRightItem(name);
        if (Utility.GetItemDrop(name, out var item) && !m_inventory.ContainsItem(item.m_itemData))
        {
            m_rightItem = item.m_itemData;
            m_inventory.AddItem(item.m_itemData);
        }
    }

    protected void SetLeftHand(string name, int variant = 0)
    {
        m_visEquipment.SetLeftItem(name, variant);
        if (Utility.GetItemDrop(name, out var item) && !m_inventory.ContainsItem(item.m_itemData))
        {
            var itemdata = item.m_itemData;
            itemdata.m_variant = variant;
            m_leftItem = itemdata;
            m_inventory.AddItem(itemdata);
        }
    }

    protected void SetBackRight(string name)
    {
        // TODO test
        m_visEquipment.SetRightBackItem(name);
        if (Utility.GetItemDrop(name, out var item) && !m_inventory.ContainsItem(item.m_itemData))
        {
            m_rightItem = item.m_itemData;
            m_inventory.AddItem(item.m_itemData);
        }
    }

    protected void SetBackLeft(string name, int variant = 0)
    {
        // TODO test
        m_visEquipment.SetLeftBackItem(name, variant);
        if (Utility.GetItemDrop(name, out var item) && !m_inventory.ContainsItem(item.m_itemData))
        {
            var itemdata = item.m_itemData;
            itemdata.m_variant = variant;
            m_inventory.AddItem(itemdata);
        }
    }

    protected Color GetRandomSkinColor()
    {
        var skintone = UnityEngine.Random.Range(0f, 1f);
        Color skinColor = Color.Lerp(_skinColorMin, _skinColorMax, skintone);
        return skinColor;
    }

    protected Color GetRandomHairColor()
    {
        var hairtone = UnityEngine.Random.Range(0f, 1f);
        var hairlevel = UnityEngine.Random.Range(0f, 1f);
        Color hairColor = Color.Lerp(_hairColorMin, _hairColorMax, hairtone) *
            Mathf.Lerp(0.1f, 1f, hairlevel);
        return hairColor;
    }

    public string GetSkinColor()
    {
        var color = m_nview.GetZDO().GetVec3(ZDOVars.s_skinColor, Vector3.zero);
        return $"Skin Color RGB: {color.x}, {color.y}, {color.z}";
    }

    public string GetHairColor()
    {
        var color = m_nview.GetZDO().GetVec3(ZDOVars.s_hairColor, Vector3.zero);
        return $"Hair Color RGB: {color.x}, {color.y}, {color.z}";
    }

    #endregion
}
