using System;
using System.Collections.Generic;
using System.Reflection;
using Jotunn;
using UnityEngine;

namespace VentureValheim.VentureQuest
{
    public class NPCFactory
    {
        private static List<Type> RemoveComponents = new List<Type>
        {
            typeof(PlayerController),
            typeof(Talker),
            typeof(Skills),
            typeof(CharacterDrop)
        };

        public static GameObject SpawnNPC(Vector3 position, Quaternion rotation, string name = "Ragnar", string model = "Player")
        {
            var prefabName = "VQ_" + model;
            var prefab = ZNetScene.instance.GetPrefab(prefabName.GetStableHashCode());
            if (prefab == null)
            {
                VentureQuestPlugin.VentureQuestLogger.LogError("No prefab found");
                return null;
            }

            var npc = GameObject.Instantiate(prefab, position, rotation);

            var npcComponent = npc.GetComponent<NPC>();
            npcComponent.SetRandom();
            npcComponent.m_name = name;
            npcComponent.SetName(name);
            var text = "Who you talkin' to? I'm busy you know, have all this stuff to do like picking berries and slaying trolls! " +
                "I don't have time for chit chattin' all day!";
            npcComponent.SetText(text);
            npcComponent.SetSpawnPoint(position);
            return npc;
        }

        public static GameObject RespawnNPC(GameObject original)
        {
            UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
            var chance = UnityEngine.Random.Range(0, 10);

            var prefabName = Utils.GetPrefabName(original.name);
            var prefab = ZNetScene.instance.GetPrefab(prefabName.GetStableHashCode());
            var npcOriginal = original.GetComponent<NPC>();

            var respawn = npcOriginal.m_nview.GetZDO().GetVec3(NPC.ZDOVar_SPAWNPOINT, Vector3.zero);
            if (respawn != Vector3.zero && chance < 5)
            {
                var gameobject = NPC.Instantiate(prefab, respawn, Quaternion.identity);
                var npcNew = gameobject.GetComponent<NPC>();
                var zdo = npcNew.m_nview.GetZDO();

                var text = npcOriginal.m_nview.GetZDO().GetVec3(NPC.ZDOVar_TEXT, Vector3.zero);
                var sitting = npcOriginal.m_nview.GetZDO().GetBool(NPC.ZDOVar_SITTING, false);
                var name = npcOriginal.m_nview.GetZDO().GetString(ZDOVars.s_tamedName, "Respawned Ragnar");

                zdo.Set(NPC.ZDOVar_SPAWNPOINT, respawn);
                zdo.Set(NPC.ZDOVar_TEXT, text);
                zdo.Set(NPC.ZDOVar_SITTING, sitting);
                zdo.Set(ZDOVars.s_tamedName, name);

                NPC.CopyVisEquipment(ref npcNew.m_visEquipment, npcOriginal.m_visEquipment);

                return gameobject;
            }

            return null;
        }

        public static void AddNPCS()
        {
            CreateNPC("Player");
            CreateNPC("Skeleton");

            //PrefabManager.OnPrefabsRegistered -= AddNPCS;
        }

        public static GameObject CreateNPC(string model)
        {
            if (ZNetScene.instance == null)
            {
                VentureQuestPlugin.VentureQuestLogger.LogError("ZNetScene not ready");
            }

            var prefab = ZNetScene.instance.GetPrefab(model.GetStableHashCode());
            if (prefab == null)
            {
                VentureQuestPlugin.VentureQuestLogger.LogError("No prefab found");
                return null;
            }

            var prefabActive = prefab.activeSelf;
            prefab.SetActive(false);

            GameObject npc = GameObject.Instantiate(prefab, VentureQuestPlugin.Root.transform, false);
            npc.name = "VQ_" + model;
            npc.transform.SetParent(VentureQuestPlugin.Root.transform, false);

            foreach (var remove in RemoveComponents)
            {
                var comp = npc.GetComponent(remove);
                if (comp != null)
                {
                    UnityEngine.Object.DestroyImmediate(comp);
                }
            }

            // Edit existing components
            Humanoid prefabHumanoid = prefab.GetComponent<Player>();
            Humanoid humanoid = npc.GetComponent<Player>();

            if (prefabHumanoid == null)
            {
                prefabHumanoid = prefab.GetComponent<Humanoid>();
                humanoid = npc.GetComponent<Humanoid>();
            }

            if (prefabHumanoid != null)
            {
                UnityEngine.Object.DestroyImmediate(humanoid);
                var human = npc.AddComponent<NPC>();

                var fields = typeof(Humanoid).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                //VentureQuestPlugin.VentureQuestLogger.LogDebug($"Fields found: {fields.Length}");

                foreach (System.Reflection.FieldInfo field in fields)
                {
                    try
                    {
                        var value = field.GetValue(prefabHumanoid);
                        field.SetValue(human, value);
                        //VentureQuestPlugin.VentureQuestLogger.LogDebug($"Setting {field} to {value}..");
                    }
                    catch { }
                }

                // TODO properly clear lists as needed
                if (model.Equals("Player"))
                {
                    human.m_defaultItems = new GameObject[] { };
                }
            }

            var baseAI = npc.GetOrAddComponent<BaseAI>();
            var character = npc.GetComponent<Character>();
            character.m_baseAI = baseAI;

            var znetview = npc.GetComponent<ZNetView>();
            znetview.m_persistent = true;
            znetview.m_type = ZDO.ObjectType.Default;

            var zsync = npc.GetComponent<ZSyncTransform>();
            zsync.m_syncBodyVelocity = false;
            zsync.m_characterParentSync = false;

            // Restore active
            prefab.SetActive(prefabActive);
            npc.SetActive(true);

            // Register prefab
            //PrefabManager.Instance.AddPrefab(npc);
            ZNetScene.instance.m_prefabs.Add(npc);
            ZNetScene.instance.m_namedPrefabs.Add(npc.name.GetStableHashCode(), npc);

            return npc;
        }
    }
}

/*public static void AddNPC()
{
    var assets = AssetUtils.LoadAssetBundleFromResources("vv_quest", Assembly.GetExecutingAssembly());
    var go = assets.LoadAsset<GameObject>(NPC.NPC_NAME);
    PrefabManager.Instance.AddPrefab(go);

    PrefabManager.OnVanillaPrefabsAvailable -= AddNPC;
}

public static void SpawnNPC(Vector3 position, string model = "Player")
{
    var prefab = ZNetScene.instance.GetPrefab(model.GetStableHashCode());
    if (prefab == null)
    {
        VentureQuestPlugin.VentureQuestLogger.LogError("No prefab found");
        return;
    }

    var visual = Utils.FindChild(prefab.transform, "Visual");

    if (visual == null)
    {
        VentureQuestPlugin.VentureQuestLogger.LogError("No visual on prefab found");
        return;
    }

    if (NPCGameObject == null)
    {
        NPCGameObject = ZNetScene.instance.GetPrefab(NPC_NAME.GetStableHashCode());
    }


}*/