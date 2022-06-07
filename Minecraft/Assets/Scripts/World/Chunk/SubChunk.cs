using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
public class SubChunk : MonoBehaviour
{
    private Chunk chunk;

    private List<Vector3> meshVertices = new List<Vector3>();
    private List<int> meshTriangles = new List<int>();
    private List<int> meshLiquidTriangles = new List<int>();
    private List<Vector2> meshUV = new List<Vector2>();
    private List<Color> meshLight = new List<Color>();
   
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    public int subChunkHeightStart { get; private set; }
    public int subChunkHeightEnd { get; private set; }

    public readonly float pixelSize = 16; // this value is based on how many blocks are on x/y axis in texture atlas
    private int chunkWidth;
    private int chunkHeight;
    private float blockSize;

    public SubChunk Constructor (Chunk chunk, int subChunkHeightNumber)
    {
        this.chunk = chunk;
        transform.position = chunk.ChunkPosition;

        chunkWidth = chunk.voxelSettingsData.GetChunkSize();
        chunkHeight = chunkWidth * chunk.voxelSettingsData.VerticalSubChunkCount;
        blockSize = chunk.voxelSettingsData.GetBlockSize();
        subChunkHeightStart = chunkWidth * subChunkHeightNumber;
        subChunkHeightEnd = subChunkHeightStart + chunkWidth;

        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshFilter.sharedMesh = new Mesh();
        mesh = meshFilter.sharedMesh;
        mesh.subMeshCount = 2;

        return this;
    }

    public async Task RenderChunkAsync (bool recalculateCollisions = true)
    {
        await Task.Run(() => CalculateMeshData());

        mesh.Clear(false);
        mesh.subMeshCount = 2;
        mesh.SetVertices(meshVertices);
        mesh.SetTriangles(meshTriangles, 0);
        mesh.SetTriangles(meshLiquidTriangles, 1);  
        mesh.SetUVs(0, meshUV);
        mesh.SetColors(meshLight);
    //    mesh.RecalculateNormals();

        meshVertices.Clear();
        meshTriangles.Clear();
        meshLiquidTriangles.Clear();
        meshUV.Clear();
        meshLight.Clear();

        if (recalculateCollisions)
        {
            await Task.Yield();
      //      meshCollider.sharedMesh = meshFilter.sharedMesh;
        }
    }

    public WaterVertsY GetWaterVertsY (int water, Vector3Int position)
    {
        if (water == 1)
        {
            if (chunk.chunkLiquids.water[position.x, position.y + 1, position.z] != 0)
            {
                return new WaterVertsY(1, 1, 1, 1);
            }
            else
            {
                float a = 0.8f, b = 0.8f, c = 0.8f, d = 0.8f;

                // top back
                if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y + 1, position.z + 1)) != 0)
                {
                    a = 1;
                    b = 1;
                }

