using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private Vector3 input = new Vector3();

    private float currentSpeed;
    private float currentGravityValue;
    private Vector3 playerVelocity;

    private CharacterController characterController;
    private Camera camera;
    private CameraController cameraController;


    public readonly float crouchSpeed = 1.31f;
    public readonly float walkSpeed = 4.3f;
    public readonly float runSpeed = 5.61f;
    private readonly float jumpHeight = 0.75f;
    private readonly float gravityValue = -31.36f;
    private readonly float inWaterGravityValue = -3.27f;


    private bool isGrounded { get { return IsGroundedCheck(); } }
    private bool isInWater { set { if (value) currentGravityValue = inWaterGravityValue; else currentGravityValue = gravityValue; } }
    private bool isSprinting { set { if (value == true) { currentSpeed = runSpeed; cameraController.SetCameraFOV(70, true, 10); }  else { currentSpeed = walkSpeed; cameraController.SetCameraFOV(60, true, 5); } } }
    private bool isFlyingModeEnabled = false;
    private bool isGodModeEnabled = true;

    private DateTime lastTimePressedW = DateTime.Now;
    private DateTime lastTimePressedSpace = DateTime.Now;
    private DateTime currentTime = DateTime.Now;

    private void Awake ()
    {
        currentSpeed = walkSpeed;
        currentGravityValue = gravityValue;

        characterController = GetComponent<CharacterController>();
        cameraController = GetComponentInChildren<CameraController>();
        camera = Camera.main;
    }


    private void Update ()
    {
        if (GameManager.singleton.gameState == eGameState.GAMEPLAY)
        {
            input.x = Input.GetAxis("Horizontal");
            input.y = 0;
            input.z = Input.GetAxis("Vertical");

            DoubleClickControls();
        }
        else
        {
            input.x = 0;
            input.y = 0;
            input.z = 0;
        }

        CalculateMovement();
    }

    private void DoubleClickControls ()
    {
        currentTime = DateTime.Now;

        if (Input.GetKeyDown(KeyCode.W))
        {
            Double elapsedMillisecs = ((TimeSpan)(currentTime - lastTimePressedW)).TotalMilliseconds;
            if (elapsedMillisecs < 250)
                isSprinting = true;

            lastTimePressedW = currentTime;
        }
        else if (Input.GetKeyUp(KeyCode.W))
        {
            Double elapsedMillisecs = ((TimeSpan)(currentTime - lastTimePressedW)).TotalMilliseconds;
            if (elapsedMillisecs > 250)
                isSprinting = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGodModeEnabled)
        {
            Double elapsedMillisecs = ((TimeSpan)(currentTime - lastTimePressedSpace)).TotalMilliseconds;
            if (elapsedMillisecs < 250)
                isFlyingModeEnabled = !isFlyingModeEnabled;

            lastTimePressedSpace = currentTime;
        }
    }

    private void CalculateMovement ()
    {
        if (isFlyingModeEnabled)
            FlyingMovement();
        else
            WalkingMovement();
    }

    private void FlyingMovement ()
    {
        playerVelocity = Quaternion.Euler(0, camera.transform.rotation.eulerAngles.y, 0) * input * currentSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.Space) && GameManager.singleton.gameState == eGameState.GAMEPLAY)
        {
            playerVelocity.y += currentSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftShift) && GameManager.singleton.gameState == eGameState.GAMEPLAY)
        {
            playerVelocity.y -= currentSpeed * Time.deltaTime;

            if (isGrounded)
                isFlyingModeEnabled = false;
        }

        characterController.Move(playerVelocity);
    }

    private void WalkingMovement ()
    {
        bool groundedPlayer;

      //  groundedPlayer = characterController.isGrounded;
        groundedPlayer = isGrounded;

        if (groundedPlayer)
        {
          //  CrouchingMovement();
        }

        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        } 

        Vector3 move = Quaternion.Euler(0, camera.transform.rotation.eulerAngles.y, 0) * input * currentSpeed * Time.deltaTime;
        characterController.Move(move);

        // Changes the height position of the player..
        if (Input.GetButton("Jump") && groundedPlayer && GameManager.singleton.gameState == eGameState.GAMEPLAY)
        {
            playerVelocity.y = 0;
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * currentGravityValue);
        }
        if (!groundedPlayer)
            playerVelocity.y += currentGravityValue * Time.deltaTime;

        characterController.Move(playerVelocity * Time.deltaTime);
    }


    public LayerMask layerMask;
    private Vector3[] crouchFakeColliders = new Vector3[]
    {
        new Vector3(0, 0, 0),
        new Vector3(0, 0, 0),
        new Vector3(0, 0, 0),
        new Vector3(0, 0, 0)
    };
    public GameObject[] crouchColliderObjects;

    private void CrouchingMovement ()
    {
        RaycastHit hit;
        int distanceDivider = 4;

        if (Physics.Raycast(transform.position + (transform.forward / distanceDivider), -transform.up, out hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(transform.position + (transform.forward / distanceDivider), -transform.up * hit.distance, Color.yellow);

            if (hit.distance > 1.2f)
                SetColliderPosition(0);
            else
                SetColliderActive(0, false);

            //   if (input.z > 0 && hit.distance > 1.2f)
            //        input.z = 0;
        }
        else
            SetColliderActive(0, false);

        if (Physics.Raycast(transform.position - (transform.forward / distanceDivider), -transform.up, out hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(transform.position - (transform.forward / distanceDivider), -transform.up * hit.distance, Color.yellow);

            if (hit.distance > 1.2f)
                SetColliderPosition(1);
            else
                SetColliderActive(1, false);

            //    if (input.z < 0 && hit.distance > 1.2f)
            //        input.z = 0;
        }
        else
            SetColliderActive(1, false);

        if (Physics.Raycast(transform.position + (transform.right / distanceDivider), -transform.up, out hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(transform.position + (transform.right / distanceDivider), -transform.up * hit.distance, Color.yellow);

            if (hit.distance > 1.2f)
                SetColliderPosition(2);
            else
                SetColliderActive(2, false);
            //    if (input.x > 0 && hit.distance > 1.2f)
            //       input.x = 0;
        }
        else
            SetColliderActive(2, false);

        if (Physics.Raycast(transform.position - (transform.right / distanceDivider), -transform.up, out hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(transform.position - (transform.right / distanceDivider), -transform.up * hit.distance, Color.yellow);

            if (hit.distance > 1.2f)
                SetColliderPosition(3);
            else
                SetColliderActive(3, false);

            //    if (input.x < 0 && hit.distance > 1.2f)
            //        input.x = 0;
        }
        else
            SetColliderActive(3, false);


        void SetColliderPosition (int id)
        {
            crouchFakeColliders[id] = hit.point - hit.normal / 2f;
            crouchFakeColliders[id].x = Mathf.Floor(crouchFakeColliders[id].x) + 0.5f;
            crouchFakeColliders[id].y = Mathf.Floor(transform.position.y) + 0.5f;
            crouchFakeColliders[id].z = Mathf.Floor(crouchFakeColliders[id].z) + 0.5f;

            SetColliderActive(id, true);
        }

        void SetColliderActive (int id, bool active)
        {
            if (active)
                crouchColliderObjects[id].transform.position = crouchFakeColliders[id];

            crouchColliderObjects[id].SetActive(active);
        }
    }


    private RaycastHit groundHit;
    private bool IsGroundedCheck ()
    {
        return characterController.isGrounded;

        if (Physics.SphereCast(transform.position, characterController.radius + 0.01f, -transform.up, out groundHit, 1))
        {
   //         Debug.Log("GROUNDED!");
            return true;
        }

     //   Debug.Log(" NOT GROUNDED :P");
        return false;
    }

    public void OnTriggerEnter (Collider other)
    {
        Debug.Log("Triggered!!! " + other.tag);
    }

}
