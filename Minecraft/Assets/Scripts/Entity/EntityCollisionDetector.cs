using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityCollisionDetector : MonoBehaviour
{
    [field: SerializeField]
    public GameObject ColliderPrefab { get; private set; }
    [field: SerializeField, Range(1, 4)]
    public int DetectionRange { get; private set; } = 4;
    [field: SerializeField]
    public bool VisualizeColliders { get; private set; } = false;
    private BoxCollider[] colliders;

    private void OnEnable ()
    {
        SpawnColliders();
    }

    private void SpawnColliders ()
    {
        int amountOfCollidersToSpawn = DetectionRange * 2 + 1;
        amountOfCollidersToSpawn = amountOfCollidersToSpawn * amountOfCollidersToSpawn * amountOfCollidersToSpawn;

        colliders = new BoxCollider[amountOfCollidersToSpawn];

        for (int i = 0; i < amountOfCollidersToSpawn; i++)
        {
            colliders[i] = Instantiate(ColliderPrefab, null, true).GetComponent<BoxCollider>();
        }

        StartCoroutine(lol());
    }

    private void UpdateCollidersInformation ()
    {
        int axisSize = DetectionRange * 2 + 1;
        int iteration = 0;

        Vector3Int entityPositionInt = Map.singleton.Vector3ToVector3Int(transform.position);
        Vector3Int chunkPosition = Map.singleton.InWorldPositionToChunkPosition(entityPositionInt);
        Vector3Int inChunkPosition = Map.singleton.InWorldPositionToInsideChunkPosition(entityPositionInt);

        Chunk chunk = Map.singleton.GetChunk(chunkPosition);

        for (int x = 0; x < axisSize; x++)
        {
            for (int y = 0; y < axisSize; y++)
            {
                for (int z = 0; z < axisSize; z++)
                {
                    int newX = x - DetectionRange;
                    int newY = y - DetectionRange;
                    int newZ = z - DetectionRange;

                    ChunkData chunkData = chunk.chunkNeighbours.TransformInsideChunkPositionToChunkReferenceAndPosition(new Vector3Int(inChunkPosition.x + newX, inChunkPosition.y + newY, inChunkPosition.z + newZ));

                    colliders[iteration].transform.position = new Vector3((entityPositionInt.x + newX) + 0.5f, (entityPositionInt.y + newY) + 0.5f, (entityPositionInt.z + newZ) + 0.5f);

                    bool isCollider = chunkData.Chunk.chunkBlocks.isSolidBlock(chunkData.InsideChunkPosition.x, chunkData.InsideChunkPosition.y, chunkData.InsideChunkPosition.z);

                    if (!isCollider)
                    {
                        bool isWater = chunkData.Chunk.chunkLiquids.IsWater(chunkData.InsideChunkPosition);
                        bool isLava = chunkData.Chunk.chunkLiquids.IsLava(chunkData.InsideChunkPosition);

                        if (isWater || isLava)
                        {
                            colliders[iteration].enabled = true;
                            colliders[iteration].isTrigger = true;
                            colliders[iteration].tag = isWater ? "Water" : "Lava";
                        }
                        else
                        {
                            colliders[iteration].enabled = false;
                            colliders[iteration].isTrigger = false;
                        }

                        iteration++;
                        continue;
                    }

                    colliders[iteration].enabled = isCollider;
                    colliders[iteration].tag = "Environment";
                    colliders[iteration].isTrigger = false;

                    if (VisualizeColliders)
                        colliders[iteration].GetComponent<MeshRenderer>().enabled = isCollider;

                    iteration++;
                }
            }
        }
    }

    private IEnumerator lol ()
    {
        yield return new WaitForSeconds(1.0f);

        while (true)
        {
            UpdateCollidersInformation();
            yield return null;
        }
    }
}
