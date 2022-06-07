using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class Map : MonoBehaviour
{
    public VoxelSettingsData voxelSettingsData;
    public VoxelSettings voxelSettings;
    [SerializeField]
    public List<List<Vector3Int>> positionsList = new List<List<Vector3Int>>();

    [SerializeField]
    private GameObject chunkPrefab;
    public List<BlockData> blocksData = new List<BlockData>(); 


    [HideInInspector] public List<Chunk> chunks = new List<Chunk>();

    public static Map singleton;

    private void Awake ()
    {
        singleton = this;
    }

    private void Start ()
    {
        LoadAndRenderChunks(PrepareChunks(FindNewChunkPositionsInOrder()));
    }

    /// <summary>
    /// Check if we should render more chunks (in case player moved)
    /// </summary>
    private void UpdateChunks ()
    {
        List<Chunk> unchangedChunks = new List<Chunk>();
        List<Chunk> toBeChangedChunks = new List<Chunk>();

        List<Vector3Int> newPositions = NewChunkPositionsToSingleList(FindNewChunkPositionsInOrder());

        foreach (Chunk chunk in chunks)
        {
            bool requireChanges = true;

            foreach (Vector3Int position in newPositions)
            {
                if (chunk.ChunkPosition == position) // position of this chunk is still valid so we add it to unchanged chunks
                {
                    requireChanges = false;
                    break;
                }
            }

            if (requireChanges)
                toBeChangedChunks.Add(chunk);
            else
                unchangedChunks.Add(chunk);
        }


    }

    private List<List<Vector3Int>> FindNewChunkPositionsInOrder ()
    {
        int count = 0;
        List<List<Vector3Int>> finalChunkPositions = new List<List<Vector3Int>>();

        List<Vector3Int> previousItems = new List<Vector3Int>();
        List<Vector3Int> currentItems = new List<Vector3Int>();
        List<Vector3Int> nextItems = new List<Vector3Int>();

        void AddToListNext (Vector3Int item)
        {
            foreach (Vector3Int previousItem in previousItems)
            {
                if (previousItem == item)
                    return;
            }

            foreach (Vector3Int currentItem in currentItems)
            {
                if (currentItem == item)
                    return;
            }

            foreach (Vector3Int nextItem in nextItems)
            {
                if (nextItem == item)
                    return;
            }

            nextItems.Add(item);
        }

        currentItems.Add(new Vector3Int(0, 0, 0));

        finalChunkPositions.Add(new List<Vector3Int>());
        finalChunkPositions[count].Add(new Vector3Int(0, 0, 0));
        count++;

        int chunkOffset = (int)(voxelSettingsData.GetChunkSize() * voxelSettingsData.GetBlockSize());

        foreach (Vector3Int currentItem in currentItems)
        {
            AddToListNext(new Vector3Int(currentItem.x + chunkOffset, currentItem.y, currentItem.z + chunkOffset));
            AddToListNext(new Vector3Int(currentItem.x, currentItem.y, currentItem.z + chunkOffset));
            AddToListNext(new Vector3Int(currentItem.x + chunkOffset, currentItem.y, currentItem.z));
            AddToListNext(new Vector3Int(currentItem.x + chunkOffset, currentItem.y, currentItem.z - chunkOffset));
            AddToListNext(new Vector3Int(currentItem.x, currentItem.y, currentItem.z - chunkOffset));
            AddToListNext(new Vector3Int(currentItem.x - chunkOffset, currentItem.y, currentItem.z - chunkOffset));
            AddToListNext(new Vector3Int(currentItem.x - chunkOffset, currentItem.y, currentItem.z));
            AddToListNext(new Vector3Int(currentItem.x - chunkOffset, currentItem.y, currentItem.z + chunkOffset));
            AddToListNext(new Vector3Int(currentItem.x - chunkOffset, currentItem.y, currentItem.z + chunkOffset));
        }

     //   foreach (Vector3Int nt in nextItems)
     //   {
   //         Debug.Log(nt);
     //   }

        finalChunkPositions.Add(new List<Vector3Int>());
        finalChunkPositions[count].AddRange(nextItems);
        count++;

      //  Debug.Log(finalChunkPositions.Count);
     //   Debug.Log("*****");

        for (int i = 0; i < voxelSettings.renderDistance; i++)
        {
         //   Debug.Log("----");
            previousItems = currentItems;
            currentItems = nextItems;
            nextItems = new List<Vector3Int>();

            foreach (Vector3Int currentItem in currentItems)
            {
                AddToListNext(new Vector3Int(currentItem.x + chunkOffset, currentItem.y, currentItem.z + chunkOffset));
                AddToListNext(new Vector3Int(currentItem.x, currentItem.y, currentItem.z + chunkOffset));
                AddToListNext(new Vector3Int(currentItem.x + chunkOffset, currentItem.y, currentItem.z));
                AddToListNext(new Vector3Int(currentItem.x + chunkOffset, currentItem.y, currentItem.z - chunkOffset));
                AddToListNext(new Vector3Int(currentItem.x, currentItem.y, currentItem.z - chunkOffset));
                AddToListNext(new Vector3Int(currentItem.x - chunkOffset, currentItem.y, currentItem.z - chunkOffset));
                AddToListNext(new Vector3Int(currentItem.x - chunkOffset, currentItem.y, currentItem.z));
                AddToListNext(new Vector3Int(currentItem.x - chunkOffset, currentItem.y, currentItem.z + chunkOffset));
                AddToListNext(new Vector3Int(currentItem.x - chunkOffset, currentItem.y, currentItem.z + chunkOffset));
            }

        //    foreach (Vector3Int nt in nextItems)
       //     {
        //        Debug.Log(nt);
        //    }

            finalChunkPositions.Add(new List<Vector3Int>());
            finalChunkPositions[count].AddRange(nextItems);
            count++;

            // load current items chunks
            // render previous items chunks
          //  positionsList = finalChunkPositions;
        }

        return finalChunkPositions;
    }

    private List<Vector3Int> NewChunkPositionsToSingleList (List<List<Vector3Int>> newChunkPositionsInOrder)
    {
        List<Vector3Int> newChunkPositionsSingleList = new List<Vector3Int>();

        foreach (List<Vector3Int> a in newChunkPositionsInOrder)
        {
            foreach (Vector3Int b in a)
            {
                newChunkPositionsSingleList.Add(b);
            }
        }

        return newChunkPositionsSingleList;
    }

    private List<List<Chunk>> PrepareChunks (List<List<Vector3Int>> chunkPositions)
    {
        List<List<Chunk>> chunks = new List<List<Chunk>>();
        int count = 0;

        foreach (List<Vector3Int> a in chunkPositions)
        {
            chunks.Add(new List<Chunk>());

            foreach (Vector3Int b in a)
            {
                GameObject chunkObject = Instantiate(chunkPrefab, null, true);
                Chunk chunk = chunkObject.GetComponent<Chunk>();
                chunk.ChunkPosition = b;
                this.chunks.Add(chunk);
                chunks[count].Add(chunk);
            }

            count++;
        }

        return chunks;
    }

    private async void LoadAndRenderChunks (List<List<Chunk>> chunks)
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            foreach (Chunk chunk in chunks[i])
            {
                await chunk.LoadChunkAsync(chunk.ChunkPosition);
            }

            if (i >= 1)
            {
                foreach (Chunk chunk in chunks[i - 1])
                {
                    await chunk.RenderChunkAsync();
                }
            }
        }
    }


    public BlockData GetBlockDataByID (int id)
    {
        foreach (BlockData blockData in blocksData)
        {
            if (blockData.id == id)
                return blockData;
        }

        return null;
    }

    public Block GetBlockById (int id)
    {
        foreach (BlockData blockData in blocksData)
        {
            if (blockData.id == id)
                return blockData.GetBlock();
        }

        return null;
    }

 /*   public Block[,,] GetChunkData (Vector3Int chunkPosition)
    {
        Chunk chunk = GetChunk(chunkPosition);

        if (chunk == null)
            return null;

        return chunk.blocks;
    } */

    public Chunk GetChunk (Vector3Int chunkPosition)
    {
       // Debug.Log("chun pos: " + chunkPosition);

        foreach (Chunk chunk in chunks)
        {
            if (chunk.ChunkPosition == chunkPosition)
                return chunk;
        }

        return null;
    }

    public void DestroyBlock (Vector3 inWorldPosition)
    {
        Chunk chunk = GetChunk(InWorldPositionToChunkPosition(Vector3ToVector3Int(inWorldPosition)));
        Vector3Int inChunkPositionInt = InWorldPositionToInsideChunkPosition(Vector3ToVector3Int(inWorldPosition));

        chunk.EditBlock(inChunkPositionInt, null);
    }

    public Block GetBlock (Vector3 inWorldPosition)
    {
        Chunk chunk = GetChunk(InWorldPositionToChunkPosition(Vector3ToVector3Int(inWorldPosition)));

        if (chunk == null)
            return null;

        Vector3Int inChunkPositionInt = InWorldPositionToInsideChunkPosition(Vector3ToVector3Int(inWorldPosition));

        return chunk.GetBlock(inChunkPositionInt);
    }

    public void PlaceBlock (Vector3 inWorldPosition, int blockID)
    {
        Chunk chunk = GetChunk(InWorldPositionToChunkPosition(Vector3ToVector3Int(inWorldPosition)));
        Vector3Int inChunkPositionInt = InWorldPositionToInsideChunkPosition(Vector3ToVector3Int(inWorldPosition));

        chunk.EditBlock(inChunkPositionInt, GetBlockById(blockID));
    }

    public void PlaceWater (Vector3 inWorldPosition)
    {
        Chunk chunk = GetChunk(InWorldPositionToChunkPosition(Vector3ToVector3Int(inWorldPosition)));
        Vector3Int inChunkPositionInt = InWorldPositionToInsideChunkPosition(Vector3ToVector3Int(inWorldPosition));

        chunk.chunkLiquids.PlaceWaterSource(inChunkPositionInt);
        chunk.ForceRenderChunkWithNeighbours();
    }

    /*
    private Block GetBlock (Vector3 inWorldPosition)
    {
        Vector3Int inWorldPositionInt = Vector3ToVector3Int(inWorldPosition);
        Vector3Int inChunkPositionInt = InWorldPositionToInsideChunkPosition(inWorldPositionInt);

        Debug.Log("block in chunk pos: " + inChunkPositionInt + "and block pos" + InWorldPositionToChunkPosition(inWorldPositionInt));

        Block[,,] chunkData = GetChunkData(InWorldPositionToChunkPosition(inWorldPositionInt));
        Block blockInChunk = chunkData[inChunkPositionInt.x, inChunkPositionInt.y, inChunkPositionInt.z];

        return blockInChunk;
    } */

    public Vector3Int InWorldPositionToInsideChunkPosition (Vector3Int inWorldPosition)
    {
        Vector3Int chunkPosition = InWorldPositionToChunkPosition(inWorldPosition);

        return new Vector3Int(inWorldPosition.x - chunkPosition.x, inWorldPosition.y, inWorldPosition.z - chunkPosition.z);
    }

    public Vector3Int InWorldPositionToChunkPosition (Vector3Int inWorldPosition)
    {
        return new Vector3Int(RoundDownToNearest16(inWorldPosition.x), 0, RoundDownToNearest16(inWorldPosition.z));
    }

    private int RoundDownToNearest16 (int value)
    {
        int nearestValue = voxelSettingsData.GetChunkSize();
        int result;

        if (value >= 0)
            result = nearestValue * (value / nearestValue);
        else
            result = (nearestValue * ((value + 1) / nearestValue)) - nearestValue;

        return (int)(result * voxelSettingsData.GetBlockSize());
    }

    public Vector3Int Vector3ToVector3Int (Vector3 vector3)
    {
        return new Vector3Int((int)System.Math.Floor(vector3.x), (int)System.Math.Floor(vector3.y), (int)System.Math.Floor(vector3.z));
    }
}

