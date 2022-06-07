using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkBlocks
{
    private Dictionary<int, Block>[,] blocks;

    public ChunkBlocks (int chunkSize)
    {
        blocks = new Dictionary<int, Block>[chunkSize, chunkSize];

        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                blocks[x, z] = new Dictionary<int, Block>();
            }
        }
    }

    public void SetBlock (int x, int y, int z, Block block)
    {
        blocks[x, z][y] = block;
    }

    public void RemoveBlock(int x, int y, int z)
    {
        blocks[x, z].Remove(y);
    }

    public Block GetBlock (int x, int y, int z)
    {
        if (blocks[x, z].TryGetValue(y, out Block block))
            return block;
        else
            return null;
    }

    public bool isSolidBlock (int x, int y, int z)
    {
        Block block = GetBlock(x, y, z);

        if (block == null)
            return false;

        return true;
    }
}
