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
        typeof(CharacterDrop),
        typeof(NpcTalk),
        typeof(Tameable)
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

        var npcComponent = npc.GetComponent<INPC>();
        if (npcComponent != null)
        {
            if (model.Equals("Player"))
            {
                npcComponent.SetRandom();
            }

            npcComponent.SetName(name);
            npcComponent.SetSpawnPoint(position);
        }
        
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

        var npcComponent = npc.GetComponent<INPC>();
        if (npcComponent != null)
        {
            npcComponent.SetFromConfig(config, true);
            npcComponent.SetSpawnPoint(position);
        }

        return npc;
    }

    public static GameObject RespawnNPC(GameObject original)
    {
        ZNetView originalZNetView = original.GetComponent<ZNetView>();
        if (originalZNetView == null)
        {
            return null;
        }

        var respawn = NPCUtils.GetSpawnPoint(originalZNetView);
        if (respawn != Vector3.zero)
        {
            var prefabName = Utils.GetPrefabName(original.name);
            var prefab = ZNetScene.instance.GetPrefab(prefabName.GetStableHashCode());

            if (prefab == null)
            {
                NPCSPlugin.NPCSLogger.LogDebug("Issue finding prefab!");
                return null;
            }

            var gameobject = GameObject.Instantiate(prefab, respawn, Quaternion.identity);
            ZNetView newZNetView = gameobject.GetComponent<ZNetView>();

            if (newZNetView == null)
            {
                NPCSPlugin.NPCSLogger.LogDebug("Issue instantiating prefab!");
                return null;
            }

            NPCUtils.CopyZDO(ref newZNetView, originalZNetView);

            VisEquipment originalVisEquipment = original.GetComponent<VisEquipment>();
            VisEquipment newVisEquipment = gameobject.GetComponent<VisEquipment>();

            if (originalVisEquipment != null && newVisEquipment != null)
            {
                NPCUtils.CopyVisEquipment(ref newVisEquipment, originalVisEquipment);
            }

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
        CreateNPC("Asksvin");
        CreateNPC("Boar");
        CreateNPC("Charred_Melee");
        CreateNPC("Deer");
        CreateNPC("Draugr");
        CreateNPC("Dverger");
        CreateNPC("Fenring");
        CreateNPC("Fenring_Cultist");
        CreateNPC("Ghost");
        CreateNPC("Goblin");
        CreateNPC("GoblinShaman");
        CreateNPC("Greydwarf");
        CreateNPC("Greyling");
        CreateNPC("Lox");
        CreateNPC("Neck");
        CreateNPC("Skeleton");
        CreateNPC("Troll");
        CreateNPC("Wolf");
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

        GameObject npc = NPCUtils.CreateGameObject(prefab, model);

        foreach (var remove in RemoveComponents)
        {
            var comp = npc.GetComponent(remove);
            if (comp != null)
            {
                UnityEngine.Object.DestroyImmediate(comp);
            }
        }

        // Edit existing components
        Humanoid originalHumanoid = prefab.GetComponent<Humanoid>();
        Humanoid newHumanoid = npc.GetComponent<Humanoid>();

        if (originalHumanoid != null)
        {
            // Copy humanoid component into new NPC component
            UnityEngine.Object.DestroyImmediate(newHumanoid);
            var npcHumanoid = npc.AddComponent<NPCHumanoid>();

            SetupHumanoidNPC(ref npcHumanoid, originalHumanoid);

            // TODO: handle default equipment for all types of npcs
            if (model.Equals("Player"))
            {
                npcHumanoid.m_defaultItems = new GameObject[] { };
                npcHumanoid.m_walkSpeed = 2f;
                npcHumanoid.m_speed = 2f;
                npcHumanoid.m_runSpeed = 4f;
                npcHumanoid.m_health = 200f;
            }

            // Make sure to set this to the new object component, otherwise attacks are broken
            npcHumanoid.m_eye = Utils.FindChild(npcHumanoid.gameObject.transform, "EyePos");

            // Prevent hobo fight club
            npcHumanoid.m_faction = Character.Faction.Dverger;
            npcHumanoid.m_tamed = false; // TODO
            npcHumanoid.m_group = NPCUtils.NPCGROUP; // TODO make it so NPCs can fight each other
        }
        else
        {
            // Setup for animals
            Character originalCharacter = prefab.GetComponent<Character>();
            Character newCharacter = npc.GetComponent<Character>();

            if (originalCharacter != null)
            {
                // Copy humanoid component into new NPC component
                UnityEngine.Object.DestroyImmediate(newCharacter);
                var npcCharacter = npc.AddComponent<NPCCharacter>();

                SetupCharacterNPC(ref npcCharacter, originalCharacter);

                // Prevent hobo fight club
                npcCharacter.m_faction = Character.Faction.Dverger;
                npcCharacter.m_tamed = false; // TODO
                npcCharacter.m_group = NPCUtils.NPCGROUP; // TODO make it so NPCs can fight each other
            }
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
            var originalAI = prefab.GetComponent<MonsterAI>();
            UnityEngine.Object.DestroyImmediate(baseAI);
            var npcAI = npc.AddComponent<NPCAI>();

            SetupMonsterAI(ref npcAI, originalAI);
        }
        else if (baseAI is AnimalAI)
        {
            var originalAI = prefab.GetComponent<AnimalAI>();
            UnityEngine.Object.DestroyImmediate(baseAI);
            var npcAI = npc.AddComponent<NPCAnimalAI>();

            SetupAnimalAI(ref npcAI, originalAI);
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
        npc.SetActive(prefabActive);

        // Register prefab
        NPCUtils.RegisterGameObject(npc);

        return npc;
    }

    private static void SetupHumanoidNPC(ref NPCHumanoid npcHumanoid, Humanoid original)
    {
        var fields = typeof(Humanoid).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (FieldInfo field in fields)
        {
            try
            {
                var value = field.GetValue(original);
                field.SetValue(npcHumanoid, value);
            }
            catch { }
        }

        // Setup Ragdolls
        var effectList = npcHumanoid.m_deathEffects.m_effectPrefabs;
        for (int lcv = 0; lcv < effectList.Length; lcv++)
        {
            var effect = effectList[lcv];
            if (effect.m_prefab.GetComponent<Ragdoll>())
            {
                effect.m_prefab = SetupRagdoll(effect.m_prefab);
                break;
            }
        }
    }

    private static void SetupCharacterNPC(ref NPCCharacter npcCharacter, Character original)
    {
        var fields = typeof(Character).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (FieldInfo field in fields)
        {
            try
            {
                var value = field.GetValue(original);
                field.SetValue(npcCharacter, value);
            }
            catch { }
        }

        // Setup Ragdolls
        var effectList = npcCharacter.m_deathEffects.m_effectPrefabs;
        for (int lcv = 0; lcv < effectList.Length; lcv++)
        {
            var effect = effectList[lcv];
            if (effect.m_prefab.GetComponent<Ragdoll>())
            {
                effect.m_prefab = SetupRagdoll(effect.m_prefab);
                break;
            }
        }
    }

    private static GameObject SetupRagdoll(GameObject original)
    {
        var ragdollActive = original.activeSelf;
        original.SetActive(false);

        GameObject npcRagdoll = NPCUtils.CreateGameObject(original, original.name);
        var ragdollFields = typeof(Ragdoll).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        var originalRagdollComponent = original.GetComponent<Ragdoll>();
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

        // TODO test npcRagdollComponent.m_removeEffect == null ||
        if (npcRagdollComponent.m_removeEffect.m_effectPrefabs.Length == 0)
        {
            var effect = ZNetScene.instance.GetPrefab("vfx_corpse_destruction_small".GetStableHashCode());
            var newData = new EffectList.EffectData();
            newData.m_prefab = effect;
            npcRagdollComponent.m_removeEffect.m_effectPrefabs = new EffectList.EffectData[1];
            npcRagdollComponent.m_removeEffect.m_effectPrefabs[0] = newData;
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
        original.SetActive(ragdollActive);
        npcRagdoll.SetActive(ragdollActive);

        // Register prefab
        NPCUtils.RegisterGameObject(npcRagdoll);

        return npcRagdoll;
    }

    private static void SetupMonsterAI(ref NPCAI npcAI, MonsterAI original)
    {
        var fields = typeof(MonsterAI).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (FieldInfo field in fields)
        {
            try
            {
                var value = field.GetValue(original);
                field.SetValue(npcAI, value);
            }
            catch { }
        }

        NPCAI.SetupExisting(ref npcAI);
    }

    private static void SetupAnimalAI(ref NPCAnimalAI npcAI, AnimalAI original)
    {
        var fields = typeof(BaseAI).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (FieldInfo field in fields)
        {
            try
            {
                var value = field.GetValue(original);
                field.SetValue(npcAI, value);
            }
            catch { }
        }

        NPCAnimalAI.SetupExisting(ref npcAI);
    }
}