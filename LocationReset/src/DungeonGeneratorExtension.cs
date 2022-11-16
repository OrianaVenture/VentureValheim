using System.Collections;
using UnityEngine;

namespace VentureValheim.LocationReset
{
    public static class DungeonGeneratorExtension
    {
        // TODO CLEANUP
        private const string LastReset = "VV_LastReset";

        public static void SetLastResetNow(this DungeonGenerator dg)
        {
            dg?.m_nview?.GetZDO()?.Set(LastReset, LocationReset.GetGameDay());
        }

        public static int GetLastReset(this DungeonGenerator dg)
        {
            return dg?.m_nview?.GetZDO()?.GetInt(LastReset, -1) ?? -1;
        }

        public static bool NeedsReset(this DungeonGenerator dg)
        {
            var lastReset = dg.GetLastReset();
            if (lastReset < 0)
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"No reset timer found for location. Adding one now.");
                dg.SetLastResetNow();
                return false;
            }

            var timePassed = LocationReset.GetGameDay() - lastReset;
            var resetTime = LocationResetPlugin.GetResetTime();

            LocationResetPlugin.LocationResetLogger.LogDebug($"Reset timer found. Time passed: {timePassed}, last reset time was {lastReset}.");

            if (timePassed >= resetTime)
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Component to add a timed reset to parent Dungeon.
    /// Wait time needed for objects to load into the scene.
    /// </summary>
    public class DungeonGeneratorReset : MonoBehaviour
    {
        public IEnumerator Start()
        {
            yield return null;
            yield return new WaitForSeconds(5);
            var dg = gameObject.GetComponent<DungeonGenerator>();
            if (dg != null)
            {
                var position = dg.transform.position;
                while (!LocationReset.LocalPlayerInRange(position))
                {
                    yield return new WaitForSeconds(1);
                }

                LocationReset.Instance.TryReset(dg);
            }

            yield return null;
            Destroy(this);
        }
    }
}