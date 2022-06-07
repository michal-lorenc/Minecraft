using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class CloudsMaster : MonoBehaviour
{
    private CloudsChunk[,] cloudsChunks = new CloudsChunk[8,8];
    public GameObject cloudsChunkPrefab;

    public eCloudsType cloudsType = eCloudsType.NORMAL;
    public Texture2D cloudsHeightMap;
    private bool[,] cloudsMap;
    private readonly int cloudsDistance = 512;
    private readonly int y = 150;


    private void Awake ()
    {
        GenerateClouds();
        RenderClouds();

        transform.position = new Vector3(0, y, 0);
    }

    private void GenerateClouds ()
    {
        cloudsMap = new bool[cloudsDistance, cloudsDistance];

        for (int x = 0; x < cloudsDistance; x++)
        {
            for (int z = 0; z < cloudsDistance; z++)
            {
                bool isCloud = cloudsHeightMap.GetPixel(x, z) == Color.white;
               // bool isCloud = Random.Range(0, 2) == 0; // random version
                cloudsMap[x, z] = isCloud;
            }
        }
    }

    private void RenderClouds ()
    {
        for (int x = 1; x < 8; x++)
        {
            for (int y = 1; y < 8; y++)
            {
                cloudsChunks[x, y] = Instantiate(cloudsChunkPrefab, transform.position, Quaternion.identity, transform).GetComponent<CloudsChunk>();
                cloudsChunks[x, y].RenderClouds(new Vector2Int(x, y), cloudsMap);
            }
        }
    }
}

public enum eCloudsType
{
    DISABLED = 0,
    FLAT = 1,
    NORMAL = 2
}
