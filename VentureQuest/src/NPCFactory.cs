using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace VentureValheim.VentureQuest;

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
        npcComponent.SetName(name);
        npcComponent.SetSpawnPoint(position);

        //var text = "Can you bring me some {useitem}? It is so dear to me that I will give you {reward}!";
        //npcComponent.SetText(text);
        //npcComponent.SetUseItem("DeerStew");
        //npcComponent.SetReward(text, "DeerStew", 1, "Coins", 10, "Wow you actually did it. I'm impressed.", -1);
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

            NPC.CopyZDO(ref zdo, npcOriginal.m_nview.GetZDO());
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

        var baseAI = npc.GetComponent<BaseAI>();

        if (baseAI == null)
        {
            baseAI = npc.AddComponent<NPCAI>();
        }
        //var character = npc.GetComponent<Character>();
        //character.m_baseAI = baseAI;

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