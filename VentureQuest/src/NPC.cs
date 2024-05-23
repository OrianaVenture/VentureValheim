using System;
using System.Collections;
using BepInEx;
using UnityEngine;

namespace VentureValheim.VentureQuest
{
    public class NPC : Humanoid, Interactable
    {
        public const string ZDOVar_SITTING = "VV_Sitting";
        public const string ZDOVar_TEXT = "VV_NPCText";
        public const string ZDOVar_SPAWNPOINT = "VV_SpawnPoint";

        public bool HasAttach = false;
        public Transform AttachPoint;
        public string AttachAnimation;
        public GameObject AttachRoot;
        public Collider[] AttachColliders;

        private readonly Color32 _hairColorMin = new Color32(0xFF, 0xED, 0xB4, 0xFF);
        private readonly Color32 _hairColorMax = new Color32(0xFF, 0x7C, 0x47, 0xFF);
        private readonly Color32 _skinColorMin = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
        private readonly Color32 _skinColorMax = new Color32(0x4C, 0x4C, 0x4C, 0xFF);

        //private IEnumerator startCoroutine;

        public override void Awake()
        {
            VentureQuestPlugin.VentureQuestLogger.LogInfo("NPC Awake!");
            base.Awake();

            var startCoroutine = SetUp();
            StartCoroutine(startCoroutine);
        }

        public IEnumerator SetUp()
        {
            VentureQuestPlugin.VentureQuestLogger.LogInfo("NPC SetUp!");
            yield return new WaitForSeconds(2);
            yield return null;

            m_name = m_nview.GetZDO().GetString(ZDOVars.s_tamedName);
            var sitting = m_nview.GetZDO().GetBool(ZDOVar_SITTING);
            if (sitting)
            {
                var chair = GetClosestChair();
                if (chair != null)
                {
                    AttachStart(chair);
                }
            }
        }

        public override void CustomFixedUpdate(float fixedDeltaTime)
        {
            if (!m_nview.IsValid() || !m_nview.IsOwner())
            {
                return;
            }

            if (HasAttach)
            {
                if (AttachPoint == null)
                {
                    AttachStop();
                }
                else
                {
                    base.transform.position = AttachPoint.position;
                    base.transform.rotation = AttachPoint.rotation;
                    Rigidbody componentInParent = AttachPoint.GetComponentInParent<Rigidbody>();
                    m_body.useGravity = false;
                    m_body.velocity = (componentInParent ?
                        componentInParent.GetPointVelocity(base.transform.position) : Vector3.zero);
                    m_body.angularVelocity = Vector3.zero;
                    m_maxAirAltitude = base.transform.position.y;
                }
            }

            base.CustomFixedUpdate(fixedDeltaTime);
        }

        public override void OnDeath()
        {
            VentureQuestPlugin.VentureQuestLogger.LogInfo($"Death! Tragity!");
            /*if (!string.IsNullOrEmpty(m_defeatSetGlobalKey))
            {
                Player.m_addUniqueKeyQueue.Add(m_defeatSetGlobalKey);
            }*/

            if (m_nview == null || !m_nview.IsOwner())
            {
                return;
            }

            GameObject[] effects = m_deathEffects.Create(base.transform.position, base.transform.rotation, base.transform);
            for (int lcv = 0; lcv < effects.Length; lcv++)
            {
                Ragdoll ragdoll = effects[lcv].GetComponent<Ragdoll>();
                if (ragdoll != null)
                {
                    Vector3 velocity = m_body.velocity;
                    if (m_pushForce.magnitude * 0.5f > velocity.magnitude)
                    {
                        velocity = m_pushForce * 0.5f;
                    }

                    ragdoll.Setup(velocity, 0f, 0f, 0f, null);

                    VisEquipment visEquip = ragdoll.GetComponent<VisEquipment>();
                    if (visEquip != null)
                    {
                        ListVisEquipment();
                        CopyVisEquipment(ref visEquip, m_visEquipment);
                    }
                }
            }

            /*if (!string.IsNullOrEmpty(m_defeatSetGlobalKey))
            {
                ZoneSystem.instance.SetGlobalKey(m_defeatSetGlobalKey);
            }*/

            /*if (m_onDeath != null)
            {
                m_onDeath();
            }*/

            NPCFactory.RespawnNPC(this.gameObject);

            ZNetScene.instance.Destroy(base.gameObject);
        }

        public static void CopyVisEquipment(ref VisEquipment copy, VisEquipment original)
        {
            VentureQuestPlugin.VentureQuestLogger.LogInfo($"CopyVisEquipment!!");
            copy.SetModel(original.m_modelIndex);
            copy.SetSkinColor(original.m_skinColor);
            copy.SetHairColor(original.m_hairColor);
            copy.SetBeardItem(original.m_beardItem);
            copy.SetHairItem(original.m_hairItem);
            copy.SetHelmetItem(original.m_helmetItem);
            copy.SetChestItem(original.m_chestItem);
            copy.SetLegItem(original.m_legItem);
            copy.SetShoulderItem(original.m_shoulderItem, original.m_shoulderItemVariant);
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            Talk();

            return false;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }

