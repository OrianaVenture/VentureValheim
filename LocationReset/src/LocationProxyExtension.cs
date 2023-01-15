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
                loc.SetLastResetNow();
                return false;
            }

            var timePassed = LocationReset.GetGameDay() - lastReset;
            var resetTime = LocationResetPlugin.GetResetTime();

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
        IEnumerator resetCoroutine;

        public void Start()
        {
            resetCoroutine = WaitForReset();
            StartCoroutine(resetCoroutine);
        }

        public IEnumerator WaitForReset()
        {
            yield return null;
            yield return new WaitForSeconds(5);
            var loc = gameObject.GetComponent<LocationProxy>();
            if (loc != null)
            {
                while (!LocationReset.LocalPlayerBeyondRange(loc.transform.position))
                {
                    if (LocationReset.LocalPlayerInRange(loc.transform.position))
                    {
                        LocationReset.Instance.TryReset(loc);
                        break;
                    }
                    yield return new WaitForSeconds(1);
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