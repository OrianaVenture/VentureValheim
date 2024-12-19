using System.Collections;
using UnityEngine;

namespace VentureValheim.NPCS;

public class NPCRagdoll : Ragdoll, Interactable, Hoverable
{
    public new void Awake()
    {
        m_nview = GetComponent<ZNetView>();
        m_bodies = GetComponentsInChildren<Rigidbody>();

        Invoke("RemoveInitVel", 2f);
        if ((bool)m_mainModel)
        {
            float hue = m_nview.GetZDO().GetFloat(ZDOVars.s_hue);
            float sat = m_nview.GetZDO().GetFloat(ZDOVars.s_saturation);
            float val = m_nview.GetZDO().GetFloat(ZDOVars.s_value);
            m_mainModel.material.SetFloat("_Hue", hue);
            m_mainModel.material.SetFloat("_Saturation", sat);
            m_mainModel.material.SetFloat("_Value", val);
        }

        m_ttl = 4f;

        var startCoroutine = SetUp();
        StartCoroutine(startCoroutine);
    }

    public IEnumerator SetUp()
    {
        yield return null;
        yield return null;

        if (!NPCZDOUtils.GetTrueDeath(m_nview))
        {
            InvokeRepeating("DestroyNow", m_ttl, 1f);
        }
    }

    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        DestroyNow();

        return false;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        // TODO raise zombies
        // Spawn dragur
        // Delete ragdoll
        return false;
    }

    public string GetHoverText()
    {
        if (m_nview != null && m_nview.GetZDO() != null && NPCZDOUtils.GetTrueDeath(m_nview))
        {
            return Localization.instance.Localize(
                $"{m_nview.GetZDO().GetString(ZDOVars.s_tamedName)}\n" +
                "[<color=yellow><b>$KEY_Use</b></color>] Bury Body\n" +
                "[<color=yellow><b>F</b></color>] Pay Respects"); ;
        }

        return "";
    }

    public string GetHoverName()
    {
        return "";
    }

}