        private void Talk()
        {
            var text = m_nview.GetZDO().GetString(ZDOVar_TEXT);
            if (Player.m_localPlayer != null && !text.IsNullOrWhiteSpace())
            {
                Chat.instance.SetNpcText(base.gameObject, Vector3.up * 2f, 10f, 30f, "", text, true);
            }
        }

        public void SetName(string name)
        {
            m_nview.GetZDO().Set(ZDOVars.s_tamedName, name);
        }

        public void SetText(string text)
        {
            m_nview.GetZDO().Set(ZDOVar_TEXT, text);
        }

        public void SetSpawnPoint(Vector3 position)
        {
            m_nview.GetZDO().Set(ZDOVar_SPAWNPOINT, position);
        }

        public Chair GetClosestChair()
        {
            var pos = base.transform.position;
            Collider[] hits = Physics.OverlapBox(pos, base.transform.localScale / 2, Quaternion.identity);
            Chair closestChair = null;

            foreach (var hit in hits)
            {
                var chairs = hit.transform.root.gameObject.GetComponentsInChildren<Chair>();
                if (chairs != null)
                {
                    for (int lcv = 0; lcv < chairs.Length; lcv++)
                    {
                        var chair = chairs[lcv];
                        if (closestChair == null || (Vector3.Distance(pos, chair.transform.position) <
                            Vector3.Distance(pos, closestChair.transform.position)))
                        {
                            closestChair = chair;
                        }
                    }
                }
            }

            return closestChair;
        }

        public void SetRandom()
        {
            UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

            // Model
            var index = UnityEngine.Random.Range(0, 2);
            SetModel(index);

            // Skin
            var skintone = UnityEngine.Random.Range(0f, 1f);
            Color skinColor = Color.Lerp(_skinColorMin, _skinColorMax, skintone);
            SetSkinColor(skinColor);

            // Hair
            var hair = UnityEngine.Random.Range(1, 32);
            SetNPCHair("Hair" + hair);

            var hairtone= UnityEngine.Random.Range(0f, 1f);
            var hairlevel = UnityEngine.Random.Range(0f, 1f);
            Color hairColor = Color.Lerp(_hairColorMin, _hairColorMax, hairtone) *
                Mathf.Lerp(0.1f, 1f, hairlevel);
            SetHairColor(hairColor);

            if (index == 0)
            {
                // Male
                SetRandomMale();
            }
            else if (index == 1)
            {
                // Female
                SetRandomFemale();
            }

            m_visEquipment.UpdateEquipmentVisuals();
        }

        public void SetRandomMale()
        {
            var beard = UnityEngine.Random.Range(1, 30);
            if (beard <= 21)
            {
                SetNPCBeard("Beard" + beard);
            }

            var helmet = UnityEngine.Random.Range(1, 25);
            if (helmet <= 10)
            {
                SetHelmet("HelmetHat" + helmet);
            }
            else if (helmet < 16)
            {
                SetHelmet("ArmorLeatherHelmet");
            }

            var chest = UnityEngine.Random.Range(1, 15);
            if (chest > 10)
            {
                SetChest("ArmorLeatherChest");
            }
            else
            {
                SetChest("ArmorTunic" + chest);
            }

            var legs = UnityEngine.Random.Range(0, 15);
            if (legs < 10)
            {
                SetLegs("ArmorLeatherLegs");
            }
            else
            {
                SetLegs("ArmorRagsLegs");
            }
        }

        public void SetRandomFemale()
        {
            var helmet = UnityEngine.Random.Range(1, 25);
            if (helmet <= 10)
            {
                SetHelmet("HelmetHat" + helmet);
            }
            else if (helmet < 17)
            {
                SetHelmet("HelmetMidsummerCrown");
            }

            var chest = UnityEngine.Random.Range(1, 15);
            if (chest > 10)
            {
                SetChest("ArmorLeatherChest");
            }
            else
            {
                SetChest("ArmorDress" + chest);
            }

            var legs = UnityEngine.Random.Range(0, 15);
            if (legs < 5)
            {
                SetLegs("ArmorLeatherLegs");
            }
            else if (legs < 8)
            {
                SetLegs("ArmorRagsLegs");
            }
        }

        public void SetNPCHair(string name)
        {
            m_hairItem = name;
            m_visEquipment.SetHairItem(name);
        }

        public void SetHairColor(Color color)
        {
            SetHairColor(color.r, color.g, color.b);
        }

        public void SetHairColor(float r, float g, float b)
        {
            m_visEquipment.SetHairColor(new Vector3(r, g, b));
        }

        public void SetSkinColor(Color color)
        {
            SetSkinColor(color.r, color.g, color.b);
        }

        public void SetSkinColor(float r, float g, float b)
        {
            m_visEquipment.SetSkinColor(new Vector3(r, g, b));
        }

        public void SetNPCBeard(string name)
        {
            m_beardItem = name;
            m_visEquipment.SetBeardItem(name);
        }

        public void SetModel(int index)
        {
            m_visEquipment.SetModel(index);
        }

