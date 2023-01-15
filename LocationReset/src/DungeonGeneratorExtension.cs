using System.Collections;
using UnityEngine;

namespace VentureValheim.LocationReset
{
    public static class DungeonGeneratorExtension
    {
        public static void SetLastResetNow(this DungeonGenerator dg)
        {
            dg?.m_nview?.GetZDO()?.Set(LocationReset.LAST_RESET, LocationReset.GetGameDay());
        }

        public static int GetLastReset(this DungeonGenerator dg)
        {
            return dg?.m_nview?.GetZDO()?.GetInt(LocationReset.LAST_RESET, -1) ?? -1;
        }

        public static bool NeedsReset(this DungeonGenerator dg)
        {
            var lastReset = dg.GetLastReset();
            if (lastReset < 0)
            {
                dg.SetLastResetNow();
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
    /// Component to add a timed reset to parent Dungeon.
    /// Wait time needed for objects to load into the scene.
    /// </summary>
    public class DungeonGeneratorReset : MonoBehaviour
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
            var dg = gameObject.GetComponent<DungeonGenerator>();
            if (dg != null)
            {
                while (!LocationReset.LocalPlayerBeyondRange(dg.transform.position))
                {
                    if (LocationReset.LocalPlayerInRange(dg.transform.position))
                    {
                        LocationReset.Instance.TryReset(dg);
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