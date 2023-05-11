using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace VentureValheim.LocationReset
{
    public static class DungeonGeneratorExtension
    {
        public const string RPCNAME_DGSetLastResetNow = "VV_DGSetLastResetNow";

        public static bool SetLastResetNow(this DungeonGenerator dg)
        {
            if (dg.m_nview?.GetZDO() == null || !dg.m_nview.IsOwner())
            {
                return false;
            }

            dg.m_nview.GetZDO().Set(LocationReset.LAST_RESET, LocationReset.GetGameDay());
            return true;
        }

        public static int GetLastReset(this DungeonGenerator dg)
        {
            return dg.m_nview?.GetZDO()?.GetInt(LocationReset.LAST_RESET, -1) ?? -1;
        }

        /// <summary>
        /// Checks if a DungeonGenerator needs a reset, if no time has been previously recorded
        /// sets the current day as the last reset time.
        /// </summary>
        /// <param name="dg"></param>
        /// <returns></returns>
        public static bool NeedsReset(this DungeonGenerator dg)
        {
            var lastReset = dg.GetLastReset();
            if (lastReset < 0)
            {
                dg.SetLastResetNow();
                return false;
            }

            var timePassed = LocationReset.GetGameDay() - lastReset;
            var resetTime = LocationResetPlugin.GetResetTime(dg.name);

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
            yield return new WaitForSeconds(5);
            yield return null;
            var dg = gameObject.GetComponent<DungeonGenerator>();
            if (dg != null)
            {
                int tries = 0;
                float range = LocationReset.GetResetRange(dg.transform.position.y);

                while (!LocationReset.LocalPlayerBeyondRange(dg.transform.position))
                {
                    if (LocationReset.LocalPlayerInRange(dg.transform.position, range) &&
                        ZNetScene.instance.IsAreaReady(dg.transform.position))
                    {
                        if (dg.m_nview != null && dg.m_nview.IsOwner())
                        {
                            LocationReset.Instance.TryReset(dg);
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