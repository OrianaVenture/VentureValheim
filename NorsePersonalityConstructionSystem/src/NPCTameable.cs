
namespace VentureValheim.NPCS;

public class NPCTamable : Tameable
{
    // TODO
    // Add ability for sellswords? to train the player's skills

    public void Awake()
    {
        m_fedDuration = 60;
        m_tamingTime = 10000;
        m_startsTamed = false;
        m_commandable = false;

        base.Awake();
    }
}