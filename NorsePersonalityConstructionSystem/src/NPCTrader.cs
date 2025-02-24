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
        m_randomTalk = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_TALKTEXTS, nview, true);
        m_randomGreets = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_GREETTEXTS, nview, true);
        m_randomGoodbye = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_GOODBYETEXTS, nview, true);
        m_randomStartTrade = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_STARTTRADETEXTS, nview, true);
        m_randomBuy = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_BUYTEXTS, nview, true);
        m_randomSell = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_SELLTEXTS, nview, true);
        m_randomGiveItemNo = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_NOTCORRECTTEXTS, nview, true);
        m_randomUseItemAlreadyRecieved = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_NOTAVAILABLETEXTS, nview, true);

        m_dialogHeight = 2.5f;
        m_hideDialogDelay = 5f;
    }
}
