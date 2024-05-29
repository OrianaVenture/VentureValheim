namespace VentureValheim.NPCS;

public class NPCAI : MonsterAI
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
        m_randomMoveInterval = 10f;
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

        // Monster AI
        m_alertRange = 10f;
        m_fleeIfHurtWhenTargetCantBeReached = true;
        m_fleeUnreachableSinceAttacking = 30f;
        m_fleeUnreachableSinceHurt = 20f;
        m_fleeIfNotAlerted = false;
        m_fleeIfLowHealth = 10f;
        m_fleeTimeSinceHurt = 20f;
        m_fleeInLava = true;
        m_circulateWhileCharging = true;
        m_enableHuntPlayer = false;
        m_attackPlayerObjects = false;
        m_privateAreaTriggerTreshold = 50;
        m_interceptTimeMax = 2f;
        m_interceptTimeMin = 0f;
        m_maxChaseDistance = 10f;
        m_minAttackInterval = 0f;

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

    public static void SetupExisting(ref NPCAI monsterAI)
    {
        monsterAI.m_viewRange = 15f;
        monsterAI.m_hearRange = 15f;
        monsterAI.m_aggravatable = true;
        monsterAI.m_enableHuntPlayer = false;
        monsterAI.m_attackPlayerObjects = false;
        monsterAI.m_privateAreaTriggerTreshold = 50;
    }
}