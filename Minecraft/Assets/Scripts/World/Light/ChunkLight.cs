using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class ChunkLight
{
    public byte[,,] naturalLightLevels;
    public byte[,,] artificialLightLevels;
    public bool[,,] isBlockTransparent;

    private readonly byte globalLightLevel = 15; // Should be always max value
    private int globalLightLevelDifference = 0;

    private readonly Chunk chunk;
    private readonly int chunkWidth;
    private readonly int chunkHeight;

    public ChunkLight (Chunk chunk)
    {
        this.chunk = chunk;
        chunkWidth = chunk.voxelSettingsData.GetChunkSize();
        chunkHeight = chunkWidth * chunk.voxelSettingsData.VerticalSubChunkCount;

        naturalLightLevels = new byte[chunkWidth, chunkHeight, chunkWidth];
        artificialLightLevels = new byte[chunkWidth, chunkHeight, chunkWidth];
        isBlockTransparent = new bool[chunkWidth, chunkHeight, chunkWidth];
    }

    public void UpdateNaturalLight (byte newGlobalLightLevel)
    {
        globalLightLevelDifference = globalLightLevel - newGlobalLightLevel;
    }

    /// <summary>
    /// Calculates light only for this one chunk, it doesn't include cross chunk light calculations.
    /// </summary>
    public void RecalculateLightsOptimized ()
    {
        ChunkBlocks blocks = chunk.chunkBlocks;

        // Initialize lists
        List<List<Vector3Int>> lightSourcesNatural = new List<List<Vector3Int>>();
        List<List<Vector3Int>> lightSourcesArtificial = new List<List<Vector3Int>>();
        for (int i = 0; i <= 15; i++)
        {
            lightSourcesNatural.Add(new List<Vector3Int>());
            lightSourcesArtificial.Add(new List<Vector3Int>());
        }

        // Reset light arrays, check which voxels are transparent and save that data to array for fast lookup,
        // find artificial light sources and add them to list for later light spreading
        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
                for (int y = chunkHeight - 1; y > 0; y--)
                {
                    Block thisBlock = blocks.GetBlock(x, y, z);

                    naturalLightLevels[x, y, z] = 0;
                    artificialLightLevels[x, y, z] = 0;

                    if (thisBlock != null)
                    {
                        if (thisBlock.BlockData.isTransparent)
                            isBlockTransparent[x, y, z] = true;
                        else
                            isBlockTransparent[x, y, z] = false;

                        if (thisBlock.BlockData.isEmmitingLight)
                        {
                            artificialLightLevels[x, y, z] = thisBlock.BlockData.lightAmount;
                            lightSourcesArtificial[thisBlock.BlockData.lightAmount].Add(new Vector3Int(x, y, z));
                        }
                    }
                    else
                    {
                        isBlockTransparent[x, y, z] = true;
                    }
                }
            }
        }

        // Fill natural sun light without any more calculations
        // for heights above highest solid voxel
        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
                for (int y = chunkHeight - 1; y > chunk.HighestSolidVoxel; y--)
                {
                    Block thisBlock = blocks.GetBlock(x, y, z);

                    if (thisBlock != null && !thisBlock.BlockData.isTransparent)
                    {
                        break;
                    }
                    else
                    {
                        naturalLightLevels[x, y, z] = globalLightLevel;
                    }
                }
            }
        }

        // Calculates the sun light ray from top to bottom.
        // Each voxel that is sunlighted is added to list for future light spreading
        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
                for (int y = chunk.HighestSolidVoxel; y > 0; y--)
                {
                    Block thisBlock = blocks.GetBlock(x, y, z);

                    if (thisBlock != null && !thisBlock.BlockData.isTransparent)
                    {
                        break;
                    }
                    else
                    {
                        naturalLightLevels[x, y, z] = globalLightLevel;
                        lightSourcesNatural[globalLightLevel].Add(new Vector3Int(x, y, z));
                    }
                }
            }
        }

        // This part of code below could use SpreadNaturalLights & SpreadArtificialLights methods instead, but it would lead to cross chunks calculations
        // and I believe this one is faster because it doesn't need to check what array to access every time

        Vector3Int[] positions = new Vector3Int[]
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
        };

        // Spread natural lights
        for (int lightLevel = 15; lightLevel > 1; lightLevel--)
        {
            int iterationsCount = lightSourcesNatural[lightLevel].Count;

            for (int i = 0; i < iterationsCount; i++)
            {
                Vector3Int thisVector = lightSourcesNatural[lightLevel][i];

                positions[0].x = thisVector.x + 1;
                positions[0].y = thisVector.y;
                positions[0].z = thisVector.z;

                positions[1].x = thisVector.x - 1;
                positions[1].y = thisVector.y;
                positions[1].z = thisVector.z;

                positions[2].x = thisVector.x;
                positions[2].y = thisVector.y + 1;
                positions[2].z = thisVector.z;

                positions[3].x = thisVector.x;
                positions[3].y = thisVector.y - 1;
                positions[3].z = thisVector.z;

                positions[4].x = thisVector.x;
                positions[4].y = thisVector.y;
                positions[4].z = thisVector.z + 1;

                positions[5].x = thisVector.x;
                positions[5].y = thisVector.y;
                positions[5].z = thisVector.z - 1;

                foreach (Vector3Int position in positions)
                {
                    if (position.x > (chunkWidth - 1) || position.x < 0 || position.z > (chunkWidth - 1) || position.z < 0 || position.y > (chunkHeight - 1) || position.y < 0)
                        continue;

                    if (naturalLightLevels[position.x, position.y, position.z] < lightLevel - 1 && isBlockTransparent[position.x, position.y, position.z])
                    {
                        naturalLightLevels[position.x, position.y, position.z] = (byte)(lightLevel - 1);
                        lightSourcesNatural[lightLevel - 1].Add(new Vector3Int(position.x, position.y, position.z));
                    }
                }
            }
        }

        // Spread artificial lights
        for (int lightLevel = 15; lightLevel > 1; lightLevel--)
        {
            int iterationsCount = lightSourcesArtificial[lightLevel].Count;

            for (int i = 0; i < iterationsCount; i++)
            {
                Vector3Int thisVector = lightSourcesArtificial[lightLevel][i];

                positions[0].x = thisVector.x + 1;
                positions[0].y = thisVector.y;
                positions[0].z = thisVector.z;

                positions[1].x = thisVector.x - 1;
                positions[1].y = thisVector.y;
                positions[1].z = thisVector.z;

                positions[2].x = thisVector.x;
                positions[2].y = thisVector.y + 1;
                positions[2].z = thisVector.z;

                positions[3].x = thisVector.x;
                positions[3].y = thisVector.y - 1;
                positions[3].z = thisVector.z;

                positions[4].x = thisVector.x;
                positions[4].y = thisVector.y;
                positions[4].z = thisVector.z + 1;

                positions[5].x = thisVector.x;
                positions[5].y = thisVector.y;
                positions[5].z = thisVector.z - 1;

                foreach (Vector3Int position in positions)
                {
                    if (position.x > (chunkWidth - 1) || position.x < 0 || position.z > (chunkWidth - 1) || position.z < 0 || position.y > (chunkHeight - 1) || position.y < 0)
                        continue;

                    if (artificialLightLevels[position.x, position.y, position.z] < lightLevel - 1 && isBlockTransparent[position.x, position.y, position.z])
                    {
                        artificialLightLevels[position.x, position.y, position.z] = (byte)(lightLevel - 1);
                        lightSourcesArtificial[lightLevel - 1].Add(new Vector3Int(position.x, position.y, position.z));
                    }
                }
            }
        }
    }

    /// <summary>
    /// After the light for this chunk is already calculated and at the same time neighbour chunks are loaded, 
    /// we can try to spread the lights that are in this chunk to the neighbour chunks.
    /// 
    /// We are looking for every transparent voxel that is placed on border of this chunk and that it's light level is greater than 1.
    /// Every voxel that meets above conditions is assigned to the appropriate list. After that we have all info needed to spread light.
    /// </summary>
    public void FloodToNeighbourChunks ()
    {
        QuickPrepareArrays();

        // Prepare lists of voxels to work on.
        List<List<Vector3Int>> lightSourcesNatural = new List<List<Vector3Int>>();
        List<List<Vector3Int>> lightSourcesArtificial = new List<List<Vector3Int>>();

        for (int i = 0; i <= 15; i++)
        {
            lightSourcesNatural.Add(new List<Vector3Int>());
            lightSourcesArtificial.Add(new List<Vector3Int>());
        }

        // Here we find front & back border chunk voxels
        for (int z = 0; z < chunkWidth; z += chunkWidth - 1)
        {
            for (int x = 0; x < chunkWidth; x++)
            {
                for (int y = 1; y < chunkHeight - 1; y++)
                {
                    TryAddLightSourceToList(x, y, z);
                }
            }
        }

        // Here we find left & right border chunk voxels
        for (int x = 0; x < chunkWidth; x += chunkWidth - 1)
        {
            for (int z = 1; z < chunkWidth - 1; z++)
            {
                for (int y = 1; y < chunkHeight - 1; y++)
                {
                    TryAddLightSourceToList(x, y, z);
                }
            }
        }

        void TryAddLightSourceToList (int x, int y, int z)
        {
            if (!isBlockTransparent[x, y, z])
                return;

            byte naturalLightLevel = naturalLightLevels[x, y, z];
            byte artificialLightLevel = artificialLightLevels[x, y, z];

            if (naturalLightLevel > 1)
                lightSourcesNatural[naturalLightLevel].Add(new Vector3Int(x, y, z));

            if (artificialLightLevel > 1)
                lightSourcesArtificial[artificialLightLevel].Add(new Vector3Int(x, y, z));
        }

        SpreadNaturalLights(lightSourcesNatural);
        SpreadArtificialLights(lightSourcesArtificial);
    }

    /// <summary>
    /// Perfect solution for quick runtime light recalculation e.g player placed or removed voxel.
    /// It's still not the fastest possible solution (it always recalculates every voxels that could be affected even if they are not)
    /// but it makes sure that there will never be any unwanted lighting bugs.
    /// </summary>
    /// <param name="inPosition">Inside this chunk position</param>
    public void RecalculateLightsFromPositionOptimized (Vector3Int inPosition)
    {
        QuickPrepareArrays();

        // wstepna inicjalizacja
        List<List<Vector3Int>> lightSourcesNatural = new List<List<Vector3Int>>();
        List<List<Vector3Int>> lightSourcesArtificial = new List<List<Vector3Int>>();
        for (int i = 0; i <= 15; i++)
        {
            lightSourcesNatural.Add(new List<Vector3Int>());
            lightSourcesArtificial.Add(new List<Vector3Int>());
        }

        int maxHeight = inPosition.y + 15;
        int minHeight = inPosition.y - 15;
        maxHeight = maxHeight > chunkHeight - 2 ? chunkHeight - 2 : maxHeight;
        minHeight = minHeight >= 0 ? minHeight : 0;

        // czyszczenie
        for (int x = inPosition.x - (chunkWidth - 2); x < inPosition.x + (chunkWidth - 1); x++)
        {
            for (int z = inPosition.z - (chunkWidth - 2); z < inPosition.z + (chunkWidth - 1); z++)
            {
                bool isSunlighted = true;
                bool isFirst = true;

                for (int y = maxHeight - 1; y > 1; y--)
                {
                    Block thisBlock = GetBlock(new Vector3Int(x, y, z));

                    if (isSunlighted)
                    {
                        ref byte thisBlockNaturalLightLevel = ref GetLightEntityInArrayNatural(new Vector3Int(x, y, z));

                        if (isFirst)
                        {
                            if (thisBlock == null)
                            {
                                if (thisBlockNaturalLightLevel != 15)
                                {
                                    isSunlighted = false;
                                    thisBlockNaturalLightLevel = 0;
                                }
                            }
                            else if (thisBlock.BlockData.isTransparent && thisBlockNaturalLightLevel == 15)
                            {

                            }
                            else
                            {
                                isSunlighted = false;
                                thisBlockNaturalLightLevel = 0;
                            }

                            isFirst = false;
                        }
                        else
                        {
                            if (thisBlock == null)
                            {
                                thisBlockNaturalLightLevel = 15;
                            }
                            else if (thisBlock.BlockData.isTransparent)
                            {
                                thisBlockNaturalLightLevel = 15;
                            }
                            else
                            {
                                isSunlighted = false;
                                thisBlockNaturalLightLevel = 0;
                            }
                        }
                    }
                    else
                    {
                        ref byte thisBlockNaturalLightLevel = ref GetLightEntityInArrayNatural(new Vector3Int(x, y, z));
                        thisBlockNaturalLightLevel = 0;
                    }

                    if (y > minHeight)
                    {
                        ref byte thisBlockArtificialLightLevel = ref GetLightEntityInArrayArtificial(new Vector3Int(x, y, z));

                        if (thisBlock != null && thisBlock.BlockData.isEmmitingLight)
                        {
                            thisBlockArtificialLightLevel = thisBlock.BlockData.lightAmount;
                        }
                        else
                        {
                            thisBlockArtificialLightLevel = 0;
                        }
                    }
                }
            }
        }

        // tworzenie list i polaczen do pozniejszego obliczania
        for (int x = inPosition.x - (chunkWidth - 1); x < inPosition.x + chunkWidth; x++)
        {
            for (int z = inPosition.z - (chunkWidth - 1); z < inPosition.z + chunkWidth; z++)
            {
                for (int y = maxHeight; y > 0; y--)
                {
                    byte thisBlockNaturalLightLevel = GetLightEntityInArrayNatural(new Vector3Int(x, y, z));
                    byte thisBlockArtificialLightLevel = GetLightEntityInArrayArtificial(new Vector3Int(x, y, z));

                    if (thisBlockNaturalLightLevel > 1)
                        lightSourcesNatural[thisBlockNaturalLightLevel].Add(new Vector3Int(x, y, z));

                    if (y >= minHeight && thisBlockArtificialLightLevel > 1)
                        lightSourcesArtificial[thisBlockArtificialLightLevel].Add(new Vector3Int(x, y, z));
                }
            }
        }

        SpreadNaturalLights(lightSourcesNatural);
        SpreadArtificialLights(lightSourcesArtificial);
    }

    private void SpreadNaturalLights (List<List<Vector3Int>> lightSourcesNatural)
    {
        Vector3Int[] positions = new Vector3Int[]
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
        };

        for (int lightLevel = 15; lightLevel > 1; lightLevel--)
        {
            int iterationsCount = lightSourcesNatural[lightLevel].Count;

            for (int i = 0; i < iterationsCount; i++)
            {
                Vector3Int thisVector = lightSourcesNatural[lightLevel][i];

                positions[0].x = thisVector.x + 1;
                positions[0].y = thisVector.y;
                positions[0].z = thisVector.z;

                positions[1].x = thisVector.x - 1;
                positions[1].y = thisVector.y;
                positions[1].z = thisVector.z;

                positions[2].x = thisVector.x;
                positions[2].y = thisVector.y + 1;
                positions[2].z = thisVector.z;

                positions[3].x = thisVector.x;
                positions[3].y = thisVector.y - 1;
                positions[3].z = thisVector.z;

                positions[4].x = thisVector.x;
                positions[4].y = thisVector.y;
                positions[4].z = thisVector.z + 1;

                positions[5].x = thisVector.x;
                positions[5].y = thisVector.y;
                positions[5].z = thisVector.z - 1;

                foreach (Vector3Int position in positions)
                {
                    ref byte thisPositionLightLevel = ref GetLightEntityInArrayNatural(position);

                    if (thisPositionLightLevel < lightLevel - 1 && CheckIsTransparentBlock(position))
                    {
                        thisPositionLightLevel = (byte)(lightLevel - 1);
                        lightSourcesNatural[lightLevel - 1].Add(new Vector3Int(position.x, position.y, position.z));
                    }
                }
            }
        }
    }

    private void SpreadArtificialLights (List<List<Vector3Int>> lightSourcesArtificial)
    {
        Vector3Int[] positions = new Vector3Int[]
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
        };

        for (int lightLevel = 15; lightLevel > 1; lightLevel--)
        {
            int iterationsCount = lightSourcesArtificial[lightLevel].Count;

            for (int i = 0; i < iterationsCount; i++)
            {
                Vector3Int thisVector = lightSourcesArtificial[lightLevel][i];

                positions[0].x = thisVector.x + 1;
                positions[0].y = thisVector.y;
                positions[0].z = thisVector.z;

                positions[1].x = thisVector.x - 1;
                positions[1].y = thisVector.y;
                positions[1].z = thisVector.z;

                positions[2].x = thisVector.x;
                positions[2].y = thisVector.y + 1;
                positions[2].z = thisVector.z;

                positions[3].x = thisVector.x;
                positions[3].y = thisVector.y - 1;
                positions[3].z = thisVector.z;

                positions[4].x = thisVector.x;
                positions[4].y = thisVector.y;
                positions[4].z = thisVector.z + 1;

                positions[5].x = thisVector.x;
                positions[5].y = thisVector.y;
                positions[5].z = thisVector.z - 1;

                foreach (Vector3Int position in positions)
                {
                    ref byte thisPositionLightLevel = ref GetLightEntityInArrayArtificial(position);

                    if (thisPositionLightLevel < lightLevel - 1 && CheckIsTransparentBlock(position))
                    {
                        thisPositionLightLevel = (byte)(lightLevel - 1);
                        lightSourcesArtificial[lightLevel - 1].Add(new Vector3Int(position.x, position.y, position.z));
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SmoothLightWithAmbientOcclusion ()
    {

    }








    public byte GetVoxelLightLevel (int x, int y, int z)
    {
        return GetVoxelLightLevel (new Vector3Int(x, y, z));
    }

    public byte GetVoxelLightLevel (Vector3Int voxelPosition)
    {
        byte naturalLightLevelRecalculated = (byte)(naturalLightLevels[voxelPosition.x, voxelPosition.y, voxelPosition.z] - (byte)globalLightLevelDifference);
        if (naturalLightLevelRecalculated < 0)
            naturalLightLevelRecalculated = 0;

       // ref byte naturalLightLevel = ref naturalLightLevels[voxelPosition.x, voxelPosition.y, voxelPosition.z];
        ref byte artificialLightLevel = ref artificialLightLevels[voxelPosition.x, voxelPosition.y, voxelPosition.z];

        if (artificialLightLevel > naturalLightLevelRecalculated)
            return artificialLightLevel;
        else
            return naturalLightLevelRecalculated;
    }

    public byte GetVoxelNaturalLightLevel(int x, int y, int z)
    {
        return GetVoxelNaturalLightLevel(new Vector3Int(x, y, z));
    }

    public byte GetVoxelNaturalLightLevel (Vector3Int voxelPosition)
    {
        return naturalLightLevels[voxelPosition.x, voxelPosition.y, voxelPosition.z];
    }

    public byte GetVoxelArtificialLightLevel(int x, int y, int z)
    {
        return GetVoxelArtificialLightLevel(new Vector3Int(x, y, z));
    }

    public byte GetVoxelArtificialLightLevel(Vector3Int voxelPosition)
    {
        return artificialLightLevels[voxelPosition.x, voxelPosition.y, voxelPosition.z];
    }

    private float LightLevelToIntensity (int lightLevel)
    {
        return lightLevel switch
        {
            15 => 0.0f,
            14 => 0.0625f,
            13 => 0.125f,
            12 => 0.1875f,
            11 => 0.25f,
            10 => 0.3125f,
            9 => 0.375f,
            8 => 0.4375f,
            7 => 0.5f,
            6 => 0.5625f,
            5 => 0.625f,
            4 => 0.6875f,
            3 => 0.75f,
            2 => 0.8125f,
            1 => 0.875f,
            0 => 0.9375f,
            _ => 1.0f,
        };
    }

    public float GetVoxelFaceLightIntensity (Vector3Int voxelPosition, eBlockFace voxelFace)
    {
        byte neighbourLightLevel;

        switch (voxelFace)
        {
            case eBlockFace.TOP:
                voxelPosition.y += 1;
                break;
            case eBlockFace.BOTTOM:
                voxelPosition.y -= 1;
                break;
            case eBlockFace.FRONT:
                voxelPosition.z -= 1;
                break;
            case eBlockFace.BACK:
                voxelPosition.z += 1;
                break;
            case eBlockFace.RIGHT:
                voxelPosition.x += 1;
                break;
            case eBlockFace.LEFT:
                voxelPosition.x -= 1;
                break;
        }

        #region Find the neighbour block (cause it might be placed on different chunk)
        if (voxelPosition.x < 0)
            neighbourLightLevel = chunk.chunkNeighbours.LeftNeighbourChunk.chunkLight.GetVoxelLightLevel(chunkWidth - 1, voxelPosition.y, voxelPosition.z);
        else if (voxelPosition.x > chunkWidth - 1)
            neighbourLightLevel = chunk.chunkNeighbours.RightNeighbourChunk.chunkLight.GetVoxelLightLevel(0, voxelPosition.y, voxelPosition.z);
        else if (voxelPosition.z < 0)
            neighbourLightLevel = chunk.chunkNeighbours.FrontNeighbourChunk.chunkLight.GetVoxelLightLevel(voxelPosition.x, voxelPosition.y, chunkWidth - 1);
        else if (voxelPosition.z > chunkWidth - 1)
            neighbourLightLevel = chunk.chunkNeighbours.BackNeighbourChunk.chunkLight.GetVoxelLightLevel(voxelPosition.x, voxelPosition.y, 0);
        else if (voxelPosition.y < 0 || voxelPosition.y > chunkHeight - 1)
            return 0;
        else
            neighbourLightLevel = GetVoxelLightLevel(voxelPosition.x, voxelPosition.y, voxelPosition.z);
        #endregion


        float neighbourlightIntensity = LightLevelToIntensity(neighbourLightLevel); 

        switch (voxelFace)
        {
            case eBlockFace.BOTTOM:
                neighbourlightIntensity += 0.04f * neighbourLightLevel;
                break;
            case eBlockFace.FRONT:
            case eBlockFace.BACK:
                neighbourlightIntensity += 0.0133f * neighbourLightLevel;
                break;
            case eBlockFace.RIGHT:
            case eBlockFace.LEFT:
                neighbourlightIntensity += 0.0266f * neighbourLightLevel;
                break;
        }

        return neighbourlightIntensity;
    }

    public float GetVoxelFaceNaturalLightIntensity (Vector3Int voxelPosition, eBlockFace voxelFace)
    {
        byte neighbourLightLevel;

        switch (voxelFace)
        {
            case eBlockFace.TOP:
                voxelPosition.y += 1;
                break;
            case eBlockFace.BOTTOM:
                voxelPosition.y -= 1;
                break;
            case eBlockFace.FRONT:
                voxelPosition.z -= 1;
                break;
            case eBlockFace.BACK:
                voxelPosition.z += 1;
                break;
            case eBlockFace.RIGHT:
                voxelPosition.x += 1;
                break;
            case eBlockFace.LEFT:
                voxelPosition.x -= 1;
                break;
        }

        #region Find the neighbour block (cause it might be placed on different chunk)
        if (voxelPosition.x < 0)
            neighbourLightLevel = chunk.chunkNeighbours.LeftNeighbourChunk.chunkLight.GetVoxelNaturalLightLevel(chunkWidth - 1, voxelPosition.y, voxelPosition.z);
        else if (voxelPosition.x > chunkWidth - 1)
            neighbourLightLevel = chunk.chunkNeighbours.RightNeighbourChunk.chunkLight.GetVoxelNaturalLightLevel(0, voxelPosition.y, voxelPosition.z);
        else if (voxelPosition.z < 0)
            neighbourLightLevel = chunk.chunkNeighbours.FrontNeighbourChunk.chunkLight.GetVoxelNaturalLightLevel(voxelPosition.x, voxelPosition.y, chunkWidth - 1);
        else if (voxelPosition.z > chunkWidth - 1)
            neighbourLightLevel = chunk.chunkNeighbours.BackNeighbourChunk.chunkLight.GetVoxelNaturalLightLevel(voxelPosition.x, voxelPosition.y, 0);
        else if (voxelPosition.y < 0 || voxelPosition.y > chunkHeight - 1)
            return 0;
        else
            neighbourLightLevel = GetVoxelNaturalLightLevel(voxelPosition.x, voxelPosition.y, voxelPosition.z);
        #endregion


        float neighbourlightIntensity = LightLevelToIntensity(neighbourLightLevel);

       /* switch (voxelFace)
        {
            case eBlockFace.BOTTOM:
                neighbourlightIntensity += 0.04f * neighbourLightLevel;
                break;
            case eBlockFace.FRONT:
            case eBlockFace.BACK:
                neighbourlightIntensity += 0.0133f * neighbourLightLevel;
                break;
            case eBlockFace.RIGHT:
            case eBlockFace.LEFT:
                neighbourlightIntensity += 0.0266f * neighbourLightLevel;
                break;
        } */

        return neighbourlightIntensity;
    }

    public float GetVoxelFaceArtificialLightIntensity (Vector3Int voxelPosition, eBlockFace voxelFace)
    {
        byte neighbourLightLevel;

        switch (voxelFace)
        {
            case eBlockFace.TOP:
                voxelPosition.y += 1;
                break;
            case eBlockFace.BOTTOM:
                voxelPosition.y -= 1;
                break;
            case eBlockFace.FRONT:
                voxelPosition.z -= 1;
                break;
            case eBlockFace.BACK:
                voxelPosition.z += 1;
                break;
            case eBlockFace.RIGHT:
                voxelPosition.x += 1;
                break;
            case eBlockFace.LEFT:
                voxelPosition.x -= 1;
                break;
        }

        #region Find the neighbour block (cause it might be placed on different chunk)
        if (voxelPosition.x < 0)
            neighbourLightLevel = chunk.chunkNeighbours.LeftNeighbourChunk.chunkLight.GetVoxelArtificialLightLevel(chunkWidth - 1, voxelPosition.y, voxelPosition.z);
        else if (voxelPosition.x > chunkWidth - 1)
            neighbourLightLevel = chunk.chunkNeighbours.RightNeighbourChunk.chunkLight.GetVoxelArtificialLightLevel(0, voxelPosition.y, voxelPosition.z);
        else if (voxelPosition.z < 0)
            neighbourLightLevel = chunk.chunkNeighbours.FrontNeighbourChunk.chunkLight.GetVoxelArtificialLightLevel(voxelPosition.x, voxelPosition.y, chunkWidth - 1);
        else if (voxelPosition.z > chunkWidth - 1)
            neighbourLightLevel = chunk.chunkNeighbours.BackNeighbourChunk.chunkLight.GetVoxelArtificialLightLevel(voxelPosition.x, voxelPosition.y, 0);
        else if (voxelPosition.y < 0 || voxelPosition.y > chunkHeight - 1)
            return 0;
        else
            neighbourLightLevel = GetVoxelArtificialLightLevel(voxelPosition.x, voxelPosition.y, voxelPosition.z);
        #endregion


        float neighbourlightIntensity = LightLevelToIntensity(neighbourLightLevel);

      /*  switch (voxelFace)
        {
            case eBlockFace.BOTTOM:
                neighbourlightIntensity += 0.04f * neighbourLightLevel;
                break;
            case eBlockFace.FRONT:
            case eBlockFace.BACK:
                neighbourlightIntensity += 0.0133f * neighbourLightLevel;
                break;
            case eBlockFace.RIGHT:
            case eBlockFace.LEFT:
                neighbourlightIntensity += 0.0266f * neighbourLightLevel;
                break;
        } */

        return neighbourlightIntensity;
    }

 

    private bool CheckIsTransparentBlock (Vector3Int pos)
    {
        if (pos.x >= chunkWidth)
        {
            pos.x -= chunkWidth;

            if (pos.z >= chunkWidth)
            {
                pos.z -= chunkWidth;
                return chunk.chunkNeighbours.BackRightNeighbourChunk.chunkLight.isBlockTransparent[pos.x, pos.y, pos.z];
            }
            else if (pos.z < 0)
            {
                pos.z += chunkWidth;
                return chunk.chunkNeighbours.FrontRightNeighbourChunk.chunkLight.isBlockTransparent[pos.x, pos.y, pos.z];
            }
            else
            {
                return chunk.chunkNeighbours.RightNeighbourChunk.chunkLight.isBlockTransparent[pos.x, pos.y, pos.z];
            }
        }
        else if (pos.x < 0)
        {
            pos.x += chunkWidth;

            if (pos.z >= chunkWidth)
            {
                pos.z -= chunkWidth;
                return chunk.chunkNeighbours.BackLeftNeighbourChunk.chunkLight.isBlockTransparent[pos.x, pos.y, pos.z];
            }
            else if (pos.z < 0)
            {
                pos.z += chunkWidth;
                return chunk.chunkNeighbours.FrontLeftNeighbourChunk.chunkLight.isBlockTransparent[pos.x, pos.y, pos.z];
            }
            else
            {
                return chunk.chunkNeighbours.LeftNeighbourChunk.chunkLight.isBlockTransparent[pos.x, pos.y, pos.z];
            }
        }
        else if (pos.z >= chunkWidth)
        {
            pos.z -= chunkWidth;
            return chunk.chunkNeighbours.BackNeighbourChunk.chunkLight.isBlockTransparent[pos.x, pos.y, pos.z];
        }
        else if (pos.z < 0)
        {
            pos.z += chunkWidth;
            return chunk.chunkNeighbours.FrontNeighbourChunk.chunkLight.isBlockTransparent[pos.x, pos.y, pos.z];
        }
        else
        {
            return isBlockTransparent[pos.x, pos.y, pos.z];
        }
    }

    private Block GetBlock (Vector3Int pos)
    {
        if (pos.x >= chunkWidth)
        {
            pos.x -= chunkWidth;

            if (pos.z >= chunkWidth)
            {
                pos.z -= chunkWidth;
                return chunk.chunkNeighbours.BackRightNeighbourChunk.chunkBlocks.GetBlock(pos.x, pos.y, pos.z);
            }
            else if (pos.z < 0)
            {
                pos.z += chunkWidth;
                return chunk.chunkNeighbours.FrontRightNeighbourChunk.chunkBlocks.GetBlock(pos.x, pos.y, pos.z);
            }
            else
            {
                return chunk.chunkNeighbours.RightNeighbourChunk.chunkBlocks.GetBlock(pos.x, pos.y, pos.z);
            }
        }
        else if (pos.x < 0)
        {
            pos.x += chunkWidth;

            if (pos.z >= chunkWidth)
            {
                pos.z -= chunkWidth;
                return chunk.chunkNeighbours.BackLeftNeighbourChunk.chunkBlocks.GetBlock(pos.x, pos.y, pos.z);
            }
            else if (pos.z < 0)
            {
                pos.z += chunkWidth;
                return chunk.chunkNeighbours.FrontLeftNeighbourChunk.chunkBlocks.GetBlock(pos.x, pos.y, pos.z);
            }
            else
            {
                return chunk.chunkNeighbours.LeftNeighbourChunk.chunkBlocks.GetBlock(pos.x, pos.y, pos.z);
            }
        }
        else if (pos.z >= chunkWidth)
        {
            pos.z -= chunkWidth;
            return chunk.chunkNeighbours.BackNeighbourChunk.chunkBlocks.GetBlock(pos.x, pos.y, pos.z);
        }
        else if (pos.z < 0)
        {
            pos.z += chunkWidth;
            return chunk.chunkNeighbours.FrontNeighbourChunk.chunkBlocks.GetBlock(pos.x, pos.y, pos.z);
        }
        else
        {
            return chunk.chunkBlocks.GetBlock(pos.x, pos.y, pos.z); 
        }
    }

  /*  public void RecalculateLightsOptimized (bool crossChunkCalculationsEnabled = true)
    {
        ChunkBlocks blocks = chunk.chunkBlocks;

        // wstepna inicjalizacja
        List<List<Vector3Int>> lightSources = new List<List<Vector3Int>>();
        for (int i = 0; i <= 15; i++)
        {
            lightSources.Add(new List<Vector3Int>());
        }

        // czyszczenie tablicy
        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
                for (int y = chunkHeight - 1; y > 0; y--)
                {
                    Block thisBlock = blocks.GetBlock(x, y, z);

                    naturalLightLevels[x, y, z] = 0;
                    artificialLightLevels[x, y, z] = 0;

                    if (thisBlock != null)
                    {
                        if (thisBlock.BlockData.isTransparent)
                            isBlockTransparent[x, y, z] = true;
                        else
                            isBlockTransparent[x, y, z] = false;

                        if (thisBlock.BlockData.isEmmitingLight)
                        {
                            artificialLightLevels[x, y, z] = thisBlock.BlockData.lightAmount;
                        }
                    }
                    else
                    {
                        isBlockTransparent[x, y, z] = true;
                    }
                }
            }
        }

        // obliczanie sloneczka
        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
                for (int y = chunkHeight - 1; y > 0; y--)
                {
                    Block thisBlock = blocks.GetBlock(x, y, z);

                    if (thisBlock != null && !thisBlock.BlockData.isTransparent)
                    {
                        break;
                    }
                    else
                    {
                        naturalLightLevels[x, y, z] = globalLightLevel;
                        lightSources[globalLightLevel].Add(new Vector3Int(x, y, z));
                    }
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

        for (int lightLevel = 15; lightLevel > 1; lightLevel--)
        {
            int iterationsCount = lightSources[lightLevel].Count;

            for (int i = 0; i < iterationsCount; i++)
            {
                Vector3Int thisVector = lightSources[lightLevel][i];

                positions[0].x = thisVector.x + 1;
                positions[0].y = thisVector.y;
                positions[0].z = thisVector.z;

                positions[1].x = thisVector.x - 1;
                positions[1].y = thisVector.y;
                positions[1].z = thisVector.z;

                positions[2].x = thisVector.x;
                positions[2].y = thisVector.y + 1;
                positions[2].z = thisVector.z;

                positions[3].x = thisVector.x;
                positions[3].y = thisVector.y - 1;
                positions[3].z = thisVector.z;

                positions[4].x = thisVector.x;
                positions[4].y = thisVector.y;
                positions[4].z = thisVector.z + 1;

                positions[5].x = thisVector.x;
                positions[5].y = thisVector.y;
                positions[5].z = thisVector.z - 1;

                foreach (Vector3Int position in positions)
                {
                    if (position.x > (chunkWidth - 1) || position.x < 0 || position.z > (chunkWidth - 1) || position.z < 0 || position.y > (chunkHeight - 1) || position.y < 0)
                        continue;

                    if (naturalLightLevels[position.x, position.y, position.z] < lightLevel - 1 && isBlockTransparent[position.x, position.y, position.z])
                    {
                        naturalLightLevels[position.x, position.y, position.z] = (byte)(lightLevel - 1);
                        lightSources[lightLevel - 1].Add(new Vector3Int(position.x, position.y, position.z));
                    }
                }
            }
        }

    } */

    // TODO CROSS CHUNKS CALCULATIONS ON NEW BYTES
    public void RecalculateLights (bool crossChunkCalculationsEnabled = true)
    {
    //    Block[,,] blocks = chunk.blocks;
        ChunkBlocks blocks = chunk.chunkBlocks;

        // Cast a sunlight ray from top to bottom
        // Once we find one none transparent block, we know that all blocks below are not sunlighted
        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
                for (int y = chunkHeight - 1; y > 0; y--)
                {
                    Block thisBlock = blocks.GetBlock(x, y, z);

                    naturalLightLevels[x, y, z] = 0;
                    artificialLightLevels[x, y, z] = 0;

                    if (thisBlock != null)
                    {
                        if (thisBlock.BlockData.isEmmitingLight)
                            artificialLightLevels[x, y, z] = thisBlock.BlockData.lightAmount;
                    }

               //     if (thisBlock != null && !thisBlock.BlockData.isEmmitingLight)
              //          artificialLightLevels[x, y, z] = 0;
                }
            }
        }

        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
                for (int y = chunkHeight - 1; y > 0; y--)
                {
                    Block thisBlock = blocks.GetBlock(x, y, z);

                    naturalLightLevels[x, y, z] = globalLightLevel;

                    if (thisBlock != null && !thisBlock.BlockData.isTransparent)
                        break;
                }
            }
        }

       /* if (crossChunkCalculationsEnabled)
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
        } */

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
                        Block thisBlock = blocks.GetBlock(x, y, z);

                        if (thisBlock != null && !thisBlock.BlockData.isTransparent)
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

                        if (naturalLightLevels[x, y, z] == i)
                        {
                            foreach (Vector3Int position in positions)
                            {
                                if (position.x > (chunkWidth - 1) || position.x < 0 || position.z > (chunkWidth - 1) || position.z < 0 || position.y > (chunkHeight - 1) || position.y < 0)
                                    continue;

                                if (naturalLightLevels[position.x, position.y, position.z] < i - 1)
                                    naturalLightLevels[position.x, position.y, position.z] = (byte)(i - 1);
                            }
                        }

                        if (artificialLightLevels[x, y, z] == i)
                        {
                            foreach (Vector3Int position in positions)
                            {
                                if (position.x > (chunkWidth - 1) || position.x < 0 || position.z > (chunkWidth - 1) || position.z < 0 || position.y > (chunkHeight - 1) || position.y < 0)
                                    continue;

                                if (artificialLightLevels[position.x, position.y, position.z] < i - 1)
                                    artificialLightLevels[position.x, position.y, position.z] = (byte)(i - 1);
                            }
                        }

                    }
                }
            }
        }
    }

    /* 
     
         public void RecalculateLights (bool crossChunkCalculationsEnabled = true)
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

                    naturalLightLevels[x, y, z] = 0;

                    if (thisBlock != null && !thisBlock.BlockData.isEmmitingLight)
                        artificialLightLevels[x, y, z] = 0;
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

                    naturalLightLevels[x, y, z] = globalLightLevel;

                    if (thisBlock != null && !thisBlock.BlockData.isTransparent)
                        break;
                }
            }
        }

       /* if (crossChunkCalculationsEnabled)
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
            for (int x = 0; x<chunkWidth; x++)
            {
                for (int z = 0; z<chunkWidth; z++)
                {
                    for (int y = chunkHeight - 1; y >= 0; y--)
                    {
                        Block thisBlock = blocks[x, y, z];

                        if (thisBlock != null && !thisBlock.BlockData.isTransparent)
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

                        if (naturalLightLevels[x, y, z] == i)
                        {
                            foreach (Vector3Int position in positions)
                            {
                                if (position.x > (chunkWidth - 1) || position.x< 0 || position.z>(chunkWidth - 1) || position.z< 0 || position.y>(chunkHeight - 1) || position.y< 0)
                                    continue;

                                if (naturalLightLevels[position.x, position.y, position.z] < i - 1)
                                    naturalLightLevels[position.x, position.y, position.z] = (byte) (i - 1);
                            }
                        }

                        if (artificialLightLevels[x, y, z] == i)
{
    foreach (Vector3Int position in positions)
    {
        if (position.x > (chunkWidth - 1) || position.x < 0 || position.z > (chunkWidth - 1) || position.z < 0 || position.y > (chunkHeight - 1) || position.y < 0)
            continue;

        if (artificialLightLevels[position.x, position.y, position.z] < i - 1)
            artificialLightLevels[position.x, position.y, position.z] = (byte)(i - 1);
    }
}

                    }
                }
            }
        }
    }
     
     */

    
    #region takie tam testy, pewnie do wywalenia w przyszlosci
    public void MojeKlyjTestowanieIWonchanie (Vector3Int position)
    {
        QuickPrepareArrays();

        for (int lightValue = 15; lightValue > 0; lightValue--)
        {
            for (int x = position.x - (chunkWidth - 1); x < position.x + chunkWidth; x++)
            {
                for (int z = position.z - (chunkWidth - 1); z < position.z + chunkWidth; z++)
                {
                    for (int y = chunkHeight - 2; y > 0; y--)
                    {
                        if (GetLightEntityInArrayArtificial(new Vector3Int(x, y, z)) == lightValue)
                        {
                            ref byte frontNeighbour = ref GetLightEntityInArrayArtificial(new Vector3Int(x, y, z - 1));
                            ref byte backNeighbour = ref GetLightEntityInArrayArtificial(new Vector3Int(x, y, z + 1));
                            ref byte leftNeighbour = ref GetLightEntityInArrayArtificial(new Vector3Int(x - 1, y, z));
                            ref byte rightNeighbour = ref GetLightEntityInArrayArtificial(new Vector3Int(x + 1, y, z));
                            ref byte topNeighbour = ref GetLightEntityInArrayArtificial(new Vector3Int(x, y + 1, z));
                            ref byte bottomNeighbour = ref GetLightEntityInArrayArtificial(new Vector3Int(x, y - 1, z));

                            if (lightValue - 1 > frontNeighbour)
                            {
                                frontNeighbour = (byte)(lightValue - 1);
                            }
                            if (lightValue - 1 > backNeighbour)
                            {
                                backNeighbour = (byte)(lightValue - 1);
                            }
                            if (lightValue - 1 > leftNeighbour)
                            {
                                leftNeighbour = (byte)(lightValue - 1);
                            }
                            if (lightValue - 1 > rightNeighbour)
                            {
                                rightNeighbour = (byte)(lightValue - 1);
                            }
                            if (lightValue - 1 > topNeighbour)
                            {
                                topNeighbour = (byte)(lightValue - 1);
                            }
                            if (lightValue - 1 > bottomNeighbour)
                            {
                                bottomNeighbour = (byte)(lightValue - 1);
                            }
                        }

                        if (GetLightEntityInArrayNatural(new Vector3Int(x, y, z)) == lightValue)
                        {
                            ref byte frontNeighbour = ref GetLightEntityInArrayNatural(new Vector3Int(x, y, z - 1));
                            ref byte backNeighbour = ref GetLightEntityInArrayNatural(new Vector3Int(x, y, z + 1));
                            ref byte leftNeighbour = ref GetLightEntityInArrayNatural(new Vector3Int(x - 1, y, z));
                            ref byte rightNeighbour = ref GetLightEntityInArrayNatural(new Vector3Int(x + 1, y, z));
                            ref byte topNeighbour = ref GetLightEntityInArrayNatural(new Vector3Int(x, y + 1, z));
                            ref byte bottomNeighbour = ref GetLightEntityInArrayNatural(new Vector3Int(x, y - 1, z));

                            if (lightValue - 1 > frontNeighbour)
                            {
                                frontNeighbour = (byte)(lightValue - 1);
                            }
                            if (lightValue - 1 > backNeighbour)
                            {
                                backNeighbour = (byte)(lightValue - 1);
                            }
                            if (lightValue - 1 > leftNeighbour)
                            {
                                leftNeighbour = (byte)(lightValue - 1);
                            }
                            if (lightValue - 1 > rightNeighbour)
                            {
                                rightNeighbour = (byte)(lightValue - 1);
                            }
                            if (lightValue - 1 > topNeighbour)
                            {
                                topNeighbour = (byte)(lightValue - 1);
                            }
                            if (lightValue - 1 > bottomNeighbour)
                            {
                                bottomNeighbour = (byte)(lightValue - 1);
                            }
                        }
                    }
                }
            }
        }
    }

    private byte[,,] leftLightArrayArtificial;
    private byte[,,] rightLightArrayArtificial;
    private byte[,,] frontLightArrayArtificial;
    private byte[,,] backLightArrayArtificial;
    private byte[,,] backLeftLightArrayArtificial;
    private byte[,,] backRightLightArrayArtificial;
    private byte[,,] frontLeftLightArrayArtificial;
    private byte[,,] frontRightLightArrayArtificial;

    private byte[,,] leftLightArrayNatural;
    private byte[,,] rightLightArrayNatural;
    private byte[,,] frontLightArrayNatural;
    private byte[,,] backLightArrayNatural;
    private byte[,,] backLeftLightArrayNatural;
    private byte[,,] backRightLightArrayNatural;
    private byte[,,] frontLeftLightArrayNatural;
    private byte[,,] frontRightLightArrayNatural;

    private void QuickPrepareArrays ()
    {
        leftLightArrayArtificial = chunk.chunkNeighbours.LeftNeighbourChunk.chunkLight.artificialLightLevels;
        rightLightArrayArtificial = chunk.chunkNeighbours.RightNeighbourChunk.chunkLight.artificialLightLevels;
        frontLightArrayArtificial = chunk.chunkNeighbours.FrontNeighbourChunk.chunkLight.artificialLightLevels;
        backLightArrayArtificial = chunk.chunkNeighbours.BackNeighbourChunk.chunkLight.artificialLightLevels;
        backLeftLightArrayArtificial = chunk.chunkNeighbours.BackLeftNeighbourChunk.chunkLight.artificialLightLevels;
        backRightLightArrayArtificial = chunk.chunkNeighbours.BackRightNeighbourChunk.chunkLight.artificialLightLevels;
        frontLeftLightArrayArtificial = chunk.chunkNeighbours.FrontLeftNeighbourChunk.chunkLight.artificialLightLevels;
        frontRightLightArrayArtificial = chunk.chunkNeighbours.FrontRightNeighbourChunk.chunkLight.artificialLightLevels;

        leftLightArrayNatural = chunk.chunkNeighbours.LeftNeighbourChunk.chunkLight.naturalLightLevels;
        rightLightArrayNatural = chunk.chunkNeighbours.RightNeighbourChunk.chunkLight.naturalLightLevels;
        frontLightArrayNatural = chunk.chunkNeighbours.FrontNeighbourChunk.chunkLight.naturalLightLevels;
        backLightArrayNatural = chunk.chunkNeighbours.BackNeighbourChunk.chunkLight.naturalLightLevels;
        backLeftLightArrayNatural = chunk.chunkNeighbours.BackLeftNeighbourChunk.chunkLight.naturalLightLevels;
        backRightLightArrayNatural = chunk.chunkNeighbours.BackRightNeighbourChunk.chunkLight.naturalLightLevels;
        frontLeftLightArrayNatural = chunk.chunkNeighbours.FrontLeftNeighbourChunk.chunkLight.naturalLightLevels;
        frontRightLightArrayNatural = chunk.chunkNeighbours.FrontRightNeighbourChunk.chunkLight.naturalLightLevels;
    }

    private ref byte GetLightEntityInArrayArtificial (Vector3Int pos)
    {
        if (pos.x >= chunkWidth)
        {
            pos.x -= chunkWidth;

            if (pos.z >= chunkWidth)
            {
                pos.z -= chunkWidth;
                return ref backRightLightArrayArtificial[pos.x, pos.y, pos.z];
            }
            else if (pos.z < 0)
            {
                pos.z += chunkWidth;
                return ref frontRightLightArrayArtificial[pos.x, pos.y, pos.z];
            }
            else
            {
                return ref rightLightArrayArtificial[pos.x, pos.y, pos.z];
            }    
        }
        else if (pos.x < 0)
        {
            pos.x += chunkWidth;

            if (pos.z >= chunkWidth)
            {
                pos.z -= chunkWidth;
                return ref backLeftLightArrayArtificial[pos.x, pos.y, pos.z];
            }
            else if (pos.z < 0)
            {
                pos.z += chunkWidth;
                return ref frontLeftLightArrayArtificial[pos.x, pos.y, pos.z];
            }
            else
            {
                return ref leftLightArrayArtificial[pos.x, pos.y, pos.z];
            }
        }
        else if (pos.z >= chunkWidth)
        {
            pos.z -= chunkWidth;
            return ref backLightArrayArtificial[pos.x, pos.y, pos.z];
        }
        else if (pos.z < 0)
        {
            pos.z += chunkWidth;
            return ref frontLightArrayArtificial[pos.x, pos.y, pos.z];
        }
        else
        {
            return ref artificialLightLevels[pos.x, pos.y, pos.z];
        }
    }

    private ref byte GetLightEntityInArrayNatural (Vector3Int pos)
    {
        if (pos.x >= chunkWidth)
        {
            pos.x -= chunkWidth;

            if (pos.z >= chunkWidth)
            {
                pos.z -= chunkWidth;
                return ref backRightLightArrayNatural[pos.x, pos.y, pos.z];
            }
            else if (pos.z < 0)
            {
                pos.z += chunkWidth;
                return ref frontRightLightArrayNatural[pos.x, pos.y, pos.z];
            }
            else
            {
                return ref rightLightArrayNatural[pos.x, pos.y, pos.z];
            }
        }
        else if (pos.x < 0)
        {
            pos.x += chunkWidth;

            if (pos.z >= chunkWidth)
            {
                pos.z -= chunkWidth;
                return ref backLeftLightArrayNatural[pos.x, pos.y, pos.z];
            }
            else if (pos.z < 0)
            {
                pos.z += chunkWidth;
                return ref frontLeftLightArrayNatural[pos.x, pos.y, pos.z];
            }
            else
            {
                return ref leftLightArrayNatural[pos.x, pos.y, pos.z];
            }
        }
        else if (pos.z >= chunkWidth)
        {
            pos.z -= chunkWidth;
            return ref backLightArrayNatural[pos.x, pos.y, pos.z];
        }
        else if (pos.z < 0)
        {
            pos.z += chunkWidth;
            return ref frontLightArrayNatural[pos.x, pos.y, pos.z];
        }
        else
        {
            return ref naturalLightLevels[pos.x, pos.y, pos.z];
        }
    }

    #endregion

    /*
    public void RecalculateLightsFromPosition (Vector3Int position)
    {
        byte sunLightLevel = globalLightLevel;
        int chunkSize = chunkWidth;

        Vector3Int[] positionVariants = new Vector3Int[]
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0)
        };

        Chunk[] chunkVariants = new Chunk[4];

        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize - x; z++)
            {
                positionVariants[0].x = position.x + x;
                positionVariants[0].y = 0;
                positionVariants[0].z = position.z + z;

                positionVariants[1].x = position.x + x;
                positionVariants[1].y = 0;
                positionVariants[1].z = position.z - z;

                positionVariants[2].x = position.x - x;
                positionVariants[2].y = 0;
                positionVariants[2].z = position.z + z;

                positionVariants[3].x = position.x - x;
                positionVariants[3].y = 0;
                positionVariants[3].z = position.z - z;

                for (int i = 0; i < positionVariants.Length; i++)
                {
                    bool front = false;
                    bool back = false;
                    bool right = false;
                    bool left = false;

                    if (positionVariants[i].x >= chunkSize)
                    {
                        positionVariants[i].x -= chunkSize;
                        right = true;
                    }
                    else if (positionVariants[i].x < 0)
                    {
                        positionVariants[i].x += chunkSize;
                        left = true;
                    }

                    if (positionVariants[i].z >= chunkSize)
                    {
                        positionVariants[i].z -= chunkSize;
                        back = true;
                    }
                    else if (positionVariants[i].z < 0)
                    {
                        positionVariants[i].z += chunkSize;
                        front = true;
                    }

                    if (!front && !back && !right && !left)
                    {
                        chunkVariants[i] = chunk;
                    }
                    else if (front)
                    {
                        if (right)
                        {
                            chunkVariants[i] = chunk.chunkNeighbours.FrontRightNeighbourChunk;
                        }
                        else if (left)
                        {
                            chunkVariants[i] = chunk.chunkNeighbours.FrontLeftNeighbourChunk;
                        }
                        else
                        {
                            chunkVariants[i] = chunk.chunkNeighbours.FrontNeighbourChunk;
                        }
                    }
                    else if (back)
                    {
                        if (right)
                        {
                            chunkVariants[i] = chunk.chunkNeighbours.BackRightNeighbourChunk;
                        }
                        else if (left)
                        {
                            chunkVariants[i] = chunk.chunkNeighbours.BackLeftNeighbourChunk;
                        }
                        else
                        {
                            chunkVariants[i] = chunk.chunkNeighbours.BackNeighbourChunk;
                        }
                    }
                    else if (right)
                    {
                        chunkVariants[i] = chunk.chunkNeighbours.RightNeighbourChunk;
                    }
                    else if (left)
                    {
                        chunkVariants[i] = chunk.chunkNeighbours.LeftNeighbourChunk;
                    }
                    else
                    {
                        UnityEngine.Debug.Log("Nie przemyslales czegos ;p");
                    }
                }

                // mamy wszystkie dane mozna dzialac

                for (int i = 0; i < positionVariants.Length; i++)
                {
                    ref Vector3Int thisVariantPosition = ref positionVariants[i];
                    bool isSunlighted = true;

                    for (int y = chunkHeight - 1; y > 0; y--)
                    {
                        thisVariantPosition.y = y;

                        if (isSunlighted)
                        {
                            if (chunkVariants[i].blocks[thisVariantPosition.x, thisVariantPosition.y, thisVariantPosition.z].BlockData.isTransparent)
                            {
                                chunkVariants[i].chunkLight.naturalLightLevels[thisVariantPosition.x, thisVariantPosition.y, thisVariantPosition.z] = sunLightLevel;

                                if (chunkVariants[i].blocks[thisVariantPosition.x, thisVariantPosition.y, thisVariantPosition.z].BlockData.isEmmitingLight)
                                {
                                    chunkVariants[i].chunkLight.artificialLightLevels[thisVariantPosition.x, thisVariantPosition.y, thisVariantPosition.z] = chunkVariants[i].blocks[thisVariantPosition.x, thisVariantPosition.y, thisVariantPosition.z].BlockData.lightAmount;
                                }
                                else
                                {
                                    chunkVariants[i].chunkLight.artificialLightLevels[thisVariantPosition.x, thisVariantPosition.y, thisVariantPosition.z] = 0;
                                }
                            }
                            else
                            {
                                chunkVariants[i].chunkLight.naturalLightLevels[thisVariantPosition.x, thisVariantPosition.y, thisVariantPosition.z] = 0;

                                if (chunkVariants[i].blocks[thisVariantPosition.x, thisVariantPosition.y, thisVariantPosition.z].BlockData.isEmmitingLight)
                                {
                                    chunkVariants[i].chunkLight.artificialLightLevels[thisVariantPosition.x, thisVariantPosition.y, thisVariantPosition.z] = chunkVariants[i].blocks[thisVariantPosition.x, thisVariantPosition.y, thisVariantPosition.z].BlockData.lightAmount;
                                }
                                else
                                {
                                    chunkVariants[i].chunkLight.artificialLightLevels[thisVariantPosition.x, thisVariantPosition.y, thisVariantPosition.z] = 0;
                                }

                                isSunlighted = false;
                            }
                        }
                        else
                        {
                            if (chunkVariants[i].blocks[thisVariantPosition.x, thisVariantPosition.y, thisVariantPosition.z].BlockData.isEmmitingLight)
                            {
                                chunkVariants[i].chunkLight.artificialLightLevels[thisVariantPosition.x, thisVariantPosition.y, thisVariantPosition.z] = chunkVariants[i].blocks[thisVariantPosition.x, thisVariantPosition.y, thisVariantPosition.z].BlockData.lightAmount;
                            }
                            else
                            {
                                chunkVariants[i].chunkLight.artificialLightLevels[thisVariantPosition.x, thisVariantPosition.y, thisVariantPosition.z] = 0;
                            }

                            chunkVariants[i].chunkLight.naturalLightLevels[thisVariantPosition.x, thisVariantPosition.y, thisVariantPosition.z] = 0;
                        }
                    }
                }
            }
        }

        RecalculateLightsFromPositionSecondPass(position);
    }

    private void RecalculateLightsFromPositionSecondPass(Vector3Int position)
    {
        int chunkSize = chunkWidth;

        Vector3Int[] positionVariants = new Vector3Int[]
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0)
        };

        Chunk[] chunkVariants = new Chunk[4];

        Vector3Int[] positionVariantsFriends = new Vector3Int[]
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 0)
        };

        Chunk[] chunkVariantsFriends = new Chunk[4];

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

      //  MojeKlyjTestowanieIWonchanie(position);
        
        for (int lightPass = 15; lightPass > 0; lightPass--)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                for (int z = 0; z <= chunkSize - x; z++)
                {
                    positionVariants[0].x = position.x + x;
                    positionVariants[0].y = 0;
                    positionVariants[0].z = position.z + z;

                    positionVariants[1].x = position.x + x;
                    positionVariants[1].y = 0;
                    positionVariants[1].z = position.z - z;

                    positionVariants[2].x = position.x - x;
                    positionVariants[2].y = 0;
                    positionVariants[2].z = position.z + z;

                    positionVariants[3].x = position.x - x;
                    positionVariants[3].y = 0;
                    positionVariants[3].z = position.z - z;

                    for (int i = 0; i < positionVariants.Length; i++)
                    {
                        bool front = false;
                        bool back = false;
                        bool right = false;
                        bool left = false;

                        if (positionVariants[i].x >= chunkSize)
                        {
                            positionVariants[i].x -= chunkSize;
                            right = true;
                        }
                        else if (positionVariants[i].x < 0)
                        {
                            positionVariants[i].x += chunkSize;
                            left = true;
                        }

                        if (positionVariants[i].z >= chunkSize)
                        {
                            positionVariants[i].z -= chunkSize;
                            back = true;
                        }
                        else if (positionVariants[i].z < 0)
                        {
                            positionVariants[i].z += chunkSize;
                            front = true;
                        }

                        if (!front && !back && !right && !left)
                        {
                            chunkVariants[i] = chunk;
                        }
                        else if (front)
                        {
                            if (right)
                            {
                                chunkVariants[i] = chunk.chunkNeighbours.FrontRightNeighbourChunk;
                            }
                            else if (left)
                            {
                                chunkVariants[i] = chunk.chunkNeighbours.FrontLeftNeighbourChunk;
                            }
                            else
                            {
                                chunkVariants[i] = chunk.chunkNeighbours.FrontNeighbourChunk;
                            }
                        }
                        else if (back)
                        {
                            if (right)
                            {
                                chunkVariants[i] = chunk.chunkNeighbours.BackRightNeighbourChunk;
                            }
                            else if (left)
                            {
                                chunkVariants[i] = chunk.chunkNeighbours.BackLeftNeighbourChunk;
                            }
                            else
                            {
                                chunkVariants[i] = chunk.chunkNeighbours.BackNeighbourChunk;
                            }
                        }
                        else if (right)
                        {
                            chunkVariants[i] = chunk.chunkNeighbours.RightNeighbourChunk;
                        }
                        else if (left)
                        {
                            chunkVariants[i] = chunk.chunkNeighbours.LeftNeighbourChunk;
                        }
                        else
                        {
                            UnityEngine.Debug.Log("Nie przemyslales czegos ;p");
                        }
                    }

                    // mamy wszystkie dane mozna dzialac

                    for (int i = 0; i < positionVariants.Length; i++)
                    {
                        ref Vector3Int thisVariantPosition = ref positionVariants[i];
                        Chunk thisVariantChunk = chunkVariants[i];

                        positionVariantsFriends[0].x = thisVariantPosition.x + 1;
                        positionVariantsFriends[0].y = 0;
                        positionVariantsFriends[0].z = thisVariantPosition.z;

                        positionVariantsFriends[1].x = thisVariantPosition.x - 1;
                        positionVariantsFriends[1].y = 0;
                        positionVariantsFriends[1].z = thisVariantPosition.z;

                        positionVariantsFriends[2].x = thisVariantPosition.x;
                        positionVariantsFriends[2].y = 0;
                        positionVariantsFriends[2].z = thisVariantPosition.z + 1;

                        positionVariantsFriends[3].x = thisVariantPosition.x;
                        positionVariantsFriends[3].y = 0;
                        positionVariantsFriends[3].z = thisVariantPosition.z - 1;

                        for (int j = 0; j < positionVariantsFriends.Length; j++)
                        {
                            if (positionVariantsFriends[j].x >= chunkSize)
                            {
                                positionVariantsFriends[j].x -= chunkSize;

                                chunkVariantsFriends[j] = thisVariantChunk.chunkNeighbours.RightNeighbourChunk;
                            }
                            else if (positionVariantsFriends[j].x < 0)
                            {
                                positionVariantsFriends[j].x += chunkSize;
                                chunkVariantsFriends[j] = thisVariantChunk.chunkNeighbours.LeftNeighbourChunk;
                            }
                            else if (positionVariantsFriends[j].z >= chunkSize)
                            {
                                positionVariantsFriends[j].z -= chunkSize;
                                chunkVariantsFriends[j] = thisVariantChunk.chunkNeighbours.BackNeighbourChunk;
                            }
                            else if (positionVariantsFriends[j].z < 0)
                            {
                                positionVariantsFriends[j].z += chunkSize;
                                chunkVariantsFriends[j] = thisVariantChunk.chunkNeighbours.FrontNeighbourChunk;
                            }
                            else
                            {
                                chunkVariantsFriends[j] = thisVariantChunk;
                            }
                        }

                        for (int y = chunkHeight - 2; y > 0; y--)
                        {
                            thisVariantPosition.y = y;
                            Block thisBlock = thisVariantChunk.blocks[thisVariantPosition.x, thisVariantPosition.y, thisVariantPosition.z];
                            ref byte thisBlockNaturalLight = ref thisVariantChunk.chunkLight.naturalLightLevels[thisVariantPosition.x, thisVariantPosition.y, thisVariantPosition.z];
                            ref byte thisBlockArtificialLight = ref thisVariantChunk.chunkLight.artificialLightLevels[thisVariantPosition.x, thisVariantPosition.y, thisVariantPosition.z];

                            if (!thisBlock.BlockData.isTransparent)
                                continue;

                            if (thisBlockNaturalLight != lightPass && thisBlockArtificialLight != lightPass)
                                continue;

                            positionVariantsFriends[0].y = y;
                            positionVariantsFriends[1].y = y;
                            positionVariantsFriends[2].y = y;
                            positionVariantsFriends[3].y = y;

                            // Natural Light Spreading
                            if (thisBlockNaturalLight == lightPass)
                            {
                                ref byte upBlockNaturalLight = ref thisVariantChunk.chunkLight.naturalLightLevels[thisVariantPosition.x, thisVariantPosition.y + 1, thisVariantPosition.z];
                                ref byte downBlockNaturalLight = ref thisVariantChunk.chunkLight.naturalLightLevels[thisVariantPosition.x, thisVariantPosition.y - 1, thisVariantPosition.z];
                                ref byte rightBlockNaturalLight = ref chunkVariantsFriends[0].chunkLight.naturalLightLevels[positionVariantsFriends[0].x, positionVariantsFriends[0].y, positionVariantsFriends[0].z];
                                ref byte leftBlockNaturalLight = ref chunkVariantsFriends[1].chunkLight.naturalLightLevels[positionVariantsFriends[1].x, positionVariantsFriends[1].y, positionVariantsFriends[1].z];
                                ref byte backBlockNaturalLight = ref chunkVariantsFriends[2].chunkLight.naturalLightLevels[positionVariantsFriends[2].x, positionVariantsFriends[2].y, positionVariantsFriends[2].z];
                                ref byte frontBlockNaturalLight = ref chunkVariantsFriends[3].chunkLight.naturalLightLevels[positionVariantsFriends[3].x, positionVariantsFriends[3].y, positionVariantsFriends[3].z];

                                if (thisBlockNaturalLight > upBlockNaturalLight)
                                {
                                    upBlockNaturalLight = (byte)(thisBlockNaturalLight - 1);
                                }
                                if (thisBlockNaturalLight > downBlockNaturalLight)
                                {
                                    downBlockNaturalLight = (byte)(thisBlockNaturalLight - 1);
                                }
                                if (thisBlockNaturalLight > rightBlockNaturalLight)
                                {
                                    rightBlockNaturalLight = (byte)(thisBlockNaturalLight - 1);
                                }
                                if (thisBlockNaturalLight > leftBlockNaturalLight)
                                {
                                    leftBlockNaturalLight = (byte)(thisBlockNaturalLight - 1);
                                }
                                if (thisBlockNaturalLight > backBlockNaturalLight)
                                {
                                    backBlockNaturalLight = (byte)(thisBlockNaturalLight - 1);
                                }
                                if (thisBlockNaturalLight > frontBlockNaturalLight)
                                {
                                    frontBlockNaturalLight = (byte)(thisBlockNaturalLight - 1);
                                }
                            }

                            // Artificial Light Spreading
                            if (thisBlockArtificialLight == lightPass)
                            {
                                ref byte upBlockArtificialLight = ref thisVariantChunk.chunkLight.artificialLightLevels[thisVariantPosition.x, thisVariantPosition.y + 1, thisVariantPosition.z];
                                ref byte downBlockArtificialLight = ref thisVariantChunk.chunkLight.artificialLightLevels[thisVariantPosition.x, thisVariantPosition.y - 1, thisVariantPosition.z];
                                ref byte rightBlockArtificialLight = ref chunkVariantsFriends[0].chunkLight.artificialLightLevels[positionVariantsFriends[0].x, positionVariantsFriends[0].y, positionVariantsFriends[0].z];
                                ref byte leftBlockArtificialLight = ref chunkVariantsFriends[1].chunkLight.artificialLightLevels[positionVariantsFriends[1].x, positionVariantsFriends[1].y, positionVariantsFriends[1].z];
                                ref byte backBlockArtificialLight = ref chunkVariantsFriends[2].chunkLight.artificialLightLevels[positionVariantsFriends[2].x, positionVariantsFriends[2].y, positionVariantsFriends[2].z];
                                ref byte frontBlockArtificialLight = ref chunkVariantsFriends[3].chunkLight.artificialLightLevels[positionVariantsFriends[3].x, positionVariantsFriends[3].y, positionVariantsFriends[3].z];

                                if (thisBlockArtificialLight > upBlockArtificialLight)
                                {
                                    upBlockArtificialLight = (byte)(thisBlockArtificialLight - 1);
                                }

                                if (thisBlockArtificialLight > downBlockArtificialLight)
                                {
                                    downBlockArtificialLight = (byte)(thisBlockArtificialLight - 1);
                                }

                                if (thisBlockArtificialLight > rightBlockArtificialLight)
                                {
                                    rightBlockArtificialLight = (byte)(thisBlockArtificialLight - 1);
                                }

                                if (thisBlockArtificialLight > leftBlockArtificialLight)
                                {
                                    leftBlockArtificialLight = (byte)(thisBlockArtificialLight - 1);
                                }

                                if (thisBlockArtificialLight > backBlockArtificialLight)
                                {
                                    backBlockArtificialLight = (byte)(thisBlockArtificialLight - 1);
                                }

                                if (thisBlockArtificialLight > frontBlockArtificialLight)
                                {
                                    frontBlockArtificialLight = (byte)(thisBlockArtificialLight - 1);
                                }
                            }
                        }
                    }
                }
            }
        }
        
        stopWatch.Stop();
        // Get the elapsed time as a TimeSpan value.
        System.TimeSpan ts = stopWatch.Elapsed;

        // Format and display the TimeSpan value.
        string elapsedTime = System.String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        UnityEngine.Debug.Log("RunTime " + elapsedTime);
    } */


}
