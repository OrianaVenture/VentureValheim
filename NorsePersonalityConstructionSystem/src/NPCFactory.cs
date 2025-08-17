using System;
using System.Collections.Generic;
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
        typeof(Tameable),
        typeof(Procreation)
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
                npcComponent.Data.SetRandom();
            }

            npcComponent.Data.SetName(name);
            npcComponent.Data.SetSpawnPoint(position);
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
            npcComponent.Data.SetFromConfig(config, true);
            npcComponent.Data.SetSpawnPoint(position);
        }

        return npc;
    }

    public static GameObject RespawnNPC(ZPackage original)
    {
        NPCSPlugin.NPCSLogger.LogDebug($"Trying RespawnNPC...");
        // TODO clean up, do not want to Deserialize twice
        ZDO copy = new ZDO();
        original.m_stream.Position = 0L;
        copy.Deserialize(original);
        var respawn = NPCZDOUtils.GetSpawnPoint(copy);

        if (respawn == Vector3.zero)
        {
            NPCSPlugin.NPCSLogger.LogDebug("No spawn point found!");
            return null;
        }

        var prefab = ZNetScene.instance.GetPrefab(copy.m_prefab);

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
            ZNetScene.instance.Destroy(gameobject);
            return null;
        }

        ZDO zdo = newZNetView.GetZDO();
        original.m_stream.Position = 0L;
        zdo.Deserialize(original);
        zdo.Set(ZDOVars.s_health, gameobject.GetComponent<Character>().GetMaxHealth());

        return gameobject;
    }

    public static void AddNPCS()
    {
        // TODO: Make this dynamic to support all creatures in the game and other mods
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
            return null;
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

        GameObject npc = Utility.CreateGameObject(prefab, model);

        foreach (var remove in RemoveComponents)
        {
            var comp = npc.GetComponent(remove);
            if (comp != null)
            {
                UnityEngine.Object.DestroyImmediate(comp);
            }
        }

        // Edit existing components
        Character originalCharacter = prefab.GetComponent<Character>();
        Character oldCharacter = npc.GetComponent<Character>();
        Character npcCharacter = null;

        if (originalCharacter != null)
        {
            UnityEngine.Object.DestroyImmediate(oldCharacter);

            if (originalCharacter is Humanoid)
            {
                npcCharacter = npc.AddComponent<NPCHumanoid>();
            }
            else
            {
                npcCharacter = npc.AddComponent<NPCCharacter>();
            }
        }

        SetupNPC(ref npcCharacter, originalCharacter);

        // TODO: handle default equipment for all types of npcs
        if (model.Equals("Player"))
        {
            (npcCharacter as NPCHumanoid).m_defaultItems = new GameObject[] { };
            npcCharacter.m_walkSpeed = 2f;
            npcCharacter.m_speed = 2f;
            npcCharacter.m_runSpeed = 4f;
            npcCharacter.m_health = 200f;

            Utility.GetItemPrefab("PlayerUnarmed".GetStableHashCode(), out var fists);
            (npcCharacter as NPCHumanoid).m_unarmedWeapon = fists.GetComponent<ItemDrop>();
        }

        // Make sure to set this to the new object component, otherwise attacks are broken
        npcCharacter.m_eye = Utils.FindChild(npcCharacter.gameObject.transform, "EyePos");

        // Setup hostility behavior
        // TODO: Set to Player faction, allow NPCs to fight different groups of NPCs
        // Allow tamable behavior when hiring NPCs as bodyguards
        npcCharacter.m_faction = Character.Faction.Dverger;
        npcCharacter.m_tamed = false;
        npcCharacter.m_group = NPCData.NPCGROUP;

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

            // Only add talker if has a MonsterAI
            var talker = npc.GetComponent<NpcTalk>();
            if (talker == null)
            {
                talker = npc.AddComponent<NpcTalk>();
            }
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
        Utility.RegisterGameObject(npc);

        return npc;
    }

    private static void SetupNPC<T>(ref T npc, Character original) where T : Character
    {
        Utility.CopyFields(original, ref npc);

        // Setup Ragdolls
        var effectList = original.m_deathEffects.m_effectPrefabs;
        EffectList.EffectData[] newEffects = new EffectList.EffectData[effectList.Length];

        for (int lcv = 0; lcv < effectList.Length; lcv++)
        {
            newEffects[lcv] = new EffectList.EffectData();
            Utility.CopyFields(effectList[lcv], ref newEffects[lcv]);
            if (effectList[lcv].m_prefab != null && effectList[lcv].m_prefab.GetComponent<Ragdoll>())
            {
                newEffects[lcv].m_prefab = SetupRagdoll(effectList[lcv].m_prefab);
            }
        }

        npc.m_deathEffects = new EffectList();
        npc.m_deathEffects.m_effectPrefabs = newEffects;
    }

    private static GameObject SetupRagdoll(GameObject original)
    {
        var ragdollActive = original.activeSelf;
        original.SetActive(false);

        GameObject npcRagdoll = Utility.CreateGameObject(original, original.name);

        var originalRagdollComponent = original.GetComponent<Ragdoll>();
        var ragdollComponent = npcRagdoll.GetComponent<Ragdoll>();
        UnityEngine.Object.DestroyImmediate(ragdollComponent);
        var npcRagdollComponent = npcRagdoll.AddComponent<NPCRagdoll>();

        Utility.CopyFields(originalRagdollComponent, ref npcRagdollComponent);
        
        if (npcRagdollComponent.m_removeEffect.m_effectPrefabs.Length == 0)
        {
            npcRagdollComponent.m_removeEffect = new EffectList();
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
        Utility.RegisterGameObject(npcRagdoll);

        return npcRagdoll;
    }

    private static void SetupMonsterAI(ref NPCAI npcAI, MonsterAI original)
    {
        Utility.CopyFields(original, ref npcAI);

        NPCAI.SetupExisting(ref npcAI);
    }

    private static void SetupAnimalAI(ref NPCAnimalAI npcAI, AnimalAI original)
    {
        Utility.CopyFields(original, ref npcAI);

        NPCAnimalAI.SetupExisting(ref npcAI);
    }
}