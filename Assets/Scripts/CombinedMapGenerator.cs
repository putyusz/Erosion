using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CombinedMapGenerator {
    public static CombinedMap GenerateCombinedMap(int width, int height, HeightMapSettings heightMapSettings, BiomeSettings biomeSettings, Vector2 sampleCentre) {
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(width, height, heightMapSettings, sampleCentre);
        BiomeMap biomeMap = BiomeMapGenerator.GenerateBiomes(width, height, heightMap, biomeSettings, sampleCentre);
        return new CombinedMap(heightMap, biomeMap);
    }
}

public struct CombinedMap {
    public readonly HeightMap heightMap;
    public readonly BiomeMap biomeMap;

    public CombinedMap(HeightMap heightMap, BiomeMap biomeMap) {
        this.heightMap = heightMap;
        this.biomeMap = biomeMap;
    }
}