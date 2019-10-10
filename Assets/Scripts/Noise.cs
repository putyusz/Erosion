using System;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {
       public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings[] settings, Vector2 sampleCentre) {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        List<float[,]> list = new List<float[,]>();


        //float maxPossibleHeight = 0.0f;       

        //for (int i = 0; i < settings.Length; i++)
        //{
        //    maxPossibleHeight += CalcMaxPossibleHeight(settings[i].octaves, settings[i].persistance);
        //}

        foreach (var s in settings) {
            list.Add(GenerateNoiseMapHelper(mapWidth, mapHeight, s, sampleCentre));
        }

        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                float value = 1.0f;
                float continentValue = 0.0f;
                if (settings[0].enabled)
                    continentValue = list[0][x, y];

                for (int i = 1; i < list.Count; i++) {
                    if (settings[i].enabled) {
                        float mask = settings[i].useContinentAsMask ? continentValue : 1;
                        value *= list[i][x, y] * mask;
                    }
                }
                value += continentValue;
                noiseMap[x, y] = value;
            }
        }

        return noiseMap;
    }

    public static float[,] GenerateNoiseMap2(int mapLength, NoiseSettings[] settings, Vector2 sampleCentre) {
        float[,] noiseMap = new float[mapLength, mapLength];

        System.Random[] prng = new System.Random[settings.Length];
        List<Vector2>[] octaveOffsets = new List<Vector2>[settings.Length];

        float[] maxPossibleHeight = new float[settings.Length];

        for (int i = 0; i < settings.Length; i++) {
            prng[i] = new System.Random(settings[i].seed);
            octaveOffsets[i] = new List<Vector2>();
            maxPossibleHeight[i] = 0.0f;
        }

        for (int s = 0; s < settings.Length; s++) {
            float amplitude = 1.0f;
            for (int i = 0; i < settings[s].octaves; i++) {
                float offsetX = prng[s].Next(-100000, 100000) + settings[s].offset.x + sampleCentre.x;
                float offsetY = prng[s].Next(-100000, 100000) - settings[s].offset.y - sampleCentre.y;
                octaveOffsets[s].Add(new Vector2(offsetX, offsetY));

                maxPossibleHeight[s] += amplitude;
                amplitude *= settings[s].persistance;
            }
        }
        //--újragondolás szükséges innentől--
        for (int y = 0; y < mapLength; y++) {
            for (int x = 0; x < mapLength; x++) {
                float value = 1.0f;
                float continentvalue = 0.0f;
                if (settings[0].enabled) {
                    continentvalue = GenerateNoiseMapHelper2(x, y, mapLength, maxPossibleHeight[0], octaveOffsets[0], settings[0]);
                }
                for (int i = 1; i < settings.Length; i++) {
                    float mask = settings[i].useContinentAsMask ? continentvalue : 1;
                    float n = GenerateNoiseMapHelper2(x, y, mapLength, maxPossibleHeight[i], octaveOffsets[i], settings[i]);
                    value *= n * mask;
                }
                value += continentvalue; //????
                noiseMap[x, y] = value;
            }
        }
        //--idáig--
        return noiseMap;
    }

    private static float GenerateNoiseMapHelper2(float x, float y, float mapLength, float maxPossibleHeight, List<Vector2> octaveOffsets, NoiseSettings settings) {
        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0.0f;
        float weight = 1.0f;
        float halfLength = mapLength / 2.0f;

        for (int i = 0; i < settings.octaves; i++) {
            float sampleX = (x - halfLength + octaveOffsets[i].x) / settings.scale * frequency;
            float sampleY = (y - halfLength + octaveOffsets[i].y) / settings.scale * frequency;

            float perlinValue = 0.0f;

            switch (settings.type) {
                case NoiseType.CONTINENT:
                    float a = Mathf.PerlinNoise(sampleX, sampleY);
                    float b = Mathf.PerlinNoise(sampleX + 250.0f, sampleY + 250.0f);

                    perlinValue = Mathf.PerlinNoise(a, b);
                    break;
                case NoiseType.DETAIL:
                    perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                    break;
                case NoiseType.RIDGID:
                    perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                    perlinValue = 1 - Mathf.Abs(perlinValue);
                    perlinValue *= perlinValue;
                    perlinValue *= weight;
                    weight = perlinValue;
                    break;
            }

            noiseHeight += perlinValue * amplitude;

            amplitude *= settings.persistance;
            frequency *= settings.lacunarity;
        }

        noiseHeight = Mathf.Max(0, noiseHeight - settings.minValue);

        float normalizedHeight = (noiseHeight * 2.0f) / (maxPossibleHeight / 0.9f);
        return Mathf.Clamp(normalizedHeight, 0, float.MaxValue);

    }

    //private static float CalcMaxPossibleHeight(int octaves, float persistance)
    //{
    //    float amplitude = 1.0f;
    //    float maxPossbileHeight = 0.0f;
    //    for (int i = 0; i < octaves; i++)
    //    {
    //        maxPossbileHeight += amplitude;
    //        amplitude *= persistance;
    //    }

    //    return maxPossbileHeight;
    //}

    private static float[,] GenerateNoiseMapHelper(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCentre) {
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
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0.0f;
                float weight = 1.0f;

                for (int i = 0; i < settings.octaves; i++) {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / settings.scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / settings.scale * frequency;

                    float perlinValue = 0.0f;

                    switch (settings.type) {
                        case NoiseType.CONTINENT:
                            float a = Mathf.PerlinNoise(sampleX, sampleY);
                            float b = Mathf.PerlinNoise(sampleX + 250.0f, sampleY + 250.0f);

                            perlinValue = Mathf.PerlinNoise(a, b);
                            break;
                        case NoiseType.DETAIL:
                            perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                            break;
                        case NoiseType.RIDGID:
                            perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                            perlinValue = 1 - Mathf.Abs(perlinValue);
                            perlinValue *= perlinValue;
                            perlinValue *= weight;
                            weight = perlinValue;
                            break;
                    }

                    noiseHeight += perlinValue * amplitude;

                    amplitude *= settings.persistance;
                    frequency *= settings.lacunarity;
                }

                noiseHeight = Mathf.Max(0, noiseHeight - settings.minValue);

                float normalizedHeight = (noiseHeight * 2.0f) / (maxPossibleHeight / 0.9f);
                //float normalizedHeight = noiseHeight / maxPossibleHeight;
                values[x, y] = Mathf.Clamp(normalizedHeight, 0, float.MaxValue);
                //values[x, y] = Mathf.Lerp(0, 1, normalizedHeight);
            }
        }

        return values;
    }
}

