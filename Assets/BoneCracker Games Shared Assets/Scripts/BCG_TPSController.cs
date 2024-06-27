//----------------------------------------------
//            BCG Shared Assets
//
// Copyright © 2014 - 2021 BoneCracker Games
// https://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("BoneCracker Games/BCG Shared Assets Pro/Character Controllers/BCG TPS Controller")]
public class BCG_TPSController : MonoBehaviour {

    /// <summary>
    /// Maximum directional speed.
    /// </summary>
    public float speed = 100f;

    /// <summary>
    /// Input X.
    /// </summary>
    private float inputMovementX;

    /// <summary>
    /// Input Y.
    /// </summary>
    private float inputMovementY;

    /// <summary>
    /// Character camera.
    /// </summary>
    public Camera characterCamera;

    /// <summary>
    /// Calculate the current rotation angles for TPS mode.
    /// </summary>
    private Quaternion wantedRotation = Quaternion.identity;

    /// <summary>
    /// Orbit rotation.
    /// </summary>
    private Quaternion orbitRotation = Quaternion.identity;

    /// <summary>
    /// Target position.
    /// </summary>
    private Vector3 targetPosition = Vector3.zero;

    /// <summary>
    /// Orbit X and Y inputs.
    /// </summary>
    private float orbitX = 0f;
    private float orbitY = 0f;

    /// <summary>
    /// Minimum and maximum Orbit X, Y degrees.
    /// </summary>
    public float minOrbitY = -20f;
    public float maxOrbitY = 80f;

    /// <summary>
    /// Distance to the TPS Camera.
    /// </summary>
    public float distance = 5f;

    /// <summary>
    /// Height of the TPS Camera.
    /// </summary>
    public float height = 1.5f;

    /// <summary>
    /// Camera sensitivity.
    /// </summary>
    public float sensitivity = 5.0f;

    /// <summary>
    /// Camera smoothing.
    /// </summary>
    public float smoothing = 2.0f;

    /// <summary>
    /// Inputs to control the player by feeding inputMovementX and inputMovementY.
    /// </summary>
    public BCG_Inputs inputs;

    private void Update() {

        Inputs();
        Camera();

    }

    private void FixedUpdate() {

        Controller();

    }

    private void Inputs() {

        inputs = BCG_InputManager.Instance.GetInputs();

        if (!BCG_EnterExitSettings.Instance.mobileController) {

            //	X and Y inputs based "Vertical" and "Horizontal" axes.
            inputMovementY = inputs.verticalInput * speed * .02f;
            inputMovementX = inputs.horizonalInput * speed * .02f;

            orbitX += inputs.aim.x;
            orbitY -= inputs.aim.y;

            // Clamping Y.
            orbitY = Mathf.Clamp(orbitY, minOrbitY, maxOrbitY);

            if (orbitX < -360f)
                orbitX += 360f;
            if (orbitX > 360f)
                orbitX -= 360f;

        } else {

            inputMovementY = BCG_MobileCharacterController.move.y * speed * .02f;
            inputMovementX = BCG_MobileCharacterController.move.x * speed * .02f;

            orbitX += BCG_MobileCharacterController.mouse.x * 100f * Time.deltaTime;
            orbitY -= BCG_MobileCharacterController.mouse.y * 100f * Time.deltaTime;

            // Clamping Y.
            orbitY = Mathf.Clamp(orbitY, minOrbitY, maxOrbitY);

            if (orbitX < -360f)
                orbitX += 360f;
            if (orbitX > 360f)
                orbitX -= 360f;

        }

    }

    private void Controller() {

        transform.Translate(inputMovementX * Time.deltaTime, 0, inputMovementY * Time.deltaTime, characterCamera.transform);
        transform.rotation = Quaternion.AngleAxis(orbitX, transform.up);

    }

    private void Camera() {

        orbitRotation = Quaternion.Lerp(orbitRotation, Quaternion.Euler(orbitY, 0f, 0f), 50f * Time.deltaTime);

        wantedRotation = transform.rotation;

        targetPosition = transform.position;
        targetPosition -= (wantedRotation * orbitRotation) * Vector3.forward * distance;
        targetPosition += Vector3.up * height;

        characterCamera.transform.position = targetPosition;
        characterCamera.transform.LookAt(transform);

    }

}