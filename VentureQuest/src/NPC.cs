using System;
using System.Collections;
using BepInEx;
using UnityEngine;

namespace VentureValheim.VentureQuest;

// TODO localization
public class NPC : Humanoid, Interactable, Hoverable
{
    public enum NPCType
    {
        None = 0,
        Information = 1,
        InformationKey = 2,
        RewardItem = 10,
        RewardKey = 11,
        Sellsword = 20,
        SlayTarget = 30
    }

    public const string ZDOVar_NPCType = "VV_NPCType";
    public int Type => m_nview.GetZDO().GetInt(ZDOVar_NPCType);
    public const string ZDOVar_SITTING = "VV_Sitting";
    public bool WasSitting;
    public const string ZDOVar_SPAWNPOINT = "VV_SpawnPoint";
    public Vector3 SpawnPoint => m_nview.GetZDO().GetVec3(ZDOVar_SPAWNPOINT, Vector3.zero);

    // Main Text
    public const string ZDOVar_DEFAULTTEXT = "VV_DefaultText";
    public string DefaultText => m_nview.GetZDO().GetString(ZDOVar_DEFAULTTEXT);
    // Use Item
    public const string ZDOVar_USEITEMTEXT = "VV_UseItemText";
    public string NPCUseItemText => m_nview.GetZDO().GetString(ZDOVar_USEITEMTEXT);
    public const string ZDOVar_USEITEM = "VV_UseItem";
    public string NPCUseItem => m_nview.GetZDO().GetString(ZDOVar_USEITEM);
    public const string ZDOVar_USEITEMAMOUNT = "VV_UseItemAmount";
    public int NPCUseItemAmount => m_nview.GetZDO().GetInt(ZDOVar_USEITEMAMOUNT);
    public const string ZDOVar_USEITEMLIMIT = "VV_UseItemLimit";
    public int NPCUseItemLimit => m_nview.GetZDO().GetInt(ZDOVar_USEITEMLIMIT);
    public const string ZDOVar_USEITEMKEY = "VV_UseItemKey";
    public string NPCUseItemKey => m_nview.GetZDO().GetString(ZDOVar_USEITEMKEY);
    // Reward
    public const string ZDOVar_REWARDTEXT = "VV_RewardText";
    public string NPCRewardText => m_nview.GetZDO().GetString(ZDOVar_REWARDTEXT);
    public const string ZDOVar_REWARDITEM = "VV_RewardItem";
    public string NPCRewardItem => m_nview.GetZDO().GetString(ZDOVar_REWARDITEM);
    public const string ZDOVar_REWARDITEMAMOUNT = "VV_RewardItemAmount";
    public int NPCRewardItemAmount => m_nview.GetZDO().GetInt(ZDOVar_REWARDITEMAMOUNT);
    public const string ZDOVar_REWARDKEY = "VV_RewardKey";
    public string NPCRewardKey => m_nview.GetZDO().GetString(ZDOVar_REWARDKEY);
    // Global Key
    public const string ZDOVar_GLOBALKEY = "VV_GlobalKey";
    public string NPCGlobalKey => m_nview.GetZDO().GetString(ZDOVar_GLOBALKEY);

    public bool HasAttach = false;
    public Transform AttachPoint;
    public string AttachAnimation;
    public GameObject AttachRoot;
    public Collider[] AttachColliders;

    private readonly Color32 _hairColorMin = new Color32(0xFF, 0xED, 0xB4, 0xFF);
    private readonly Color32 _hairColorMax = new Color32(0xFF, 0x7C, 0x47, 0xFF);
    private readonly Color32 _skinColorMin = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
    private readonly Color32 _skinColorMax = new Color32(0x4C, 0x4C, 0x4C, 0xFF);

    //private IEnumerator startCoroutine;

    public override void Awake()
    {
        VentureQuestPlugin.VentureQuestLogger.LogInfo("NPC Awake!");
        base.Awake();

        var startCoroutine = SetUp();
        StartCoroutine(startCoroutine);
    }

    /*public override void Start()
    {
        
    }*/

