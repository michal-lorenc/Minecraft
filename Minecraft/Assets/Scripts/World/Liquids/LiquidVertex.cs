using UnityEngine;

public class LiquidVertex
{
    private readonly Chunk chunk;

    public LiquidVertex (Chunk chunk)
    {
        this.chunk = chunk;
    }

    public LiquidVertexData GetLiquidVertices (Vector3Int position, int liquidValue)
    {
        byte[] liquidNeighbours = new byte[]
        {
            chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y, position.z + 1)),
            chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y, position.z + 1)),
            chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z + 1)),
            chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z)),
            chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x + 1, position.y, position.z - 1)),
            chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x, position.y, position.z - 1)),
            chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y, position.z - 1)),
            chunk.chunkNeighbours.GetWaterValueCrossChunkSafe(new Vector3Int(position.x - 1, position.y, position.z)),
        };

        float a = 0, b = 0, c = 0, d = 0;

        // top-left corner
        // if this liquid level is higher than liquid levels of neighbours to this corner
        if (liquidValue > liquidNeighbours[0] || liquidValue > liquidNeighbours[1] || liquidValue > liquidNeighbours[7])
        {
            a = GetLiquidMinValue(GetLowestValue(liquidNeighbours[0], liquidNeighbours[1], liquidNeighbours[7]));
        }
        else
        {
            a = GetLiquidMaxValue(GetLowestValue(liquidNeighbours[0], liquidNeighbours[1], liquidNeighbours[7]));
        }

        if (liquidValue > liquidNeighbours[1] || liquidValue > liquidNeighbours[2] || liquidValue > liquidNeighbours[3])
        {
            b = GetLiquidMinValue(GetLowestValue(liquidNeighbours[1], liquidNeighbours[2], liquidNeighbours[3]));
        }
        else
        {
            b = GetLiquidMaxValue(GetLowestValue(liquidNeighbours[1], liquidNeighbours[2], liquidNeighbours[3]));
        }

        if (liquidValue > liquidNeighbours[3] || liquidValue > liquidNeighbours[4] || liquidValue > liquidNeighbours[5])
        {
            c = GetLiquidMinValue(GetLowestValue(liquidNeighbours[3], liquidNeighbours[4], liquidNeighbours[5]));
        }
        else
        {
            c = GetLiquidMaxValue(GetLowestValue(liquidNeighbours[3], liquidNeighbours[4], liquidNeighbours[5]));
        }

        if (liquidValue > liquidNeighbours[5] || liquidValue > liquidNeighbours[6] || liquidValue > liquidNeighbours[7])
        {
            d = GetLiquidMinValue(GetLowestValue(liquidNeighbours[5], liquidNeighbours[6], liquidNeighbours[7]));
        }
        else
        {
            d = GetLiquidMaxValue(GetLowestValue(liquidNeighbours[5], liquidNeighbours[6], liquidNeighbours[7]));
        }

        return new LiquidVertexData(a, b, c, d);
    }

    public float GetLiquidMaxValue (byte liquidID)
    {
        switch (liquidID)
        {
            case 1:
                return 0.8f;
            case 2:
                return 0.8f;
            case 3:
                return 0.7f;
            case 4:
                return 0.6f;
            case 5:
                return 0.5f;
            case 6:
                return 0.4f;
            case 7:
                return 0.3f;
            case 8:
                return 0.2f;
            default:
                return 0.0f;
        }
    }

    public float GetLiquidMinValue (byte liquidID)
    {
        switch (liquidID)
        {
            case 1:
                return 0.8f;
            case 2:
                return 0.7f;
            case 3:
                return 0.6f;
            case 4:
                return 0.5f;
            case 5:
                return 0.4f;
            case 6:
                return 0.3f;
            case 7:
                return 0.2f;
            case 8:
                return 0.1f;
            default:
                return 0.0f;
        }
    }

    private byte GetLowestValue (byte a, byte b, byte c)
    {
        if (a <= b && a <= c)
            return a;

        if (b <= a && b <= c)
            return b;

        if (c <= a && c <= b)
            return c;

        return a;
    }

    private byte GetHighestValue(byte a, byte b, byte c)
    {
        if (a >= b && a >= c)
            return a;

        if (b >= a && b >= c)
            return b;

        if (c >= a && c >= b)
            return c;

        return a;
    }
}
