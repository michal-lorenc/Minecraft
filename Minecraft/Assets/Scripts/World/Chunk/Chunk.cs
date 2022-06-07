using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;


public class Chunk : MonoBehaviour
{
    public VoxelSettingsData voxelSettingsData;
    public GameObject subChunkPrefab;
    public List<SubChunk> SubChunks { get; private set; } = new List<SubChunk>();

    public ChunkLiquids chunkLiquids;
    public ChunkBlocks chunkBlocks;
    public ChunkLight chunkLight;
    public ChunkNeighbours chunkNeighbours;

    public eChunkState ChunkState { get; private set; } = eChunkState.IDLE;
    public Vector3Int ChunkPosition { get; set; }
    public int HighestSolidVoxel { get; set; }

    private CancellationTokenSource loadChunkAsyncCancelTokenSource;
    private CancellationToken loadChunkAsyncCancelToken;

    public async Task LoadChunkAsync (Vector3Int chunkPosition)
    {
        if (ChunkState != eChunkState.IDLE)
        {
            loadChunkAsyncCancelTokenSource.Cancel();
            UnloadChunk();
            await LoadChunkAsync(chunkPosition);
            return;
        }

        ChunkState = eChunkState.LOADING;

        loadChunkAsyncCancelTokenSource = new CancellationTokenSource();
        loadChunkAsyncCancelToken = loadChunkAsyncCancelTokenSource.Token;

        transform.position = chunkPosition;
        ChunkPosition = chunkPosition;

        chunkLiquids = new ChunkLiquids(this);

        await Task.Run(() => GenerateChunkData(chunkPosition), loadChunkAsyncCancelToken);

        chunkLight = new ChunkLight(this);
        await Task.Run(() => chunkLight.RecalculateLightsOptimized(), loadChunkAsyncCancelToken);

        ChunkState = eChunkState.LOADED;
    }

    public async Task RenderChunkAsync (bool recalculateCollisions = true)
    {
        if (chunkNeighbours == null)
        {
            chunkNeighbours = new ChunkNeighbours(this);
            _= Task.Run(() => chunkLight.FloodToNeighbourChunks());
        }

        if (SubChunks.Count < 1)
        {
            for (int i = 0; i < voxelSettingsData.VerticalSubChunkCount; i++)
            {
                SubChunks.Add(Instantiate(subChunkPrefab, transform, false).GetComponent<SubChunk>().Constructor(this, i));
            }
        }

        foreach (SubChunk subChunk in SubChunks)
        {
           _ = subChunk.RenderChunkAsync();
        }

        await Task.Yield();
    }

    private void GenerateChunkData (Vector3 chunkPosition)
    {
        chunkBlocks = NoiseGenerator.singleton.GenerateChunkDataOptimized(new Vector3Int((int)chunkPosition.x, 0, (int)chunkPosition.z), this);
    }

    public void UnloadChunk ()
    {

    }

    /// <summary>
    /// Changes block to the new one & forces light recalculation & chunk rendering
    /// </summary>
    /// <param name="insideChunkPosition"></param>
    /// <param name="newBlock">removes block if null, otherwise replace block with new one</param>
    public async void EditBlock (Vector3Int insideChunkPosition, Block newBlock)
    {
        if (newBlock == null)
        {
            chunkBlocks.RemoveBlock(insideChunkPosition.x, insideChunkPosition.y, insideChunkPosition.z);
            chunkLight.isBlockTransparent[insideChunkPosition.x, insideChunkPosition.y, insideChunkPosition.z] = true;
        }
        else
        {
            chunkBlocks.SetBlock(insideChunkPosition.x, insideChunkPosition.y, insideChunkPosition.z, newBlock);
            chunkLight.isBlockTransparent[insideChunkPosition.x, insideChunkPosition.y, insideChunkPosition.z] = newBlock.BlockData.isTransparent;
        }

        await Task.Run(() => chunkLight.RecalculateLightsFromPositionOptimized(insideChunkPosition));

        /*  if (insideChunkPosition.x == 0)
               _ = chunkNeighbours.LeftNeighbourChunk.RenderChunkAsync();
           if (insideChunkPosition.x == chunkWidth - 1)
               _ = chunkNeighbours.RightNeighbourChunk.RenderChunkAsync();
           if (insideChunkPosition.z == 0)
               _ = chunkNeighbours.FrontNeighbourChunk.RenderChunkAsync();
           if (insideChunkPosition.z == chunkWidth - 1)
               _ = chunkNeighbours.BackNeighbourChunk.RenderChunkAsync(); */



        _ = chunkNeighbours.LeftNeighbourChunk.RenderChunkAsync();
        _ = chunkNeighbours.RightNeighbourChunk.RenderChunkAsync();
        _ = chunkNeighbours.FrontNeighbourChunk.RenderChunkAsync();
        _ = chunkNeighbours.BackNeighbourChunk.RenderChunkAsync();

        _ = chunkNeighbours.BackLeftNeighbourChunk.RenderChunkAsync();
        _ = chunkNeighbours.BackRightNeighbourChunk.RenderChunkAsync();
        _ = chunkNeighbours.FrontLeftNeighbourChunk.RenderChunkAsync();
        _ = chunkNeighbours.FrontRightNeighbourChunk.RenderChunkAsync();

        _ = RenderChunkAsync();
    }

    public Block GetBlock (Vector3Int insideChunkPosition)
    {
        if (chunkBlocks == null)
            return null;

        return chunkBlocks.GetBlock(insideChunkPosition.x, insideChunkPosition.y, insideChunkPosition.z);
    }

    public void ForceRenderChunkWithNeighbours ()
    {
        _ = chunkNeighbours.LeftNeighbourChunk.RenderChunkAsync();
        _ = chunkNeighbours.RightNeighbourChunk.RenderChunkAsync();
        _ = chunkNeighbours.FrontNeighbourChunk.RenderChunkAsync();
        _ = chunkNeighbours.BackNeighbourChunk.RenderChunkAsync();

        _ = chunkNeighbours.BackLeftNeighbourChunk.RenderChunkAsync();
        _ = chunkNeighbours.BackRightNeighbourChunk.RenderChunkAsync();
        _ = chunkNeighbours.FrontLeftNeighbourChunk.RenderChunkAsync();
        _ = chunkNeighbours.FrontRightNeighbourChunk.RenderChunkAsync();

        _ = RenderChunkAsync();
    }
}