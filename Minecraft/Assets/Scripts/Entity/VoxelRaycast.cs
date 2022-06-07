using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelRaycast
{
    public static VoxelRaycastInfo CastRay (Vector3 from, Vector3 to)
    {
        float distance = Vector3.Distance(from, to);
        int pointCount = ((int)distance + 1) * 20;

        Vector3 difference = new Vector3(to.x - from.x, to.y - from.y, to.z - from.z);
        Vector3 interval = new Vector3(difference.x / (pointCount + 1), difference.y / (pointCount + 1), difference.z / (pointCount + 1));

    //    Debug.Log("Start position: " + from);
    //    Debug.Log("End position: " + to);

        for (int i = 0; i <= pointCount; i++)
        {
            Vector3 foundPosition = new Vector3(from.x + interval.x * i, from.y + interval.y * i, from.z + interval.z * i);
            Vector3Int foundPositionInt = Map.singleton.Vector3ToVector3Int(foundPosition);

            Block foundBlock = Map.singleton.GetBlock(foundPosition);

            if (foundBlock != null)
                return new VoxelRaycastInfo(foundPositionInt, foundBlock);

         //   Debug.Log("Found position: " + foundPosition);
        }
        

        return null;
    }

    public class VoxelRaycastInfo
    {
        public Vector3Int VoxelPosition { get; private set; }
        public Block Voxel { get; private set; }

        public VoxelRaycastInfo (Vector3Int pos, Block block)
        {
            VoxelPosition = pos;
            Voxel = block;
        }
    }

}
