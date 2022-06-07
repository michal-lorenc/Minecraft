using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    public VoxelSettingsData voxelSettingsData;
   // private int chunkWidth = 16;
  //  private int chunkHeight = 384;

    private int chunkHeightNoise = 100;

    public float noiseScale = 0.03f;
    public int seed = 2137;

    public NoiseSettings noiseSettings = new NoiseSettings();
    public eWorldType worldType = eWorldType.NORMAL;

    public static NoiseGenerator singleton;

    private void Awake()
    {
        singleton = this;
    }

    public Block[,,] GenerateChunkData (Vector3Int chunkPosition)
    {
        if (worldType == eWorldType.FLAT)
            return GenerateFlatWorld();

        int chunkWidth = voxelSettingsData.GetChunkSize();
        int chunkHeight = voxelSettingsData.VerticalSubChunkCount * chunkWidth;
        float blockSize = voxelSettingsData.GetBlockSize();


        Block[,,] blocks = new Block[chunkWidth, chunkHeight, chunkWidth];

        //chunkPosition.z = 0;

        Block airBlock = Map.singleton.GetBlockById(2);
        Block grassBlock = Map.singleton.GetBlockById(1);
        Block dirtBlock = Map.singleton.GetBlockById(0);

        BlockData airBlockData = Map.singleton.GetBlockDataByID(2);
        BlockData grassBlockData = Map.singleton.GetBlockDataByID(1);
        BlockData dirtBlockData = Map.singleton.GetBlockDataByID(0);

        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
               // float noiseValue = Mathf.PerlinNoise((seed + chunkPosition.x + x) * noiseScale, (seed + chunkPosition.z + z) * noiseScale);
                // float noiseValue = OctavePerlin(x, z, chunkPosition, noiseSettings);

                // float groundPosition = Mathf.RoundToInt(noiseValue * chunkHeight);
                int groundPosition = GetSurfaceHeightNoise(blockSize * x + chunkPosition.x, blockSize * z + chunkPosition.z, chunkPosition);

                for (int y = 0; y < chunkHeight; y++)
                {
                    if (y > groundPosition)
                    {
                        //blocks[x, y, z] = airBlock; // air
                        blocks[x, y, z] = new Block(airBlockData); // air
                    }
                    else if (y == groundPosition)
                    {
                       // blocks[x, y, z] = grassBlock;
                        blocks[x, y, z] = new Block(grassBlockData);
                    }
                    else
                    {
                       // blocks[x, y, z] = dirtBlock;
                        blocks[x, y, z] = new Block(dirtBlockData);
                    }
                }
            }
        }

        return blocks;
    }

    public Block[,,] GenerateFlatWorld ()
    {
        int chunkWidth = voxelSettingsData.GetChunkSize();
        int chunkHeight = voxelSettingsData.VerticalSubChunkCount * chunkWidth;

        Block[,,] blocks = new Block[chunkWidth, chunkHeight, chunkWidth];

        Block airBlock = Map.singleton.GetBlockDataByID(2).GetBlock();
        Block grassBlock = Map.singleton.GetBlockDataByID(1).GetBlock();
        Block dirtBlock = Map.singleton.GetBlockDataByID(0).GetBlock();

        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
                for (int y = 0; y < chunkHeight; y++)
                {
                    if (y > 30)
                    {
                        blocks[x, y, z] = airBlock; // air
                        //blocks[x, y, z] = new Block(airBlockData); // air
                    }
                    else if (y == 30)
                    {
                         blocks[x, y, z] = grassBlock;
                        //blocks[x, y, z] = new Block(grassBlockData);
                    }
                    else
                    {
                         blocks[x, y, z] = dirtBlock;
                        //blocks[x, y, z] = new Block(dirtBlockData);
                    }
                }
            }
        }

        return blocks;
    }

    public ChunkBlocks GenerateChunkDataOptimized (Vector3Int chunkPosition, Chunk chunk)
    {
        if (worldType == eWorldType.FLAT)
            return GenerateFlatWorldOptimized();

        int chunkWidth = voxelSettingsData.GetChunkSize();
        int chunkHeight = voxelSettingsData.VerticalSubChunkCount * chunkWidth;
        float blockSize = voxelSettingsData.GetBlockSize();


        //Block[,,] blocks = new Block[chunkWidth, chunkHeight, chunkWidth];
        ChunkBlocks blocks = new ChunkBlocks(chunkWidth);

        Block grassBlock = Map.singleton.GetBlockById(1);
        Block dirtBlock = Map.singleton.GetBlockById(0);

        int waterLevel = 30;
        int highestSolidVoxel = waterLevel;

        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
                int groundPosition = GetSurfaceHeightNoise(blockSize * x + chunkPosition.x, blockSize * z + chunkPosition.z, chunkPosition);

                if (waterLevel > groundPosition)
                {
                    for (int y = waterLevel; y > groundPosition; y--)
                    {
                        chunk.chunkLiquids.water[x, y, z] = 1;
                    }
                }
                else
                {
                    if (groundPosition > highestSolidVoxel)
                        highestSolidVoxel = groundPosition;
                }

                for (int y = 0; y <= groundPosition; y++)
                {
                    if (y == groundPosition)
                    {
                        blocks.SetBlock(x, y, z, grassBlock);
                    }
                    else if (y < groundPosition)
                    {
                        blocks.SetBlock(x, y, z, dirtBlock);
                    }
                }
            }
        }

        chunk.HighestSolidVoxel = highestSolidVoxel;
        return blocks;
    }

    public static bool firstTimeInnit = true;

    public ChunkBlocks GenerateFlatWorldOptimized ()
    {
        int chunkWidth = voxelSettingsData.GetChunkSize();
        int chunkHeight = voxelSettingsData.VerticalSubChunkCount * chunkWidth;

        ChunkBlocks chunkBlocks = new ChunkBlocks(chunkWidth);

        Block grassBlock = Map.singleton.GetBlockDataByID(1).GetBlock();
        Block lightBlock = Map.singleton.GetBlockDataByID(3).GetBlock();
        Block dirtBlock = Map.singleton.GetBlockDataByID(0).GetBlock();

        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
                for (int y = 0; y < chunkHeight; y++)
                {
                    if (y == 32 && x == 8 && z == 14)
                    {
                        if (firstTimeInnit)
                        {
                            chunkBlocks.SetBlock(x, y, z, lightBlock);
                            firstTimeInnit = false;
                        }
                    }
                    else if (y == 31)
                    {
                        chunkBlocks.SetBlock(x, y, z, grassBlock);
                    }
                    else if (y <= 30)
                    {
                        chunkBlocks.SetBlock(x, y, z, dirtBlock);
                    }

                }
            }
        }

        return chunkBlocks;
    }

    private int GetSurfaceHeightNoise (float x, float z, Vector3Int offset)
    {
        float terrainHeight = OctavePerlin(x, z, offset, noiseSettings);
        terrainHeight = Redistribution(terrainHeight, noiseSettings);
        int surfaceHeight = RemapValue01ToInt(terrainHeight, 0, chunkHeightNoise);
        return surfaceHeight;
    }

    public float OctavePerlin (float x, float z, Vector3Int offset, NoiseSettings settings)
    {
        x *= settings.noiseZoom;
        z *= settings.noiseZoom;
        x += settings.noiseZoom;
        z += settings.noiseZoom;

        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float amplitudeSum = 0;

        for (int i = 0; i < settings.octaves; i++)
        {
            total += Mathf.PerlinNoise((seed + x) * frequency, (seed + z) * frequency) * amplitude;

            amplitudeSum += amplitude;

            amplitude *= settings.persistance;
            frequency *= 2;
        }


        return total / amplitudeSum;
    }

    private float RemapValue (float value, float initialMin, float initialMax, float outputMin, float outputMax)
    {
        return outputMin + (value - initialMin) * (outputMax - outputMin) / (initialMax - initialMin);
    }

    private float RemapValue01(float value, float outputMin, float outputMax)
    {
        return outputMin + (value - 0) * (outputMax - outputMin) / (1 - 0);
    }

    private int RemapValue01ToInt(float value, float outputMin, float outputMax)
    {
        return (int)RemapValue01(value, outputMin, outputMax);
    }

    private float Redistribution (float noise, NoiseSettings settings)
    {
        return Mathf.Pow(noise * settings.redistribution, settings.exponent);
    }
}

public class NoiseSettings
{
    public float noiseZoom = 0.01f;
    public int octaves = 5;
    public float persistance = 0.5f;
    public float redistribution = 1.2f;
    public float exponent = 2f;
}

public enum eWorldType
{
    NORMAL = 0,
    FLAT = 1
}