public enum NoiseType {
    CONTINENT, DETAIL, RIDGID
};

public interface INoiseSettings {
    float Evaluate(float x, float y);
    void ValidateValues();
    bool Enabled();
}

[System.Serializable]
public class NoiseSettings {
    public bool enabled;
    public bool useContinentAsMask;
    public NoiseType type;

    public float scale = 50;

    public int octaves = 6;
    [Range(0, 1)]
    public float persistance = .6f;
    public float lacunarity = 2f;

    public int seed;
    public Vector2 offset;

    public float minValue = 0.5f;

    public void ValidateValues() {
        scale = Mathf.Max(scale, 0.01f);
        octaves = Mathf.Clamp(octaves, 1, 8);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistance = Mathf.Clamp01(persistance);
    }
}

[System.Serializable]
public class ContinentNoiseSettings : INoiseSettings {
    public bool enabled;

    public float scale = 50;
    public int octaves = 6;
    [Range(0, 1)]
    public float persistance = .6f;
    public float lacunarity = 2f;

    public int seed;
    public Vector2 noiseOffset;
    public Vector2 secondaryNoiseOffset;

    public float minValue = 0.5f;

    public bool Enabled() {
        return enabled;
    }

    public float Evaluate(float x, float y) {
        float a = Mathf.PerlinNoise(x, y);
        float b = Mathf.PerlinNoise(x + secondaryNoiseOffset.x, y + secondaryNoiseOffset.y);

        return Mathf.PerlinNoise(a, b);
    }

    public void ValidateValues() {
        scale = Mathf.Max(scale, 0.01f);
        octaves = Mathf.Clamp(octaves, 1, 8);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistance = Mathf.Clamp01(persistance);
    }
}

[System.Serializable]
public class DetailNoiseSettings : INoiseSettings {
    public bool enabled;
    public bool useContinentAsMask;

    public float scale = 50;
    public int octaves = 6;
    [Range(0, 1)]
    public float persistance = .6f;
    public float lacunarity = 2f;

    public int seed;
    public Vector2 offset;

    public bool Enabled() {
        return enabled;
    }

    public float Evaluate(float x, float y) {
        return Mathf.PerlinNoise(x, y);
    }

    public void ValidateValues() {
        scale = Mathf.Max(scale, 0.01f);
        octaves = Mathf.Clamp(octaves, 1, 8);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistance = Mathf.Clamp01(persistance);
    }
}

[System.Serializable]
public class RidgidNoiseSettings : INoiseSettings {
    public bool enabled;
    public bool useContinentAsMask;

    public float scale = 50;
    public int octaves = 6;
    [Range(0, 1)]
    public float persistance = .6f;
    public float lacunarity = 2f;

    public int seed;
    public Vector2 offset;

    float weight = 1.0f;

    public bool Enabled() {
        return enabled;
    }

    public float Evaluate(float x, float y) {
        float value = 0.0f;
        value = Mathf.PerlinNoise(x, y);
        value = 1 - Mathf.Abs(value);
        value *= value;
        value *= weight;
        weight = value;
        return value;
    }

    public void ValidateValues() {
        scale = Mathf.Max(scale, 0.01f);
        octaves = Mathf.Clamp(octaves, 1, 8);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistance = Mathf.Clamp01(persistance);
    }
}

public class NoiseSettingsFactory {
    public INoiseSettings GetNoiseSettings(NoiseType type) {
        if (type == NoiseType.CONTINENT) {
            return new ContinentNoiseSettings();
        } else if (type == NoiseType.DETAIL) {
            return new DetailNoiseSettings();
        } else if (type == NoiseType.RIDGID) {
            return new RidgidNoiseSettings();
        }
        return null;
    }
}