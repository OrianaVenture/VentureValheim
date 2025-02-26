using System.Collections;

namespace VentureValheim.NPCS;

public class NPCHumanoid : Humanoid, Interactable, Hoverable, INPC
{
    public NPCData Data { get; protected set; }

    public override void Awake()
    {
        base.Awake();

        Data = new NPCData(this);

        var startCoroutine = SetUp();
        StartCoroutine(startCoroutine);
    }

    public override void Start()
    {
        // Prevent giving default items if initialized
        if (!NPCZDOUtils.GetInitialized(m_nview))
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

        Data.Setup();
    }

    public override void CustomFixedUpdate(float fixedDeltaTime)
    {
        if (!m_nview.IsValid())
        {
            return;
        }

        base.CustomFixedUpdate(fixedDeltaTime);

        if (m_nview.GetZDO() != null && m_nview.IsOwner())
        {
            Data.UpdateAttach();
        }
    }

    public override void OnDeath()
    {
        NPCUtils.OnDeath(this);
    }

    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        return NPCUtils.TryInteract(this);
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        return NPCUtils.TryUseItem(this, item);
    }

    public override string GetHoverText()
    {
        return NPCUtils.GetHoverText(this, m_baseAI);
    }

    public override string GetHoverName()
    {
        return m_name;
    }

    public override bool IsAttached()
    {
        return Data.IsAttached() || base.IsAttached();
    }

    public void Attach(bool attach, Chair chair = null)
    {
        Data.Attach(attach, "",chair);
    }

    public override void AttachStop()
    {
        Data.SetAttachStop();
    }
}
