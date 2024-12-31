using System.Collections;
using UnityEngine;

namespace VentureValheim.LocationReset
{
    public static class LocationProxyExtension
    {
        public static bool SetLastResetNow(this LocationProxy loc)
        {
            if (loc.m_nview == null || loc.m_nview.GetZDO() == null || !loc.m_nview.IsOwner())
            {
                return false;
            }

            loc.m_nview.GetZDO().Set(LocationReset.LAST_RESET, LocationReset.GetGameDay());
            return true;
        }

        public static int GetLastReset(this LocationProxy loc)
        {
            if (loc.m_nview == null || loc.m_nview.GetZDO() == null)
            {
                return -1;
            }

            return loc.m_nview.GetZDO().GetInt(LocationReset.LAST_RESET, -1);
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
                LocationResetPlugin.LocationResetLogger.LogDebug($"Location does not need a reset. No timer found.");
                return false;
            }

            var timePassed = LocationReset.GetGameDay() - lastReset;
            var resetTime = LocationResetPlugin.GetResetTime(hash);

            if (timePassed >= resetTime)
            {
                return true;
            }

            LocationResetPlugin.LocationResetLogger.LogDebug($"Location does not need a reset. {resetTime - timePassed} days remaining.");
            return false;
        }
    }

    /// <summary>
    /// Component to add a timed reset to parent location.
    /// Wait time needed for objects to load into the scene.
    /// </summary>
    public class LocationProxyReset : MonoBehaviour
    {
        private const float RESET_RANGE = 100f;
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
                int hash = 0;
                if (loc.m_nview != null && loc.m_nview.GetZDO() != null)
                {
                    hash = loc.m_nview.GetZDO().GetInt(ZDOVars.s_location, 0);
                }

                if (hash != 0 && !LocationReset.IgnoreLocationHashes.Contains(hash) &&
                    !LocationReset.Instance.CustomIgnoreLocationHashes.Contains(hash))
                {
                    int tries = 0;

                    while (!LocationReset.LocalPlayerBeyondRange(loc.transform.position))
                    {
                        if (LocationReset.LocalPlayerInRange(loc.transform.position, RESET_RANGE) &&
                            ZNetScene.instance.IsAreaReady(loc.transform.position))
                        {
                            if (loc.m_nview != null && loc.m_nview.IsOwner())
                            {
                                LocationReset.Instance.TryReset(loc, hash);
                                break;
                            }
                            else
                            {
                                tries++;
                            }

                            if (tries > 100)
                            {
                                break;
                            }
                        }

                        yield return new WaitForSeconds(1);
                    }
                }
                else
                {
                    LocationResetPlugin.LocationResetLogger.LogDebug($"Location with hash {hash} is ignored. Skipping.");
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