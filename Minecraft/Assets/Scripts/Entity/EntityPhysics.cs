using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EntityCollider))]
public class EntityPhysics : MonoBehaviour
{
    [SerializeField] private float gravityScale = 31.36f;
    public Vector3 Velocity;
    private EntityCollider entityCollider;
    private Transform _transform; // caching transform is slightly faster

    public bool IsGrounded 
    { 
        get
        {
            return Velocity.y > -0.01f && Velocity.y < 0.01f;
        } 
    }


    private void Awake ()
    {
        _transform = transform;
        entityCollider = GetComponent<EntityCollider>();
        entityCollider.OnCollisionDetected += (s, e) => OnCollisionDetected(e);
     //   SetVelocity(new Vector3(0, -gravityScale, 0));
    }

    private void Update ()
    {
      //  Velocity.y -= gravityScale * Time.deltaTime;
        _transform.position += Velocity * Time.deltaTime;
    }

    public void SetVelocity (Vector3 velocity)
    {
        ClampVelocity(ref velocity);
        Velocity = velocity;
    }

    public void SetVelocityIgnoreY (Vector3 velocity)
    {
        velocity.y = Velocity.y - gravityScale * Time.deltaTime;
        ClampVelocity(ref velocity);
        Velocity = velocity;
    }

    /// <summary>
    /// Makes sure that velocity is not higher than max gravity value.
    /// </summary>
    private void ClampVelocity (ref Vector3 velocity)
    {
        if (velocity.x > gravityScale) velocity.x = gravityScale;
        if (velocity.x < -gravityScale) velocity.x = -gravityScale;
        if (velocity.y > gravityScale) velocity.y = gravityScale;
        if (velocity.y < -gravityScale) velocity.y = -gravityScale;
        if (velocity.z > gravityScale) velocity.z = gravityScale;
        if (velocity.z < -gravityScale) velocity.z = -gravityScale;
    }

    /// <summary>
    /// Might be called multiple times per frame (depends on found collision count).
    /// </summary>
    /// <param name="data">Received data about collision.</param>
    private void OnCollisionDetected (DetectedCollisionData data)
    {
        if (Velocity.y < 0 && data.CollisionNormal.y == 1)
        {
            float y = data.OtherColliderBoundaries.MaxY + (entityCollider.Size.y / 2) - entityCollider.Center.y;
            _transform.position = new Vector3(_transform.position.x, y, _transform.position.z);
            SetVelocity(new Vector3(Velocity.x, 0, Velocity.z));
        }

        if (Velocity.y > 0 && data.CollisionNormal.y == -1)
        {
            float y = data.OtherColliderBoundaries.MinY - (entityCollider.Size.y / 2) - entityCollider.Center.y;
            _transform.position = new Vector3(_transform.position.x, y, _transform.position.z);
            SetVelocity(new Vector3(Velocity.x, 0, Velocity.z));
        }

        if (Velocity.x < 0 && data.CollisionNormal.x == -1)
        {
            float x = data.OtherColliderBoundaries.MaxX + (entityCollider.Size.x / 2) - entityCollider.Center.x;
            _transform.position = new Vector3(x, _transform.position.y, _transform.position.z);
            SetVelocity(new Vector3(0, Velocity.y, Velocity.z));
        }

        if (Velocity.x > 0 && data.CollisionNormal.x == 1)
        {
            float x = data.OtherColliderBoundaries.MinX - (entityCollider.Size.x / 2) - entityCollider.Center.x;
            _transform.position = new Vector3(x, _transform.position.y, _transform.position.z);
            SetVelocity(new Vector3(0, Velocity.y, Velocity.z));
        }

        if (Velocity.z < 0 && data.CollisionNormal.z == -1)
        {
            float z = data.OtherColliderBoundaries.MaxZ + (entityCollider.Size.z / 2) - entityCollider.Center.z;
            _transform.position = new Vector3(_transform.position.x, _transform.position.y, z);
            SetVelocity(new Vector3(Velocity.x, Velocity.y, 0));
        }

        if (Velocity.z > 0 && data.CollisionNormal.z == 1)
        {
            float z = data.OtherColliderBoundaries.MinZ - (entityCollider.Size.z / 2) - entityCollider.Center.z;
            _transform.position = new Vector3(_transform.position.x, _transform.position.y, z);
            SetVelocity(new Vector3(Velocity.x, Velocity.y, 0));
        }
    }
}
