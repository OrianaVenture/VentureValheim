using UnityEngine;

namespace VentureValheim.TravelTotems;

public class TravelTotemPortalTrigger : MonoBehaviour
{
    private TravelTotemPortal Trigger;

    private void Awake()
    {
        Trigger = GetComponentInParent<TravelTotemPortal>();
    }

    private void OnTriggerEnter(Collider colliderIn)
    {
        Player player = colliderIn.GetComponent<Player>();
        if (player && (Player.m_localPlayer == player))
        {
            TravelTotemMap.HandleTotemOnTriggerEnter(Trigger);
        }
    }

    private void OnTriggerExit(Collider colliderIn)
    {
        Player player = colliderIn.GetComponent<Player>();
        if (player && (Player.m_localPlayer == player))
        {
            TravelTotemMap.HandleTotemOnTriggerExit();
        }
    }
}