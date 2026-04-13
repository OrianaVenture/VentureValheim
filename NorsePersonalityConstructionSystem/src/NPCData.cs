using BepInEx;
using System;
using System.Xml.Linq;
using UnityEngine;

namespace VentureValheim.NPCS;

public class NPCData
{
    private Character _character;

    private bool _newSpawn = false;
    private NPCQuest[] _quests;
    private int _questIndex = -1;
    private bool _questsInitialized = false;

    private NPCQuest _currentQuest = null;

    public bool HasAttach = false;
    public Vector3 AttachPosition = Vector3.zero;
    public Quaternion AttachRotation = Quaternion.identity;
    public string AttachAnimation = "";
    public GameObject AttachRoot;
    public Collider[] AttachColliders;

    public enum NPCType
    {
        None = 0,
        Information = 1, // Legacy
        Reward = 2, // Legacy
        Sellsword = 3,
        SlayTarget = 4,
        Trader = 5,
        Quest = 6
    }

    public enum NPCKeyType
    {
        Player = 0,
        Global = 1
    }

    public const string NPCGROUP = "VV_NPC";

    public static readonly Color32 HairColorMin = new Color32(0xFF, 0xED, 0xB4, 0xFF);
    public static readonly Color32 HairColorMax = new Color32(0xFF, 0x7C, 0x47, 0xFF);
    public static readonly Color32 SkinColorMin = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
    public static readonly Color32 SkinColorMax = new Color32(0x4C, 0x4C, 0x4C, 0xFF);

    public NPCData(Character character)
    {
        _character = character;
    }

    public void Setup()
    {
        ZDO zdo = _character.m_nview.GetZDO();
        NPCZDOUtils.UpgradeVersion(ref zdo);

        _character.m_name = NPCZDOUtils.GetTamedName(zdo);
        _character.m_defeatSetGlobalKey = NPCZDOUtils.GetNPCDefeatKey(zdo);

        // TODO: fixup for tamed
        _character.m_nview.GetZDO().Set(ZDOVars.s_tamed, false);
        _character.m_tamed = false;

        if (!UpdateTrader())
        {
            // Setup talker for all non-trader npcs
            UpdateTalker();
        }

        if (_character is NPCHumanoid)
        {
            var humanoid = _character as Humanoid;
            if (!_newSpawn && humanoid.m_visEquipment != null && humanoid.m_visEquipment.m_nview != null)
            {
                SetHelmet(zdo.GetInt(ZDOVars.s_helmetItem));
                SetChest(zdo.GetInt(ZDOVars.s_chestItem));
                SetLegs(zdo.GetInt(ZDOVars.s_legItem));
                SetShoulder(zdo.GetInt(ZDOVars.s_shoulderItem),
                    zdo.GetInt(ZDOVars.s_shoulderItemVariant));
                SetUtility(zdo.GetInt(ZDOVars.s_utilityItem));
                SetLeftHand(zdo.GetInt(ZDOVars.s_leftItem),
                    zdo.GetInt(ZDOVars.s_leftItemVariant));
                SetRightHand(zdo.GetInt(ZDOVars.s_rightItem));
            }
        }

        var rotation = NPCZDOUtils.GetRotation(zdo);
        if (rotation != Quaternion.identity)
        {
            _character.transform.rotation = rotation;
        }

        if (NPCZDOUtils.GetAttached(zdo))
        {
            string animation = NPCZDOUtils.GetAnimation(zdo);
            AttachStart(animation);
        }
    }

    protected bool UpdateTrader()
    {
        ZDO zdo = _character.m_nview.GetZDO();
        if (NPCZDOUtils.GetType(zdo) == (int)NPCType.Trader)
        {
            if (!_character.TryGetComponent<NPCTrader>(out var trader))
            {
                trader = _character.gameObject.AddComponent<NPCTrader>();
            }

            trader.Setup();
            return true;
        }

        return false;
    }

