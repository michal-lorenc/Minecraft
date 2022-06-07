using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float cameraOffset3rdPerson = -5;

    public eCameraMode cameraMode;
    public LayerMask firstPersonRenderLayers;
    public LayerMask thirdPersonRenderLayers;
    [SerializeField]
    private Transform headJoint;
    [SerializeField]
    private float mouseSensitivity = 10f;
    private float xRotation = 0.0f;
    private float yRotation = 0.0f;
    private float mouseX;
    private float mouseY;
    private Camera cam;

    private void Awake ()
    {
        cam = GetComponent<Camera>();
        ChangeCameraMode(eCameraMode.FIRST_PERSON);
    }

    public void SwitchCameraMode ()
    {
        if (cameraMode == eCameraMode.FIRST_PERSON)
            ChangeCameraMode(eCameraMode.THIRD_PERSON);
        else
            ChangeCameraMode(eCameraMode.FIRST_PERSON);
    }

    public void ChangeCameraMode (eCameraMode cameraMode)
    {
        if (this.cameraMode == cameraMode)
            return;

        this.cameraMode = cameraMode;

        transform.localPosition = new Vector3(0, 0, cameraMode == eCameraMode.FIRST_PERSON ? 0 : cameraOffset3rdPerson);
        cam.cullingMask = cameraMode == eCameraMode.FIRST_PERSON ? firstPersonRenderLayers : thirdPersonRenderLayers;
    }

    public void SetCameraFOV (float newFOV, bool smooth = true, float smoothSpeedMultiplier = 1)
    {
        if (smooth)
        {
            if (SmoothFOVTransitionIE != null)
                StopCoroutine(SmoothFOVTransitionIE);

            SmoothFOVTransitionIE = SmoothFOVTransition(cam.fieldOfView, newFOV, smoothSpeedMultiplier);
            StartCoroutine(SmoothFOVTransitionIE);
        }
        else
        {
            cam.fieldOfView = newFOV;
        }
    }

    private IEnumerator SmoothFOVTransitionIE;
    private IEnumerator SmoothFOVTransition (float fromFOV, float toFOV, float speedMultiplier = 1)
    {
        float progress = 0;

        while (progress < 1)
        {
            progress += Time.deltaTime * speedMultiplier;
            cam.fieldOfView = Mathf.Lerp(fromFOV, toFOV, progress);
            yield return null;
        }
    }

    private void LateUpdate ()
    {
        if (GameManager.singleton.gameState == eGameState.GAMEPLAY)
        {
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        }
        else
        {
            mouseX = 0;
            mouseY = 0;
        }

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -89, 89);

        headJoint.transform.eulerAngles = new Vector3(xRotation, yRotation, 0.0f);
    }
}

public enum eCameraMode
{
    FIRST_PERSON = 0,
    THIRD_PERSON = 1
}
