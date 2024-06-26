using System.Collections.Generic;
using UnityEngine;

namespace VentureValheim.NPCS;

public interface INPC
{
    public HashSet<string> GetRequiredKeysSet();
    public HashSet<string> GetNotRequiredKeysSet();
    public void SetFromConfig(NPCConfiguration.NPCConfig config, bool newSpawn);
    public void SetRandom();
    public void SetName(string name);
    public void SetSpawnPoint(Vector3 position);
    public void SetTrueDeath(bool death);
    public void SetRotation(Quaternion rotation);
    public void Attach(bool attach, Chair chair = null);
}
