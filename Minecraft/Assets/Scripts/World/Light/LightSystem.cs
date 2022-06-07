using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
public static class LightSystem
{
    public static int globalLightLevel = 5;
    /*
    public static void RecalculateLights (Block[,,] blocks)
    {
        CalculateAllLights(blocks);
    }

    private static void CalculateAllLights (Block[,,] blocks)
    {
        int chunkWidth = blocks.GetLength(0);
        int chunkHeight = blocks.GetLength(1);

        // Cast a sunlight ray from top to bottom
        // Once we find one none transparent block, we know that all blocks below are not sunlighted
        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
                for (int y = chunkHeight - 1; y > 0; y--)
                {
                    Block thisBlock = blocks[x, y, z];

                    thisBlock.lightData.naturalLightLevel = 0;

                    if (!thisBlock.BlockData.isEmmitingLight)
                        thisBlock.lightData.artificialLightLevel = 0;
                }
            }
        }

        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
                for (int y = chunkHeight - 1; y > 0; y--)
                {
                    Block thisBlock = blocks[x, y, z];

                    thisBlock.lightData.naturalLightLevel = globalLightLevel;

                    if (!thisBlock.BlockData.isTransparent)
                        break;
                }
            }
        }

        Vector3Int[] positions = new Vector3Int[]
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
        };

        for (int i = 15; i > 0; i--)
        { 
            for (int x = 0; x < chunkWidth; x++)
            {
                for (int z = 0; z < chunkWidth; z++)
                {
                    for (int y = chunkHeight - 1; y >= 0; y--)
                    {
                        Block thisBlock = blocks[x, y, z];

                        if (!thisBlock.BlockData.isTransparent)
                            continue;

                        positions[0].x = x + 1;
                        positions[0].y = y;
                        positions[0].z = z;

                        positions[1].x = x - 1;
                        positions[1].y = y;
                        positions[1].z = z;

                        positions[2].x = x;
                        positions[2].y = y + 1;
                        positions[2].z = z;

                        positions[3].x = x;
                        positions[3].y = y - 1;
                        positions[3].z = z;

                        positions[4].x = x;
                        positions[4].y = y;
                        positions[4].z = z + 1;

                        positions[5].x = x;
                        positions[5].y = y;
                        positions[5].z = z - 1;

                        if (thisBlock.lightData.naturalLightLevel == i)
                        {
                            foreach (Vector3Int position in positions)
                            {
                                if (position.x > (chunkWidth - 1) || position.x < 0 || position.z > (chunkWidth - 1) || position.z < 0 || position.y > (chunkHeight - 1) || position.y < 0)
                                    continue;

                                if (blocks[position.x, position.y, position.z].lightData.naturalLightLevel < i - 1)
                                    blocks[position.x, position.y, position.z].lightData.naturalLightLevel = i - 1;
                            }
                        }

                        if (thisBlock.lightData.artificialLightLevel == i)
                        {
                            foreach (Vector3Int position in positions)
                            {
                                if (position.x > (chunkWidth - 1) || position.x < 0 || position.z > (chunkWidth - 1) || position.z < 0 || position.y > (chunkHeight - 1) || position.y < 0)
                                    continue;

                                if (blocks[position.x, position.y, position.z].lightData.artificialLightLevel < i - 1)
                                    blocks[position.x, position.y, position.z].lightData.artificialLightLevel = i - 1;
                            }
                        }

                    }
                }
            }
        }
    }


    public static void RecalculateAllLights (Chunk chunk, bool crossChunkCalculationsEnabled = true)
    {
        Block[,,] blocks = chunk.blocks;

        int chunkWidth = blocks.GetLength(0);
        int chunkHeight = blocks.GetLength(1);

        // Cast a sunlight ray from top to bottom
        // Once we find one none transparent block, we know that all blocks below are not sunlighted
        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
                for (int y = chunkHeight - 1; y > 0; y--)
                {
                    Block thisBlock = blocks[x, y, z];

                    thisBlock.lightData.naturalLightLevel = 0;

                    if (!thisBlock.BlockData.isEmmitingLight)
                        thisBlock.lightData.artificialLightLevel = 0;
                }
            }
        }

        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
                for (int y = chunkHeight - 1; y > 0; y--)
                {
                    Block thisBlock = blocks[x, y, z];

                    thisBlock.lightData.naturalLightLevel = globalLightLevel;

                    if (!thisBlock.BlockData.isTransparent)
                        break;
                }
            }
        }

        if (crossChunkCalculationsEnabled)
        {
            Block[,,] blocksFront = chunk.chunkNeighbours.FrontNeighbourChunk.blocks;
            Block[,,] blocksBack = chunk.chunkNeighbours.BackNeighbourChunk.blocks;

            for (int x = 0; x < chunkWidth; x++)
            {
                for (int y = 0; y < chunkHeight; y++)
                {
                    Block thisBlockFront = blocks[x, y, 0];
                    Block thisBlockFrontNeighbour = blocksFront[x, y, chunkWidth - 1];

                    if (thisBlockFrontNeighbour.lightData.naturalLightLevel > thisBlockFront.lightData.naturalLightLevel)
                        thisBlockFront.lightData.naturalLightLevel = thisBlockFrontNeighbour.lightData.naturalLightLevel - 1;

                    if (thisBlockFrontNeighbour.lightData.artificialLightLevel > thisBlockFront.lightData.artificialLightLevel)
                        thisBlockFront.lightData.artificialLightLevel = thisBlockFrontNeighbour.lightData.artificialLightLevel - 1;

                    Block thisBlockBack = blocks[x, y, chunkWidth - 1];
                    Block thisBlockBackNeighbour = blocksBack[x, y, 0];

                    if (thisBlockBackNeighbour.lightData.naturalLightLevel > thisBlockBack.lightData.naturalLightLevel)
                        thisBlockBack.lightData.naturalLightLevel = thisBlockBackNeighbour.lightData.naturalLightLevel - 1;

                    if (thisBlockBackNeighbour.lightData.artificialLightLevel > thisBlockBack.lightData.artificialLightLevel)
                        thisBlockBack.lightData.artificialLightLevel = thisBlockBackNeighbour.lightData.artificialLightLevel - 1;
                }
            }

            Block[,,] blocksRight = chunk.chunkNeighbours.RightNeighbourChunk.blocks;
            Block[,,] blocksLeft = chunk.chunkNeighbours.LeftNeighbourChunk.blocks;

            for (int z = 0; z < chunkWidth; z++)
            {
                for (int y = 0; y < chunkHeight; y++)
                {
                    Block thisBlockLeft = blocks[0, y, z];
                    Block thisBlockLeftNeighbour = blocksLeft[chunkWidth - 1, y, z];

                    if (thisBlockLeftNeighbour.lightData.naturalLightLevel > thisBlockLeft.lightData.naturalLightLevel)
                        thisBlockLeft.lightData.naturalLightLevel = thisBlockLeftNeighbour.lightData.naturalLightLevel - 1;

                    if (thisBlockLeftNeighbour.lightData.artificialLightLevel > thisBlockLeft.lightData.artificialLightLevel)
                        thisBlockLeft.lightData.artificialLightLevel = thisBlockLeftNeighbour.lightData.artificialLightLevel - 1;

                    Block thisBlockRight = blocks[chunkWidth - 1, y, z];
                    Block thisBlockRightNeighbour = blocksRight[0, y, z];

                    if (thisBlockRightNeighbour.lightData.naturalLightLevel > thisBlockRight.lightData.naturalLightLevel)
                        thisBlockRight.lightData.naturalLightLevel = thisBlockRightNeighbour.lightData.naturalLightLevel - 1;

                    if (thisBlockRightNeighbour.lightData.artificialLightLevel > thisBlockRight.lightData.artificialLightLevel)
                        thisBlockRight.lightData.artificialLightLevel = thisBlockRightNeighbour.lightData.artificialLightLevel - 1;
                }
            }
        }

        Vector3Int[] positions = new Vector3Int[]
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
        };

        for (int i = 15; i > 0; i--)
        {
            for (int x = 0; x < chunkWidth; x++)
            {
                for (int z = 0; z < chunkWidth; z++)
                {
                    for (int y = chunkHeight - 1; y >= 0; y--)
                    {
                        Block thisBlock = blocks[x, y, z];

                        if (!thisBlock.BlockData.isTransparent)
                            continue;

                        positions[0].x = x + 1;
                        positions[0].y = y;
                        positions[0].z = z;

                        positions[1].x = x - 1;
                        positions[1].y = y;
                        positions[1].z = z;

                        positions[2].x = x;
                        positions[2].y = y + 1;
                        positions[2].z = z;

                        positions[3].x = x;
                        positions[3].y = y - 1;
                        positions[3].z = z;

                        positions[4].x = x;
                        positions[4].y = y;
                        positions[4].z = z + 1;

                        positions[5].x = x;
                        positions[5].y = y;
                        positions[5].z = z - 1;

                        if (thisBlock.lightData.naturalLightLevel == i)
                        {
                            foreach (Vector3Int position in positions)
                            {
                                if (position.x > (chunkWidth - 1) || position.x < 0 || position.z > (chunkWidth - 1) || position.z < 0 || position.y > (chunkHeight - 1) || position.y < 0)
                                    continue;

                                if (blocks[position.x, position.y, position.z].lightData.naturalLightLevel < i - 1)
                                    blocks[position.x, position.y, position.z].lightData.naturalLightLevel = i - 1;
                            }
                        }

                        if (thisBlock.lightData.artificialLightLevel == i)
                        {
                            foreach (Vector3Int position in positions)
                            {
                                if (position.x > (chunkWidth - 1) || position.x < 0 || position.z > (chunkWidth - 1) || position.z < 0 || position.y > (chunkHeight - 1) || position.y < 0)
                                    continue;

                                if (blocks[position.x, position.y, position.z].lightData.artificialLightLevel < i - 1)
                                    blocks[position.x, position.y, position.z].lightData.artificialLightLevel = i - 1;
                            }
                        }

                    }
                }
            }
        }
    }



    */

}
