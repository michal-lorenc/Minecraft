using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EntityPhysics))]
public class EntityMovement : MonoBehaviour
{
    [SerializeField] private new Camera camera;
    private EntityPhysics entityPhysics;
    private float currentSpeed = 5.0f;
    private Vector3 input = new Vector3();

    private void Awake ()
    {
        entityPhysics = GetComponent<EntityPhysics>();
    }

    private void Update ()
    {
        input.x = Input.GetAxis("Horizontal");
        input.y = 0;
        input.z = Input.GetAxis("Vertical");

        Vector3 playerVelocity = Quaternion.Euler(0, camera.transform.rotation.eulerAngles.y, 0) * input * currentSpeed;

        if (Input.GetButton("Jump") && entityPhysics.IsGrounded && GameManager.singleton.gameState == eGameState.GAMEPLAY)
        {
            playerVelocity.y = Mathf.Sqrt(1f * -3.0f * -31.26f);
            entityPhysics.SetVelocity(playerVelocity);
        }
        else
        {
            entityPhysics.SetVelocityIgnoreY(playerVelocity);
        }
    }
}
