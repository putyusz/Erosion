using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator {

	public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }    

    public static Texture2D TextureFromHeightMap(HeightMap heightMap)
    {
        int width = heightMap.values.GetLength(0);
        int height = heightMap.values.GetLength(1);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(heightMap.minValue, heightMap.maxValue, heightMap.values[x, y]));
            }
        }
        return TextureFromColorMap(colorMap, width, height);
    }

    public static Texture2D TextureFromMoistureMap(float[,] moistureMap, MoistureSettings settings) {
        int width = moistureMap.GetLength(0);
        int height = moistureMap.GetLength(1);
        Color[] colorMap = new Color[width * height];

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                colorMap[y * width + x] = Color.Lerp(Color.white, Color.blue, Mathf.InverseLerp(0f, 1f, moistureMap[x, y]));
            }
        }

        return TextureFromColorMap(colorMap, width, height);
    }

    public static Texture2D TextureFromBiomeMap(Biome[,] biomeMap)
    {
        int width = biomeMap.GetLength(0);
        int height = biomeMap.GetLength(1);
        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color c = Color.white;
                switch (biomeMap[x, y].GetBiomeName())
                {
                    case Biomes.SUBTROPICAL_DESERT:
                        c = new Color(1,1,0);
                        break;
                    case Biomes.TEMPERATE_DESERT:
                        c = new Color(240.0f / 255.0f, 230.0f / 255.0f, 140.0f / 255.0f);
                        break;
                    case Biomes.SCORCHED_LAND:
                        c = new Color(1, 99.0f / 255.0f, 71.0f / 255.0f);
                        break;
                    case Biomes.GRASSLAND:
                        c = new Color(0, 128.0f / 255.0f, 0);
                        break;
                    case Biomes.SHRUBLAND:
                        c = new Color(50.0f / 255.0f, 205.0f / 255.0f, 50.0f / 255.0f);
                        break;
                    case Biomes.BARE_LAND:
                        c = new Color(210.0f / 255.0f, 180.0f / 255.0f, 140.0f / 255.0f);
                        break;
                    case Biomes.TROPICAL_SEASONAL_FOREST:
                        c = new Color(46.0f / 255.0f, 139.0f / 255.0f, 87.0f / 255.0f);
                        break;
                    case Biomes.TEMPERATE_DECIDOUS_FOREST:
                        c = new Color(34.0f / 255.0f, 139.0f / 255.0f, 34.0f / 255.0f);
                        break;
                    case Biomes.TAIGA:
                        c = new Color(135.0f / 255.0f, 206.0f / 255.0f, 250.0f / 255.0f);
                        break;
                    case Biomes.TUNDRA:
                        c = new Color(135.0f / 255.0f, 206.0f / 255.0f, 235.0f / 255.0f);
                        break;
                    case Biomes.TROPICAL_RAIN_FOREST:
                        c = new Color(85.0f / 255.0f, 107.0f / 255.0f, 47.0f / 255.0f);
                        break;
                    case Biomes.TEMPERATE_RAIN_FOREST:
                        c = new Color(0, 100.0f / 255.0f, 0);
                        break;
                    case Biomes.SNOW:
                        c = new Color(1, 1, 1);
                        break;
                }
                colorMap[y * width + x] = c;
            }
        }
        return TextureFromColorMap(colorMap, width, height);
    }
}
