namespace VentureValheim.NPCS;

public class NPCTrader : Trader
{
    public void Setup()
    {
        // TODO: random talking only works when animator is checking standing
        NPCSPlugin.NPCSLogger.LogWarning($"Trader Setup!");
        var nview = gameObject.GetComponent<ZNetView>();
        if (!TryGetComponent<LookAt>(out var trader))
        {
            gameObject.AddComponent<LookAt>();
        }

        m_items = NPCZDOUtils.GetNPCTradeItems(nview);
        m_useItems = NPCZDOUtils.GetNPCTraderUseItems(nview);
        m_randomTalk = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_TALKTEXTS, nview);
        m_randomGreets = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_GREETTEXTS, nview);
        m_randomGoodbye = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_GOODBYETEXTS, nview);
        m_randomStartTrade = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_STARTTRADETEXTS, nview);
        m_randomBuy = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_BUYTEXTS, nview);
        m_randomSell = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_SELLTEXTS, nview);
        m_randomGiveItemNo = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_NOTCORRECTTEXTS, nview);
        m_randomUseItemAlreadyRecieved = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_NOTAVAILABLETEXTS, nview);

        m_dialogHeight = 2.5f;
        m_hideDialogDelay = 5f;
    }
}
