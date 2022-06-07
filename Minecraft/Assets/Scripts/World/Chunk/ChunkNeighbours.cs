using UnityEngine;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using System.Collections.Generic;

public class ChunkNeighbours
{
    public Chunk CenterChunk { get; private set; }
    public Chunk FrontNeighbourChunk { get; private set; }
    public Chunk BackNeighbourChunk { get; private set; }
    public Chunk LeftNeighbourChunk { get; private set; }
    public Chunk RightNeighbourChunk { get; private set; }

    public Chunk FrontLeftNeighbourChunk { get; private set; }
    public Chunk FrontRightNeighbourChunk { get; private set; }
    public Chunk BackLeftNeighbourChunk { get; private set; }
    public Chunk BackRightNeighbourChunk { get; private set; }

    private readonly int chunkSize;

    public ChunkNeighbours (Chunk thisChunk)
    {
        CenterChunk = thisChunk;
        chunkSize = CenterChunk.voxelSettingsData.GetChunkSize();
        FindNeighbourChunks();
    }

    public void FindNeighbourChunks ()
    {
        int chunkOffset = (int)(chunkSize * CenterChunk.voxelSettingsData.GetBlockSize());

        FrontNeighbourChunk = Map.singleton.GetChunk(new Vector3Int(CenterChunk.ChunkPosition.x, CenterChunk.ChunkPosition.y, CenterChunk.ChunkPosition.z - chunkOffset));
        BackNeighbourChunk = Map.singleton.GetChunk(new Vector3Int(CenterChunk.ChunkPosition.x, CenterChunk.ChunkPosition.y, CenterChunk.ChunkPosition.z + chunkOffset));
        LeftNeighbourChunk = Map.singleton.GetChunk(new Vector3Int(CenterChunk.ChunkPosition.x - chunkOffset, CenterChunk.ChunkPosition.y, CenterChunk.ChunkPosition.z));
        RightNeighbourChunk = Map.singleton.GetChunk(new Vector3Int(CenterChunk.ChunkPosition.x + chunkOffset, CenterChunk.ChunkPosition.y, CenterChunk.ChunkPosition.z));

        FrontLeftNeighbourChunk = Map.singleton.GetChunk(new Vector3Int(CenterChunk.ChunkPosition.x - chunkOffset, CenterChunk.ChunkPosition.y, CenterChunk.ChunkPosition.z - chunkOffset));
        FrontRightNeighbourChunk = Map.singleton.GetChunk(new Vector3Int(CenterChunk.ChunkPosition.x + chunkOffset, CenterChunk.ChunkPosition.y, CenterChunk.ChunkPosition.z - chunkOffset));
        BackLeftNeighbourChunk = Map.singleton.GetChunk(new Vector3Int(CenterChunk.ChunkPosition.x - chunkOffset, CenterChunk.ChunkPosition.y, CenterChunk.ChunkPosition.z + chunkOffset));
        BackRightNeighbourChunk = Map.singleton.GetChunk(new Vector3Int(CenterChunk.ChunkPosition.x + chunkOffset, CenterChunk.ChunkPosition.y, CenterChunk.ChunkPosition.z + chunkOffset));
    }

    /// <summary>
    /// 
    /// </summary>
    public ref byte GetWaterValueCrossChunkSafe(Vector3Int position)
    {
        if (position.x >= chunkSize)
        {
            position.x = 0;

            if (position.z >= chunkSize)
            {
                position.z = 0;
                return ref BackRightNeighbourChunk.chunkLiquids.water[position.x, position.y, position.z];
            }
            else if (position.z < 0)
            {
                position.z = chunkSize - 1;
                return ref FrontRightNeighbourChunk.chunkLiquids.water[position.x, position.y, position.z];
            }
            else
            {
                return ref RightNeighbourChunk.chunkLiquids.water[position.x, position.y, position.z];
            }
        }
        else if (position.x < 0)
        {
            position.x = chunkSize - 1;

            if (position.z >= chunkSize)
            {
                position.z = 0;
                return ref BackLeftNeighbourChunk.chunkLiquids.water[position.x, position.y, position.z];
            }
            else if (position.z < 0)
            {
                position.z = chunkSize - 1;
                return ref FrontLeftNeighbourChunk.chunkLiquids.water[position.x, position.y, position.z];
            }
            else
            {
                return ref LeftNeighbourChunk.chunkLiquids.water[position.x, position.y, position.z];
            }
        }
        else if (position.z >= chunkSize)
        {
            return ref BackNeighbourChunk.chunkLiquids.water[position.x, position.y, 0];
        }
        else if (position.z < 0)
        {
            return ref FrontNeighbourChunk.chunkLiquids.water[position.x, position.y, chunkSize - 1];
        }
        else
        {
            return ref CenterChunk.chunkLiquids.water[position.x, position.y, position.z];
        }
    }

    /// <summary>
    /// If you are not sure if voxel you are trying to access is placed on this chunk or on one of it's neighbours
    /// then this method will determine it
    /// </summary>
    public ChunkData TransformInsideChunkPositionToChunkReferenceAndPosition (Vector3Int position)
    {
        if (position.x >= chunkSize)
        {
            position.x -= chunkSize;

            if (position.z >= chunkSize)
            {
                position.z -= chunkSize;
                return new ChunkData(BackRightNeighbourChunk, position);
            }
            else if (position.z < 0)
            {
                position.z += chunkSize;
                return new ChunkData(FrontRightNeighbourChunk, position);
            }
            else
            {
                return new ChunkData(RightNeighbourChunk, position);
            }
        }
        else if (position.x < 0)
        {
            position.x += chunkSize;

            if (position.z >= chunkSize)
            {
                position.z -= chunkSize;
                return new ChunkData(BackLeftNeighbourChunk, position);
            }
            else if (position.z < 0)
            {
                position.z += chunkSize;
                return new ChunkData(FrontLeftNeighbourChunk, position);
            }
            else
            {
                return new ChunkData(LeftNeighbourChunk, position);
            }
        }
        else if (position.z >= chunkSize)
        {
            position.z -= chunkSize;

            return new ChunkData(BackNeighbourChunk, position);
        }
        else if (position.z < 0)
        {
            position.z += chunkSize;
            return new ChunkData(FrontNeighbourChunk, position);
        }
        else
        {
            return new ChunkData(CenterChunk, position);
        }
    }
}

public struct ChunkData
{
    public Vector3Int InsideChunkPosition { get; private set; }
    public Chunk Chunk { get; private set; }

    public ChunkData(Chunk chunk, Vector3Int insideChunkPosition)
    {
        InsideChunkPosition = insideChunkPosition;
        Chunk = chunk;
    }
}