        public void SetHelmet(string name)
        {
            if (Patches.GetItemDrop(name, out ItemDrop item))
            {
                m_helmetItem = item.m_itemData;
                m_visEquipment.SetHelmetItem(name);
            }
        }

        public void SetChest(string name)
        {
            if (Patches.GetItemDrop(name, out ItemDrop item))
            {
                m_chestItem = item.m_itemData;
                m_visEquipment.SetChestItem(name);
            }
        }
        
        public void SetLegs(string name)
        {
            if (Patches.GetItemDrop(name, out ItemDrop item))
            {
                m_legItem = item.m_itemData;
                m_visEquipment.SetLegItem(name);
            }
        }

        public void SetShoulder(string name, int variant = 0)
        {
            if (Patches.GetItemDrop(name, out ItemDrop item))
            {
                m_shoulderItem = item.m_itemData;
                m_visEquipment.SetShoulderItem(name, variant);
            }
        }

        public void SetUtility(string name)
        {
            if (Patches.GetItemDrop(name, out ItemDrop item))
            {
                m_utilityItem = item.m_itemData;
                m_visEquipment.SetUtilityItem(name);
            }
        }

        public void SetRightHand(string name)
        {
            if (Patches.GetItemDrop(name, out ItemDrop item))
            {
                m_rightItem = item.m_itemData;
                m_visEquipment.SetRightItem(name);
            }
        }

        public void SetLeftHand(string name, int variant = 0)
        {
            if (Patches.GetItemDrop(name, out ItemDrop item))
            {
                m_leftItem = item.m_itemData;
                m_visEquipment.SetLeftItem(name, variant);
            }
        }

        public void SetBackRight(string name)
        {
            if (Patches.GetItemDrop(name, out ItemDrop item))
            {
                // TODO
                m_visEquipment.SetRightBackItem(name);
            }
        }

        public void SetBackLeft(string name, int variant = 0)
        {
            if (Patches.GetItemDrop(name, out ItemDrop item))
            {
                // TODO
                m_visEquipment.SetLeftBackItem(name, variant);
            }
        }

        private void ListVisEquipment()
        {
            VentureQuestPlugin.VentureQuestLogger.LogInfo($"ListVisEquipment... " +
                $"model: {m_visEquipment.m_modelIndex}, " +
                $"skinColor: {m_visEquipment.m_skinColor}, " +
                $"hairColor: {m_visEquipment.m_hairColor}, " +
                $"beardItem: {m_visEquipment.m_beardItem}, " +
                $"hairItem: {m_visEquipment.m_hairItem}, " +
                $"helmetItem: {m_visEquipment.m_helmetItem}, " +
                $"chestItem: {m_visEquipment.m_chestItem}, " +
                $"legItem: {m_visEquipment.m_legItem}, " +
                $"shoulderItemVariant: {m_visEquipment.m_shoulderItemVariant}, " +
                $"shoulderItem: {m_visEquipment.m_shoulderItem}, ");
        }

        public void AttachStart(Chair chair)
        {
            VentureQuestPlugin.VentureQuestLogger.LogDebug("Starting");

            HasAttach = true;
            AttachPoint = chair.m_attachPoint;
            AttachAnimation = chair.m_attachAnimation;
            AttachRoot = chair.transform.root.gameObject;

            base.transform.position = AttachPoint.position;
            base.transform.rotation = AttachPoint.rotation;

            m_zanim.SetBool(AttachAnimation, value: true);
            m_nview.GetZDO().Set(ZDOVar_SITTING, value: true);

            m_body.mass = 1000;

            Rigidbody componentInParent = AttachPoint.GetComponentInParent<Rigidbody>();
            m_body.useGravity = false;
            m_body.velocity = (componentInParent ? 
                componentInParent.GetPointVelocity(base.transform.position) : Vector3.zero);
            m_body.angularVelocity = Vector3.zero;
            m_maxAirAltitude = base.transform.position.y;

            // TODO, fix chair jitters
            AttachColliders = AttachRoot.GetComponentsInChildren<Collider>();

            Collider[] attachColliders = AttachColliders;
            foreach (Collider collider in attachColliders)
            {
                Physics.IgnoreCollision(m_collider, collider, ignore: true);
            }

            HideHandItems();
            ResetCloth();
        }

        public override void AttachStop()
        {
            VentureQuestPlugin.VentureQuestLogger.LogDebug("Stopping");
            if (!HasAttach)
            {
                return;
            }

            m_nview.GetZDO().Set(ZDOVar_SITTING, value: false);

            HasAttach = false;
            AttachAnimation = "";
            AttachPoint = null;
            m_body.useGravity = true;
            m_body.mass = 10; // TODO, set to original

            Collider[] attachColliders = AttachColliders;
            foreach (Collider collider in attachColliders)
            {
                Physics.IgnoreCollision(m_collider, collider, ignore: false);
            }
            AttachColliders = null;
            AttachRoot = null;

            m_zanim.SetBool(AttachAnimation, value: false);
            ResetCloth();
        }
    }
}