using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Biome : IBiome
{
    public Biomes biomeName;
    [Range(0,100)]
    public float vegetationFrequency;
    public GameObject[] prefabs;

    public GameObject GetRandomPlant()
    {
        if(prefabs.Length == 0) {
            return null;
        }
        System.Random rnd = new System.Random(System.DateTime.Now.Millisecond);
        int v = rnd.Next(0, (prefabs.Length - 1) * 100);
        for (int i = 0; i < prefabs.Length; i++) {
            if (v < i * 100) {
                return prefabs[i];
            }
        }
        return prefabs[0];
    }

    public GameObject[] GetPlants() {
        return prefabs;
    }

    public float GetVegetationFrequency()
    {
        return vegetationFrequency;
    }

    public Biomes GetBiomeName()
    {
        return biomeName;
    }

}
