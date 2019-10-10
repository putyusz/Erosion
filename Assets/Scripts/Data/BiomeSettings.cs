using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BiomeSettings : UpdatableData
{
    public MoistureSettings moistureSettings;
    public List<Biome> biomes;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        moistureSettings.ValidateValues();
        base.OnValidate();
    }
#endif
}
