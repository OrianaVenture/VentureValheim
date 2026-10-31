using Jotunn.Managers;
using UnityEngine;

namespace VentureValheim.SlimChicks;

public class SlimChicks
{
    public static void SlimThoseChicks()
    {
        GameObject chicken = PrefabManager.Instance.GetPrefab("Chicken");
        if (chicken != null && chicken.TryGetComponent<CapsuleCollider>(out CapsuleCollider chickenCollider) &&
            chicken.TryGetComponent<Rigidbody>(out Rigidbody chickenRigidbody))
        {
            chickenCollider.center = new Vector3(0f, 0.33f, 0.1f); // 0, 0.5468788, -0.003623307
            chickenCollider.radius = 0.25f; // 0.5674182
            chickenCollider.height = 0.7f; // 1.157784

            chickenRigidbody.mass = 1f; // 4
        }
        else
        {
            SlimChicksPlugin.SlimChicksLogger.LogWarning("Could not modify Chicken!");
        }

        GameObject hen = PrefabManager.Instance.GetPrefab("Hen");
        if (hen != null && hen.TryGetComponent<CapsuleCollider>(out CapsuleCollider henCollider) &&
            hen.TryGetComponent<Rigidbody>(out Rigidbody henRigidbody))
        {
            henCollider.center = new Vector3(0f, 0.33f, 0.05f); // 0, 0.4182911, 0
            henCollider.radius = 0.25f; // 0.3412103
            henCollider.height = 0.7f; // 0.871891

            henRigidbody.mass = 4f; // 10
        }
        else
        {
            SlimChicksPlugin.SlimChicksLogger.LogWarning("Could not modify Hen!");
        }
    }
}