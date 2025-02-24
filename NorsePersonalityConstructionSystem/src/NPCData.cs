using System;
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

    public bool HasAttach = false; // TODO fix zdo syncing for this?
    public Vector3 AttachPosition;
    public Quaternion AttachRotation;
    public string AttachAnimation;
    public GameObject AttachRoot;
    public Collider[] AttachColliders;

    public enum NPCType
    {
        None = 0,
        Information = 1,
        Reward = 2,
        Sellsword = 3,
        SlayTarget = 4,
        Trader = 5
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
        NPCZDOUtils.UpgradeVersion(ref _character.m_nview);

        _character.m_name = NPCZDOUtils.GetTamedName(_character.m_nview);

        _character.m_defeatSetGlobalKey = NPCZDOUtils.GetNPCDefeatKey(_character.m_nview);

        // TODO: fixup for tamed
        _character.m_nview.GetZDO().Set(ZDOVars.s_tamed, false);
        _character.m_tamed = false;

        UpdateTrader();

        if (_character is NPCHumanoid)
        {
            var humanoid = _character as Humanoid;
            if (!_newSpawn && humanoid.m_visEquipment != null && humanoid.m_visEquipment.m_nview != null)
            {
                var zdo = humanoid.m_visEquipment.m_nview.GetZDO();
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

        if (NPCZDOUtils.GetAttached(_character.m_nview) && NPCZDOUtils.GetAnimation(_character.m_nview) == "attach_chair")
        {
            var chair = Utility.GetClosestChair(_character.transform.position, _character.transform.localScale / 2);
            if (chair != null)
            {
                AttachStart(chair);
            }
            else
            {
                AttachStart();
            }
        }
        else if (NPCZDOUtils.GetAttached(_character.m_nview))
        {
            AttachStart();
        }

        if (NPCZDOUtils.GetAttached(_character.m_nview))
        {
            string animation = NPCZDOUtils.GetAnimation(_character.m_nview);
            AttachStart(animation);
        }

        var rotation = NPCZDOUtils.GetRotation(_character.m_nview);
        if (rotation != Quaternion.identity)
        {
            NPCSPlugin.NPCSLogger.LogDebug("Updating from saved rotation!");
            _character.transform.rotation = rotation;
        }

        var talker = _character.GetComponent<NpcTalk>();
        if (talker != null)
        {
            talker.m_randomTalk = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_TALKTEXTS, _character.m_nview);
            talker.m_randomGreets = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_GREETTEXTS, _character.m_nview);
            talker.m_randomGoodbye = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_GOODBYETEXTS, _character.m_nview);
            talker.m_aggravated = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_AGGROTEXTS, _character.m_nview);
            talker.m_name = _character.m_name;
        }
    }

    public void UpdateTrader()
    {
        if (NPCZDOUtils.GetType(_character.m_nview) == (int)NPCType.Trader)
        {
            if (!_character.TryGetComponent<NPCTrader>(out var trader))
            {
                trader = _character.gameObject.AddComponent<NPCTrader>();
            }

            trader.Setup();
        }
    }

    public void RefreshQuestList(bool forceReset = false)
    {
        // TODO: Optimize only to load quest at index when ready
        if (_quests == null || forceReset)
        {
            NPCSPlugin.NPCSLogger.LogDebug($"Refreshing quests list");
            int count = NPCZDOUtils.GetNPCQuestCount(_character.m_nview);
            if (count > 0)
            {
                _quests = new NPCQuest[count];

                for (int lcv = 0; lcv < count; lcv++)
                {
                    NPCZDOUtils.GetNPCQuest(_character.m_nview, lcv, out NPCQuest quest);
                    _quests[lcv] = quest;
                }
            }
        }

        _questsInitialized = true;
        _questIndex = 0;
    }

    public void UpdateQuest(bool forceReset = false)
    {
        if (!_questsInitialized)
        {
            NPCSPlugin.NPCSLogger.LogDebug("Updating Quest: refreshing quest list because not init");
            RefreshQuestList(false);
        }

        if (forceReset)
        {
            _questIndex = 0;
        }

        if (_quests != null)
        {
            for (int lcv = _questIndex; lcv < _quests.Length; lcv++)
            {
                // TODO, decide how this will work
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

            NPCSPlugin.NPCSLogger.LogDebug($"Selecting quest at index: {_questIndex}, out of {_quests.Length} total");
        }
    }

    public NPCQuest GetCurrentQuest()
    {
        if (!_questsInitialized)
        {
            NPCSPlugin.NPCSLogger.LogDebug("Updating Quest: refreshing quest list because not init");
            RefreshQuestList(false);
            UpdateQuest(true);
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
            if (AttachRoot != null)
            {
                Rigidbody componentInParent = AttachRoot.GetComponentInParent<Rigidbody>();
                if (componentInParent != null)
                {
                    velocity = componentInParent.GetPointVelocity(_character.transform.position);
                }
            }
            _character.m_body.velocity = velocity;
            _character.m_body.useGravity = false;
            _character.m_body.angularVelocity = Vector3.zero;
            _character.m_maxAirAltitude = _character.transform.position.y;
        }
        // TODO check for attach stop as needed
    }

    public bool IsAttached()
    {
        return HasAttach;
    }

    public void Attach(bool attach, Chair chair = null)
    {
        _character.m_nview.ClaimOwnership();
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

    public void AttachStart(string animation)
    {
        if (animation == "attach_chair" || animation == "attach_throne")
        {
            var chair = Utility.GetClosestChair(_character.transform.position, _character.transform.localScale / 2);
            if (chair != null)
            {
                AttachStart(chair);
            }
        }
        else
        {
            AttachStart();
        }
    }

    public void AttachStart()
    {
        HasAttach = true;
        AttachPosition = _character.transform.position;
        AttachRotation = _character.transform.rotation; //TODO
        AttachAnimation = ""; // todo test "Stand" // ZSyncAnimation.GetHash("dodge");
        if (_character.m_nview.IsOwner())
        {
            //_character.m_zanim.SetBool(AttachAnimation, value: true);
            NPCZDOUtils.SetAttached(ref _character.m_nview, true);
        }

        _character.m_body.mass = 1000f;

        _character.m_body.useGravity = false;
        _character.m_body.velocity = Vector3.zero;
        _character.m_body.angularVelocity = Vector3.zero;
        _character.m_maxAirAltitude = _character.transform.position.y;
    }

    public void AttachStart(Chair chair)
    {
        HasAttach = true;
        AttachPosition = chair.m_attachPoint.position;
        AttachRotation = chair.m_attachPoint.rotation;
        AttachAnimation = chair.m_attachAnimation;
        AttachRoot = chair.transform.root.gameObject;

        _character.transform.position = AttachPosition;
        _character.transform.rotation = AttachRotation;

        if (_character.m_nview.IsOwner())
        {
            _character.m_zanim.SetBool(AttachAnimation, value: true);
            NPCZDOUtils.SetAnimation(ref _character.m_nview, AttachAnimation);
            //NPCZDOUtils.SetSitting(ref _character.m_nview, true);
        }

        _character.m_body.mass = 1000f;

        Rigidbody componentInParent = AttachRoot.GetComponent<Rigidbody>();
        _character.m_body.useGravity = false;
        _character.m_body.velocity = (componentInParent ?
            componentInParent.GetPointVelocity(_character.transform.position) : Vector3.zero);
        _character.m_body.angularVelocity = Vector3.zero;
        _character.m_maxAirAltitude = _character.transform.position.y;

        AttachColliders = AttachRoot.GetComponentsInChildren<Collider>();

        Collider[] attachColliders = AttachColliders;
        foreach (Collider collider in attachColliders)
        {
            Physics.IgnoreCollision(_character.m_collider, collider, ignore: true);
        }
        
        if (_character is Humanoid)
        {
            (_character as Humanoid).HideHandItems();
        }
        _character.ResetCloth();
    }

    public void AttachStop()
    {
        if (!HasAttach)
        {
            return;
        }

        if (_character.m_nview.IsOwner())
        {
            NPCZDOUtils.SetAnimation(ref _character.m_nview, "");
            NPCZDOUtils.SetAttached(ref _character.m_nview, false);
        }

        if (_character.m_zanim.IsOwner() && AttachAnimation != null)
        {
            _character.m_zanim.SetBool(AttachAnimation, value: false);
        }

        if (AttachColliders != null)
        {
            Collider[] attachColliders = AttachColliders;
            foreach (Collider collider in attachColliders)
            {
                Physics.IgnoreCollision(_character.m_collider, collider, ignore: false);
            }
        }

        HasAttach = false;
        AttachAnimation = "";
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

        _character.m_nview.ClaimOwnership();

        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        // Clear previous inventory? TODO test

        if (config.StandStill && !HasAttach)
        {
            AttachStart();
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
            if (config.SkinColorR.HasValue &&
                config.SkinColorG.HasValue &&
                config.SkinColorB.HasValue)
            {
                SetSkinColor(config.SkinColorR.Value, config.SkinColorG.Value, config.SkinColorB.Value);
            }
            else //if (config.Model.Equals("Player"))
            {
                SetSkinColor(GetRandomSkinColor());
            }

            if (config.HairColorR.HasValue &&
                config.HairColorG.HasValue &&
                config.HairColorB.HasValue)
            {
                SetHairColor(config.HairColorR.Value, config.HairColorG.Value, config.HairColorB.Value);
            }
            else //if (config.Model.Equals("Player"))
            {
                SetHairColor(GetRandomHairColor());
            }

            SetNPCHair(config.Hair);
            SetNPCBeard(config.Beard);
            SetHelmet(config.Helmet);
            SetChest(config.Chest);
            SetLegs(config.Legs);
            SetUtility(config.Utility);
            SetShoulder(config.Shoulder, config.ShoulderVariant.Value);
            SetLeftHand(config.LeftHand, config.LeftHandVariant.Value);
            SetRightHand(config.RightHand);
        }

        NPCZDOUtils.SetZDOFromConfig(ref _character.m_nview, config);
        UpdateQuest(true);
        UpdateTrader();
    }

    public void SetRotation(Quaternion rotation)
    {
        _character.m_nview.ClaimOwnership();

        if (IsAttached())
        {
            AttachRotation = rotation;
        }
        else
        {
            _character.transform.rotation = rotation;
        }

        NPCZDOUtils.SetRotation(ref _character.m_nview, rotation);
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
        NPCZDOUtils.SetSpawnPoint(ref _character.m_nview, position);
    }

    public void SetTrueDeath(bool death)
    {
        _character.m_nview.ClaimOwnership();
        NPCZDOUtils.SetTrueDeath(ref _character.m_nview, death);
    }

    public void SetRandom()
    {
        if (_character is not Humanoid)
        {
            return;
        }

        _character.m_nview.ClaimOwnership();
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
        if (_character is not Humanoid)
        {
            return;
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

    protected void SetNPCBeard(string name)
    {
        if (_character is not Humanoid)
        {
            return;
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

    private void SetItem(ref ItemDrop.ItemData slot, int hash, int variant = -1)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        if ((_character as Humanoid).m_inventory == null || !Utility.GetItemPrefab(hash, out var item))
        {
            slot = null;
            return;
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
    }

    protected void SetHelmet(string name)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        (_character as Humanoid).m_visEquipment.SetHelmetItem(name);
        SetHelmet(name.GetStableHashCode());
    }

    protected void SetHelmet(int hash)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        SetItem(ref (_character as Humanoid).m_helmetItem, hash);
    }

    protected void SetChest(string name)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        (_character as Humanoid).m_visEquipment.SetChestItem(name);
        SetChest(name.GetStableHashCode());
    }

    protected void SetChest(int hash)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        SetItem(ref (_character as Humanoid).m_chestItem, hash);
    }

    protected void SetLegs(string name)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        (_character as Humanoid).m_visEquipment.SetLegItem(name);
        SetLegs(name.GetStableHashCode());
    }

    protected void SetLegs(int hash)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        SetItem(ref (_character as Humanoid).m_legItem, hash);
    }

    protected void SetShoulder(string name, int variant = 0)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        (_character as Humanoid).m_visEquipment.SetShoulderItem(name, variant);
        SetShoulder(name.GetStableHashCode(), variant);
    }

    protected void SetShoulder(int hash, int variant = 0)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        SetItem(ref (_character as Humanoid).m_shoulderItem, hash, variant);
    }

    protected void SetUtility(string name)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        (_character as Humanoid).m_visEquipment.SetUtilityItem(name);
        SetUtility(name.GetStableHashCode());
    }

    protected void SetUtility(int hash)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        SetItem(ref (_character as Humanoid).m_utilityItem, hash);
    }

    protected void SetRightHand(string name)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        (_character as Humanoid).m_visEquipment.SetRightItem(name);
        SetRightHand(name.GetStableHashCode());
    }

    protected void SetRightHand(int hash)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        SetItem(ref (_character as Humanoid).m_rightItem, hash);
    }

    protected void SetLeftHand(string name, int variant = 0)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        (_character as Humanoid).m_visEquipment.SetLeftItem(name, variant);
        SetLeftHand(name.GetStableHashCode(), variant);
    }

    protected void SetLeftHand(int hash, int variant = 0)
    {
        if (_character is not Humanoid)
        {
            return;
        }

        SetItem(ref (_character as Humanoid).m_leftItem, hash, variant);
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
        var color = NPCZDOUtils.GetSkinColor(_character.m_nview);
        return $"Skin Color RGB: {color.x}, {color.y}, {color.z}";
    }

    public string GetHairColor()
    {
        var color = NPCZDOUtils.GetHairColor(_character.m_nview);
        return $"Hair Color RGB: {color.x}, {color.y}, {color.z}";
    }

    #endregion
}
