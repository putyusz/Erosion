using System.Collections.Generic;
using System.Diagnostics;
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

    LODInfo[] detailLevels;
    LODMesh[] lodMeshes;
    readonly int colliderLODIndex;

    HeightMap heightMap;

    bool mapReceived;
    int previousLODIndex = -1;
    bool hasSetCollider;
    readonly float maxViewDistance;

    readonly HeightMapSettings heightMapSettings;
    MeshSettings meshSettings;
    Transform viewer;

    private Erosion erosion = new Erosion();

    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material mapMaterial) {
        this.coord = coord;
        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.viewer = viewer;

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

    private static float[] MatrixToList(float[,] matrix) {
        int size = matrix.GetLength(0);
        float[] list = new float[size * size];

        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                list[j + i * size] = matrix[i, j];
            }
        }

        return list;
    }

    private static float[,] ListToMatrix(float[] list) {
        int size = (int)Mathf.Sqrt(list.Length);
        float[,] matrix = new float[size, size];

        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                matrix[i, j] = list[j + i * size];
            }
        }

        return matrix;
    }

    public void Load() {
        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.NumVerticesPerLine, meshSettings.NumVerticesPerLine, heightMapSettings, sampleCentre), OnHeightMapReceived);
    }

    public void OnHeightMapReceived(object heightMapObject) {
        heightMap = (HeightMap)heightMapObject;
        var input = MatrixToList(heightMap.values);
        Stopwatch st = new Stopwatch();
        st.Start();
        var output = erosion.Erode(input, heightMap.values.GetLength(0), 70000);
        st.Stop();
        using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@"C:\Users\patyu\Desktop\test.txt", true)) {
            file.WriteLine(st.Elapsed);
        }
        heightMap.values = ListToMatrix(output);
        mapReceived = true;
        UpdateTerrainChunk();
    }

    Vector2 ViewerPosition {
        get {
            return new Vector2(viewer.position.x, viewer.position.z);
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
            }

            if (wasVisible != visible) {
                SetVisible(visible);
                OnVisibilityChanged?.Invoke(this, visible);
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