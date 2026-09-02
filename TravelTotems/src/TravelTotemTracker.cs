using UnityEngine;

namespace VentureValheim.TravelTotems;

public class TravelTotemTracker : MonoBehaviour
{
    public static TravelTotemTracker Instance;
    public ZNetView ZNetView;

    private void Awake()
    {
        ZNetView = GetComponent<ZNetView>();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            TravelTotemsPlugin.TravelTotemsLogger.LogWarning("Multiple Totem trackers found! IDs may not be tracked accurately!");
            // TODO: destroy extra ones?
            return;
        }
    }

    private void Start()
    {
        if (TravelTotems.TravelTotemTrackerZDO == null)
        {
            TravelTotems.TravelTotemTrackerZDO = Instance.ZNetView.GetZDO();
            TravelTotems.InitializeTracker();
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}