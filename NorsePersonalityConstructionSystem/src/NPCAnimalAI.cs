namespace VentureValheim.NPCS;

public class NPCAnimalAI : AnimalAI
{
    public override void Awake()
    {
        m_viewRange = 15f;
        m_viewAngle = 90f;
        m_hearRange = 15f;
        m_mistVision = false;

        m_pathAgentType = Pathfinding.AgentType.Humanoid;
        m_moveMinAngle = 90f;
        m_smoothMovement = true;
        m_serpentMovement = false;

        m_jumpInterval = 0f;

        m_randomCircleInterval = 3f;
        m_randomMoveInterval = 15f;
        m_randomMoveRange = 5f;

        // Fly
        m_randomFly = false;

        // other
        m_avoidFire = false;
        m_afraidOfFire = false;
        m_avoidWater = true;
        m_avoidLava = true;
        m_skipLavaTargets = true;
        m_avoidLavaFlee = true;
        m_aggravatable = true;
        m_passiveAggresive = false;
        m_spawnMessage = "";
        m_deathMessage = "";
        m_alertedMessage = "";

        // Flee
        m_fleeRange = 25f;
        m_fleeAngle = 45f;
        m_fleeInterval = 10f;

        base.Awake();
    }

    public override void SetAlerted(bool alert)
    {
        base.SetAlerted(alert);

        if (!alert)
        {
            // Reset aggravated
            SetAggravated(false, AggravatedReason.Damage);
        }
    }

    public static void SetupExisting(ref NPCAnimalAI animalAI)
    {
        animalAI.m_viewRange = 15f;
        animalAI.m_hearRange = 15f;
        animalAI.m_aggravatable = true;
    }
}