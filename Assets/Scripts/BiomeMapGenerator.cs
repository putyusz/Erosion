using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Biomes {
    SUBTROPICAL_DESERT, //cactuses
    TEMPERATE_DESERT, //dead bushes
    SCORCHED_LAND, //rocks
    GRASSLAND, //grass
    SHRUBLAND, //shrubs x
    BARE_LAND, //nothing x
    TROPICAL_SEASONAL_FOREST, //large jungle trees, medium jungle trees
    TEMPERATE_DECIDOUS_FOREST, //decidous trees x
    TAIGA, //conifers1 x
    TUNDRA, //moss
    TROPICAL_RAIN_FOREST, //large jungle trees, ferns
    TEMPERATE_RAIN_FOREST, //conifers2, moss 
    SNOW
};

public static class BiomeMapGenerator {
    public static BiomeMap GenerateBiomes(int mapWidth, int mapHeight, HeightMap heightMap, BiomeSettings settings, Vector2 sampleCentre) {
        Biome[,] biomes = new Biome[mapWidth, mapHeight];
        float[,] moistureMap = GenerateMoistureMap(mapWidth, mapHeight, heightMap, settings.moistureSettings, sampleCentre);

        for (int x = 0; x < mapWidth; x++) {
            for (int y = 0; y < mapHeight; y++) {
                float height = heightMap.values[x, y] / heightMap.maxValue;//(heightMap.values[x, y] * 2) / (heightMap.maxValue / 0.9f);
                float moisture = moistureMap[x, y];
                Biome biome = settings.biomes[(int)Biomes.BARE_LAND];

                //The most beautiful IF ELSE statements that the world has ever seen
                if (height <= 0.25f) {
                    if (moisture <= 0.25f) {
                        biome = settings.biomes[(int)Biomes.SUBTROPICAL_DESERT];
                    } else if (moisture <= 0.75f) {
                        biome = settings.biomes[(int)Biomes.TEMPERATE_DESERT];
                    } else if (moisture <= 1.0f) {
                        biome = settings.biomes[(int)Biomes.SCORCHED_LAND];
                    }
                } else if (height <= 0.5f) {
                    if (moisture <= 0.5f) {
                        biome = settings.biomes[(int)Biomes.GRASSLAND];
                    } else if (moisture <= 0.75f) {
                        biome = settings.biomes[(int)Biomes.SHRUBLAND];
                    } else if (moisture <= 1.0f) {
                        biome = settings.biomes[(int)Biomes.BARE_LAND];
                    }
                } else if (height <= 0.75f) {
                    if (moisture <= 0.25f) {
                        biome = settings.biomes[(int)Biomes.TROPICAL_SEASONAL_FOREST];
                    } else if (moisture <= 0.5f) {
                        biome = settings.biomes[(int)Biomes.TEMPERATE_DECIDOUS_FOREST];
                    } else if (moisture <= 0.75f) {
                        biome = settings.biomes[(int)Biomes.TAIGA];
                    } else if (moisture <= 1.0f) {
                        biome = settings.biomes[(int)Biomes.TUNDRA];
                    }
                } else if (height <= 1.0f) {
                    if (moisture <= 0.25f) {
                        biome = settings.biomes[(int)Biomes.TROPICAL_RAIN_FOREST];
                    } else if (moisture <= 0.5f) {
                        biome = settings.biomes[(int)Biomes.TEMPERATE_RAIN_FOREST];
                    } else if (moisture <= 0.75f) {
                        biome = settings.biomes[(int)Biomes.TAIGA];
                    } else if (moisture <= 1.0f) {
                        biome = settings.biomes[(int)Biomes.SNOW];
                    }
                }
                biomes[x, y] = biome;
            }
        }
        return new BiomeMap(biomes);
    }

    public static float[,] GenerateMoistureMap(int mapWidth, int mapHeight, HeightMap heightMap, MoistureSettings settings, Vector2 sampleCentre) {
        float[,] values = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(settings.seed);
        Vector2[] octaveOffsets = new Vector2[settings.octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < settings.octaves; i++) {
            float offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCentre.x;
            float offsetY = prng.Next(-100000, 100000) - settings.offset.y - sampleCentre.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= settings.persistance;
        }

        float halfWidth = mapWidth / 2.0f;
        float halfHeight = mapHeight / 2.0f;

        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                if (heightMap.values[x, y] < settings.minValue) {
                    values[x, y] = 1.0f;
                } else /*if (heightMap.values[x, y] > settings.maxValue) {
                    values[x, y] = 0.0f;
                } else */{
                    amplitude = 1;
                    frequency = 1;
                    float noiseHeight = 0.0f;

                    for (int i = 0; i < settings.octaves; i++) {
                        float sampleX = (x - halfWidth + octaveOffsets[i].x) / settings.scale * frequency;
                        float sampleY = (y - halfHeight + octaveOffsets[i].y) / settings.scale * frequency;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);

                        noiseHeight += perlinValue * amplitude;

                        amplitude *= settings.persistance;
                        frequency *= settings.lacunarity;
                    }

                    noiseHeight = Mathf.Max(0, noiseHeight - settings.minValue);
                    //noiseHeight = Mathf.Max(0, settings.maxValue - noiseHeight);

                    float normalizedHeight = (noiseHeight * 2.0f) / (maxPossibleHeight / 0.9f);
                    values[x, y] = Mathf.Clamp(normalizedHeight, 0, float.MaxValue);
                }
            }
        }
        return values;
    }
}

public struct BiomeMap {
    public Biome[,] biomes;

    public BiomeMap(Biome[,] biomes) {
        this.biomes = biomes;
    }
}

[System.Serializable]
public class MoistureSettings {
    public float scale = 50;

    public int octaves = 6;
    [Range(0, 1)]
    public float persistance = .6f;
    public float lacunarity = 2f;

    public int seed;
    public Vector2 offset;

    public float minValue = 0.0f;
    //public float maxValue = 1.0f;

    public void ValidateValues() {
        scale = Mathf.Max(scale, 0.01f);
        octaves = Mathf.Clamp(octaves, 1, 8);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistance = Mathf.Clamp01(persistance);
    }
}