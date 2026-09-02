using UnityEngine;

namespace VentureValheim.TravelTotems;

public class TravelTotemPortalArea : MonoBehaviour
{
    private void Awake()
    {

    }

    private void OnTriggerEnter(Collider colliderIn)
    {
        Player player = colliderIn.GetComponent<Player>();
        if (player && (Player.m_localPlayer == player))
        {
            TravelTotemMap.HandleTotemOnTriggerEnter();
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