[System.Serializable]
public class VoxelSettings
{
    [Range(4, 48), Space(5)]
    public int renderDistance = 16;
  //  [Range(16, 128), Space(5)]
 //   public int chunkSize = 16;
  //  [Range(8, 24), Space(5)]
  //  public int verticalSubChunksAmount = 8;
 //   [Range(0.1f, 1)]
  //  public float blockSize = 1;

}


/*
private void PrepareChunks ()
{
    for (int i = 0; i < (renderDistance * 2) * (renderDistance * 2); i++)
    {
        GameObject chunkObject = Instantiate(chunkPrefab, null, true);
        Chunk chunk = chunkObject.GetComponent<Chunk>();
        chunks.Add(chunk);
    }
}

private async void LoadChunks ()
{
    Vector3 playerPosition = new Vector3(0, 0, 0);
    int chunkID = 0;

    for (int i = 0; i < renderDistance; i++)
    {
        int horizontalAmount = renderDistance;

        for (int j = 0; j < horizontalAmount; j++)
        {
            int negativeI = -i;
            int negativeJ = -j;


            await chunks[chunkID].LoadChunkAsync(new Vector3Int(0 + (i * chunks[chunkID].chunkWidth), 0, 0 + (j * chunks[chunkID].chunkWidth)));

            if (i < (renderDistance - 1) && j < (renderDistance - 1))
            {
                chunksToRender.Enqueue(chunks[chunkID]);
            }

            chunkID++;



            if (j != 0)
            {
                await chunks[chunkID].LoadChunkAsync(new Vector3Int(0 + (i * chunks[chunkID].chunkWidth), 0, 0 + (-j * chunks[chunkID].chunkWidth)));

                if (i < (renderDistance - 1) && j < (renderDistance - 1))
                {
                    chunksToRender.Enqueue(chunks[chunkID]);
                }

                chunkID++;
            }

            if (i != 0 && j != 0)
            {
                await chunks[chunkID].LoadChunkAsync(new Vector3Int(0 + (-i * chunks[chunkID].chunkWidth), 0, 0 + (-j * chunks[chunkID].chunkWidth)));

                if (i < (renderDistance - 1) && j < (renderDistance - 1))
                {
                    chunksToRender.Enqueue(chunks[chunkID]);
                }

                chunkID++;
            }

            if (i != 0)
            {
                await chunks[chunkID].LoadChunkAsync(new Vector3Int(0 + (-i * chunks[chunkID].chunkWidth), 0, 0 + (j * chunks[chunkID].chunkWidth)));

                if (i < (renderDistance - 1) && j < (renderDistance - 1))
                {
                    chunksToRender.Enqueue(chunks[chunkID]);
                }

                chunkID++;
            }
        }
    }

    RenderChunks();
}

private async void TestLoad ()
{
    Queue<Chunk> toBeRenderedInNextLoop = new Queue<Chunk>();

    int chunkWidth = 16;

    int chunkID = 0;
    for (int i = 0; i < renderDistance; i++)
    {
        if (i == 0)
        {
            await chunks[chunkID].LoadChunkAsync(new Vector3Int(0 + (i * chunks[chunkID].chunkWidth), 0, 0 + (i * chunks[chunkID].chunkWidth)));
            chunkID++;
            continue;
        }

        Vector3Int[] newChunkPositions = new Vector3Int[] 
        {
            new Vector3Int (i * chunkWidth, 0, i * chunkWidth),
        };


    }
} */