using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Voxel Settings File", menuName = "ScriptableObjects/World Settings/Create Voxel Settings File", order = 1)]
public class VoxelSettingsData : ScriptableObject
{
    [SerializeField] private eChunkSize chunkSize = eChunkSize.SIZE_16;
    [SerializeField] private eBlockSize blockSize = eBlockSize.SIZE_1;
    [field: SerializeField, Range(8, 24)] public int VerticalSubChunkCount { get; private set; }

    public int GetChunkSize ()
    {
        return (int)chunkSize;
    }

    public float GetBlockSize ()
    {
        return blockSize switch
        {
            eBlockSize.SIZE_1 => 1.0f,
            eBlockSize.SIZE_0_75 => 0.75f,
            eBlockSize.SIZE_0_5 => 0.5f,
            eBlockSize.SIZE_0_25 => 0.25f,
            _ => 1.0f,
        };
    }
}

public enum eChunkSize
{
    SIZE_32 = 32,
    SIZE_16 = 16
}

public enum eBlockSize
{
    SIZE_1 = 1,
    SIZE_0_75 = 2,
    SIZE_0_5 = 3,
    SIZE_0_25 = 4,
}