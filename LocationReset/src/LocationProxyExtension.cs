using System.Collections;
using UnityEngine;

namespace VentureValheim.LocationReset
{
    public static class LocationProxyExtension
    {
        public static void SetLastResetNow(this LocationProxy loc)
        {
            loc?.m_nview?.GetZDO()?.Set(LocationReset.LAST_RESET, LocationReset.GetGameDay());
        }

        public static int GetLastReset(this LocationProxy loc)
        {
            return loc?.m_nview?.GetZDO()?.GetInt(LocationReset.LAST_RESET, -1) ?? -1;
        }

        public static bool NeedsReset(this LocationProxy loc)
        {
            var lastReset = loc.GetLastReset();
            if (lastReset < 0)
            {
                //LocationResetPlugin.LocationResetLogger.LogDebug($"No reset timer found for location. Adding one now.");
                loc.SetLastResetNow();
                return false;
            }

            var timePassed = LocationReset.GetGameDay() - lastReset;
            var resetTime = LocationResetPlugin.GetResetTime();

            //LocationResetPlugin.LocationResetLogger.LogDebug($"Reset timer found. Time passed: {timePassed}, last reset time was {lastReset}.");

            if (timePassed >= resetTime)
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Component to add a timed reset to parent location.
    /// Wait time needed for objects to load into the scene.
    /// </summary>
    public class LocationProxyReset : MonoBehaviour
    {
        public IEnumerator Start()
        {
            LocationResetPlugin.LocationResetLogger.LogDebug($"LocationProxyReset Starting...");
            yield return null;
            yield return new WaitForSeconds(5);
            var loc = gameObject.GetComponent<LocationProxy>();
            if (loc != null)
            {
                var position = loc.transform.position;
                while (!LocationReset.LocalPlayerInRange(position))
                {
                    yield return new WaitForSeconds(1);
                }

                LocationReset.Instance.TryReset(loc);
            }

            yield return null;
            LocationResetPlugin.LocationResetLogger.LogDebug($"LocationProxyReset Destroying Self...");
            Destroy(this);
        }
    }
}