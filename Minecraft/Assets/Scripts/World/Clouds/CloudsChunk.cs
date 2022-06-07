using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudsChunk : MonoBehaviour
{
    private bool[,] chunkData;
    private int chunkSize = 64;
    private int blockHeight = 4;
    private int blockWidth = 12;
    private int y = 0;
    private eCloudsType cloudsType = eCloudsType.NORMAL;

    private Mesh mesh;
    private MeshFilter meshFilter;

    private List<Vector3> meshVertices = new List<Vector3>();
    private List<int> meshTriangles = new List<int>();
    private List<Color> meshLight = new List<Color>();

    public void RenderClouds (Vector2Int position, bool[,] chunkData)
    {
        this.chunkData = chunkData;

        int estiminatedPostion = (512 / chunkSize) / 2; 

        transform.position = new Vector3Int((position.x - estiminatedPostion) * chunkSize * blockWidth, 0, (position.y - estiminatedPostion) * chunkSize * blockWidth);

        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
            meshFilter.sharedMesh = new Mesh();
            mesh = meshFilter.sharedMesh;
        }

        meshVertices.Clear();
        meshTriangles.Clear();
        meshLight.Clear();

        for (int x = -chunkSize / 2; x < chunkSize / 2; x++)
        {
            for (int z = -chunkSize / 2; z < chunkSize / 2; z++)
            {
                if (!chunkData[(position.x * chunkSize) + x + chunkSize / 2, (position.y * chunkSize) + z + chunkSize / 2])
                    continue;

                for (int i = 0; i < 6; i++)
                {
                    eBlockFace thisFace = (eBlockFace)i;
                    int currentVerticesCount = meshVertices.Count;

                    if (IsFaceVisible(new Vector2Int((position.x * chunkSize) + x + chunkSize / 2, (position.y * chunkSize) + z + chunkSize / 2), thisFace))
                    {
                        Color blockColor = new Color(0, 0, 0, GetFaceColor(thisFace));
                        meshLight.AddRange(new Color[] { blockColor, blockColor, blockColor, blockColor });

                        switch (thisFace)
                        {
                            case eBlockFace.FRONT:
                                meshVertices.AddRange(new Vector3[] { new Vector3(x * blockWidth, y * blockHeight, z * blockWidth), new Vector3((x + 1) * blockWidth, y * blockHeight, z * blockWidth), new Vector3(x * blockWidth, (y + 1) * blockHeight, z * blockWidth), new Vector3((x + 1) * blockWidth, (y + 1) * blockHeight, z * blockWidth) });
                                meshTriangles.AddRange(new int[] { 0 + currentVerticesCount, 2 + currentVerticesCount, 1 + currentVerticesCount, 2 + currentVerticesCount, 3 + currentVerticesCount, 1 + currentVerticesCount });
                                break;
                            case eBlockFace.BACK:
                                meshVertices.AddRange(new Vector3[] { new Vector3(x * blockWidth, y * blockHeight, (z + 1) * blockWidth), new Vector3((x + 1) * blockWidth, y * blockHeight, (z + 1) * blockWidth), new Vector3(x * blockWidth, (y + 1) * blockHeight, (z + 1) * blockWidth), new Vector3((x + 1) * blockWidth, (y + 1) * blockHeight, (z + 1) * blockWidth) });
                                meshTriangles.AddRange(new int[] { 1 + currentVerticesCount, 2 + currentVerticesCount, 0 + currentVerticesCount, 1 + currentVerticesCount, 3 + currentVerticesCount, 2 + currentVerticesCount });
                                break;
                            case eBlockFace.RIGHT:
                                meshVertices.AddRange(new Vector3[] { new Vector3((x + 1) * blockWidth, y * blockHeight, z * blockWidth), new Vector3((x + 1) * blockWidth, (y + 1) * blockHeight, z * blockWidth), new Vector3((x + 1) * blockWidth, y * blockHeight, (z + 1) * blockWidth), new Vector3((x + 1) * blockWidth, (y + 1) * blockHeight, (z + 1) * blockWidth) });
                                meshTriangles.AddRange(new int[] { 1 + currentVerticesCount, 2 + currentVerticesCount, 0 + currentVerticesCount, 1 + currentVerticesCount, 3 + currentVerticesCount, 2 + currentVerticesCount });
                                break;
                            case eBlockFace.LEFT:
                                meshVertices.AddRange(new Vector3[] { new Vector3(x * blockWidth, y * blockHeight, z * blockWidth), new Vector3(x * blockWidth, (y + 1) * blockHeight, z * blockWidth), new Vector3(x * blockWidth, y * blockHeight, (z + 1) * blockWidth), new Vector3(x * blockWidth, (y + 1) * blockHeight, (z + 1) * blockWidth) });
                                meshTriangles.AddRange(new int[] { 0 + currentVerticesCount, 2 + currentVerticesCount, 1 + currentVerticesCount, 2 + currentVerticesCount, 3 + currentVerticesCount, 1 + currentVerticesCount });
                                break;
                            case eBlockFace.TOP:
                                meshVertices.AddRange(new Vector3[] { new Vector3(x * blockWidth, (y + 1) * blockHeight, z * blockWidth), new Vector3(x * blockWidth, (y + 1) * blockHeight, (z + 1) * blockWidth), new Vector3((x + 1) * blockWidth, (y + 1) * blockHeight, z * blockWidth), new Vector3((x + 1) * blockWidth, (y + 1) * blockHeight, (z + 1) * blockWidth) });
                                meshTriangles.AddRange(new int[] { 1 + currentVerticesCount, 2 + currentVerticesCount, 0 + currentVerticesCount, 1 + currentVerticesCount, 3 + currentVerticesCount, 2 + currentVerticesCount });
                                break;
                            case eBlockFace.BOTTOM:
                                meshVertices.AddRange(new Vector3[] { new Vector3(x * blockWidth, y * blockHeight, z * blockWidth), new Vector3(x * blockWidth, y * blockHeight, (z + 1) * blockWidth), new Vector3((x + 1) * blockWidth, y * blockHeight, z * blockWidth), new Vector3((x + 1) * blockWidth, y * blockHeight, (z + 1) * blockWidth) });
                                meshTriangles.AddRange(new int[] { 0 + currentVerticesCount, 2 + currentVerticesCount, 1 + currentVerticesCount, 2 + currentVerticesCount, 3 + currentVerticesCount, 1 + currentVerticesCount });
                                break;
                        }
                    }
                }
            }
        }

        mesh.Clear(false);
        mesh.SetVertices(meshVertices);
        mesh.SetTriangles(meshTriangles, 0);
        mesh.SetColors(meshLight);
    }

    private bool IsFaceVisible (Vector2Int position, eBlockFace face)
    {
        if (cloudsType == eCloudsType.FLAT)
        {
            switch (face)
            {
                case eBlockFace.BOTTOM:
                    return true;
                default:
                    return false;
            }
        }

        switch (face)
        {
            case eBlockFace.TOP:
                return true;
            case eBlockFace.BOTTOM:
                return true;
            case eBlockFace.FRONT:
                position.y -= 1;
                break;
            case eBlockFace.BACK:
                position.y += 1;
                break;
            case eBlockFace.RIGHT:
                position.x += 1;
                break;
            case eBlockFace.LEFT:
                position.x -= 1;
                break;
        }

        if (position.x >= 512 || position.x < 0 || position.y >= 512 || position.y < 0)
            return true;

        return !chunkData[position.x, position.y];
    }

    private float GetFaceColor(eBlockFace face)
    {
        byte neighbourLightLevel = 15;
        float neighbourlightIntensity = 0.1f;

        switch (face)
        {
            case eBlockFace.BOTTOM:
                neighbourlightIntensity += 0.0266f * neighbourLightLevel;
                break;
            case eBlockFace.FRONT:
            case eBlockFace.BACK:
                neighbourlightIntensity += 0.0066f * neighbourLightLevel;
                break;
            case eBlockFace.RIGHT:
            case eBlockFace.LEFT:
                neighbourlightIntensity += 0.0133f * neighbourLightLevel;
                break;
        }

        return neighbourlightIntensity;
    }
}
