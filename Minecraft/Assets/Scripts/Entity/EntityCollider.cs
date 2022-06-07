using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// AABB collision detection with voxel worlds.
/// </summary>
public class EntityCollider : MonoBehaviour
{
    public EventHandler<DetectedCollisionData> OnCollisionDetected;

    [field: SerializeField] public Vector3 Center { get; private set; }
    [field: SerializeField] public Vector3 Size { get; private set; }

    private float GetMinX ()
    {
        Vector3 centerPoint = _transform.position + Center;
        centerPoint.x -= Size.x / 2;
        return centerPoint.x;
    }

    private float GetMaxX ()
    {
        Vector3 centerPoint = _transform.position + Center;
        centerPoint.x += Size.x / 2;
        return centerPoint.x;
    }

    private float GetMinY ()
    {
        Vector3 centerPoint = _transform.position + Center;
        centerPoint.y -= Size.y / 2;
        return centerPoint.y;
    }

    private float GetMaxY ()
    {
        Vector3 centerPoint = _transform.position + Center;
        centerPoint.y += Size.y / 2;
        return centerPoint.y;
    }

    private float GetMinZ ()
    {
        Vector3 centerPoint = _transform.position + Center;
        centerPoint.z -= Size.z / 2;
        return centerPoint.z;
    }

    private float GetMaxZ ()
    {
        Vector3 centerPoint = _transform.position + Center;
        centerPoint.z += Size.z / 2;
        return centerPoint.z;
    }


    private Map _map;
    private Transform _transform; // caching transform is slightly faster
    private Dictionary<Vector3, Block> neighbourVoxelsData = new Dictionary<Vector3, Block>();


    private void Start ()
    {
        _map = Map.singleton;
        _transform = transform;
    }


    private void Update ()
    {
        GetNeighbourColliders();
        CheckCollisions();
    }

    private void GetNeighbourColliders ()
    {
        neighbourVoxelsData.Clear();

        int minX = (int)Math.Floor(GetMinX());
        int maxX = (int)Math.Floor(GetMaxX());

        int minY = (int)Math.Floor(GetMinY());
        int maxY = (int)Math.Floor(GetMaxY());

        int minZ = (int)Math.Floor(GetMinZ());
        int maxZ = (int)Math.Floor(GetMaxZ());

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    Vector3 blockPosition = new Vector3(x, y, z);
                    Block block = _map.GetBlock(blockPosition);

                    if (block != null)
                        neighbourVoxelsData.Add(blockPosition, block);
                }
            }
        }
    }

    private void CheckCollisions ()
    {
        foreach (var otherCollider in neighbourVoxelsData)
        {
            float oMinX = otherCollider.Key.x, oMinY = otherCollider.Key.y, oMinZ = otherCollider.Key.z;
            float oMaxX = oMinX + 1, oMaxY = oMinY + 1, oMaxZ = oMinZ + 1;

            ColliderBoundaries thisColliderBoundaries = new ColliderBoundaries(GetMinX(), GetMinY(), GetMinZ(), GetMaxX(), GetMaxY(), GetMaxZ());
            ColliderBoundaries otherColliderBoundaries = new ColliderBoundaries(oMinX, oMinY, oMinZ, oMaxX, oMaxY, oMaxZ);
            Vector3Int hitNormal;

            if (Intersect(thisColliderBoundaries, otherColliderBoundaries, out hitNormal) || Intersect(otherColliderBoundaries, thisColliderBoundaries, out hitNormal))
            {
                OnCollisionDetected?.Invoke(this, new DetectedCollisionData(thisColliderBoundaries, otherColliderBoundaries, hitNormal));
            }
        }   
    }

    private bool Intersect (ColliderBoundaries a, ColliderBoundaries b, out Vector3Int hitNormal)
    {
        bool xIntersecting = false, yIntersecting = false, zIntersecting = false;
        hitNormal = new Vector3Int(0, 0, 0);

        // X axis intersection check
        if ((a.MinX >= b.MinX && a.MinX <= b.MaxX) || (a.MaxX <= b.MaxX && a.MaxX >= b.MinX))
        {
            xIntersecting = true;
        }
        else return false;

        // Y axis intersection check
        if ((a.MinY >= b.MinY && a.MinY <= b.MaxY) || (a.MaxY <= b.MaxY && a.MaxY >= b.MinY))
        {
            yIntersecting = true;
        }
        else return false;

        // Z axis intersection check
        if ((a.MinZ >= b.MinZ && a.MinZ <= b.MaxZ) || (a.MaxZ <= b.MaxZ && a.MaxZ >= b.MinZ))
        {
            zIntersecting = true;
        }
        else return false;

        // Find collision normal
        if (xIntersecting && yIntersecting && zIntersecting)
        {
            float topNormalDistance = Math.Abs(a.MinY - b.MaxY);
            float botNormalDistance = Math.Abs(a.MaxY - b.MinY);

            float leftNormalDistance = Math.Abs(a.MinX - b.MaxX);
            float rightNormalDistance = Math.Abs(a.MaxX - b.MinX);

            float frontNormalDistance = Math.Abs(a.MinZ - b.MaxZ);
            float backNormalDistance = Math.Abs(a.MaxZ - b.MinZ);

            if (topNormalDistance <= botNormalDistance && topNormalDistance <= leftNormalDistance && topNormalDistance <= rightNormalDistance && topNormalDistance <= frontNormalDistance && topNormalDistance <= backNormalDistance)
                hitNormal = new Vector3Int(0, 1, 0);
            else if (botNormalDistance <= topNormalDistance && botNormalDistance <= leftNormalDistance && botNormalDistance <= rightNormalDistance && botNormalDistance <= frontNormalDistance && botNormalDistance <= backNormalDistance)
                hitNormal = new Vector3Int(0, -1, 0);
            else if (leftNormalDistance <= topNormalDistance && leftNormalDistance <= botNormalDistance && leftNormalDistance <= rightNormalDistance && leftNormalDistance <= frontNormalDistance && leftNormalDistance <= backNormalDistance)
                hitNormal = new Vector3Int(-1, 0, 0);
            else if (rightNormalDistance <= topNormalDistance && rightNormalDistance <= botNormalDistance && rightNormalDistance <= leftNormalDistance && rightNormalDistance <= frontNormalDistance && rightNormalDistance <= backNormalDistance)
                hitNormal = new Vector3Int(1, 0, 0);
            else if (frontNormalDistance <= topNormalDistance && frontNormalDistance <= botNormalDistance && frontNormalDistance <= leftNormalDistance && frontNormalDistance <= rightNormalDistance && frontNormalDistance <= backNormalDistance)
                hitNormal = new Vector3Int(0, 0, -1);
            else if (backNormalDistance <= topNormalDistance && backNormalDistance <= botNormalDistance && backNormalDistance <= leftNormalDistance && backNormalDistance <= rightNormalDistance && backNormalDistance <= frontNormalDistance)
                hitNormal = new Vector3Int(0, 0, 1);
            else
                Debug.Log("UNHANDLED EX");

            return true;
        }
        else
        {
            return false;
        }
    }
}


