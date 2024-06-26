using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VentureValheim.NPCS;

// TODO localization
public class NPCCharacter : Character, Interactable, Hoverable, INPC
{
    public bool HasAttach = false; // TODO fix zdo syncing for this?
    public Vector3 AttachPosition;
    public Quaternion AttachRotation;
    public string AttachAnimation;
    public GameObject AttachRoot;
    public Collider[] AttachColliders;

    protected HashSet<string> NPCRequiredKeysSet = new HashSet<string>();
    protected HashSet<string> NPCNotRequiredKeysSet = new HashSet<string>();

    #region Humanoid and Components

    public override void Awake()
    {
        base.Awake();

        var startCoroutine = SetUp();
        StartCoroutine(startCoroutine);
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

        if (NPCUtils.GetAttached(m_nview))
        {
            AttachStart();
        }

        NPCRequiredKeysSet = Utility.StringToSet(NPCUtils.GetNPCRequiredKeys(m_nview));
        NPCNotRequiredKeysSet = Utility.StringToSet(NPCUtils.GetNPCNotRequiredKeys(m_nview));

        m_defeatSetGlobalKey = NPCUtils.GetNPCDefeatKey(m_nview);

        // TODO: fixup for tamed human
        m_nview.GetZDO().Set(ZDOVars.s_tamed, false);
        m_tamed = false;
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
            NPCFactory.RespawnNPC(this.transform.root.gameObject);
        }

        ZNetScene.instance.Destroy(base.gameObject);
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

        ResetCloth();
    }

    #endregion

    #region Style and Setup

    public void SetFromConfig(NPCConfiguration.NPCConfig config, bool newSpawn)
    {
        if (config == null)
        {
            return;
        }

        m_nview.ClaimOwnership();

        if (config.StandStill && !HasAttach)
        {
            AttachStart();
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