                // top front
                if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y + 1, position.z - 1)) != 0)
                {
                    c = 1;
                    d = 1;
                }

                // top right
                if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y + 1, position.z)) != 0)
                {
                    b = 1;
                    c = 1;
                }

                // top left
                if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y + 1, position.z)) != 0)
                {
                    a = 1;
                    d = 1;
                }

                // corners
                // top left back
                if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y + 1, position.z + 1)) != 0)
                {
                    a = 1;
                }

                // top right back
                if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y + 1, position.z + 1)) != 0)
                {
                    b = 1;
                }

                // top left front
                if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y + 1, position.z - 1)) != 0)
                {
                    d = 1;
                }

                // top right front
                if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y + 1, position.z - 1)) != 0)
                {
                    c = 1;
                }

                return new WaterVertsY(a, b, c, d);
            }
        }
        else if (water == 2)
        {
            float a = 0.8f, b = 0.8f, c = 0.8f, d = 0.8f;

            // top back & back left
            if ((chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y + 1, position.z + 1)) != 0 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y, position.z + 1)) != 0) || (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y + 1, position.z + 1)) != 0 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y, position.z + 1)) != 0) || (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y + 1, position.z)) != 0 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y, position.z)) != 0))
            {
                a = 1;
            }
            else if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y, position.z + 1)) != 1 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y, position.z + 1)) != 1 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y, position.z)) != 1)
            {
                a = 0.7f;
            }

            if ((chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y + 1, position.z + 1)) != 0 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y, position.z + 1)) != 0) || (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y + 1, position.z + 1)) != 0 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z + 1)) != 0) || (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y + 1, position.z)) != 0 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z)) != 0))
            {
                b = 1;
            }
            else if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y, position.z + 1)) != 1 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z + 1)) != 1 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z)) != 1)
            {
                b = 0.7f;
            }

            if ((chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y + 1, position.z - 1)) != 0 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y, position.z - 1)) != 0) || (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y + 1, position.z - 1)) != 0 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z - 1)) != 0) || (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y + 1, position.z)) != 0 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z)) != 0))
            {
                c = 1;
            }
            else if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y, position.z - 1)) != 1 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z - 1)) != 1 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z)) != 1)
            {
                c = 0.7f;
            }

            if ((chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y + 1, position.z - 1)) != 0 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y, position.z - 1)) != 0) || (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y + 1, position.z - 1)) != 0 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y, position.z - 1)) != 0) || (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y + 1, position.z)) != 0 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y, position.z)) != 0))
            {
                d = 1;
            }
            else if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y, position.z - 1)) != 1 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y, position.z - 1)) != 1 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y, position.z)) != 1)
            {
                d = 0.7f;
            }

            return new WaterVertsY(a, b, c, d);
        }
        else if (water == 3)
        {
            float a = 0.7f, b = 0.7f, c = 0.7f, d = 0.7f;

            // top back & back left
            if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y, position.z + 1)) == 1)
            {
                if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y + 1, position.z + 1)) != 0)
                {
                    a = 1;
                }
                else
                {
                    a = 0.8f;
                }
            }
            else if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y, position.z + 1)) != 1 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y, position.z + 1)) != 1 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y, position.z)) != 1)
            {
                a = 0.7f;
            }

            if ((chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y + 1, position.z + 1)) != 0 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y, position.z + 1)) != 0) || (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y + 1, position.z + 1)) != 0 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z + 1)) != 0) || (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y + 1, position.z)) != 0 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z)) != 0))
            {
                b = 1;
            }
            else if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y, position.z + 1)) != 1 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z + 1)) != 1 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z)) != 1)
            {
                b = 0.7f;
            }

            if ((chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y + 1, position.z - 1)) != 0 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y, position.z - 1)) != 0) || (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y + 1, position.z - 1)) != 0 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z - 1)) != 0) || (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y + 1, position.z)) != 0 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z)) != 0))
            {
                c = 1;
            }
            else if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y, position.z - 1)) != 1 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z - 1)) != 1 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z)) != 1)
            {
                c = 0.7f;
            }

            if ((chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y + 1, position.z - 1)) != 0 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y, position.z - 1)) != 0) || (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y + 1, position.z - 1)) != 0 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y, position.z - 1)) != 0) || (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y + 1, position.z)) != 0 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y, position.z)) != 0))
            {
                d = 1;
            }
            else if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y, position.z - 1)) != 1 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y, position.z - 1)) != 1 && chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y, position.z)) != 1)
            {
                d = 0.7f;
            }

            return new WaterVertsY(a, b, c, d);
        }
        else if (water == 8)
        {
            float a = 0.15f, b = 0.15f, c = 0.15f, d = 0.15f;

            // top back
            if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y, position.z + 1)) != 0)
            {
                a = 1;
                b = 1;
            }

            // top front
            if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y, position.z - 1)) != 0)
            {
                c = 1;
                d = 1;
            }

            // top right
            if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z)) != 0)
            {
                b = 1;
                c = 1;
            }

            // top left
            if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y, position.z)) != 0)
            {
                a = 1;
                d = 1;
            }

            // corners
            // top left back
            if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y, position.z + 1)) != 0)
            {
                a = 1;
            }

            // top right back
            if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z + 1)) != 0)
            {
                b = 1;
            }

            // top left front
            if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y, position.z - 1)) != 0)
            {
                d = 1;
            }

            // top right front
            if (chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z - 1)) != 0)
            {
                c = 1;
            }

            return new WaterVertsY(a, b, c, d);
        }
        else
        {
            return new WaterVertsY(1, 1, 1, 1);
        }
    }

    public struct WaterVertsY
    {
        public float a; // clockwise verts order
        public float b;
        public float c;
        public float d;

        public WaterVertsY (float a, float b, float c, float d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }
    }

    /// <summary>
    /// Recalculates position of vertices, triangles, uv map, vertex colors.
    /// </summary>
    public void CalculateMeshData ()
    {
        meshVertices.Clear();
        meshTriangles.Clear();
        meshLiquidTriangles.Clear();
        meshUV.Clear();
        meshLight.Clear();

        float uvSmallFixValue = 0.001f;
        float tilePerc = 1f / pixelSize;

        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
                for (int y = subChunkHeightStart; y < subChunkHeightEnd; y++)
                {
                    ref byte thisWater = ref chunk.chunkLiquids.water[x, y, z];

                    if (thisWater != 0) // here is water rendering
                    {
                        // WaterVertsY wvy = GetWaterVertsY(thisWater, new Vector3Int(x, y, z));
                        LiquidVertexData wvy = chunk.chunkLiquids.liquidVertex.GetLiquidVertices(new Vector3Int(x, y, z), thisWater);

                        for (int i = 0; i < 6; i++)
                        {
                            eBlockFace thisFace = (eBlockFace)i;
                            int currentVerticesCount = meshVertices.Count;

                            if (IsLiquidFaceVisible(new Vector3Int(x, y, z), thisFace))
                            {
                                Vector2 faceTextureID = new Vector2(0, 0);
                                float uMin = faceTextureID.x;
                                float vMin = faceTextureID.y;
                                float uMax = (faceTextureID.x + 1);
                                float vMax = (faceTextureID.y + 1);

                                Color blockColor = new Color(0, GetFaceDarknessPercentage(thisFace), chunk.chunkLight.GetVoxelFaceArtificialLightIntensity(new Vector3Int(x, y, z), thisFace), chunk.chunkLight.GetVoxelFaceNaturalLightIntensity(new Vector3Int(x, y, z), thisFace));
                                Color[] blockColors = new Color[] { blockColor, blockColor, blockColor, blockColor };

                                VertexAmbientValues vav = AddAmbientOcclusion(thisFace, new Vector3Int(x, y, z), blockColors);

                                meshLight.AddRange(blockColors);

                                switch (thisFace)
                                {
                                    case eBlockFace.FRONT:
                                        meshVertices.AddRange(new Vector3[] { new Vector3(x * blockSize, y * blockSize, z * blockSize), new Vector3((x + 1) * blockSize, y * blockSize, z * blockSize), new Vector3(x * blockSize, (y + wvy.d) * blockSize, z * blockSize), new Vector3((x + 1) * blockSize, (y + wvy.c) * blockSize, z * blockSize) });

                                        if (vav.a00 + vav.a11 > vav.a01 + vav.a10) // if true, generates flipped quad (it's a fix for ambient occlusion)
                                            meshLiquidTriangles.AddRange(new int[] { 0 + currentVerticesCount, 3 + currentVerticesCount, 1 + currentVerticesCount, 2 + currentVerticesCount, 3 + currentVerticesCount, 0 + currentVerticesCount });
                                        else
                                            meshLiquidTriangles.AddRange(new int[] { 0 + currentVerticesCount, 2 + currentVerticesCount, 1 + currentVerticesCount, 2 + currentVerticesCount, 3 + currentVerticesCount, 1 + currentVerticesCount });

                                        meshUV.AddRange(new Vector2[] { new Vector2(uMin, vMin), new Vector2(uMax, vMin), new Vector2(uMin, vMax), new Vector2(uMax, vMax) });
                                        break;
                                    case eBlockFace.BACK:
                                        meshVertices.AddRange(new Vector3[] { new Vector3(x * blockSize, y * blockSize, (z + 1) * blockSize), new Vector3((x + 1) * blockSize, y * blockSize, (z + 1) * blockSize), new Vector3(x * blockSize, (y + wvy.a) * blockSize, (z + 1) * blockSize), new Vector3((x + 1) * blockSize, (y + wvy.b) * blockSize, (z + 1) * blockSize) });

                                        if (vav.a00 + vav.a11 > vav.a01 + vav.a10) // if true, generates flipped quad (it's a fix for ambient occlusion)
                                            meshLiquidTriangles.AddRange(new int[] { 1 + currentVerticesCount, 3 + currentVerticesCount, 0 + currentVerticesCount, 0 + currentVerticesCount, 3 + currentVerticesCount, 2 + currentVerticesCount });
                                        else
                                            meshLiquidTriangles.AddRange(new int[] { 1 + currentVerticesCount, 2 + currentVerticesCount, 0 + currentVerticesCount, 1 + currentVerticesCount, 3 + currentVerticesCount, 2 + currentVerticesCount });

                                        meshUV.AddRange(new Vector2[] { new Vector2(uMin, vMin), new Vector2(uMax, vMin), new Vector2(uMin, vMax), new Vector2(uMax, vMax) });
                                        break;
                                    case eBlockFace.RIGHT:
                                        meshVertices.AddRange(new Vector3[] { new Vector3((x + 1) * blockSize, y * blockSize, z * blockSize), new Vector3((x + 1) * blockSize, (y + wvy.c) * blockSize, z * blockSize), new Vector3((x + 1) * blockSize, y * blockSize, (z + 1) * blockSize), new Vector3((x + 1) * blockSize, (y + wvy.b) * blockSize, (z + 1) * blockSize) });

                                        if (vav.a00 + vav.a11 > vav.a01 + vav.a10) // if true, generates flipped quad (it's a fix for ambient occlusion)
                                            meshLiquidTriangles.AddRange(new int[] { 1 + currentVerticesCount, 3 + currentVerticesCount, 0 + currentVerticesCount, 0 + currentVerticesCount, 3 + currentVerticesCount, 2 + currentVerticesCount });
                                        else
                                            meshLiquidTriangles.AddRange(new int[] { 1 + currentVerticesCount, 2 + currentVerticesCount, 0 + currentVerticesCount, 1 + currentVerticesCount, 3 + currentVerticesCount, 2 + currentVerticesCount });

                                        meshUV.AddRange(new Vector2[] { new Vector2(uMin, vMin), new Vector2(uMin, vMax), new Vector2(uMax, vMin), new Vector2(uMax, vMax) });
                                        break;
                                    case eBlockFace.LEFT:
                                        meshVertices.AddRange(new Vector3[] { new Vector3(x * blockSize, y * blockSize, z * blockSize), new Vector3(x * blockSize, (y + wvy.d) * blockSize, z * blockSize), new Vector3(x * blockSize, y * blockSize, (z + 1) * blockSize), new Vector3(x * blockSize, (y + wvy.a) * blockSize, (z + 1) * blockSize) });

                                        if (vav.a00 + vav.a11 > vav.a01 + vav.a10) // if true, generates flipped quad (it's a fix for ambient occlusion)
                                            meshLiquidTriangles.AddRange(new int[] { 0 + currentVerticesCount, 3 + currentVerticesCount, 1 + currentVerticesCount, 2 + currentVerticesCount, 3 + currentVerticesCount, 0 + currentVerticesCount });
                                        else
                                            meshLiquidTriangles.AddRange(new int[] { 0 + currentVerticesCount, 2 + currentVerticesCount, 1 + currentVerticesCount, 2 + currentVerticesCount, 3 + currentVerticesCount, 1 + currentVerticesCount });

                                        meshUV.AddRange(new Vector2[] { new Vector2(uMin, vMin), new Vector2(uMin, vMax), new Vector2(uMax, vMin), new Vector2(uMax, vMax) });
                                        break;
                                    case eBlockFace.TOP:
                                        meshVertices.AddRange(new Vector3[] { new Vector3(x * blockSize, (y + wvy.d) * blockSize, z * blockSize), new Vector3(x * blockSize, (y + wvy.a) * blockSize, (z + 1) * blockSize), new Vector3((x + 1) * blockSize, (y + wvy.c) * blockSize, z * blockSize), new Vector3((x + 1) * blockSize, (y + wvy.b) * blockSize, (z + 1) * blockSize) });

                                        if (vav.a00 + vav.a11 > vav.a01 + vav.a10) // if true, generates flipped quad (it's a fix for ambient occlusion)
                                            meshLiquidTriangles.AddRange(new int[] { 1 + currentVerticesCount, 3 + currentVerticesCount, 0 + currentVerticesCount, 0 + currentVerticesCount, 3 + currentVerticesCount, 2 + currentVerticesCount });
                                        else
                                            meshLiquidTriangles.AddRange(new int[] { 1 + currentVerticesCount, 2 + currentVerticesCount, 0 + currentVerticesCount, 1 + currentVerticesCount, 3 + currentVerticesCount, 2 + currentVerticesCount });

                                        meshUV.AddRange(new Vector2[] { new Vector2(uMin, vMin), new Vector2(uMin, vMax), new Vector2(uMax, vMin), new Vector2(uMax, vMax) });
                                        break;
                                    case eBlockFace.BOTTOM:
                                        meshVertices.AddRange(new Vector3[] { new Vector3(x * blockSize, y * blockSize, z * blockSize), new Vector3(x * blockSize, y * blockSize, (z + 1) * blockSize), new Vector3((x + 1) * blockSize, y * blockSize, z * blockSize), new Vector3((x + 1) * blockSize, y * blockSize, (z + 1) * blockSize) });

                                        if (vav.a00 + vav.a11 > vav.a01 + vav.a10) // if true, generates flipped quad (it's a fix for ambient occlusion)
                                            meshLiquidTriangles.AddRange(new int[] { 0 + currentVerticesCount, 3 + currentVerticesCount, 1 + currentVerticesCount, 2 + currentVerticesCount, 3 + currentVerticesCount, 0 + currentVerticesCount });
                                        else
                                            meshLiquidTriangles.AddRange(new int[] { 0 + currentVerticesCount, 2 + currentVerticesCount, 1 + currentVerticesCount, 2 + currentVerticesCount, 3 + currentVerticesCount, 1 + currentVerticesCount });

                                        meshUV.AddRange(new Vector2[] { new Vector2(uMin, vMin), new Vector2(uMin, vMax), new Vector2(uMax, vMin), new Vector2(uMax, vMax) });
                                        break;
                                }
                            }
                        }
                    }

                    Block thisBlock = chunk.chunkBlocks.GetBlock(x, y, z);

                    if (thisBlock == null)
                        continue;

                    if (!thisBlock.BlockData.isRendered)
                        continue;

                    for (int i = 0; i < 6; i++)
                    {
                        eBlockFace thisFace = (eBlockFace)i;
                        int currentVerticesCount = meshVertices.Count;

                        if (IsFaceVisible(new Vector3Int(x, y, z), thisFace))
                        {
                            Vector2 faceTextureID = thisBlock.GetTextureForFace(thisFace);
                            float uMin = faceTextureID.x * tilePerc + uvSmallFixValue;
                            float vMin = faceTextureID.y * tilePerc + uvSmallFixValue;
                            float uMax = (faceTextureID.x + 1) * tilePerc - uvSmallFixValue;
                            float vMax = (faceTextureID.y + 1) * tilePerc - uvSmallFixValue;

                            Color blockColor = new Color(0, GetFaceDarknessPercentage(thisFace), chunk.chunkLight.GetVoxelFaceArtificialLightIntensity(new Vector3Int(x, y, z), thisFace), chunk.chunkLight.GetVoxelFaceNaturalLightIntensity(new Vector3Int(x, y, z), thisFace));
                            Color[] blockColors = new Color[] { blockColor, blockColor, blockColor, blockColor };

                            VertexAmbientValues vav = AddAmbientOcclusion(thisFace, new Vector3Int(x, y, z), blockColors);

                            meshLight.AddRange(blockColors);

                            switch (thisFace)
                            {
                                case eBlockFace.FRONT:
                                    meshVertices.AddRange(new Vector3[] { new Vector3(x * blockSize, y * blockSize, z * blockSize), new Vector3((x + 1) * blockSize, y * blockSize, z * blockSize), new Vector3(x * blockSize, (y + 1) * blockSize, z * blockSize), new Vector3((x + 1) * blockSize, (y + 1) * blockSize, z * blockSize) });

                                    if (vav.a00 + vav.a11 > vav.a01 + vav.a10) // if true, generates flipped quad (it's a fix for ambient occlusion)
                                        meshTriangles.AddRange(new int[] { 0 + currentVerticesCount, 3 + currentVerticesCount, 1 + currentVerticesCount, 2 + currentVerticesCount, 3 + currentVerticesCount, 0 + currentVerticesCount });
                                    else
                                        meshTriangles.AddRange(new int[] { 0 + currentVerticesCount, 2 + currentVerticesCount, 1 + currentVerticesCount, 2 + currentVerticesCount, 3 + currentVerticesCount, 1 + currentVerticesCount });

                                    meshUV.AddRange(new Vector2[] { new Vector2(uMin, vMin), new Vector2(uMax, vMin), new Vector2(uMin, vMax), new Vector2(uMax, vMax) });
                                    break;
                                case eBlockFace.BACK:
                                    meshVertices.AddRange(new Vector3[] { new Vector3(x * blockSize, y * blockSize, (z + 1) * blockSize), new Vector3((x + 1) * blockSize, y * blockSize, (z + 1) * blockSize), new Vector3(x * blockSize, (y + 1) * blockSize, (z + 1) * blockSize), new Vector3((x + 1) * blockSize, (y + 1) * blockSize, (z + 1) * blockSize) });

                                    if (vav.a00 + vav.a11 > vav.a01 + vav.a10) // if true, generates flipped quad (it's a fix for ambient occlusion)
                                        meshTriangles.AddRange(new int[] { 1 + currentVerticesCount, 3 + currentVerticesCount, 0 + currentVerticesCount, 0 + currentVerticesCount, 3 + currentVerticesCount, 2 + currentVerticesCount });
                                    else
                                        meshTriangles.AddRange(new int[] { 1 + currentVerticesCount, 2 + currentVerticesCount, 0 + currentVerticesCount, 1 + currentVerticesCount, 3 + currentVerticesCount, 2 + currentVerticesCount });

                                    meshUV.AddRange(new Vector2[] { new Vector2(uMin, vMin), new Vector2(uMax, vMin), new Vector2(uMin, vMax), new Vector2(uMax, vMax) });
                                    break;
                                case eBlockFace.RIGHT:
                                    meshVertices.AddRange(new Vector3[] { new Vector3((x + 1) * blockSize, y * blockSize, z * blockSize), new Vector3((x + 1) * blockSize, (y + 1) * blockSize, z * blockSize), new Vector3((x + 1) * blockSize, y * blockSize, (z + 1) * blockSize), new Vector3((x + 1) * blockSize, (y + 1) * blockSize, (z + 1) * blockSize) });

                                    if (vav.a00 + vav.a11 > vav.a01 + vav.a10) // if true, generates flipped quad (it's a fix for ambient occlusion)
                                        meshTriangles.AddRange(new int[] { 1 + currentVerticesCount, 3 + currentVerticesCount, 0 + currentVerticesCount, 0 + currentVerticesCount, 3 + currentVerticesCount, 2 + currentVerticesCount });
                                    else
                                        meshTriangles.AddRange(new int[] { 1 + currentVerticesCount, 2 + currentVerticesCount, 0 + currentVerticesCount, 1 + currentVerticesCount, 3 + currentVerticesCount, 2 + currentVerticesCount });

                                    meshUV.AddRange(new Vector2[] { new Vector2(uMin, vMin), new Vector2(uMin, vMax), new Vector2(uMax, vMin), new Vector2(uMax, vMax) });
                                    break;
                                case eBlockFace.LEFT:
                                    meshVertices.AddRange(new Vector3[] { new Vector3(x * blockSize, y * blockSize, z * blockSize), new Vector3(x * blockSize, (y + 1) * blockSize, z * blockSize), new Vector3(x * blockSize, y * blockSize, (z + 1) * blockSize), new Vector3(x * blockSize, (y + 1) * blockSize, (z + 1) * blockSize) });

                                    if (vav.a00 + vav.a11 > vav.a01 + vav.a10) // if true, generates flipped quad (it's a fix for ambient occlusion)
                                        meshTriangles.AddRange(new int[] { 0 + currentVerticesCount, 3 + currentVerticesCount, 1 + currentVerticesCount, 2 + currentVerticesCount, 3 + currentVerticesCount, 0 + currentVerticesCount });
                                    else
                                        meshTriangles.AddRange(new int[] { 0 + currentVerticesCount, 2 + currentVerticesCount, 1 + currentVerticesCount, 2 + currentVerticesCount, 3 + currentVerticesCount, 1 + currentVerticesCount });

                                    meshUV.AddRange(new Vector2[] { new Vector2(uMin, vMin), new Vector2(uMin, vMax), new Vector2(uMax, vMin), new Vector2(uMax, vMax) });
                                    break;
                                case eBlockFace.TOP:
                                    meshVertices.AddRange(new Vector3[] { new Vector3(x * blockSize, (y + 1) * blockSize, z * blockSize), new Vector3(x * blockSize, (y + 1) * blockSize, (z + 1) * blockSize), new Vector3((x + 1) * blockSize, (y + 1) * blockSize, z * blockSize), new Vector3((x + 1) * blockSize, (y + 1) * blockSize, (z + 1) * blockSize) });
                                    
                                    if (vav.a00 + vav.a11 > vav.a01 + vav.a10) // if true, generates flipped quad (it's a fix for ambient occlusion)
                                        meshTriangles.AddRange(new int[] { 1 + currentVerticesCount, 3 + currentVerticesCount, 0 + currentVerticesCount, 0 + currentVerticesCount, 3 + currentVerticesCount, 2 + currentVerticesCount });
                                    else
                                        meshTriangles.AddRange(new int[] { 1 + currentVerticesCount, 2 + currentVerticesCount, 0 + currentVerticesCount, 1 + currentVerticesCount, 3 + currentVerticesCount, 2 + currentVerticesCount });

                                    meshUV.AddRange(new Vector2[] { new Vector2(uMin, vMin), new Vector2(uMin, vMax), new Vector2(uMax, vMin), new Vector2(uMax, vMax) });
                                    break;
                                case eBlockFace.BOTTOM:
                                    meshVertices.AddRange(new Vector3[] { new Vector3(x * blockSize, y * blockSize, z * blockSize), new Vector3(x * blockSize, y * blockSize, (z + 1) * blockSize), new Vector3((x + 1) * blockSize, y * blockSize, z * blockSize), new Vector3((x + 1) * blockSize, y * blockSize, (z + 1) * blockSize) });
                                    
                                    if (vav.a00 + vav.a11 > vav.a01 + vav.a10) // if true, generates flipped quad (it's a fix for ambient occlusion)
                                        meshTriangles.AddRange(new int[] { 0 + currentVerticesCount, 3 + currentVerticesCount, 1 + currentVerticesCount, 2 + currentVerticesCount, 3 + currentVerticesCount, 0 + currentVerticesCount });
                                    else
                                        meshTriangles.AddRange(new int[] { 0 + currentVerticesCount, 2 + currentVerticesCount, 1 + currentVerticesCount, 2 + currentVerticesCount, 3 + currentVerticesCount, 1 + currentVerticesCount });

                                    meshUV.AddRange(new Vector2[] { new Vector2(uMin, vMin), new Vector2(uMin, vMax), new Vector2(uMax, vMin), new Vector2(uMax, vMax) });
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }



    private VertexAmbientValues AddAmbientOcclusion (eBlockFace face, Vector3Int blockPosition, Color[] vertexColors)
    {
        int[] ambientModePerVertex = new int[4];
        float[] naturalColorPerVertex = new float[4];
        float[] artificialColorPerVertex = new float[4];

        float thisArtificialLight = vertexColors[0].b;
        float thisNaturalLight = vertexColors[0].a;

        if (face == eBlockFace.TOP || face == eBlockFace.BOTTOM)
        {
            int[] isNeighbourSolidBlock = new int[8];
            float[] neighbourNaturalLight = new float[8];
            float[] neighbourArtificialLight = new float[8];

            Vector3Int[] neighbourPositions = new Vector3Int[8];

            if (face == eBlockFace.TOP)
            {
                Vector3Int[] neighbourPositionsTop = new Vector3Int[8]
                {
                    new Vector3Int(blockPosition.x - 1, blockPosition.y + 1, blockPosition.z + 1),
                    new Vector3Int(blockPosition.x, blockPosition.y + 1, blockPosition.z + 1),
                    new Vector3Int(blockPosition.x + 1, blockPosition.y + 1, blockPosition.z + 1),
                    new Vector3Int(blockPosition.x + 1, blockPosition.y + 1, blockPosition.z),
                    new Vector3Int(blockPosition.x + 1, blockPosition.y + 1, blockPosition.z - 1),
                    new Vector3Int(blockPosition.x, blockPosition.y + 1, blockPosition.z - 1),
                    new Vector3Int(blockPosition.x - 1, blockPosition.y + 1, blockPosition.z - 1),
                    new Vector3Int(blockPosition.x - 1, blockPosition.y + 1, blockPosition.z)
                };
                neighbourPositions = neighbourPositionsTop;
            }
            else
            {
                Vector3Int[] neighbourPositionsBottom = new Vector3Int[8]
                {
                    new Vector3Int(blockPosition.x - 1, blockPosition.y - 1, blockPosition.z + 1),
                    new Vector3Int(blockPosition.x, blockPosition.y - 1, blockPosition.z + 1),
                    new Vector3Int(blockPosition.x + 1, blockPosition.y - 1, blockPosition.z + 1),
                    new Vector3Int(blockPosition.x + 1, blockPosition.y - 1, blockPosition.z),
                    new Vector3Int(blockPosition.x + 1, blockPosition.y - 1, blockPosition.z - 1),
                    new Vector3Int(blockPosition.x, blockPosition.y - 1, blockPosition.z - 1),
                    new Vector3Int(blockPosition.x - 1, blockPosition.y - 1, blockPosition.z - 1),
                    new Vector3Int(blockPosition.x - 1, blockPosition.y - 1, blockPosition.z)
                };
                neighbourPositions = neighbourPositionsBottom;
            }

            for (int i = 0; i < 8; i++)
            {
                isNeighbourSolidBlock[i] = IsSolidBlock(neighbourPositions[i]);
                neighbourNaturalLight[i] = GetNaturalLight(neighbourPositions[i]);
                neighbourArtificialLight[i] = GetArtificialLight(neighbourPositions[i]);
            }


            // upper left vertex
            ambientModePerVertex[1] = VertexAO(isNeighbourSolidBlock[7], isNeighbourSolidBlock[1], isNeighbourSolidBlock[0]);
            naturalColorPerVertex[1] = NaturalIntensityAverage(neighbourNaturalLight[7], neighbourNaturalLight[1], neighbourNaturalLight[0]);
            artificialColorPerVertex[1] = ArtificialIntensityAverage(neighbourArtificialLight[7], neighbourArtificialLight[1], neighbourArtificialLight[0]);
            
            // upper right vertex
            ambientModePerVertex[3] = VertexAO(isNeighbourSolidBlock[1], isNeighbourSolidBlock[3], isNeighbourSolidBlock[2]);
            naturalColorPerVertex[3] = NaturalIntensityAverage(neighbourNaturalLight[1], neighbourNaturalLight[3], neighbourNaturalLight[2]);
            artificialColorPerVertex[3] = ArtificialIntensityAverage(neighbourArtificialLight[1], neighbourArtificialLight[3], neighbourArtificialLight[2]);

            // lower right vertex
            ambientModePerVertex[2] = VertexAO(isNeighbourSolidBlock[3], isNeighbourSolidBlock[5], isNeighbourSolidBlock[4]);
            naturalColorPerVertex[2] = NaturalIntensityAverage(neighbourNaturalLight[3], neighbourNaturalLight[5], neighbourNaturalLight[4]);
            artificialColorPerVertex[2] = ArtificialIntensityAverage(neighbourArtificialLight[3], neighbourArtificialLight[5], neighbourArtificialLight[4]);

            // lower left vertex
            ambientModePerVertex[0] = VertexAO(isNeighbourSolidBlock[5], isNeighbourSolidBlock[7], isNeighbourSolidBlock[6]);
            naturalColorPerVertex[0] = NaturalIntensityAverage(neighbourNaturalLight[5], neighbourNaturalLight[7], neighbourNaturalLight[6]);
            artificialColorPerVertex[0] = ArtificialIntensityAverage(neighbourArtificialLight[5], neighbourArtificialLight[7], neighbourArtificialLight[6]);

            for (int i = 0; i < 4; i++)
            {
                vertexColors[i].g += ConvertToDarkness(ambientModePerVertex[i]);
                vertexColors[i].b = artificialColorPerVertex[i];
                vertexColors[i].a = naturalColorPerVertex[i];
            }

           // return new VertexAmbientValues(ambientModePerVertex[0], ambientModePerVertex[1], ambientModePerVertex[2], ambientModePerVertex[3]);
            return new VertexAmbientValues(ambientModePerVertex[0], ambientModePerVertex[1], ambientModePerVertex[3], ambientModePerVertex[2]);
          //  return new VertexAmbientValues(ambientModePerVertex[1], ambientModePerVertex[3], ambientModePerVertex[2], ambientModePerVertex[0]);
        }
        else if (face == eBlockFace.FRONT || face == eBlockFace.BACK)
        {
            int[] isNeighbourSolidBlock = new int[8];
            float[] neighbourNaturalLight = new float[8];
            float[] neighbourArtificialLight = new float[8];

            Vector3Int[] neighbourPositions = new Vector3Int[8];

            if (face == eBlockFace.FRONT)
            {
                Vector3Int[] neighbourPositionsFront = new Vector3Int[8]
                {
                    new Vector3Int(blockPosition.x - 1, blockPosition.y + 1, blockPosition.z - 1),
                    new Vector3Int(blockPosition.x, blockPosition.y + 1, blockPosition.z - 1),
                    new Vector3Int(blockPosition.x + 1, blockPosition.y + 1, blockPosition.z - 1),
                    new Vector3Int(blockPosition.x + 1, blockPosition.y, blockPosition.z - 1),
                    new Vector3Int(blockPosition.x + 1, blockPosition.y - 1, blockPosition.z - 1),
                    new Vector3Int(blockPosition.x, blockPosition.y - 1, blockPosition.z - 1),
                    new Vector3Int(blockPosition.x - 1, blockPosition.y - 1, blockPosition.z - 1),
                    new Vector3Int(blockPosition.x - 1, blockPosition.y, blockPosition.z - 1)
                };
                neighbourPositions = neighbourPositionsFront;
            }
            else
            {
                Vector3Int[] neighbourPositionsBack = new Vector3Int[8]
                {
                    new Vector3Int(blockPosition.x - 1, blockPosition.y + 1, blockPosition.z + 1),
                    new Vector3Int(blockPosition.x, blockPosition.y + 1, blockPosition.z + 1),
                    new Vector3Int(blockPosition.x + 1, blockPosition.y + 1, blockPosition.z + 1),
                    new Vector3Int(blockPosition.x + 1, blockPosition.y, blockPosition.z + 1),
                    new Vector3Int(blockPosition.x + 1, blockPosition.y - 1, blockPosition.z + 1),
                    new Vector3Int(blockPosition.x, blockPosition.y - 1, blockPosition.z + 1),
                    new Vector3Int(blockPosition.x - 1, blockPosition.y - 1, blockPosition.z + 1),
                    new Vector3Int(blockPosition.x - 1, blockPosition.y, blockPosition.z + 1)
                };
                neighbourPositions = neighbourPositionsBack;
            }

            for (int i = 0; i < 8; i++)
            {
                isNeighbourSolidBlock[i] = IsSolidBlock(neighbourPositions[i]);
                neighbourNaturalLight[i] = GetNaturalLight(neighbourPositions[i]);
                neighbourArtificialLight[i] = GetArtificialLight(neighbourPositions[i]);
            }


            // upper left vertex
            ambientModePerVertex[2] = VertexAO(isNeighbourSolidBlock[7], isNeighbourSolidBlock[1], isNeighbourSolidBlock[0]);
            naturalColorPerVertex[2] = NaturalIntensityAverage(neighbourNaturalLight[7], neighbourNaturalLight[1], neighbourNaturalLight[0]);
            artificialColorPerVertex[2] = ArtificialIntensityAverage(neighbourArtificialLight[7], neighbourArtificialLight[1], neighbourArtificialLight[0]);

            // upper right vertex
            ambientModePerVertex[3] = VertexAO(isNeighbourSolidBlock[1], isNeighbourSolidBlock[3], isNeighbourSolidBlock[2]);
            naturalColorPerVertex[3] = NaturalIntensityAverage(neighbourNaturalLight[1], neighbourNaturalLight[3], neighbourNaturalLight[2]);
            artificialColorPerVertex[3] = ArtificialIntensityAverage(neighbourArtificialLight[1], neighbourArtificialLight[3], neighbourArtificialLight[2]);

            // lower right vertex
            ambientModePerVertex[1] = VertexAO(isNeighbourSolidBlock[3], isNeighbourSolidBlock[5], isNeighbourSolidBlock[4]);
            naturalColorPerVertex[1] = NaturalIntensityAverage(neighbourNaturalLight[3], neighbourNaturalLight[5], neighbourNaturalLight[4]);
            artificialColorPerVertex[1] = ArtificialIntensityAverage(neighbourArtificialLight[3], neighbourArtificialLight[5], neighbourArtificialLight[4]);

            // lower left vertex
            ambientModePerVertex[0] = VertexAO(isNeighbourSolidBlock[5], isNeighbourSolidBlock[7], isNeighbourSolidBlock[6]);
            naturalColorPerVertex[0] = NaturalIntensityAverage(neighbourNaturalLight[5], neighbourNaturalLight[7], neighbourNaturalLight[6]);
            artificialColorPerVertex[0] = ArtificialIntensityAverage(neighbourArtificialLight[5], neighbourArtificialLight[7], neighbourArtificialLight[6]);

            for (int i = 0; i < 4; i++)
            {
                vertexColors[i].g += ConvertToDarkness(ambientModePerVertex[i]);
                vertexColors[i].b = artificialColorPerVertex[i];
                vertexColors[i].a = naturalColorPerVertex[i];
            }

            // return new VertexAmbientValues(ambientModePerVertex[0], ambientModePerVertex[1], ambientModePerVertex[2], ambientModePerVertex[3]);
            return new VertexAmbientValues(ambientModePerVertex[0], ambientModePerVertex[1], ambientModePerVertex[3], ambientModePerVertex[2]);
            //  return new VertexAmbientValues(ambientModePerVertex[1], ambientModePerVertex[3], ambientModePerVertex[2], ambientModePerVertex[0]);
        }
        else if (face == eBlockFace.LEFT || face == eBlockFace.RIGHT)
        {
            int[] isNeighbourSolidBlock = new int[8];
            float[] neighbourNaturalLight = new float[8];
            float[] neighbourArtificialLight = new float[8];

            Vector3Int[] neighbourPositions = new Vector3Int[8];

            if (face == eBlockFace.LEFT)
            {
                Vector3Int[] neighbourPositionsLeft = new Vector3Int[8]
                {
                    new Vector3Int(blockPosition.x - 1, blockPosition.y + 1, blockPosition.z + 1),
                    new Vector3Int(blockPosition.x - 1, blockPosition.y + 1, blockPosition.z),
                    new Vector3Int(blockPosition.x - 1, blockPosition.y + 1, blockPosition.z - 1),
                    new Vector3Int(blockPosition.x - 1, blockPosition.y, blockPosition.z - 1),
                    new Vector3Int(blockPosition.x - 1, blockPosition.y - 1, blockPosition.z - 1),
                    new Vector3Int(blockPosition.x - 1, blockPosition.y - 1, blockPosition.z),
                    new Vector3Int(blockPosition.x - 1, blockPosition.y - 1, blockPosition.z + 1),
                    new Vector3Int(blockPosition.x - 1, blockPosition.y, blockPosition.z + 1)
                };
                neighbourPositions = neighbourPositionsLeft;
            }
            else
            {
                Vector3Int[] neighbourPositionsRight = new Vector3Int[8]
                {
                    new Vector3Int(blockPosition.x + 1, blockPosition.y + 1, blockPosition.z + 1),
                    new Vector3Int(blockPosition.x + 1, blockPosition.y + 1, blockPosition.z),
                    new Vector3Int(blockPosition.x + 1, blockPosition.y + 1, blockPosition.z - 1),
                    new Vector3Int(blockPosition.x + 1, blockPosition.y, blockPosition.z - 1),
                    new Vector3Int(blockPosition.x + 1, blockPosition.y - 1, blockPosition.z - 1),
                    new Vector3Int(blockPosition.x + 1, blockPosition.y - 1, blockPosition.z),
                    new Vector3Int(blockPosition.x + 1, blockPosition.y - 1, blockPosition.z + 1),
                    new Vector3Int(blockPosition.x + 1, blockPosition.y, blockPosition.z + 1)
                };
                neighbourPositions = neighbourPositionsRight;
            }

            for (int i = 0; i < 8; i++)
            {
                isNeighbourSolidBlock[i] = IsSolidBlock(neighbourPositions[i]);
                neighbourNaturalLight[i] = GetNaturalLight(neighbourPositions[i]);
                neighbourArtificialLight[i] = GetArtificialLight(neighbourPositions[i]);
            }


            // upper left vertex
            ambientModePerVertex[3] = VertexAO(isNeighbourSolidBlock[7], isNeighbourSolidBlock[1], isNeighbourSolidBlock[0]);
            naturalColorPerVertex[3] = NaturalIntensityAverage(neighbourNaturalLight[7], neighbourNaturalLight[1], neighbourNaturalLight[0]);
            artificialColorPerVertex[3] = ArtificialIntensityAverage(neighbourArtificialLight[7], neighbourArtificialLight[1], neighbourArtificialLight[0]);

            // upper right vertex
            ambientModePerVertex[1] = VertexAO(isNeighbourSolidBlock[1], isNeighbourSolidBlock[3], isNeighbourSolidBlock[2]);
            naturalColorPerVertex[1] = NaturalIntensityAverage(neighbourNaturalLight[1], neighbourNaturalLight[3], neighbourNaturalLight[2]);
            artificialColorPerVertex[1] = ArtificialIntensityAverage(neighbourArtificialLight[1], neighbourArtificialLight[3], neighbourArtificialLight[2]);

            // lower right vertex
            ambientModePerVertex[0] = VertexAO(isNeighbourSolidBlock[3], isNeighbourSolidBlock[5], isNeighbourSolidBlock[4]);
            naturalColorPerVertex[0] = NaturalIntensityAverage(neighbourNaturalLight[3], neighbourNaturalLight[5], neighbourNaturalLight[4]);
            artificialColorPerVertex[0] = ArtificialIntensityAverage(neighbourArtificialLight[3], neighbourArtificialLight[5], neighbourArtificialLight[4]);

            // lower left vertex
            ambientModePerVertex[2] = VertexAO(isNeighbourSolidBlock[5], isNeighbourSolidBlock[7], isNeighbourSolidBlock[6]);
            naturalColorPerVertex[2] = NaturalIntensityAverage(neighbourNaturalLight[5], neighbourNaturalLight[7], neighbourNaturalLight[6]);
            artificialColorPerVertex[2] = ArtificialIntensityAverage(neighbourArtificialLight[5], neighbourArtificialLight[7], neighbourArtificialLight[6]);

            for (int i = 0; i < 4; i++)
            {
                vertexColors[i].g += ConvertToDarkness(ambientModePerVertex[i]);
                vertexColors[i].b = artificialColorPerVertex[i];
                vertexColors[i].a = naturalColorPerVertex[i];
            }

            // return new VertexAmbientValues(ambientModePerVertex[0], ambientModePerVertex[1], ambientModePerVertex[2], ambientModePerVertex[3]);
            return new VertexAmbientValues(ambientModePerVertex[0], ambientModePerVertex[1], ambientModePerVertex[3], ambientModePerVertex[2]);
            //  return new VertexAmbientValues(ambientModePerVertex[1], ambientModePerVertex[3], ambientModePerVertex[2], ambientModePerVertex[0]);
        }
        else
        {
            return new VertexAmbientValues();
        }

        int VertexAO (int side1, int side2, int corner)
        {
            if (side1 + side2 == 2)
            {
                return 0;
            }

            return 3 - (side1 + side2 + corner);
        }

        float ConvertToDarkness (int ao)
        {
            if (ao == 3)
                return 0;

            return 10;
        }

        float NaturalIntensityAverage (float neighbour1, float neighbour2, float neighbour3 = 0)
        {
            float sum = thisNaturalLight;
            float divide = 1;

            if (neighbour1 < 0.9375f)
            {
                sum += neighbour1;
                divide += 1;
            }

            if (neighbour2 < 0.9375f)
            {
                sum += neighbour2;
                divide += 1;
            }

            if (neighbour3 < 0.9375f)
            {
                sum += neighbour3;
                divide += 1;
            }

            return sum / divide;

            //    return (thisNaturalLight + neighbour1 + neighbour2) / 3f; // no corner

            return (thisNaturalLight + neighbour1 + neighbour2 + neighbour3) / 4f;
        }

        float ArtificialIntensityAverage(float neighbour1, float neighbour2, float neighbour3 = 0)
        {
            float sum = thisArtificialLight;
            float divide = 1;

            if (neighbour1 < 0.9375f)
            {
                sum += neighbour1;
                divide += 1;
            }

            if (neighbour2 < 0.9375f)
            {
                sum += neighbour2;
                divide += 1;
            }

            if (neighbour3 < 0.9375f)
            {
                sum += neighbour3;
                divide += 1;
            }

            return sum / divide;

            //    return (thisArtificialLight + neighbour1 + neighbour2) / 3f; // no corner

            return (thisArtificialLight + neighbour1 + neighbour2 + neighbour3) / 4f;
        }
    }

    public struct VertexAmbientValues
    {
        public int a00;
        public int a01;
        public int a10;
        public int a11;


        public VertexAmbientValues (int a, int b, int c, int d)
        {
            a00 = a;
            a01 = b;
            a10 = d;
            a11 = c;

        }
    }

    private float GetNaturalLight (Vector3Int blockPosition)
    {
        try
        {
            byte neighbourBlock;

            if (blockPosition.x < 0)
            {
                if (blockPosition.z > chunkWidth - 1)
                {
                    neighbourBlock = chunk.chunkNeighbours.BackLeftNeighbourChunk.chunkLight.GetVoxelNaturalLightLevel(chunkWidth - 1, blockPosition.y, 0);
                }
                else if (blockPosition.z < 0)
                {
                    neighbourBlock = chunk.chunkNeighbours.FrontLeftNeighbourChunk.chunkLight.GetVoxelNaturalLightLevel(chunkWidth - 1, blockPosition.y, chunkWidth - 1);
                }
                else
                {
                    neighbourBlock = chunk.chunkNeighbours.LeftNeighbourChunk.chunkLight.GetVoxelNaturalLightLevel(chunkWidth - 1, blockPosition.y, blockPosition.z);
                }
            }
            else if (blockPosition.x > chunkWidth - 1)
            {
                if (blockPosition.z > chunkWidth - 1)
                {
                    neighbourBlock = chunk.chunkNeighbours.BackRightNeighbourChunk.chunkLight.GetVoxelNaturalLightLevel(0, blockPosition.y, 0);
                }
                else if (blockPosition.z < 0)
                {
                    neighbourBlock = chunk.chunkNeighbours.FrontRightNeighbourChunk.chunkLight.GetVoxelNaturalLightLevel(0, blockPosition.y, chunkWidth - 1);
                }
                else
                {
                    neighbourBlock = chunk.chunkNeighbours.RightNeighbourChunk.chunkLight.GetVoxelNaturalLightLevel(0, blockPosition.y, blockPosition.z);
                }
            }
            else if (blockPosition.z < 0)
                neighbourBlock = chunk.chunkNeighbours.FrontNeighbourChunk.chunkLight.GetVoxelNaturalLightLevel(blockPosition.x, blockPosition.y, chunkWidth - 1);
            else if (blockPosition.z > chunkWidth - 1)
                neighbourBlock = chunk.chunkNeighbours.BackNeighbourChunk.chunkLight.GetVoxelNaturalLightLevel(blockPosition.x, blockPosition.y, 0);
            else if (blockPosition.y < 0)
                return 1;
            else if (blockPosition.y > chunkHeight - 1)
                return 0;
            else
                neighbourBlock = chunk.chunkLight.GetVoxelNaturalLightLevel(blockPosition.x, blockPosition.y, blockPosition.z);

            return LightLevelToIntensity(neighbourBlock);
        }
        catch (System.Exception e)
        {
            Debug.Log(blockPosition.x + " " + e);
        }

        return 0;
    }

    private float GetArtificialLight(Vector3Int blockPosition)
    {
        try
        {
            byte neighbourBlock;

            if (blockPosition.x < 0)
            {
                if (blockPosition.z > chunkWidth - 1)
                {
                    neighbourBlock = chunk.chunkNeighbours.BackLeftNeighbourChunk.chunkLight.GetVoxelArtificialLightLevel(chunkWidth - 1, blockPosition.y, 0);
                }
                else if (blockPosition.z < 0)
                {
                    neighbourBlock = chunk.chunkNeighbours.FrontLeftNeighbourChunk.chunkLight.GetVoxelArtificialLightLevel(chunkWidth - 1, blockPosition.y, chunkWidth - 1);
                }
                else
                {
                    neighbourBlock = chunk.chunkNeighbours.LeftNeighbourChunk.chunkLight.GetVoxelArtificialLightLevel(chunkWidth - 1, blockPosition.y, blockPosition.z);
                }
            }
            else if (blockPosition.x > chunkWidth - 1)
            {
                if (blockPosition.z > chunkWidth - 1)
                {
                    neighbourBlock = chunk.chunkNeighbours.BackRightNeighbourChunk.chunkLight.GetVoxelArtificialLightLevel(0, blockPosition.y, 0);
                }
                else if (blockPosition.z < 0)
                {
                    neighbourBlock = chunk.chunkNeighbours.FrontRightNeighbourChunk.chunkLight.GetVoxelArtificialLightLevel(0, blockPosition.y, chunkWidth - 1);
                }
                else
                {
                    neighbourBlock = chunk.chunkNeighbours.RightNeighbourChunk.chunkLight.GetVoxelArtificialLightLevel(0, blockPosition.y, blockPosition.z);
                }
            }
            else if (blockPosition.z < 0)
                neighbourBlock = chunk.chunkNeighbours.FrontNeighbourChunk.chunkLight.GetVoxelArtificialLightLevel(blockPosition.x, blockPosition.y, chunkWidth - 1);
            else if (blockPosition.z > chunkWidth - 1)
                neighbourBlock = chunk.chunkNeighbours.BackNeighbourChunk.chunkLight.GetVoxelArtificialLightLevel(blockPosition.x, blockPosition.y, 0);
            else if (blockPosition.y < 0)
                return 1;
            else if (blockPosition.y > chunkHeight - 1)
                return 0;
            else
                neighbourBlock = chunk.chunkLight.GetVoxelArtificialLightLevel(blockPosition.x, blockPosition.y, blockPosition.z);

            return LightLevelToIntensity(neighbourBlock);
        }
        catch (System.Exception e)
        {
            Debug.Log(blockPosition.x + " " + e);
        }

        return 0;
    }

    private float LightLevelToIntensity(int lightLevel)
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

    private int IsSolidBlock (Vector3Int blockPosition)
    {
        try
        {
            Block neighbourBlock;

            if (blockPosition.x < 0)
            {
                if (blockPosition.z > chunkWidth - 1)
                {
                    neighbourBlock = chunk.chunkNeighbours.BackLeftNeighbourChunk.chunkBlocks.GetBlock(chunkWidth - 1, blockPosition.y, 0);
                }
                else if (blockPosition.z < 0)
                {
                    neighbourBlock = chunk.chunkNeighbours.FrontLeftNeighbourChunk.chunkBlocks.GetBlock(chunkWidth - 1, blockPosition.y, chunkWidth - 1);
                }
                else
                {
                    neighbourBlock = chunk.chunkNeighbours.LeftNeighbourChunk.chunkBlocks.GetBlock(chunkWidth - 1, blockPosition.y, blockPosition.z);
                }
            }
            else if (blockPosition.x > chunkWidth - 1)
            {
                if (blockPosition.z > chunkWidth - 1)
                {
                    neighbourBlock = chunk.chunkNeighbours.BackRightNeighbourChunk.chunkBlocks.GetBlock(0, blockPosition.y, 0);
                }
                else if (blockPosition.z < 0)
                {
                    neighbourBlock = chunk.chunkNeighbours.FrontRightNeighbourChunk.chunkBlocks.GetBlock(0, blockPosition.y, chunkWidth - 1);
                }
                else
                {
                    neighbourBlock = chunk.chunkNeighbours.RightNeighbourChunk.chunkBlocks.GetBlock(0, blockPosition.y, blockPosition.z);
                }
            }
            else if (blockPosition.z < 0)
                neighbourBlock = chunk.chunkNeighbours.FrontNeighbourChunk.chunkBlocks.GetBlock(blockPosition.x, blockPosition.y, chunkWidth - 1);
            else if (blockPosition.z > chunkWidth - 1)
                neighbourBlock = chunk.chunkNeighbours.BackNeighbourChunk.chunkBlocks.GetBlock(blockPosition.x, blockPosition.y, 0);
            else if (blockPosition.y < 0)
                return 1;
            else if (blockPosition.y > chunkHeight - 1)
                return 0;
            else
                neighbourBlock = chunk.chunkBlocks.GetBlock(blockPosition.x, blockPosition.y, blockPosition.z);

            if (neighbourBlock == null)
                return 0;

            return 1;
        }
        catch (System.Exception e)
        {
            Debug.Log(blockPosition.x + " " + e);
        }

        return 0;
    }

    /// <summary>
    /// Check if we should render block face on given position based on block's neighbour.
    /// </summary>
    /// <param name="blockPosition"></param>
    /// <param name="blockFace"></param>
    /// <returns></returns>
    private bool IsFaceVisible (Vector3Int blockPosition, eBlockFace blockFace)
    {
        Block thisBlock = chunk.chunkBlocks.GetBlock(blockPosition.x, blockPosition.y, blockPosition.z);
        Block neighbourBlock;

        switch (blockFace)
        {
            case eBlockFace.TOP:
                blockPosition.y += 1;
                break;
            case eBlockFace.BOTTOM:
                blockPosition.y -= 1;
                break;
            case eBlockFace.FRONT:
                blockPosition.z -= 1;
                break;
            case eBlockFace.BACK:
                blockPosition.z += 1;
                break;
            case eBlockFace.RIGHT:
                blockPosition.x += 1;
                break;
            case eBlockFace.LEFT:
                blockPosition.x -= 1;
                break;
        }

        #region Find the neighbour block (cause it might be placed on different chunk)
        if (blockPosition.x < 0)
            neighbourBlock = chunk.chunkNeighbours.LeftNeighbourChunk.chunkBlocks.GetBlock(chunkWidth - 1, blockPosition.y, blockPosition.z);
        else if (blockPosition.x > chunkWidth - 1)
            neighbourBlock = chunk.chunkNeighbours.RightNeighbourChunk.chunkBlocks.GetBlock(0, blockPosition.y, blockPosition.z);
        else if (blockPosition.z < 0)
            neighbourBlock = chunk.chunkNeighbours.FrontNeighbourChunk.chunkBlocks.GetBlock(blockPosition.x, blockPosition.y, chunkWidth - 1);
        else if (blockPosition.z > chunkWidth - 1)
            neighbourBlock = chunk.chunkNeighbours.BackNeighbourChunk.chunkBlocks.GetBlock(blockPosition.x, blockPosition.y, 0);
        else if (blockPosition.y < 0)
            return false;
        else if (blockPosition.y > chunkHeight - 1)
            return true;
        else
            neighbourBlock = chunk.chunkBlocks.GetBlock(blockPosition.x, blockPosition.y, blockPosition.z);
        #endregion

        if (neighbourBlock == null)
            return true;

     //   Debug.Log("HIHO");

        if (neighbourBlock.BlockData.isRendered)
        {
            if (neighbourBlock.BlockData.isTransparent)
            {
                if (thisBlock.BlockData.isTransparent && thisBlock.BlockData.id == neighbourBlock.BlockData.id)
                    return false;

                return true;
            }

            return false;
        }
        else
        {
            return true;
        }
    }

    private bool IsLiquidFaceVisible (Vector3Int blockPosition, eBlockFace blockFace)
    {
        Block neighbourBlock;
        byte neighbourWater;

        switch (blockFace)
        {
            case eBlockFace.TOP:
                blockPosition.y += 1;
                break;
            case eBlockFace.BOTTOM:
                blockPosition.y -= 1;
                break;
            case eBlockFace.FRONT:
                blockPosition.z -= 1;
                break;
            case eBlockFace.BACK:
                blockPosition.z += 1;
                break;
            case eBlockFace.RIGHT:
                blockPosition.x += 1;
                break;
            case eBlockFace.LEFT:
                blockPosition.x -= 1;
                break;
        }

        #region Find the neighbour block (cause it might be placed on different chunk)
        if (blockPosition.x < 0)
        {
            neighbourBlock = chunk.chunkNeighbours.LeftNeighbourChunk.chunkBlocks.GetBlock(chunkWidth - 1, blockPosition.y, blockPosition.z);
            neighbourWater = chunk.chunkNeighbours.LeftNeighbourChunk.chunkLiquids.water[chunkWidth - 1, blockPosition.y, blockPosition.z];
        }
        else if (blockPosition.x > chunkWidth - 1)
        {
            neighbourBlock = chunk.chunkNeighbours.RightNeighbourChunk.chunkBlocks.GetBlock(0, blockPosition.y, blockPosition.z);
            neighbourWater = chunk.chunkNeighbours.RightNeighbourChunk.chunkLiquids.water[0, blockPosition.y, blockPosition.z];
        }
        else if (blockPosition.z < 0)
        {
            neighbourBlock = chunk.chunkNeighbours.FrontNeighbourChunk.chunkBlocks.GetBlock(blockPosition.x, blockPosition.y, chunkWidth - 1);
            neighbourWater = chunk.chunkNeighbours.FrontNeighbourChunk.chunkLiquids.water[blockPosition.x, blockPosition.y, chunkWidth - 1];
        }
        else if (blockPosition.z > chunkWidth - 1)
        {
            neighbourBlock = chunk.chunkNeighbours.BackNeighbourChunk.chunkBlocks.GetBlock(blockPosition.x, blockPosition.y, 0);
            neighbourWater = chunk.chunkNeighbours.BackNeighbourChunk.chunkLiquids.water[blockPosition.x, blockPosition.y, 0];
        }
        else if (blockPosition.y < 0)
            return false;
        else if (blockPosition.y > chunkHeight - 1)
            return true;
        else
        {
            neighbourBlock = chunk.chunkBlocks.GetBlock(blockPosition.x, blockPosition.y, blockPosition.z);
            neighbourWater = chunk.chunkLiquids.water[blockPosition.x, blockPosition.y, blockPosition.z];
        }
        #endregion

        if (neighbourBlock == null)
        {
            if (neighbourWater == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (neighbourBlock.BlockData.isTransparent)
                return true;
            else
                return false;
        }

    }

    /// <summary>
    /// How much darker should the face be than the top face (percentage int 0-100), this value is passed to fragment shader.
    /// </summary>
    /// <param name="blockFace"></param>
    /// <returns></returns>
    private int GetFaceDarknessPercentage (eBlockFace blockFace)
    {
        switch (blockFace)
        {
            case eBlockFace.TOP:
                return 0;
            case eBlockFace.BOTTOM:
                return 60;
            case eBlockFace.FRONT:
            case eBlockFace.BACK:
                return 40;
            case eBlockFace.RIGHT:
            case eBlockFace.LEFT:
                return 20;
            default:
                return 100;
        }
    }
}


/*
    /// <summary>
    /// Recalculates position of vertices, triangles, uv map, vertex colors.
    /// </summary>
    private void CalculateMeshData ()
    {
        meshVertices.Clear();
        meshTriangles.Clear();
        meshUV.Clear();
        meshLight.Clear();

        float uvSmallFixValue = 0.001f;
        float tilePerc = 1f / chunk.pixelSize;

        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
                for (int y = subChunkHeightStart; y < subChunkHeightEnd; y++)
                {
                    Block thisBlock = chunk.blocks[x, y, z];

                    if (thisBlock == null)
                        continue;

                    if (!thisBlock.BlockData.isRendered)
                        continue;

                    for (int i = 0; i < 6; i++)
                    {
                        eBlockFace thisFace = (eBlockFace)i;
                        int currentVerticesCount = meshVertices.Count;

                        if (IsFaceVisible(new Vector3Int(x, y, z), thisFace))
                        {
                            Vector2 faceTextureID = thisBlock.GetTextureForFace(thisFace);
                            float uMin = faceTextureID.x * tilePerc + uvSmallFixValue;
                            float vMin = faceTextureID.y * tilePerc + uvSmallFixValue;
                            float uMax = (faceTextureID.x + 1) * tilePerc - uvSmallFixValue;
                            float vMax = (faceTextureID.y + 1) * tilePerc - uvSmallFixValue;

                            Color blockColor = new Color(0, GetFaceDarknessPercentage(thisFace), chunk.chunkLight.GetVoxelFaceArtificialLightIntensity(new Vector3Int(x, y, z), thisFace), chunk.chunkLight.GetVoxelFaceNaturalLightIntensity(new Vector3Int(x, y, z), thisFace));
                            meshLight.AddRange(new Color[] { blockColor, blockColor, blockColor, blockColor });

                            switch (thisFace)
                            {
                                case eBlockFace.FRONT:
                                    meshVertices.AddRange(new Vector3[] { new Vector3(x * blockSize, y * blockSize, z * blockSize), new Vector3((x + 1) * blockSize, y * blockSize, z * blockSize), new Vector3(x * blockSize, (y + 1) * blockSize, z * blockSize), new Vector3((x + 1) * blockSize, (y + 1) * blockSize, z * blockSize) });
                                    meshTriangles.AddRange(new int[] { 0 + currentVerticesCount, 2 + currentVerticesCount, 1 + currentVerticesCount, 2 + currentVerticesCount, 3 + currentVerticesCount, 1 + currentVerticesCount });
                                    meshUV.AddRange(new Vector2[] { new Vector2(uMin, vMin), new Vector2(uMax, vMin), new Vector2(uMin, vMax), new Vector2(uMax, vMax) });
                                    break;
                                case eBlockFace.BACK:
                                    meshVertices.AddRange(new Vector3[] { new Vector3(x * blockSize, y * blockSize, (z + 1) * blockSize), new Vector3((x + 1) * blockSize, y * blockSize, (z + 1) * blockSize), new Vector3(x * blockSize, (y + 1) * blockSize, (z + 1) * blockSize), new Vector3((x + 1) * blockSize, (y + 1) * blockSize, (z + 1) * blockSize) });
                                    meshTriangles.AddRange(new int[] { 1 + currentVerticesCount, 2 + currentVerticesCount, 0 + currentVerticesCount, 1 + currentVerticesCount, 3 + currentVerticesCount, 2 + currentVerticesCount });
                                    meshUV.AddRange(new Vector2[] { new Vector2(uMin, vMin), new Vector2(uMax, vMin), new Vector2(uMin, vMax), new Vector2(uMax, vMax) });
                                    break;
                                case eBlockFace.RIGHT:
                                    meshVertices.AddRange(new Vector3[] { new Vector3((x + 1) * blockSize, y * blockSize, z * blockSize), new Vector3((x + 1) * blockSize, (y + 1) * blockSize, z * blockSize), new Vector3((x + 1) * blockSize, y * blockSize, (z + 1) * blockSize), new Vector3((x + 1) * blockSize, (y + 1) * blockSize, (z + 1) * blockSize) });
                                    meshTriangles.AddRange(new int[] { 1 + currentVerticesCount, 2 + currentVerticesCount, 0 + currentVerticesCount, 1 + currentVerticesCount, 3 + currentVerticesCount, 2 + currentVerticesCount });
                                    meshUV.AddRange(new Vector2[] { new Vector2(uMin, vMin), new Vector2(uMin, vMax), new Vector2(uMax, vMin), new Vector2(uMax, vMax) });
                                    break;
                                case eBlockFace.LEFT:
                                    meshVertices.AddRange(new Vector3[] { new Vector3(x * blockSize, y * blockSize, z * blockSize), new Vector3(x * blockSize, (y + 1) * blockSize, z * blockSize), new Vector3(x * blockSize, y * blockSize, (z + 1) * blockSize), new Vector3(x * blockSize, (y + 1) * blockSize, (z + 1) * blockSize) });
                                    meshTriangles.AddRange(new int[] { 0 + currentVerticesCount, 2 + currentVerticesCount, 1 + currentVerticesCount, 2 + currentVerticesCount, 3 + currentVerticesCount, 1 + currentVerticesCount });
                                    meshUV.AddRange(new Vector2[] { new Vector2(uMin, vMin), new Vector2(uMin, vMax), new Vector2(uMax, vMin), new Vector2(uMax, vMax) });
                                    break;
                                case eBlockFace.TOP:
                                    meshVertices.AddRange(new Vector3[] { new Vector3(x * blockSize, (y + 1) * blockSize, z * blockSize), new Vector3(x * blockSize, (y + 1) * blockSize, (z + 1) * blockSize), new Vector3((x + 1) * blockSize, (y + 1) * blockSize, z * blockSize), new Vector3((x + 1) * blockSize, (y + 1) * blockSize, (z + 1) * blockSize) });
                                    meshTriangles.AddRange(new int[] { 1 + currentVerticesCount, 2 + currentVerticesCount, 0 + currentVerticesCount, 1 + currentVerticesCount, 3 + currentVerticesCount, 2 + currentVerticesCount });
                                    meshUV.AddRange(new Vector2[] { new Vector2(uMin, vMin), new Vector2(uMin, vMax), new Vector2(uMax, vMin), new Vector2(uMax, vMax) });
                                    break;
                                case eBlockFace.BOTTOM:
                                    meshVertices.AddRange(new Vector3[] { new Vector3(x * blockSize, y * blockSize, z * blockSize), new Vector3(x * blockSize, y * blockSize, (z + 1) * blockSize), new Vector3((x + 1) * blockSize, y * blockSize, z * blockSize), new Vector3((x + 1) * blockSize, y * blockSize, (z + 1) * blockSize) });
                                    meshTriangles.AddRange(new int[] { 0 + currentVerticesCount, 2 + currentVerticesCount, 1 + currentVerticesCount, 2 + currentVerticesCount, 3 + currentVerticesCount, 1 + currentVerticesCount });
                                    meshUV.AddRange(new Vector2[] { new Vector2(uMin, vMin), new Vector2(uMin, vMax), new Vector2(uMax, vMin), new Vector2(uMax, vMax) });
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check if we should render block face on given position based on block's neighbour.
    /// </summary>
    /// <param name="blockPosition"></param>
    /// <param name="blockFace"></param>
    /// <returns></returns>
    private bool IsFaceVisible (Vector3Int blockPosition, eBlockFace blockFace)
    {
        Block thisBlock = chunk.blocks[blockPosition.x, blockPosition.y, blockPosition.z];
        Block neighbourBlock;

        switch (blockFace)
        {
            case eBlockFace.TOP:
                blockPosition.y += 1;
                break;
            case eBlockFace.BOTTOM:
                blockPosition.y -= 1;
                break;
            case eBlockFace.FRONT:
                blockPosition.z -= 1;
                break;
            case eBlockFace.BACK:
                blockPosition.z += 1;
                break;
            case eBlockFace.RIGHT:
                blockPosition.x += 1;
                break;
            case eBlockFace.LEFT:
                blockPosition.x -= 1;
                break;
        }

        #region Find the neighbour block (cause it might be placed on different chunk)
        if (blockPosition.x < 0)
            neighbourBlock = chunk.chunkNeighbours.LeftNeighbourChunk.blocks[chunkWidth - 1, blockPosition.y, blockPosition.z];
        else if (blockPosition.x > chunkWidth - 1)
            neighbourBlock = chunk.chunkNeighbours.RightNeighbourChunk.blocks[0, blockPosition.y, blockPosition.z];
        else if (blockPosition.z < 0)
            neighbourBlock = chunk.chunkNeighbours.FrontNeighbourChunk.blocks[blockPosition.x, blockPosition.y, chunkWidth - 1];
        else if (blockPosition.z > chunkWidth - 1)
            neighbourBlock = chunk.chunkNeighbours.BackNeighbourChunk.blocks[blockPosition.x, blockPosition.y, 0];
        else if (blockPosition.y < 0)
            return false;
        else if (blockPosition.y > chunkHeight - 1)
            return true;
        else
            neighbourBlock = chunk.blocks[blockPosition.x, blockPosition.y, blockPosition.z];
        #endregion

        if (neighbourBlock == null)
            return true;

        if (neighbourBlock.BlockData.isRendered)
        {
            if (neighbourBlock.BlockData.isTransparent)
            {
                if (thisBlock.BlockData.isTransparent && thisBlock.BlockData.id == neighbourBlock.BlockData.id)
                    return false;

                return true;
            }

            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// How much darker should the face be than the top face (percentage int 0-100), this value is passed to fragment shader.
    /// </summary>
    /// <param name="blockFace"></param>
    /// <returns></returns>
    private int GetFaceDarknessPercentage (eBlockFace blockFace)
    {
        switch (blockFace)
        {
            case eBlockFace.TOP:
                return 0;
            case eBlockFace.BOTTOM:
                return 60;
            case eBlockFace.FRONT:
            case eBlockFace.BACK:
                return 40;
            case eBlockFace.RIGHT:
            case eBlockFace.LEFT:
                return 20;
            default:
                return 100;
        }
    }
*/