using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private Transform neckTransform;
    [SerializeField] private CameraController cameraController;

    private void Start ()
    {
        animator = GetComponent<Animator>();
    }

    private void Update ()
    {
        animator.SetFloat("X", Input.GetAxis("Horizontal"));
        animator.SetFloat("Y", Input.GetAxis("Vertical"));
        transform.rotation = Quaternion.Euler(0, cameraController.transform.rotation.eulerAngles.y, 0);

        neckTransform.rotation = cameraController.transform.rotation;
    }
}