    public IEnumerator SetUp()
    {
        VentureQuestPlugin.VentureQuestLogger.LogInfo("NPC SetUp!");
        yield return new WaitForSeconds(1);
        yield return null;

        m_name = m_nview.GetZDO().GetString(ZDOVars.s_tamedName);
        WasSitting = m_nview.GetZDO().GetBool(ZDOVar_SITTING);

        if (WasSitting)
        {
            var chair = GetClosestChair();
            if (chair != null)
            {
                AttachStart(chair);
            }
        }

        if (Type == (int)NPCType.SlayTarget)
        {
            m_defeatSetGlobalKey = m_nview.GetZDO().GetString(ZDOVar_GLOBALKEY);
        }

        VentureQuestPlugin.VentureQuestLogger.LogDebug("NPC Awoken");
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
            if (AttachPoint == null)
            {
                // TODO allow players to request npcs get out of chairs
                AttachStop();
            }
            else
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
        }
    }

    public override void OnDeath()
    {
        VentureQuestPlugin.VentureQuestLogger.LogInfo($"Death! Tragity!");

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

                VisEquipment visEquip = ragdoll.GetComponent<VisEquipment>();
                if (visEquip != null)
                {
                    CopyVisEquipment(ref visEquip, m_visEquipment);
                }
            }
        }

        SetKey(m_defeatSetGlobalKey);

        if (m_onDeath != null)
        {
            m_onDeath();
        }

        NPCFactory.RespawnNPC(this.gameObject);

        ZNetScene.instance.Destroy(base.gameObject);
    }

    private void SetKey(string key)
    {
        /*if (!string.IsNullOrEmpty(key))
        {
            Player.m_addUniqueKeyQueue.Add(key);
        }*/
        if (!string.IsNullOrEmpty(key))
        {
            ZoneSystem.instance.SetGlobalKey(key);
        }
    }

    public static void CopyZDO(ref ZDO copy, ZDO original)
    {
        copy.Set(ZDOVars.s_tamedName, original.GetString(ZDOVars.s_tamedName));
        copy.Set(ZDOVar_NPCType, original.GetInt(ZDOVar_NPCType));
        copy.Set(ZDOVar_NPCType, original.GetVec3(ZDOVar_SPAWNPOINT, Vector3.zero));
        copy.Set(ZDOVar_DEFAULTTEXT, original.GetString(ZDOVar_DEFAULTTEXT));

        copy.Set(ZDOVar_USEITEMTEXT, original.GetString(ZDOVar_USEITEMTEXT));
        copy.Set(ZDOVar_USEITEM, original.GetString(ZDOVar_USEITEM));
        copy.Set(ZDOVar_USEITEMAMOUNT, original.GetInt(ZDOVar_USEITEMAMOUNT));
        copy.Set(ZDOVar_USEITEMLIMIT, original.GetInt(ZDOVar_USEITEMLIMIT, -1)); // -1 is unlimited
        copy.Set(ZDOVar_USEITEMKEY, original.GetString(ZDOVar_USEITEMKEY));

        copy.Set(ZDOVar_REWARDTEXT, original.GetString(ZDOVar_REWARDTEXT));
        copy.Set(ZDOVar_REWARDITEM, original.GetString(ZDOVar_REWARDITEM));
        copy.Set(ZDOVar_REWARDITEMAMOUNT, original.GetInt(ZDOVar_REWARDITEMAMOUNT));
        copy.Set(ZDOVar_REWARDKEY, original.GetString(ZDOVar_REWARDKEY));

        copy.Set(ZDOVar_GLOBALKEY, original.GetString(ZDOVar_GLOBALKEY));
        //copy = original.Clone();
    }

    public static void CopyVisEquipment(ref VisEquipment copy, VisEquipment original)
    {
        copy.SetModel(original.m_nview.GetZDO().GetInt(ZDOVars.s_modelIndex));
        copy.SetSkinColor(original.m_nview.GetZDO().GetVec3(ZDOVars.s_skinColor, Vector3.zero));
        copy.SetHairColor(original.m_nview.GetZDO().GetVec3(ZDOVars.s_hairColor, Vector3.zero));
        copy.m_nview.GetZDO().Set(ZDOVars.s_beardItem, original.m_nview.GetZDO().GetInt(ZDOVars.s_beardItem));
        copy.m_nview.GetZDO().Set(ZDOVars.s_hairItem, original.m_nview.GetZDO().GetInt(ZDOVars.s_hairItem));
        copy.m_nview.GetZDO().Set(ZDOVars.s_helmetItem, original.m_nview.GetZDO().GetInt(ZDOVars.s_helmetItem));
        copy.m_nview.GetZDO().Set(ZDOVars.s_chestItem, original.m_nview.GetZDO().GetInt(ZDOVars.s_chestItem));
        copy.m_nview.GetZDO().Set(ZDOVars.s_legItem, original.m_nview.GetZDO().GetInt(ZDOVars.s_legItem));
        copy.m_nview.GetZDO().Set(ZDOVars.s_shoulderItem, original.m_nview.GetZDO().GetInt(ZDOVars.s_shoulderItem));
        //copy.m_nview.GetZDO().Set(ZDOVars.s_variant, original.m_nview.GetZDO().GetInt(ZDOVars.s_variant));
        // TODO fix variants
    }

    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        if (Type == (int)NPCType.InformationKey)
        {
            SetKey(NPCGlobalKey);
        }

        if (Type == (int)NPCType.RewardItem || Type == (int)NPCType.RewardKey)
        {
            Talk(SetupText(NPCUseItemText));
        }
        else
        {
            Talk(SetupText(DefaultText));
        }


        return false;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        // TODO: test

        if (Type != (int)NPCType.RewardItem && Type != (int)NPCType.RewardKey)
        {
            return false;
        }

        int useItemLimit = NPCUseItemLimit;

        if (useItemLimit == 0 ||
            (!NPCUseItemKey.IsNullOrWhiteSpace() && !ZoneSystem.instance.GetGlobalKey(NPCUseItemKey)))
        {
            // TODO, prevent this from triggering ever
            Talk("Sorry, that's not availible right now!");
            return true;
        }
        else if (item != null && item.m_dropPrefab.name.Equals(NPCUseItem) && Player.m_localPlayer != null)
        {
            var player = Player.m_localPlayer;
            var count = player.GetInventory().CountItems(item.m_shared.m_name);
            VentureQuestPlugin.VentureQuestLogger.LogDebug($"Found {count} of the item");
            if (count >= NPCUseItemAmount)
            {
                player.UnequipItem(item);
                player.GetInventory().RemoveItem(item, NPCUseItemAmount);

                // TODO allow item reward and key reward at same time
                if (Type == (int)NPCType.RewardKey)
                {
                    SetKey(NPCGlobalKey);
                }
                else
                {
                    var reward = ObjectDB.instance.GetItemPrefab(NPCRewardItem);

                    if (reward != null)
                    {
                        var go = GameObject.Instantiate(reward,
                            transform.position + (transform.rotation * Vector3.forward),
                            transform.rotation);

                        var itemdrop = go.GetComponent<ItemDrop>();
                        itemdrop.SetStack(NPCRewardItemAmount);
                    }
                }

                Talk(SetupText(NPCRewardText));

                if (useItemLimit > 0)
                {
                    useItemLimit--;

                    if (!m_nview.IsOwner())
                    {
                        m_nview.ClaimOwnership();
                    }

                    m_nview.GetZDO().Set(ZDOVar_USEITEMLIMIT, useItemLimit);

                    // TODO change npc type when rewards run out?
                }

                return true;
            }
            else
            {
                Talk("Hmmm that's not enough... " + SetupText(NPCUseItemText));
            }
        }
        else
        {
            Talk("Hmmm that's not right... " + SetupText(NPCUseItemText));
        }
        

        return false;
    }

    public override string GetHoverText()
    {
        var type = m_nview.GetZDO().GetInt(ZDOVar_NPCType);
        string text = "";
        if (type == (int)NPCType.Information || type == (int)NPCType.RewardItem || type == (int)NPCType.Sellsword)
        {
            text = Localization.instance.Localize(
                "[<color=yellow><b>$KEY_Use</b></color>] Talk");
        }

        if (type == (int)NPCType.RewardItem)
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

    private string SetupText(string text)
    {
        string useItemText = NPCUseItemAmount > 1 ? $"{NPCUseItemAmount} {NPCUseItem}" : $"{NPCUseItem}";
        string rewardItemText = $"{NPCRewardItemAmount} {NPCRewardItem}";

        text = text.Replace("{useitem}", useItemText).Replace("{reward}", rewardItemText);
        return text;
    }

    private void Talk(string text)
    {
        if (Player.m_localPlayer != null && !text.IsNullOrWhiteSpace())
        {
            Chat.instance.SetNpcText(base.gameObject, Vector3.up * 2f, 10f, 30f, "", text, true);
        }
    }

    // TODO make sure setters update character live

    private void ClaimOwnership()
    {
        if (!m_nview.IsOwner())
        {
            m_nview.ClaimOwnership();
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

    public void SetTypeInformation(string text, string key = "")
    {
        ClaimOwnership();

        m_nview.GetZDO().Set(ZDOVar_NPCType,
            key.IsNullOrWhiteSpace() ? (int)NPCType.Information : (int)NPCType.InformationKey);
        m_nview.GetZDO().Set(ZDOVar_GLOBALKEY, key);
        m_nview.GetZDO().Set(ZDOVar_DEFAULTTEXT, text);
    }

    public void SetTypeRewardKey(string defaultText, string itemText, string item, int amount,
        string rewardText, string rewardKey, string useKey = "", int limit = -1)
    {
        ClaimOwnership();

        m_nview.GetZDO().Set(ZDOVar_NPCType, (int)NPCType.RewardKey);
        m_nview.GetZDO().Set(ZDOVar_DEFAULTTEXT, defaultText);

        m_nview.GetZDO().Set(ZDOVar_USEITEMTEXT, itemText);
        m_nview.GetZDO().Set(ZDOVar_USEITEM, item);
        m_nview.GetZDO().Set(ZDOVar_USEITEMAMOUNT, amount);
        m_nview.GetZDO().Set(ZDOVar_USEITEMLIMIT, limit);
        m_nview.GetZDO().Set(ZDOVar_USEITEMKEY, useKey);

        m_nview.GetZDO().Set(ZDOVar_REWARDTEXT, rewardText);
        m_nview.GetZDO().Set(ZDOVar_GLOBALKEY, rewardKey);
    }

    public void SetTypeRewardItem(string defaultText, string itemText, string item, int amount,
         string rewardText, string reward, int rewardAmount, string useKey = "", string rewardKey = "", int limit = -1)
    {
        ClaimOwnership();

        m_nview.GetZDO().Set(ZDOVar_NPCType, (int)NPCType.RewardItem);
        m_nview.GetZDO().Set(ZDOVar_DEFAULTTEXT, defaultText);

        m_nview.GetZDO().Set(ZDOVar_USEITEMTEXT, itemText);
        m_nview.GetZDO().Set(ZDOVar_USEITEM, item);
        m_nview.GetZDO().Set(ZDOVar_USEITEMAMOUNT, amount);
        m_nview.GetZDO().Set(ZDOVar_USEITEMLIMIT, limit);
        m_nview.GetZDO().Set(ZDOVar_USEITEMKEY, useKey);

        m_nview.GetZDO().Set(ZDOVar_REWARDTEXT, rewardText);
        m_nview.GetZDO().Set(ZDOVar_REWARDITEM, reward);
        m_nview.GetZDO().Set(ZDOVar_REWARDITEMAMOUNT, rewardAmount);
        m_nview.GetZDO().Set(ZDOVar_REWARDKEY, rewardKey);
    }

    public void SetTypeSellSword()
    {
        // TODO
    }

    public void SetTypeSlayTarget()
    {
        // TODO
    }

    public Chair GetClosestChair()
    {
        var pos = base.transform.position;
        Collider[] hits = Physics.OverlapBox(pos, base.transform.localScale / 2, Quaternion.identity);
        Chair closestChair = null;

        foreach (var hit in hits)
        {
            var chairs = hit.transform.root.gameObject.GetComponentsInChildren<Chair>();
            if (chairs != null)
            {
                for (int lcv = 0; lcv < chairs.Length; lcv++)
                {
                    var chair = chairs[lcv];
                    if (closestChair == null || (Vector3.Distance(pos, chair.transform.position) <
                        Vector3.Distance(pos, closestChair.transform.position)))
                    {
                        closestChair = chair;
                    }
                }
            }
        }

        return closestChair;
    }

    public void SetRandom()
    {
        ClaimOwnership();
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        // Model
        var index = UnityEngine.Random.Range(0, 2);
        SetModel(index);

        // Skin
        var skintone = UnityEngine.Random.Range(0f, 1f);
        Color skinColor = Color.Lerp(_skinColorMin, _skinColorMax, skintone);
        SetSkinColor(skinColor);

        // Hair
        var hair = UnityEngine.Random.Range(1, 32);
        SetNPCHair("Hair" + hair);

        var hairtone= UnityEngine.Random.Range(0f, 1f);
        var hairlevel = UnityEngine.Random.Range(0f, 1f);
        Color hairColor = Color.Lerp(_hairColorMin, _hairColorMax, hairtone) *
            Mathf.Lerp(0.1f, 1f, hairlevel);
        SetHairColor(hairColor);

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

    public void SetRandomMale()
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
            SetHelmet("ArmorLeatherHelmet");
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

    public void SetRandomFemale()
    {
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
        if (chest > 10)
        {
            SetChest("ArmorLeatherChest");
        }
        else
        {
            SetChest("ArmorDress" + chest);
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
    }

    protected void SetChest(string name)
    {
        m_visEquipment.SetChestItem(name);
    }

    protected void SetLegs(string name)
    {
        m_visEquipment.SetLegItem(name);
    }

    protected void SetShoulder(string name, int variant = 0)
    {
        m_visEquipment.SetShoulderItem(name, variant);
    }

    protected void SetUtility(string name)
    {
        m_visEquipment.SetUtilityItem(name);
    }

    protected void SetRightHand(string name)
    {
        m_visEquipment.SetRightItem(name);
    }

    protected void SetLeftHand(string name, int variant = 0)
    {
        m_visEquipment.SetLeftItem(name, variant);
    }

    protected void SetBackRight(string name)
    {
        m_visEquipment.SetRightBackItem(name);
    }

    protected void SetBackLeft(string name, int variant = 0)
    {
        m_visEquipment.SetLeftBackItem(name, variant);
    }

    public override void SetupVisEquipment(VisEquipment visEq, bool isRagdoll)
    {
        // Do nothing, the values for these npcs are not populated
    }

    /*private void ListVisEquipment()
    {
        if (m_visEquipment == null || m_nview == null || m_nview.GetZDO() == null)
        {
            return;
        }

        VentureQuestPlugin.VentureQuestLogger.LogInfo($"ListVisEquipment... \n" +
            $"model: {m_visEquipment.m_modelIndex}, {m_nview.GetZDO().GetInt(ZDOVars.s_modelIndex)} \n" +
            $"skinColor: {m_visEquipment.m_skinColor}, {m_nview.GetZDO().GetVec3(ZDOVars.s_skinColor, Vector3.zero)} \n" +
            $"hairColor: {m_visEquipment.m_hairColor}, {m_nview.GetZDO().GetVec3(ZDOVars.s_hairColor, Vector3.zero)} \n" +
            $"beardItem: {m_visEquipment.m_beardItem}, {m_beardItem} \n" +
            $"hairItem: {m_visEquipment.m_hairItem}, {m_hairItem} \n" +
            $"helmetItem: {m_visEquipment.m_helmetItem}, {m_helmetItem?.m_shared?.m_name} \n" +
            $"chestItem: {m_visEquipment.m_chestItem}, {m_chestItem?.m_shared?.m_name} \n" +
            $"legItem: {m_visEquipment.m_legItem}, {m_legItem?.m_shared?.m_name} \n" +
            $"shoulderItemVariant: {m_visEquipment.m_shoulderItemVariant}, {m_shoulderItem?.m_variant} \n" +
            $"shoulderItem: {m_visEquipment.m_shoulderItem}, {m_shoulderItem?.m_shared?.m_name} \n");
    }*/

    public override bool IsAttached()
    {
        return HasAttach || base.IsAttached();
    }

    public void AttachStart(Chair chair)
    {
        VentureQuestPlugin.VentureQuestLogger.LogDebug("Starting");

        HasAttach = true;
        AttachPoint = chair.m_attachPoint;
        AttachAnimation = chair.m_attachAnimation;
        AttachRoot = chair.transform.root.gameObject;

        base.transform.position = AttachPoint.position;
        base.transform.rotation = AttachPoint.rotation;

        m_zanim.SetBool(AttachAnimation, value: true);
        m_nview.GetZDO().Set(ZDOVar_SITTING, value: true);

        m_body.mass = 1000;

        Rigidbody componentInParent = AttachPoint.GetComponentInParent<Rigidbody>();
        m_body.useGravity = false;
        m_body.velocity = (componentInParent ? 
            componentInParent.GetPointVelocity(base.transform.position) : Vector3.zero);
        m_body.angularVelocity = Vector3.zero;
        m_maxAirAltitude = base.transform.position.y;

        // TODO, fix chair jitters
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
        VentureQuestPlugin.VentureQuestLogger.LogDebug("Stopping");
        if (!HasAttach)
        {
            return;
        }

        m_nview.GetZDO().Set(ZDOVar_SITTING, value: false);

        HasAttach = false;
        AttachAnimation = "";
        AttachPoint = null;
        m_body.useGravity = true;
        m_body.mass = 10; // TODO, set to original

        Collider[] attachColliders = AttachColliders;
        foreach (Collider collider in attachColliders)
        {
            Physics.IgnoreCollision(m_collider, collider, ignore: false);
        }
        AttachColliders = null;
        AttachRoot = null;

        m_zanim.SetBool(AttachAnimation, value: false);
        ResetCloth();
    }
}



/*Type = m_nview.GetZDO().GetInt(ZDOVar_NPCType);
SpawnPoint = m_nview.GetZDO().GetVec3(ZDOVar_SPAWNPOINT, Vector3.zero);
DefaultText = m_nview.GetZDO().GetString(ZDOVar_DEFAULTTEXT);

NPCUseItemText = m_nview.GetZDO().GetString(ZDOVar_USEITEMTEXT);
NPCUseItem = m_nview.GetZDO().GetString(ZDOVar_USEITEM);
NPCUseItemAmount = m_nview.GetZDO().GetInt(ZDOVar_USEITEMAMOUNT);
NPCUseItemLimit = m_nview.GetZDO().GetInt(ZDOVar_USEITEMLIMIT, -1); // -1 is unlimited
NPCUseItemKey = m_nview.GetZDO().GetString(ZDOVar_USEITEMKEY);

NPCRewardText = m_nview.GetZDO().GetString(ZDOVar_REWARDTEXT);
NPCRewardItem = m_nview.GetZDO().GetString(ZDOVar_REWARDITEM);
NPCRewardItemAmount = m_nview.GetZDO().GetInt(ZDOVar_REWARDITEMAMOUNT);
NPCRewardKey = m_nview.GetZDO().GetString(ZDOVar_REWARDKEY);

NPCGlobalKey = m_nview.GetZDO().GetString(ZDOVar_GLOBALKEY);*/


/*try
{
    m_hairItem = m_nview.GetZDO().GetString(ZDOVars.s_hairItem);
    m_beardItem = m_nview.GetZDO().GetString(ZDOVars.s_beardItem);
    Patches.GetItemDrop(m_nview.GetZDO().GetString(ZDOVars.s_helmetItem), out var helmet);
    m_helmetItem = helmet?.m_itemData;
    Patches.GetItemDrop(m_nview.GetZDO().GetString(ZDOVars.s_chestItem), out var chest);
    m_chestItem = chest?.m_itemData;
    Patches.GetItemDrop(m_nview.GetZDO().GetString(ZDOVars.s_legItem), out var legs);
    m_legItem = legs?.m_itemData;
    Patches.GetItemDrop(m_nview.GetZDO().GetString(ZDOVars.s_shoulderItem), out var shoulder);
    m_shoulderItem = shoulder?.m_itemData;
    m_shoulderItem.m_variant = m_nview.GetZDO().GetInt(ZDOVars.s_shoulderItemVariant);
    // TODO finish
}
catch
{
    VentureQuestPlugin.VentureQuestLogger.LogDebug("Failure.");
}*/