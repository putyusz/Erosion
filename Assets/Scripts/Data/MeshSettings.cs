using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MeshSettings : UpdatableData {
    public const int numSupportedLODs = 5;
    public const int numSupportedChunkSizes = 9;
    public static readonly int[] supportedChunkSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };

    public float meshScale = 2.5f;

    [Range(0, numSupportedChunkSizes - 1)]
    public int chunkSizeIndex;

    //num vertices per line of mesh rendered ad LOD = 0. Includes the 2 extra vertices that are excluded from final mesh, but used for calculating normals
    public int NumVerticesPerLine
    {
        get
        {
            return supportedChunkSizes[chunkSizeIndex] + 1;
        }
    }

    public float MeshWorldSize
    {
        get
        {
            return (NumVerticesPerLine - 3) * meshScale;
        }
    }
}
