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
        npcComponent.SetSpawnPoint(position); // TODO remove? only set if added via bed?
        return npc;
    }

    public static GameObject SpawnSavedNPC(Vector3 position, Quaternion rotation, string id)
    {
        var config = NPCConfiguration.GetConfig(id);
        if (config == null)
        {
            return null;
        }

        var prefabName = "VQ_" + config.Model;
        var prefab = ZNetScene.instance.GetPrefab(prefabName.GetStableHashCode());
        if (prefab == null)
        {
            VentureQuestPlugin.VentureQuestLogger.LogError("No prefab found");
            return null;
        }

        var npc = GameObject.Instantiate(prefab, position, rotation);

        var npcComponent = npc.GetComponent<NPC>();
        npcComponent.SetFromConfig(config);
        npcComponent.SetSpawnPoint(position); // TODO remove? only set if added via bed?
        return npc;
    }

    public static GameObject RespawnNPC(GameObject original)
    {
        var prefabName = Utils.GetPrefabName(original.name);
        var prefab = ZNetScene.instance.GetPrefab(prefabName.GetStableHashCode());
        var npcOriginal = original.GetComponent<NPC>();

        var respawn = npcOriginal.m_nview.GetZDO().GetVec3(NPC.ZDOVar_SPAWNPOINT, Vector3.zero);
        if (respawn != Vector3.zero)
        {
            var gameobject = NPC.Instantiate(prefab, respawn, Quaternion.identity);
            var npcNew = gameobject.GetComponent<NPC>();
            var zdo = npcNew.m_nview.GetZDO();

            NPC.CopyZDO(ref zdo, npcOriginal.m_nview.GetZDO());
            NPC.CopyVisEquipment(ref npcNew.m_visEquipment, npcOriginal.m_visEquipment);

            return gameobject;
        }
        else
        {
            VentureQuestPlugin.VentureQuestLogger.LogDebug("No spawn point found!");
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

            foreach (FieldInfo field in fields)
            {
                try
                {
                    var value = field.GetValue(prefabHumanoid);
                    field.SetValue(human, value);
                }
                catch { }
            }

            // Setup Ragdoll TODO test
            foreach (var effect in human.m_deathEffects.m_effectPrefabs)
            {
                if (effect.m_prefab.name.Equals("Player_ragdoll"))
                {
                    var ragdollActive = effect.m_prefab.activeSelf;
                    effect.m_prefab.SetActive(false);

                    GameObject npcRagdoll = GameObject.Instantiate(effect.m_prefab, VentureQuestPlugin.Root.transform, false);
                    npcRagdoll.name = "VQ_" + effect.m_prefab.name;
                    var ragdollFields = typeof(Ragdoll).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                    var originalRagdollComponent = effect.m_prefab.GetComponent<Ragdoll>();
                    var ragdollComponent = npcRagdoll.GetComponent<Ragdoll>();
                    UnityEngine.Object.DestroyImmediate(ragdollComponent);
                    var npcRagdollComponent = npcRagdoll.AddComponent<NPCRagdoll>();

                    foreach (FieldInfo ragdollField in ragdollFields)
                    {
                        try
                        {
                            var value = ragdollField.GetValue(originalRagdollComponent);
                            ragdollField.SetValue(npcRagdollComponent, value);
                        }
                        catch { }
                    }

                    var znetviewRagdoll = npcRagdoll.GetComponent<ZNetView>();
                    znetviewRagdoll.m_persistent = true;
                    znetviewRagdoll.m_type = ZDO.ObjectType.Default;

                    // Set up interractable by adding collider and changing layer
                    npcRagdoll.layer = 0;

                    var bodies = npcRagdoll.GetComponentsInChildren<Rigidbody>();
                    foreach (var body in bodies)
                    {
                        body.gameObject.layer = 0;
                    }

                    effect.m_prefab = npcRagdoll;

                    ZNetScene.instance.m_prefabs.Add(npcRagdoll);
                    ZNetScene.instance.m_namedPrefabs.Add(npcRagdoll.name.GetStableHashCode(), npcRagdoll);

                    effect.m_prefab.SetActive(ragdollActive);
                    npcRagdoll.SetActive(ragdollActive);
                    break;
                }
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
            npc.AddComponent<NPCAI>();
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