public struct ColliderBoundaries
{
    public float MinX { get; private set; }
    public float MinY { get; private set; }
    public float MinZ { get; private set; }

    public float MaxX { get; private set; }
    public float MaxY { get; private set; }
    public float MaxZ { get; private set; }

    public ColliderBoundaries(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
    {
        MinX = minX;
        MinY = minY;
        MinZ = minZ;

        MaxX = maxX;
        MaxY = maxY;
        MaxZ = maxZ;
    }
}

public struct DetectedCollisionData
{
    public ColliderBoundaries OurColliderBoundaries { get; private set; }
    public ColliderBoundaries OtherColliderBoundaries { get; private set; }
    public Vector3Int CollisionNormal { get; private set; }

    public DetectedCollisionData (ColliderBoundaries ourColliderBoundaries, ColliderBoundaries otherColliderBoundaries, Vector3Int hitNormal)
    {
        OurColliderBoundaries = ourColliderBoundaries;
        OtherColliderBoundaries = otherColliderBoundaries;

        CollisionNormal = hitNormal;
    }
}

/*    if (a.minX <= b.maxX && a.maxX >= b.minX) &&
(a.minY <= b.maxY && a.maxY >= b.minY) &&
(a.minZ <= b.maxZ && a.maxZ >= b.minZ)
{

}

if (_transform.position.x < player2.x + player2.width &&
    player1.x + player1.width > player2.x &&
    player1.y < player2.y + player2.height &&
    player1.y + player1.height > player2.y)
{
    Debug.Log("Collision detected.");
} */

/*

Vector3[] bottomPositions = new Vector3[5];

private void CheckBottom ()
{
    Vector3 bottomBorder = _transform.position + Center;
    bottomBorder.y -= Size.y;

    bottomPositions[0] = bottomBorder;
    bottomPositions[1] = new Vector3(bottomBorder.x - Size.x, bottomBorder.y, bottomBorder.z);
    bottomPositions[2] = new Vector3(bottomBorder.x + Size.x, bottomBorder.y, bottomBorder.z);
    bottomPositions[3] = new Vector3(bottomBorder.x, bottomBorder.y, bottomBorder.z + Size.z);
    bottomPositions[4] = new Vector3(bottomBorder.x, bottomBorder.y, bottomBorder.z - Size.z);

    IsCollidingBottom = false;

    for (int i = 0; i < 5; i++)
    {
        Block block = _map.GetBlock(bottomPositions[i]);

        if (block == null)
        {
        //    Debug.Log("not colliding.");
         //   IsCollidingBottom = false;
        }
        else
        {
            Debug.Log($"colliding with: {block.BlockData.name}");
            IsCollidingBottom = true;
        }
    }
} */