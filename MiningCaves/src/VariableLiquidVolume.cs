using UnityEngine;

public class VV_VariableLiquidVolume : WaterVolume
{
    public LiquidType VariableLiquidType = LiquidType.Water;

    public EffectList RandomEffectList = new EffectList();
    public float RandomEffectInterval = 3f;

    private float randomEffectTimer;
    private float randomEffectIntervalInternal = 0f;

    public float Width = 10;
    public float Length = 10;
    public float Scale = 1f;
    
    private void Update()
    {
        UpdateEffects(Time.deltaTime);
    }

    private void UpdateEffects(float dt)
    {
        if (!RandomEffectList.HasEffects() || RandomEffectInterval <= 0 || m_forceDepth < 0.2f)
        {
            return;
        }

        randomEffectTimer += dt;
        if (randomEffectTimer < randomEffectIntervalInternal)
        {
            return;
        }

        randomEffectIntervalInternal = UnityEngine.Random.Range(RandomEffectInterval * 0.5f, RandomEffectInterval);
        randomEffectTimer = 0f;
        float halfWidth = Width / 2f;
        float halfLength = Length / 2f;

        Vector3 point = new Vector3(UnityEngine.Random.Range(-halfWidth, halfWidth),
            0,
            UnityEngine.Random.Range(-halfLength, halfLength)) + this.transform.position;
        float height = GetWaterSurface(point);
        point.y = height;
        RandomEffectList.Create(point, Quaternion.identity, null);
    }
}