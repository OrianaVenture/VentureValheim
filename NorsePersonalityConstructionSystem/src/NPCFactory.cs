using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace VentureValheim.NPCS;

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
        var prefabName = NPCSPlugin.MOD_PREFIX + model;
        var prefab = ZNetScene.instance.GetPrefab(prefabName.GetStableHashCode());
        if (prefab == null)
        {
            NPCSPlugin.NPCSLogger.LogError("No prefab found");
            return null;
        }

        var npc = GameObject.Instantiate(prefab, position, rotation);

        var npcComponent = npc.GetComponent<NPC>();
        if (model.Equals("Player"))
        {
            npcComponent.SetRandom();
        }
        npcComponent.SetName(name);
        npcComponent.SetSpawnPoint(position);
        return npc;
    }

    public static GameObject SpawnSavedNPC(Vector3 position, Quaternion rotation, string id)
    {
        var config = NPCConfiguration.GetConfig(id);
        if (config == null)
        {
            return null;
        }

        var prefabName = NPCSPlugin.MOD_PREFIX + config.Model;
        var prefab = ZNetScene.instance.GetPrefab(prefabName.GetStableHashCode());
        if (prefab == null)
        {
            NPCSPlugin.NPCSLogger.LogError("No prefab found");
            return null;
        }

        var npc = GameObject.Instantiate(prefab, position, rotation);

        var npcComponent = npc.GetComponent<NPC>();
        npcComponent.SetFromConfig(config);
        npcComponent.SetSpawnPoint(position);
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
            NPCSPlugin.NPCSLogger.LogDebug("No spawn point found!");
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
            NPCSPlugin.NPCSLogger.LogError("ZNetScene not ready");
        }

        var prefab = ZNetScene.instance.GetPrefab(model.GetStableHashCode());
        if (prefab == null)
        {
            NPCSPlugin.NPCSLogger.LogError("No prefab found");
            return null;
        }

        // Set up copy of prefab
        var prefabActive = prefab.activeSelf;
        prefab.SetActive(false);

        GameObject npc = GameObject.Instantiate(prefab, NPCSPlugin.Root.transform, false);
        npc.name = NPCSPlugin.MOD_PREFIX + model;
        npc.transform.SetParent(NPCSPlugin.Root.transform, false);

        foreach (var remove in RemoveComponents)
        {
            var comp = npc.GetComponent(remove);
            if (comp != null)
            {
                UnityEngine.Object.DestroyImmediate(comp);
            }
        }

        // Edit existing components
        Humanoid prefabHumanoid = prefab.GetComponent<Humanoid>();
        Humanoid humanoid = npc.GetComponent<Humanoid>();

        if (prefabHumanoid != null)
        {
            // Copy humanoid component into new NPC component
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

            // Setup Ragdoll for Player NPCs
            foreach (var effect in human.m_deathEffects.m_effectPrefabs)
            {
                if (effect.m_prefab.name.Equals("Player_ragdoll"))
                {
                    var ragdollActive = effect.m_prefab.activeSelf;
                    effect.m_prefab.SetActive(false);

                    GameObject npcRagdoll = GameObject.Instantiate(effect.m_prefab, NPCSPlugin.Root.transform, false);
                    npcRagdoll.name = NPCSPlugin.MOD_PREFIX + effect.m_prefab.name;
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

                    // Set up intractable by changing layers
                    npcRagdoll.layer = 0;

                    var bodies = npcRagdoll.GetComponentsInChildren<Rigidbody>();
                    foreach (var body in bodies)
                    {
                        body.gameObject.layer = 0;
                    }

                    // Restore active
                    npcRagdoll.SetActive(ragdollActive);
                    effect.m_prefab = npcRagdoll;

                    // Register prefab
                    ZNetScene.instance.m_prefabs.Add(npcRagdoll);
                    ZNetScene.instance.m_namedPrefabs.Add(npcRagdoll.name.GetStableHashCode(), npcRagdoll);

                    break;
                }
            }

            // TODO: handle default equipment for all types of npcs
            if (model.Equals("Player"))
            {
                human.m_defaultItems = new GameObject[] { };
                human.m_walkSpeed = 2f;
                human.m_speed = 2f;
                human.m_runSpeed = 4f;
                human.m_health = 200f;
            }

            // Prevent hobo fight club
            human.m_faction = Character.Faction.Players;
            human.m_tamed = false; // TODO
            human.m_group = "VV_NPC";
        }

        // TODO
        /*var tamable = npc.GetComponent<Tameable>();
        if (tamable == null)
        {
            npc.AddComponent<NPCTamable>();
        }*/

        // Set up AI to ensure NPCs respect players
        var baseAI = npc.GetComponent<BaseAI>();
        if (baseAI == null)
        {
            npc.AddComponent<NPCAI>();
        }
        else if (baseAI is MonsterAI)
        {
            var monsterAI = prefab.GetComponent<MonsterAI>();
            UnityEngine.Object.DestroyImmediate(baseAI);
            var npcai = npc.AddComponent<NPCAI>();

            var fields = typeof(MonsterAI).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                try
                {
                    var value = field.GetValue(monsterAI);
                    field.SetValue(npcai, value);
                }
                catch { }
            }

            NPCAI.SetupExisting(ref npcai);
        }

        var znetview = npc.GetComponent<ZNetView>();
        znetview.m_persistent = true;
        znetview.m_type = ZDO.ObjectType.Default;

        var zsync = npc.GetComponent<ZSyncTransform>();
        zsync.m_syncPosition = true;
        zsync.m_syncRotation = true;
        zsync.m_syncBodyVelocity = false;
        zsync.m_characterParentSync = false;

        // Restore active
        prefab.SetActive(prefabActive);
        npc.SetActive(true);

        // Register prefab
        //PrefabManager.Instance.AddPrefab(npc);
        ZNetScene.instance.m_prefabs.Add(npc);
        ZNetScene.instance.m_namedPrefabs.Add(npc.name.GetStableHashCode(), npc);

        NPCSPlugin.NPCSLogger.LogDebug($"Added prefab for {npc.name}");

        return npc;
    }
}