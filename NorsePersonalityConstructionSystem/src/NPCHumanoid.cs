using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VentureValheim.NPCS;

// TODO localization
public class NPCHumanoid : Humanoid, Interactable, Hoverable, INPC
{
    public bool HasAttach = false; // TODO fix zdo syncing for this?
    public Vector3 AttachPosition;
    public Quaternion AttachRotation;
    public string AttachAnimation;
    public GameObject AttachRoot;
    public Collider[] AttachColliders;

    protected HashSet<string> NPCRequiredKeysSet = new HashSet<string>();
    protected HashSet<string> NPCNotRequiredKeysSet = new HashSet<string>();

    private bool _newSpawn = false;

    #region Humanoid and Components

    public override void Awake()
    {
        base.Awake();

        var startCoroutine = SetUp();
        StartCoroutine(startCoroutine);
    }

    public override void Start()
    {
        // Prevent giving default items if initialized
        if (!NPCUtils.GetInitialized(m_nview))
        {
            base.Start();
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    public IEnumerator SetUp()
    {
        yield return null;
        yield return null;

        m_name = NPCUtils.GetTamedName(m_nview);

        if (NPCUtils.GetSitting(m_nview))
        {
            var chair = Utility.GetClosestChair(base.transform.position, base.transform.localScale / 2);
            if (chair != null)
            {
                AttachStart(chair);
            }
        }
        else if (NPCUtils.GetAttached(m_nview))
        {
            AttachStart();
        }

        NPCRequiredKeysSet = Utility.StringToSet(NPCUtils.GetNPCRequiredKeys(m_nview));
        NPCNotRequiredKeysSet = Utility.StringToSet(NPCUtils.GetNPCNotRequiredKeys(m_nview));

        m_defeatSetGlobalKey = NPCUtils.GetNPCDefeatKey(m_nview);

        // TODO: fixup for tamed human
        m_nview.GetZDO().Set(ZDOVars.s_tamed, false);
        m_tamed = false;

        if (!_newSpawn && m_visEquipment != null && m_visEquipment.m_nview != null)
        {
            SetHelmet(m_visEquipment.m_nview.GetZDO().GetInt(ZDOVars.s_helmetItem));
            SetChest(m_visEquipment.m_nview.GetZDO().GetInt(ZDOVars.s_chestItem));
            SetLegs(m_visEquipment.m_nview.GetZDO().GetInt(ZDOVars.s_legItem));
            SetShoulder(m_visEquipment.m_nview.GetZDO().GetInt(ZDOVars.s_shoulderItem),
                m_visEquipment.m_nview.GetZDO().GetInt(ZDOVars.s_shoulderItemVariant));
            SetUtility(m_visEquipment.m_nview.GetZDO().GetInt(ZDOVars.s_utilityItem));
            SetLeftHand(m_visEquipment.m_nview.GetZDO().GetInt(ZDOVars.s_leftItem),
                m_visEquipment.m_nview.GetZDO().GetInt(ZDOVars.s_leftItemVariant));
            SetRightHand(m_visEquipment.m_nview.GetZDO().GetInt(ZDOVars.s_rightItem));
        }
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
            base.transform.position = AttachPosition;
            base.transform.rotation = AttachRotation;
            var velocity = Vector3.zero;
            if (AttachRoot != null)
            {
                Rigidbody componentInParent = AttachRoot.GetComponentInParent<Rigidbody>();
                if (componentInParent != null)
                {
                    velocity = componentInParent.GetPointVelocity(base.transform.position);
                }
            }
            m_body.velocity = velocity;
            m_body.useGravity = false;
            m_body.angularVelocity = Vector3.zero;
            m_maxAirAltitude = base.transform.position.y;
        }
        // TODO check for attach stop as needed
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
                    NPCUtils.CopyVisEquipment(ref visEquip, m_visEquipment);
                }

                NPCUtils.SetTamedName(ref ragdoll.m_nview, m_name);
                NPCUtils.SetTrueDeath(ref ragdoll.m_nview, NPCUtils.GetTrueDeath(m_nview));
            }
        }

        Utility.SetKey(m_defeatSetGlobalKey, true);

        if (m_onDeath != null)
        {
            m_onDeath();
        }

        if (!NPCUtils.GetTrueDeath(m_nview))
        {
            NPCFactory.RespawnNPC(transform.root.gameObject);
        }

        ZNetScene.instance.Destroy(gameObject);
    }

    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        return NPCUtils.TryInteract(gameObject);
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        return NPCUtils.TryUseItem(gameObject, item);
    }

    public override string GetHoverText()
    {
        return NPCUtils.GetHoverText(m_nview, m_baseAI);
    }

    public override string GetHoverName()
    {
        return m_name;
    }

    public HashSet<string> GetRequiredKeysSet()
    {
        return NPCRequiredKeysSet;
    }

    public HashSet<string> GetNotRequiredKeysSet()
    {
        return NPCNotRequiredKeysSet;
    }

    public override bool IsAttached()
    {
        return HasAttach || base.IsAttached();
    }

    public void Attach(bool attach, Chair chair = null)
    {
        m_nview.ClaimOwnership();
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
        AttachPosition = transform.position;
        AttachRotation = transform.rotation;
        if (m_nview.IsOwner())
        {
            NPCUtils.SetAttached(ref m_nview, true);
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
        AttachPosition = chair.m_attachPoint.position;
        AttachRotation = chair.m_attachPoint.rotation;
        AttachAnimation = chair.m_attachAnimation;
        AttachRoot = chair.transform.root.gameObject;

        base.transform.position = AttachPosition;
        base.transform.rotation = AttachRotation;

        if (m_nview.IsOwner())
        {
            m_zanim.SetBool(AttachAnimation, value: true);
            NPCUtils.SetSitting(ref m_nview, true);
        }

        m_body.mass = 1000f;

        Rigidbody componentInParent = AttachRoot.GetComponent<Rigidbody>();
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
            NPCUtils.SetAttached(ref m_nview, false);
            NPCUtils.SetSitting(ref m_nview, false);
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

        HasAttach = false;
        AttachAnimation = "";
        m_body.useGravity = true;
        m_body.mass = m_originalMass;
        AttachColliders = null;
        AttachRoot = null;

        ShowHandItems();
        ResetCloth();
    }

    #endregion

    #region Style and Setup

    public void SetFromConfig(NPCConfiguration.NPCConfig config, bool newSpawn)
    {
        _newSpawn = newSpawn;
        if (config == null)
        {
            return;
        }

        m_nview.ClaimOwnership();

        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        // Clear previous inventory? TODO test
        m_inventory.RemoveAll();

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
        if (m_visEquipment != null)
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

        NPCRequiredKeysSet = Utility.StringToSet(config.RequiredKeys);
        NPCNotRequiredKeysSet = Utility.StringToSet(config.NotRequiredKeys);

        NPCUtils.SetZDOFromConfig(ref m_nview, config);
    }

    public void SetName(string name)
    {
        m_nview.ClaimOwnership();
        m_nview.GetZDO().Set(ZDOVars.s_tamedName, name);
        m_name = name;
    }

    public void SetSpawnPoint(Vector3 position)
    {
        m_nview.ClaimOwnership();
        NPCUtils.SetSpawnPoint(ref m_nview, position);
    }

    public void SetTrueDeath(bool death)
    {
        m_nview.ClaimOwnership();
        NPCUtils.SetTrueDeath(ref m_nview, death);
    }

    public void SetRandom()
    {
        m_nview.ClaimOwnership();
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

    private void SetItem(ref ItemDrop.ItemData slot, int hash, int variant = -1)
    {
        if (m_inventory == null || !Utility.GetItemPrefab(hash, out var item))
        {
            slot = null;
            return;
        }

        ItemDrop.ItemData itemData = PickupPrefab(item, 0, autoequip: false);
        if (itemData != null)
        {
            if (variant != -1)
            {
                itemData.m_variant = variant;
            }

            EquipItem(itemData, triggerEquipEffects: false);
        }
    }

    protected void SetHelmet(string name)
    {
        m_visEquipment.SetHelmetItem(name);
        SetHelmet(name.GetStableHashCode());
    }

    protected void SetHelmet(int hash)
    {
        SetItem(ref m_helmetItem, hash);
    }

    protected void SetChest(string name)
    {
        m_visEquipment.SetChestItem(name);
        SetChest(name.GetStableHashCode());
    }

    protected void SetChest(int hash)
    {
        SetItem(ref m_chestItem, hash);
    }

    protected void SetLegs(string name)
    {
        m_visEquipment.SetLegItem(name);
        SetLegs(name.GetStableHashCode());
    }

    protected void SetLegs(int hash)
    {
        SetItem(ref m_legItem, hash);
    }

    protected void SetShoulder(string name, int variant = 0)
    {
        m_visEquipment.SetShoulderItem(name, variant);
        SetShoulder(name.GetStableHashCode(), variant);
    }

    protected void SetShoulder(int hash, int variant = 0)
    {
        SetItem(ref m_shoulderItem, hash, variant);
    }

    protected void SetUtility(string name)
    {
        m_visEquipment.SetUtilityItem(name);
        SetUtility(name.GetStableHashCode());
    }

    protected void SetUtility(int hash)
    {
        SetItem(ref m_utilityItem, hash);
    }

    protected void SetRightHand(string name)
    {
        m_visEquipment.SetRightItem(name);
        SetRightHand(name.GetStableHashCode());
    }

    protected void SetRightHand(int hash)
    {
        SetItem(ref m_rightItem, hash);
    }

    protected void SetLeftHand(string name, int variant = 0)
    {
        m_visEquipment.SetLeftItem(name, variant);
        SetLeftHand(name.GetStableHashCode(), variant);
    }

    protected void SetLeftHand(int hash, int variant = 0)
    {
        SetItem(ref m_leftItem, hash, variant);
    }

    protected Color GetRandomSkinColor()
    {
        var skintone = UnityEngine.Random.Range(0f, 1f);
        Color skinColor = Color.Lerp(NPCUtils.SkinColorMin, NPCUtils.SkinColorMax, skintone);
        return skinColor;
    }

    protected Color GetRandomHairColor()
    {
        var hairtone = UnityEngine.Random.Range(0f, 1f);
        var hairlevel = UnityEngine.Random.Range(0f, 1f);
        Color hairColor = Color.Lerp(NPCUtils.HairColorMin, NPCUtils.HairColorMax, hairtone) *
            Mathf.Lerp(0.1f, 1f, hairlevel);
        return hairColor;
    }

    public string GetSkinColor()
    {
        var color = NPCUtils.GetSkinColor(m_nview);
        return $"Skin Color RGB: {color.x}, {color.y}, {color.z}";
    }

    public string GetHairColor()
    {
        var color = NPCUtils.GetHairColor(m_nview);
        return $"Hair Color RGB: {color.x}, {color.y}, {color.z}";
    }

    public void SetRotation(Quaternion rotation)
    {
        m_nview.ClaimOwnership();

        if (NPCUtils.GetAttached(m_nview) && !NPCUtils.GetSitting(m_nview))
        {
            AttachRotation = rotation;
        }
        else
        {
            transform.rotation = rotation;
        }
    }

    #endregion
}
