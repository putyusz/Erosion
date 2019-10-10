using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk {
    const float colliderGenerationDistanceTreshold = 5;
    public event System.Action<TerrainChunk, bool> OnVisibilityChanged;

    public Vector2 coord;


    GameObject meshObject;
    Vector2 sampleCentre;
    Bounds bounds;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    GameObject cloudObject;

    private List<GameObject> plants;

    MeshRenderer cloudRenderer;
    MeshFilter cloudFilter;

    LODInfo[] detailLevels;
    LODMesh[] lodMeshes;
    readonly int colliderLODIndex;

    HeightMap heightMap;
    BiomeMap biomeMap;

    bool mapReceived;
    bool firstGeneration = true;
    int previousLODIndex = -1;
    bool hasSetCollider;
    readonly float maxViewDistance;

    readonly HeightMapSettings heightMapSettings;
    readonly BiomeSettings biomeSettings;
    MeshSettings meshSettings;
    Transform viewer;

    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, BiomeSettings biomeSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material mapMaterial, Material cloudMaterial, Mesh cloudMesh) {
        this.coord = coord;
        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;
        this.heightMapSettings = heightMapSettings;
        this.biomeSettings = biomeSettings;
        this.meshSettings = meshSettings;
        this.viewer = viewer;
        plants = new List<GameObject>();

        sampleCentre = coord * meshSettings.MeshWorldSize / meshSettings.meshScale;
        Vector2 position = coord * meshSettings.MeshWorldSize;
        bounds = new Bounds(position, Vector3.one * meshSettings.MeshWorldSize);

        //----mesh----
        meshObject = new GameObject("Terrain Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        meshRenderer.material = mapMaterial;

        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;

        //----cloud----
        cloudObject = new GameObject("Cloud");
        cloudRenderer = cloudObject.AddComponent<MeshRenderer>();
        cloudFilter = cloudObject.AddComponent<MeshFilter>();
        cloudRenderer.sharedMaterial = cloudMaterial;
        cloudFilter.mesh = cloudMesh;

        cloudObject.transform.position = new Vector3(position.x, 300, position.y);
        cloudObject.transform.rotation = Quaternion.AngleAxis(180, new Vector3(1.0f, 0.0f, 0.0f));
        cloudObject.transform.parent = parent;

        SetVisible(false);

        lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < lodMeshes.Length; i++) {
            lodMeshes[i] = new LODMesh(detailLevels[i].lod);
            lodMeshes[i].UpdateCallback += UpdateTerrainChunk;
            if (i == colliderLODIndex) {
                lodMeshes[i].UpdateCallback += UpdateCollisionMesh;
            }
        }

        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;

    }

    public void Load() {
        ThreadedDataRequester.RequestData(() => CombinedMapGenerator.GenerateCombinedMap(meshSettings.NumVerticesPerLine, meshSettings.NumVerticesPerLine, heightMapSettings, biomeSettings, sampleCentre), OnCombinedMapReceived);
    }

    public void OnCombinedMapReceived(object combinedMapObject) {
        CombinedMap map = (CombinedMap)combinedMapObject;
        heightMap = map.heightMap;
        biomeMap = map.biomeMap;
        mapReceived = true;
        UpdateTerrainChunk();
    }

    Vector2 ViewerPosition {
        get {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }

    System.Random rnd = new System.Random();

    private void PlantTrees(HeightMap heightMap, BiomeMap biomeMap) {
        int length = heightMap.values.GetLength(0);
        for (int y = 0; y < length; y++) {
            for (int x = 0; x < length; x++) {
                if (rnd.Next(0, 5000) < biomeMap.biomes[x, y].GetVegetationFrequency()) {
                    Vector3 plantPos = new Vector3(x - length / 2.0f + sampleCentre.x, heightMap.values[x, y], -y + length / 2.0f + sampleCentre.y);
                    //PoolManager.Instance.ReuseObject(biomeMap.biomes[x, y].GetRandomPlant(), treePos);
                    //var plant = PoolManager.Instance.RequestObject(biomeMap.biomes[x, y].GetRandomPlant(), treePos);
                    var plant = PoolManager.Instance.GetObject(biomeMap.biomes[x, y].GetRandomPlant(), plantPos);
                    if (plant != null) {
                        plants.Add(plant);
                    }
                }
            }
        }
    }

    public void UpdateTerrainChunk() {
        if (mapReceived) {
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(ViewerPosition));

            bool wasVisible = IsVisible();
            bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

            if (visible) {
                int lodIndex = 0;
                for (int i = 0; i < detailLevels.Length - 1; i++) {
                    if (viewerDistanceFromNearestEdge > detailLevels[i].visibleDistanceThreshold) {
                        lodIndex = i + 1;
                    } else {
                        break;
                    }
                }

                if (lodIndex != previousLODIndex) {
                    LODMesh lodMesh = lodMeshes[lodIndex];
                    if (lodMesh.hasMesh) {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    } else if (!lodMesh.hasRequestedMesh) {
                        lodMesh.RequestMesh(heightMap.values, meshSettings);
                    }
                }
                if (firstGeneration) {
                    PlantTrees(heightMap, biomeMap);
                    firstGeneration = false;
                }
            }

            if (wasVisible != visible) {
                SetVisible(visible);
                if (OnVisibilityChanged != null) {
                    OnVisibilityChanged(this, visible);
                }
            }
        }
    }

    public void UpdateCollisionMesh() {
        if (!hasSetCollider) {
            float sqrDistanceFromVieweToEdge = bounds.SqrDistance(ViewerPosition);

            if (sqrDistanceFromVieweToEdge < detailLevels[colliderLODIndex].SqrVisibleDistanceThreshold) {
                if (!lodMeshes[colliderLODIndex].hasRequestedMesh) {
                    lodMeshes[colliderLODIndex].RequestMesh(heightMap.values, meshSettings);
                }
            }

            if (sqrDistanceFromVieweToEdge < colliderGenerationDistanceTreshold * colliderGenerationDistanceTreshold) {
                if (lodMeshes[colliderLODIndex].hasMesh) {
                    meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                    hasSetCollider = true;
                }
            }
        }
    }

    public void SetVisible(bool visible) {
        meshObject.SetActive(visible);
        cloudObject.SetActive(visible);
        if (!visible) {
            foreach (var plant in plants) {
                plant.SetActive(false);
            }
        }
    }

    public bool IsVisible() {
        return meshObject.activeSelf;
    }
}

class LODMesh {
    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    readonly int lod;
    public event System.Action UpdateCallback;

    public LODMesh(int lod) {
        this.lod = lod;
    }

    void OnMeshDataReceived(object meshDataObject) {
        mesh = ((MeshData)meshDataObject).CreateMesh();
        hasMesh = true;
        UpdateCallback();
    }

    public void RequestMesh(float[,] values, MeshSettings meshSettings) {
        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(values, meshSettings, lod), OnMeshDataReceived);
    }
}