using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPreview : MonoBehaviour
{

    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public enum DrawMode
    {
        NoiseMap, Mesh, Moisture, Biomes
    };

    public DrawMode drawMode;

    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int editorPreviewLOD;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureSettings textureData;
    public BiomeSettings biomeSettings;

    public Material terrainMaterial;

    public bool autoUpdate;

    public float[,] falloffMap;

    public void DrawMapInEditor()
    {
        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.MinHeight, heightMapSettings.MaxHeight);

        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.NumVerticesPerLine, meshSettings.NumVerticesPerLine, heightMapSettings, Vector2.zero);
        BiomeMap biomeMap = BiomeMapGenerator.GenerateBiomes(meshSettings.NumVerticesPerLine, meshSettings.NumVerticesPerLine, heightMap, biomeSettings, Vector2.zero);
        float[,] moistureMap = BiomeMapGenerator.GenerateMoistureMap(meshSettings.NumVerticesPerLine, meshSettings.NumVerticesPerLine, heightMap, biomeSettings.moistureSettings, Vector2.zero);

        switch (drawMode) {
            case DrawMode.NoiseMap:
                DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
                break;
            case DrawMode.Mesh:
                DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
                break;
            case DrawMode.Moisture:
                DrawTexture(TextureGenerator.TextureFromMoistureMap(moistureMap, biomeSettings.moistureSettings));
                break;
            case DrawMode.Biomes:
                DrawTexture(TextureGenerator.TextureFromBiomeMap(biomeMap.biomes));
                break;
            default:
                break;
        }
    }

    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10f;

        textureRenderer.gameObject.SetActive(true);
        meshFilter.gameObject.SetActive(false);
    }

    public void DrawMesh(MeshData meshData)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();

        textureRenderer.gameObject.SetActive(false);
        meshFilter.gameObject.SetActive(true);
    }

    void OnValuesUpdated()
    {
        if (!Application.isPlaying) {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    private void OnValidate()
    {
        if (meshSettings != null) {
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (heightMapSettings != null) {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (textureData != null) {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
        if (biomeSettings != null) {
            biomeSettings.OnValuesUpdated -= OnValuesUpdated;
            biomeSettings.OnValuesUpdated += OnValuesUpdated;
        }
    }
}
