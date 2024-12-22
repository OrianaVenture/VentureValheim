using UnityEngine;

namespace VentureValheim.LocationReset
{
    internal static class GameObjectExtensions
    {
        private const string CloneSuffix = "(Clone)";
        private const string MineRock5Name = "___MineRock5";

        /// <summary>
        /// Gets the root Prefab name using Utils.GetPrefabName 
        /// or if the object is a MineRock5 using the MineRock5PrefabTracker.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        internal static string GetPrefabName(this GameObject gameObject)
        {
            string prefabName = gameObject.name;
            if (prefabName.Contains(MineRock5Name))
            {
                if (gameObject.TryGetComponent(out MineRock5PrefabTracker tracker))
                {
                    return tracker.m_prefabName;
                }
            }
            return Utils.GetPrefabName(gameObject);
        }


        /// <summary>
        ///     Adds MineRock5Tracker component and sets `m_prefabName` to be the root prefab name of the GameObject.
        /// </summary>
        /// <param name="gameObject"></param>
        internal static void AddMineRock5PrefabTracker(this GameObject gameObject)
        {
            if (gameObject.GetComponent<MineRock5PrefabTracker>())
            {
                return;
            }
            var mineRock5PrefabTracker = gameObject.AddComponent<MineRock5PrefabTracker>();
            mineRock5PrefabTracker.m_prefabName = gameObject.GetPrefabName();
        }
    }
}
