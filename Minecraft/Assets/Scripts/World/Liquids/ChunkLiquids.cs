using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkLiquids
{
    public byte[,,] water; // values 0-8, 0 is null, 1 is water source block, 2-8 flowing water
    public byte[,,] lava; // values 0-8, 0 is null, 1 is lava source block, 2-8 flowing lava

    public readonly LiquidVertex liquidVertex;
    private readonly Chunk chunk;
    private readonly int chunkWidth;
    private readonly int chunkHeight;

    public ChunkLiquids (Chunk chunk)
    {
        this.chunk = chunk;
        chunkWidth = chunk.voxelSettingsData.GetChunkSize();
        chunkHeight = chunkWidth * chunk.voxelSettingsData.VerticalSubChunkCount;

        liquidVertex = new LiquidVertex(chunk);
        water = new byte[chunkWidth, chunkHeight, chunkWidth];
        lava = new byte[chunkWidth, chunkHeight, chunkWidth];
    }

    public bool IsWater (Vector3Int position)
    {
        return water[position.x, position.y, position.z] != 0;
    }

    public bool IsLava (Vector3Int position)
    {
        return lava[position.x, position.y, position.z] != 0;
    }

    public void PlaceWaterSource (Vector3Int position)
    {
        water[position.x, position.y, position.z] = 1;
        SpreadWater(position);
    }

    public void SpreadWater (Vector3Int position)
    {
        // Let's check if provided voxel position exists on this chunk.
        // If not, we will try to execute this method on different chunk.
        if (position.x >= chunkWidth)
        {
            chunk.chunkNeighbours.RightNeighbourChunk.chunkLiquids.SpreadWater(new Vector3Int(0, position.y, position.z));
            return;
        }
        else if (position.z >= chunkWidth)
        {
            chunk.chunkNeighbours.BackNeighbourChunk.chunkLiquids.SpreadWater(new Vector3Int(position.x, position.y, 0));
            return;
        }
        else if (position.x < 0)
        {
            chunk.chunkNeighbours.LeftNeighbourChunk.chunkLiquids.SpreadWater(new Vector3Int(chunkWidth - 1, position.y, position.z));
            return;
        }
        else if (position.z < 0)
        {
            chunk.chunkNeighbours.FrontNeighbourChunk.chunkLiquids.SpreadWater(new Vector3Int(position.x, position.y, chunkWidth - 1));
            return;
        }
        else if (position.y <= 0)
        {
            return;
        }

        // Here we know that the position is valid and occurs in this chunk
        ref byte thisWater = ref water[position.x, position.y, position.z];

        // If the water amount on this voxel is null then don't do anything
        if (thisWater == 0)
            return;

        // If below this voxel, there is a solid voxel, we can start spreading water horizontally
        if (IsSolidBlock(new Vector3Int(position.x, position.y - 1, position.z)))
        {
            // 8 is the minimum water amount so it cannot spread horizontally anymore
            if (thisWater >= 8) 
                return;

            ref byte leftWater = ref GetWater(new Vector3Int(position.x - 1, position.y, position.z));
            ref byte rightWater = ref GetWater(new Vector3Int(position.x + 1, position.y, position.z));
            ref byte frontWater = ref GetWater(new Vector3Int(position.x, position.y, position.z - 1));
            ref byte backWater = ref GetWater(new Vector3Int(position.x, position.y, position.z + 1));

            if (!IsSolidBlock(new Vector3Int(position.x + 1, position.y, position.z)) && (rightWater == 0 || thisWater + 1 < rightWater))
            {
                rightWater = (byte)(thisWater + 1);
                SpreadWater(new Vector3Int(position.x + 1, position.y, position.z));
            }

            if (!IsSolidBlock(new Vector3Int(position.x - 1, position.y, position.z)) && (leftWater == 0 || thisWater + 1 < leftWater))
            {
                leftWater = (byte)(thisWater + 1);
                SpreadWater(new Vector3Int(position.x - 1, position.y, position.z));
            }

            if (!IsSolidBlock(new Vector3Int(position.x, position.y, position.z - 1)) && (frontWater == 0 || thisWater + 1 < frontWater))
            {
                frontWater = (byte)(thisWater + 1);
                SpreadWater(new Vector3Int(position.x, position.y, position.z - 1));
            }

            if (!IsSolidBlock(new Vector3Int(position.x, position.y, position.z + 1)) && (backWater == 0 || thisWater + 1 < backWater))
            {
                backWater = (byte)(thisWater + 1);
                SpreadWater(new Vector3Int(position.x, position.y, position.z + 1));
            }

        }
        else // Otherwise we try to spread water down
        {
            // Let's check if there is already water below
            ref byte lowerWater = ref water[position.x, position.y - 1, position.z];

            // If there is already exactly the same water as we would place then do nothing
            if (lowerWater == 1)
                return;

            // Otherwise we set lower voxel to water source, force chunk render,
            // and maybe wait a few frames before next propagation, so we can see effects in realtime
            lowerWater = 1;
            SpreadWater(new Vector3Int(position.x, position.y - 1, position.z));
        }

    }

    private ref byte GetWater (Vector3Int position)
    {
        if (position.x >= chunkWidth)
        {
            return ref chunk.chunkNeighbours.RightNeighbourChunk.chunkLiquids.water[0, position.y, position.z];
        }
        else if (position.z >= chunkWidth)
        {
            return ref chunk.chunkNeighbours.BackNeighbourChunk.chunkLiquids.water[position.x, position.y, 0];
        }
        else if (position.x < 0)
        {
            return ref chunk.chunkNeighbours.LeftNeighbourChunk.chunkLiquids.water[chunkWidth - 1, position.y, position.z];
        }
        else if (position.z < 0)
        {
            return ref chunk.chunkNeighbours.FrontNeighbourChunk.chunkLiquids.water[position.x, position.y, chunkWidth - 1];
        }
        else
        {
            return ref water[position.x, position.y, position.z];
        }
    }

    private bool IsSolidBlock (Vector3Int position)
    {
        if (position.x >= chunkWidth)
        {
            return chunk.chunkNeighbours.RightNeighbourChunk.chunkBlocks.isSolidBlock(0, position.y, position.z);
        }
        else if (position.z >= chunkWidth)
        {
            return chunk.chunkNeighbours.BackNeighbourChunk.chunkBlocks.isSolidBlock(position.x, position.y, 0);
        }
        else if (position.x < 0)
        {
            return chunk.chunkNeighbours.LeftNeighbourChunk.chunkBlocks.isSolidBlock(chunkWidth - 1, position.y, position.z);
        }
        else if (position.z < 0)
        {
            return chunk.chunkNeighbours.FrontNeighbourChunk.chunkBlocks.isSolidBlock(position.x, position.y, chunkWidth - 1);
        }
        else
        {
            return chunk.chunkBlocks.isSolidBlock(position.x, position.y, position.z);
        }
    }
}
