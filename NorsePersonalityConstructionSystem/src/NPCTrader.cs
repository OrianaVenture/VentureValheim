namespace VentureValheim.NPCS;

public class NPCTrader : Trader
{
    public void Setup()
    {
        // TODO: random talking only works when animator is checking standing
        NPCSPlugin.NPCSLogger.LogWarning($"Trader Setup!");
        var zdo = gameObject.GetComponent<ZNetView>().GetZDO();
        if (!TryGetComponent<LookAt>(out var trader))
        {
            gameObject.AddComponent<LookAt>();
        }

        m_items = NPCZDOUtils.GetNPCTradeItems(zdo);
        m_useItems = NPCZDOUtils.GetNPCTraderUseItems(zdo);
        m_randomTalk = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_TALKTEXTS, zdo, true);
        m_randomGreets = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_GREETTEXTS, zdo, true);
        m_randomGoodbye = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_GOODBYETEXTS, zdo, true);
        m_randomStartTrade = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_STARTTRADETEXTS, zdo, true);
        m_randomBuy = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_BUYTEXTS, zdo, true);
        m_randomSell = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_SELLTEXTS, zdo, true);
        m_randomGiveItemNo = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_NOTCORRECTTEXTS, zdo, true);
        m_randomUseItemAlreadyRecieved = NPCZDOUtils.GetNPCTexts(NPCZDOUtils.ZDOVar_NOTAVAILABLETEXTS, zdo, true);

        m_dialogHeight = 2.5f;
        m_hideDialogDelay = 5f;
    }
}
