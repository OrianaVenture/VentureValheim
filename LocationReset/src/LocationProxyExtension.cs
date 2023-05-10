using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace VentureValheim.LocationReset
{
    public static class LocationProxyExtension
    {
        public const string RPCNAME_LPSetLastResetNow = "VV_LPSetLastResetNow";

        public static void SetLastResetNow(this LocationProxy loc)
        {
            loc.m_nview?.InvokeRPC(ZRoutedRpc.Everybody, RPCNAME_LPSetLastResetNow);
        }

        public static void RPC_SetLastResetNow(this LocationProxy loc, long sender)
        {
            if (loc.m_nview != null && loc.m_nview.IsOwner())
            {
                loc.m_nview.GetZDO()?.Set(LocationReset.LAST_RESET, LocationReset.GetGameDay());
            }
        }

        public static int GetLastReset(this LocationProxy loc)
        {
            return loc.m_nview?.GetZDO()?.GetInt(LocationReset.LAST_RESET, -1) ?? -1;
        }

        /// <summary>
        /// Checks if a LocationProxy needs a reset, if no time has been previously recorded
        /// sets the current day as the last reset time.
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static bool NeedsReset(this LocationProxy loc, int hash)
        {
            var lastReset = loc.GetLastReset();
            if (lastReset < 0)
            {
                loc.SetLastResetNow();
                return false;
            }

            var timePassed = LocationReset.GetGameDay() - lastReset;
            var resetTime = LocationResetPlugin.GetResetTime(hash);

            if (timePassed >= resetTime)
            {
                return true;
            }

            return false;
        }

        [HarmonyPatch(typeof(LocationProxy), nameof(LocationProxy.Awake))]
        public static class Patch_LocationProxy_Awake
        {
            private static void Postfix(LocationProxy __instance)
            {
                __instance.m_nview.Register(RPCNAME_LPSetLastResetNow, __instance.RPC_SetLastResetNow);
            }
        }
    }

    /// <summary>
    /// Component to add a timed reset to parent location.
    /// Wait time needed for objects to load into the scene.
    /// </summary>
    public class LocationProxyReset : MonoBehaviour
    {
        IEnumerator resetCoroutine;

        public void Start()
        {
            resetCoroutine = WaitForReset();
            StartCoroutine(resetCoroutine);
        }

        public IEnumerator WaitForReset()
        {
            yield return new WaitForSeconds(5);
            yield return null;
            var loc = gameObject.GetComponent<LocationProxy>();
            if (loc != null)
            {
                int hash = loc.m_nview?.GetZDO()?.GetInt("location") ?? 0;

                if (hash != 0)
                {
                    float range = LocationReset.GetResetRange(hash);

                    while (!LocationReset.LocalPlayerBeyondRange(loc.transform.position))
                    {
                        if (LocationReset.LocalPlayerInRange(loc.transform.position, range) &&
                            ZNetScene.instance.IsAreaReady(loc.transform.position))
                        {
                            LocationReset.Instance.TryReset(loc);
                            break;
                        }

                        yield return new WaitForSeconds(1);
                    }
                }
            }

            yield return null;
            Destroy(this);
        }

        public void OnDestroy()
        {
            if (resetCoroutine != null)
            {
                StopCoroutine(resetCoroutine);
            }
        }
    }
}