    protected bool UpdateTalker()
    {
        var talker = _character.gameObject.GetComponent<NpcTalk>();
        if (talker != null)
        {
            ZDO zdo = _character.m_nview.GetZDO();
            talker.m_randomTalk = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_TALKTEXTS, zdo);
            talker.m_randomGreets = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_GREETTEXTS, zdo);
            talker.m_randomGoodbye = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_GOODBYETEXTS, zdo);
            talker.m_aggravated = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_AGGROTEXTS, zdo);
            talker.m_name = _character.m_name;
            return true;
        }

        return false;
    }

    protected void RefreshQuestList(bool forceReset = false)
    {
        // TODO: Optimize only to load quest at index when ready
        if (_quests == null || forceReset)
        {
            ZDO zdo = _character.m_nview.GetZDO();
            int count = NPCZDOUtils.GetNPCQuestCount(zdo);
            if (count > 0)
            {
                _quests = new NPCQuest[count];

                for (int lcv = 0; lcv < count; lcv++)
                {
                    NPCZDOUtils.GetNPCQuest(zdo, lcv, out NPCQuest quest);
                    _quests[lcv] = quest;
                }
            }
        }

        _questsInitialized = true;
        _questIndex = 0;
    }

    // TODO: When removing keys is implemented a force reset must be used here.
    protected void UpdateQuest(bool forceReset = false)
    {
        if (!_questsInitialized)
        {
            RefreshQuestList(false);
        }

        if (forceReset || _questIndex < 0)
        {
            _questIndex = 0;
        }

        if (_quests == null)
        {
            return;
        }

        for (int lcv = _questIndex; lcv < _quests.Length; lcv++)
        {
            if (_quests[lcv] == null ||
                _quests[lcv].RewardLimit.Value == 0 ||
                !NPCUtils.HasCorrectNotReqiuredKeys(_quests[lcv].NotRequiredKeysSet))
            {
                continue;
            }

            _questIndex = lcv;
            break;
        }

        if (_questIndex >= 0 && _questIndex < _quests.Length)
        {
            _currentQuest = _quests[_questIndex];
        }
        else
        {
            _currentQuest = null;
        }
    }

    public NPCQuest GetCurrentQuest(bool update = true)
    {
        if (!_questsInitialized)
        {
            UpdateQuest(true);
        }
        else if (update)
        {
            UpdateQuest(false);
        }

        return _currentQuest;
    }

    #region Attachments

    public void UpdateAttach()
    {
        if (HasAttach)
        {
            _character.transform.position = AttachPosition;
            _character.transform.rotation = AttachRotation;
            var velocity = Vector3.zero;
            if (AttachRoot)
            {
                Rigidbody componentInParent = AttachRoot.GetComponentInParent<Rigidbody>();
                if (componentInParent)
                {
                    velocity = componentInParent.GetPointVelocity(_character.transform.position);
                }
            }
            _character.m_body.linearVelocity = velocity;
            _character.m_body.useGravity = false;
            _character.m_body.angularVelocity = Vector3.zero;
            _character.m_maxAirAltitude = _character.transform.position.y;
        }
        // TODO: check for attach stop as needed
    }

    public bool IsAttached()
    {
        return HasAttach;
    }

    public void Attach(bool attach, string animation = "", Chair chair = null)
    {
        if (attach)
        {
            AttachStop();
            SetAttachStart(animation, chair);
        }
        else
        {
            SetAttachStop(animation);
        }
    }

    public void SetAttachStart(string animation = "", Chair chair = null)
    {
        ZDO zdo = _character.m_nview.GetZDO();
        _character.m_nview.ClaimOwnership();
        NPCZDOUtils.SetAttached(ref zdo, true);

        if (chair)
        {
            animation = chair.m_attachAnimation;
        }

        NPCZDOUtils.SetAnimation(ref zdo, animation);

        AttachStart(animation, chair);
    }

    protected void AttachStart(string animation = "", Chair chair = null)
    {
        HasAttach = true;

        if (!chair && (animation == "attach_chair" || animation == "attach_throne"))
        {
            chair = Utility.GetClosestChair(_character.transform.position, _character.transform.localScale / 2);
        }

        // Reset previous animation state
        if (!AttachAnimation.IsNullOrWhiteSpace())
        {
            _character.m_zanim.SetBool(AttachAnimation, value: false);
        }

        if (chair)
        {
            AttachPosition = chair.m_attachPoint.position;
            AttachRotation = chair.m_attachPoint.rotation;
            _character.transform.position = AttachPosition;
            _character.transform.rotation = AttachRotation;
            AttachAnimation = chair.m_attachAnimation;
            AttachRoot = chair.transform.root.gameObject;
        }
        else
        {
            AttachPosition = _character.transform.position;
            AttachRotation = _character.transform.rotation;
            AttachAnimation = animation;
            AttachRoot = null;
        }

        // Set new animation state
        if (!AttachAnimation.IsNullOrWhiteSpace()) //_character.m_zanim.IsOwner() todo: check needed
        {
            _character.m_zanim.SetBool(AttachAnimation, value: true);
        }

        var velocity = Vector3.zero;
        if (AttachRoot)
        {
            Rigidbody componentInParent = AttachRoot.GetComponentInParent<Rigidbody>();
            if (componentInParent != null)
            {
                velocity = componentInParent.GetPointVelocity(_character.transform.position);
            }

            AttachColliders = AttachRoot.GetComponentsInChildren<Collider>();
            Collider[] attachColliders = AttachColliders;
            foreach (Collider collider in attachColliders)
            {
                Physics.IgnoreCollision(_character.m_collider, collider, ignore: true);
            }
        }

        _character.m_body.linearVelocity = velocity;
        _character.m_body.mass = 1000f;
        _character.m_body.useGravity = false;
        _character.m_body.angularVelocity = Vector3.zero;
        _character.m_maxAirAltitude = _character.transform.position.y;

        if (_character is Humanoid)
        {
            (_character as Humanoid).HideHandItems();
        }
        _character.ResetCloth();
    }

    public void SetAttachStop(string animation = "")
    {
        if (!HasAttach)
        {
            return;
        }

        _character.m_nview.ClaimOwnership();

        ZDO zdo = _character.m_nview.GetZDO();
        NPCZDOUtils.SetAnimation(ref zdo, animation);
        NPCZDOUtils.SetAttached(ref zdo, false);

        AttachStop(animation);
    }

    protected void AttachStop(string animation = "")
    {
        if (AttachColliders != null)
        {
            Collider[] attachColliders = AttachColliders;
            foreach (Collider collider in attachColliders)
            {
                Physics.IgnoreCollision(_character.m_collider, collider, ignore: false);
            }
        }

        HasAttach = false;

        // Reset previous animation state
        if (!AttachAnimation.IsNullOrWhiteSpace()) //_character.m_zanim.IsOwner() todo: check needed
        {
            _character.m_zanim.SetBool(AttachAnimation, value: false);
        }

        AttachAnimation = animation;

        // Set new animation state
        if (!AttachAnimation.IsNullOrWhiteSpace())
        {
            _character.m_zanim.SetBool(AttachAnimation, value: true);
        }

        _character.m_body.useGravity = true;
        _character.m_body.mass = _character.m_originalMass;
        AttachColliders = null;
        AttachRoot = null;

        if (_character is Humanoid)
        {
            (_character as Humanoid).ShowHandItems();
        }
        _character.ResetCloth();
    }

    #endregion

    #region Style and Setup

    public void SetFromConfig(NPCConfig config, bool newSpawn)
    {
        _newSpawn = newSpawn;
        if (config == null)
        {
            return;
        }

        ZDO zdo = _character.m_nview.GetZDO();

        _character.m_nview.ClaimOwnership();

        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        // Clear previous inventory? TODO test

        if (config.StandStill && !HasAttach)
        {
            SetAttachStart(config.Animation);
        }
        else if (!config.StandStill && HasAttach)
        {
            SetAttachStop(config.Animation);
        }

        if (config.ModelIndex.HasValue)
        {
            SetModel(config.ModelIndex.Value);
        }

        // Style
        // TODO: check setting random skin/hair color here works for other models
        if (_character is Humanoid && (_character as Humanoid).m_inventory != null)
        {
            (_character as Humanoid).m_inventory.RemoveAll();
        }

        if (_character is Humanoid && (_character as Humanoid).m_visEquipment != null)
        {
            bool isHuman = IsHuman();
            bool isFemale = isHuman && zdo.GetInt(ZDOVars.s_modelIndex) == 1;

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

            SetNPCHair(config.Hair, isHuman);
            SetNPCBeard(config.Beard, isHuman, isFemale);
            SetHelmet(config.Helmet, isHuman, isFemale);
            SetChest(config.Chest, isHuman, isFemale);
            SetLegs(config.Legs, isHuman, isFemale);
            SetUtility(config.Utility);
            SetTrinket(config.Trinket);
            SetShoulder(config.Shoulder, config.ShoulderVariant.Value);
            SetLeftHand(config.LeftHand, isHuman, config.LeftHandVariant.Value);
            SetRightHand(config.RightHand, isHuman);
        }

        NPCZDOUtils.SetZDOFromConfig(ref zdo, config);
        UpdateQuest(true);
        if (!UpdateTrader())
        {
            // TODO: Fix issues when changing NPC type, such as trader to non trader
            UpdateTalker();
        }

        // Refresh Hud and name in case name updated
        EnemyHud.instance.RemoveCharacterHud(_character);
        _character.m_name = config.Name;
    }

    public void SetRotation(Quaternion rotation)
    {
        _character.m_nview.ClaimOwnership();

        _character.transform.rotation = rotation;
        if (IsAttached())
        {
            AttachRotation = rotation;
        }

        ZDO zdo = _character.m_nview.GetZDO();
        NPCZDOUtils.SetRotation(ref zdo, rotation);
    }

    public void SetName(string name)
    {
        _character.m_nview.ClaimOwnership();
        _character.m_nview.GetZDO().Set(ZDOVars.s_tamedName, name);
        _character.m_name = name;
    }

    public void SetSpawnPoint(Vector3 position)
    {
        _character.m_nview.ClaimOwnership();
        ZDO zdo = _character.m_nview.GetZDO();
        NPCZDOUtils.SetSpawnPoint(ref zdo, position);
    }

    public void SetTrueDeath(bool death)
    {
        _character.m_nview.ClaimOwnership();
        ZDO zdo = _character.m_nview.GetZDO();
        NPCZDOUtils.SetTrueDeath(ref zdo, death);
    }

    public void SetRandom(string item = null)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        _character.m_nview.ClaimOwnership();
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        bool isHuman = IsHuman();
        bool isFemale = (_character as Humanoid).m_visEquipment.GetModelIndex() == 1;

        if (item.IsNullOrWhiteSpace())
        {
            if (isHuman)
            {
                int index = UnityEngine.Random.Range(0, 2);
                SetModel(index);
                isFemale = index == 1;
            }

            ClearInventory(isHuman);
            SetRandom(isHuman, isFemale);
        }
        else
        {
            if (item == "skincolor")
            {
                SetSkinColor(GetRandomSkinColor());
            }
            else if (item == "hair")
            {
                SetNPCHair(NPCConfig.RANDOM, isHuman);
            }
            else if (item == "haircolor")
            {
                SetHairColor(GetRandomHairColor());
            }
            else if (item == "beard")
            {
                SetNPCBeard(NPCConfig.RANDOM, isHuman, isFemale);
            }
            else if (item == "helmet")
            {
                SetHelmet(NPCConfig.RANDOM, isHuman, isFemale);
            }
            else if (item == "chest")
            {
                SetChest(NPCConfig.RANDOM, isHuman, isFemale);
            }
            else if (item == "legs")
            {
                SetLegs(NPCConfig.RANDOM, isHuman, isFemale);
            }
            else if (item == "righthand")
            {
                SetRightHand(NPCConfig.RANDOM, isHuman);
            }
            else if (item == "lefthand")
            {
                SetLeftHand(NPCConfig.RANDOM, isHuman);
            }
        }


            ZDO zdo = _character.m_nview.GetZDO();
        NPCZDOUtils.SetInitialized(ref zdo, true);
    }

    protected string GetRandomBeard()
    {
        var beard = UnityEngine.Random.Range(1, 35);
        if (beard <= 26)
        {
            return "Beard" + beard;
        }

        return "";
    }

    protected string GetRandomHairStyle()
    {
        var hair = UnityEngine.Random.Range(1, 37);
        return "Hair" + hair;
    }

    protected string GetRandomHelmetMale()
    {
        var helmet = UnityEngine.Random.Range(1, 25);
        if (helmet <= 10)
        {
            return "HelmetHat" + helmet;
        }
        else if (helmet < 16)
        {
            return "HelmetLeather";
        }
        else
        {
            return "";
        }
    }

    protected string GetRandomChestMale()
    {
        var chest = UnityEngine.Random.Range(1, 15);
        if (chest > 11)
        {
            return "ArmorLeatherChest";
        }
        else if (chest == 11)
        {
            return "ArmorHarvester1";
        }
        else
        {
            return "ArmorTunic" + chest;
        }
    }

    protected string GetRandomLegsMale()
    {
        var legs = UnityEngine.Random.Range(0, 15);
        if (legs < 10)
        {
            return "ArmorLeatherLegs";
        }
        else
        {
            return "ArmorRagsLegs";
        }
    }

    protected string GetRandomHelmetFemale()
    {
        var helmet = UnityEngine.Random.Range(1, 25);
        if (helmet <= 10)
        {
            return "HelmetHat" + helmet;
        }
        else if (helmet < 17)
        {
            return "HelmetMidsummerCrown";
        }
        else
        {
            return "";
        }
    }

    protected string GetRandomChestFemale()
    {
        var chest = UnityEngine.Random.Range(1, 15);
        if (chest <= 10)
        {
            return "ArmorDress" + chest;
        }
        else if (chest == 11)
        {
            return "ArmorHarvester2";
        }
        else
        {
            return "ArmorLeatherChest";
        }
    }

    protected string GetRandomLegsFemale()
    {
        var legs = UnityEngine.Random.Range(0, 15);
        if (legs < 8)
        {
            return "ArmorLeatherLegs";
        }
        else
        {
            return "ArmorRagsLegs";
        }
    }

    protected void ClearInventory(bool isHuman)
    {
        if (_character is Humanoid humanoid)
        {
            humanoid.m_inventory.RemoveAll();

            if (!isHuman)
            {
                // TODO: Implement randomizing default items by changing the seed
                humanoid.GiveDefaultItems();
            }
        }
    }

    protected void SetRandom(bool isHuman, bool isFemale)
    {
        SetSkinColor(GetRandomSkinColor());
        SetNPCHair(NPCConfig.RANDOM, isHuman);
        SetHairColor(GetRandomHairColor());
        SetNPCBeard(NPCConfig.RANDOM, isHuman, isFemale);

        SetHelmet(NPCConfig.RANDOM, isHuman, isFemale);
        SetChest(NPCConfig.RANDOM, isHuman, isFemale);
        SetLegs(NPCConfig.RANDOM, isHuman, isFemale);
        SetRightHand(NPCConfig.RANDOM, isHuman);
    }

    protected void SetNPCHair(string name, bool isHuman)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        if (name == NPCConfig.RANDOM)
        {
            if (isHuman)
            {
                name = GetRandomHairStyle();
            }
            else
            {
                name = "";
            }
        }

        (_character as Humanoid).m_hairItem = name;
        (_character as Humanoid).m_visEquipment.SetHairItem(name);
    }

    protected void SetHairColor(Color color)
    {
        SetHairColor(color.r, color.g, color.b);
    }

    protected void SetHairColor(float r, float g, float b)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        var color = new Vector3(r, g, b);
        (_character as Humanoid).m_visEquipment.SetHairColor(color);
    }

    protected void SetSkinColor(Color color)
    {
        SetSkinColor(color.r, color.g, color.b);
    }

    protected void SetSkinColor(float r, float g, float b)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        var color = new Vector3(r, g, b);
        (_character as Humanoid).m_visEquipment.SetSkinColor(color);
    }

    protected void SetNPCBeard(string name, bool isHuman, bool female)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        if (name == NPCConfig.RANDOM)
        {
            if (isHuman && !female)
            {
                name = GetRandomBeard();
            }
            else
            {
                name = "";
            }
        }

        (_character as Humanoid).m_beardItem = name;
        (_character as Humanoid).m_visEquipment.SetBeardItem(name);
    }

    protected void SetModel(int index)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        (_character as Humanoid).m_visEquipment.SetModel(index);
    }

    private bool SetItem(ref ItemDrop.ItemData slot, int hash, int variant = -1)
    {
        if (_character is not Humanoid || (_character as Humanoid).m_inventory == null)
        {
            return false;
        }

        // Remove old item
        if (slot != null && (_character as Humanoid).m_inventory.m_inventory.Contains(slot))
        {
            (_character as Humanoid).m_inventory.RemoveItem(slot);
            slot = null;
        }

        if (hash == 0 || hash == "".GetStableHashCode())
        {
            // Item is empty
            return true;
        }

        // Check if new item exists
        if (!Utility.GetItemPrefab(hash, out var item))
        {
            NPCSPlugin.NPCSLogger.LogDebug($"Item with hash {hash} not found! Skipping adding new item.");
            return true;
        }

        ItemDrop.ItemData itemData = (_character as Humanoid).PickupPrefab(item, 0, autoequip: false);
        if (itemData != null)
        {
            if (variant != -1)
            {
                itemData.m_variant = variant;
            }

            (_character as Humanoid).EquipItem(itemData, triggerEquipEffects: false);
        }

        return true;
    }

    protected void SetHelmet(string name, bool isHuman, bool female)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        if (name == NPCConfig.RANDOM)
        {
            if (!isHuman)
            {
                name = "";
            }
            else if (female)
            {
                name = GetRandomHelmetFemale();
            }
            else
            {
                name = GetRandomHelmetMale();
            }
        }

        if (SetHelmet(name.GetStableHashCode()))
        {
            (_character as Humanoid).m_visEquipment.SetHelmetItem(name);
        }
    }

    protected bool SetHelmet(int hash)
    {
        if (_character is not Humanoid)
        {
            return false;
        }

        return SetItem(ref (_character as Humanoid).m_helmetItem, hash);
    }

    protected void SetChest(string name, bool isHuman, bool female)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        if (name == NPCConfig.RANDOM)
        {
            if (!isHuman)
            {
                name = "";
            }
            else if (female)
            {
                name = GetRandomChestFemale();
            }
            else
            {
                name = GetRandomChestMale();
            }
        }

        if (SetChest(name.GetStableHashCode()))
        {
            (_character as Humanoid).m_visEquipment.SetChestItem(name);
        }
    }

    protected bool SetChest(int hash)
    {
        if (_character is not Humanoid)
        {
            return false;
        }

        return SetItem(ref (_character as Humanoid).m_chestItem, hash);
    }

    protected void SetLegs(string name, bool isHuman, bool female)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        if (name == NPCConfig.RANDOM)
        {
            if (!isHuman)
            {
                name = "";
            }
            else if (female)
            {
                name = GetRandomLegsFemale();
            }
            else
            {
                name = GetRandomLegsMale();
            }
        }

        if (SetLegs(name.GetStableHashCode()))
        {
            (_character as Humanoid).m_visEquipment.SetLegItem(name);
        }
    }

    protected bool SetLegs(int hash)
    {
        if (_character is not Humanoid)
        {
            return false;
        }

        return SetItem(ref (_character as Humanoid).m_legItem, hash);
    }

    protected void SetShoulder(string name, int variant = 0)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        if (SetShoulder(name.GetStableHashCode(), variant))
        {
            (_character as Humanoid).m_visEquipment.SetShoulderItem(name, variant);
        }
    }

    protected bool SetShoulder(int hash, int variant = 0)
    {
        if (_character is not Humanoid)
        {
            return false;
        }

        return SetItem(ref (_character as Humanoid).m_shoulderItem, hash, variant);
    }

    protected void SetUtility(string name)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        if (SetUtility(name.GetStableHashCode()))
        {
            (_character as Humanoid).m_visEquipment.SetUtilityItem(name);
        }
    }

    protected bool SetUtility(int hash)
    {
        if (_character is not Humanoid)
        {
            return false;
        }

        return SetItem(ref (_character as Humanoid).m_utilityItem, hash);
    }

    protected void SetTrinket(string name)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        if (SetTrinket(name.GetStableHashCode()))
        {
            (_character as Humanoid).m_visEquipment.SetTrinketItem(name);
        }
    }

    protected bool SetTrinket(int hash)
    {
        if (_character is not Humanoid)
        {
            return false;
        }

        return SetItem(ref (_character as Humanoid).m_trinketItem, hash);
    }

    protected string GetRandomRightHand()
    {
        int weapon = UnityEngine.Random.Range(0, 30);
        if (weapon == 0)
        {
            return "KnifeFlint";
        }
        else if (weapon == 1)
        {
            return "AxeFlint";
        }
        else if (weapon == 2)
        {
            return "SpearFlint";
        }
        else if (weapon == 3)
        {
            return "AxeStone";
        }
        else if (weapon == 4)
        {
            return "Club";
        }
        else if (weapon == 5)
        {
            return "Torch";
        }

        return "";
    }

    protected void SetRightHand(string name, bool isHuman)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        if (name == NPCConfig.RANDOM)
        {
            if (!isHuman)
            {
                // Do not override default items
                return;
            }
            else
            {
                name = GetRandomRightHand();
            }
        }

        if (SetRightHand(name.GetStableHashCode()))
        {
            (_character as Humanoid).m_visEquipment.SetRightItem(name);
        }
    }

    protected bool SetRightHand(int hash)
    {
        if (_character is not Humanoid)
        {
            return false;
        }

        // Add new item
        return SetItem(ref (_character as Humanoid).m_rightItem, hash);
    }

    protected string GetRandomLeftHand()
    {
        int weapon = UnityEngine.Random.Range(0, 20);
        if (weapon == 0)
        {
            return "ShieldWood";
        }
        else if (weapon == 1)
        {
            return "ShieldWoodTower";
        }

        return "";
    }

    protected void SetLeftHand(string name, bool isHuman, int variant = 0)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        if (name == NPCConfig.RANDOM)
        {
            if (!isHuman)
            {
                // Do not override default items
                return;
            }
            else
            {
                name = GetRandomLeftHand();
            }
        }

        if (SetLeftHand(name.GetStableHashCode(), variant))
        {
            (_character as Humanoid).m_visEquipment.SetLeftItem(name, variant);
        }
    }

    protected bool SetLeftHand(int hash, int variant = 0)
    {
        if (_character is not Humanoid)
        {
            return false;
        }

        return SetItem(ref (_character as Humanoid).m_leftItem, hash, variant);
    }

    protected Color GetRandomSkinColor()
    {
        var skintone = UnityEngine.Random.Range(0f, 1f);
        Color skinColor = Color.Lerp(SkinColorMin, SkinColorMax, skintone);
        return skinColor;
    }

    protected Color GetRandomHairColor()
    {
        var hairtone = UnityEngine.Random.Range(0f, 1f);
        var hairlevel = UnityEngine.Random.Range(0f, 1f);
        Color hairColor = Color.Lerp(HairColorMin, HairColorMax, hairtone) *
            Mathf.Lerp(0.1f, 1f, hairlevel);
        return hairColor;
    }

    public string GetSkinColor()
    {
        var color = NPCZDOUtils.GetSkinColor(_character.m_nview.GetZDO());
        return $"Skin Color RGB: {color.x}, {color.y}, {color.z}";
    }

    public string GetHairColor()
    {
        var color = NPCZDOUtils.GetHairColor(_character.m_nview.GetZDO());
        return $"Hair Color RGB: {color.x}, {color.y}, {color.z}";
    }

    public bool IsHuman()
    {
        return Utils.GetPrefabName(_character.gameObject).Equals($"{NPCSPlugin.MOD_PREFIX}Player");
    }

    #endregion
}
