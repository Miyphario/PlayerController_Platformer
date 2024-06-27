//----------------------------------------------
//            BCG Shared Assets
//
// Copyright © 2014 - 2023 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Simple FPS Character controller used in demo scenes. Not professional.
/// You can use your 3rd party or any character controller instead of this script. 
/// </summary>
[AddComponentMenu("BoneCracker Games/BCG Shared Assets Pro/Character Controllers/BCG FPS Controller")]
public class BCG_FPSController : MonoBehaviour {

    /// <summary>
    /// Main car controller.
    /// </summary>
    private BCG_EnterExitPlayer _player;
    private BCG_EnterExitPlayer Player {

        get {

            if (_player == null)
                _player = GetComponentInParent<BCG_EnterExitPlayer>(true);

            return _player;

        }

    }

    /// <summary>
    /// Maximum directional speed.
    /// </summary>
    public float speed = 100f;

    /// <summary>
    /// Input Y clamped (-1, 1f)
    /// </summary>
    private float inputMovementY;

    /// <summary>
    /// Input X clamped (-1, 1f)
    /// </summary>
    private float inputMovementX;

    /// <summary>
    /// Camera of the character.
    /// </summary>
    public Camera characterCamera;

    /// <summary>
    /// Sensitivity of the camera.
    /// </summary>
    public float sensitivity = 5.0f;

    /// <summary>
    /// Smoothing factor of the camera.
    /// </summary>
    public float smoothing = 2.0f;

    /// <summary>
    /// Mouse input for the camera.
    /// </summary>
    private Vector2 mouseInputVector;

    /// <summary>
    /// Smooth the mouse moving
    /// </summary>
    private Vector2 smoothV;

    /// <summary>
    /// Inputs to control the character by feeding inputMovementX and inputMovementY.
    /// </summary>
    public BCG_Inputs inputs;

    private void Start() {

        //	Find character camera at the start.
        if (!characterCamera)
            characterCamera = GetComponentInChildren<Camera>();

    }

    private void OnEnable() {

        inputMovementX = 0f;
        inputMovementY = 0f;

    }

    private void Update() {

        //	If canControl is enabled, enable the camera, receive inputs from the player, and feed it.
        if (Player.canControl) {

            Inputs();       //	Receive inputs form the player.
            Camera();       //	Process the camera.
            characterCamera.gameObject.SetActive(true);

        } else {

            inputMovementX = 0f;
            inputMovementY = 0f;
            characterCamera.gameObject.SetActive(false);

        }

    }

    private void FixedUpdate() {

        Controller();       //	Process the controller.

    }

    /// <summary>
    /// Receive inputs from the player.
    /// </summary>
    private void Inputs() {

        inputs = BCG_InputManager.Instance.GetInputs();

        //Receive keyboard inputs if controller type is not mobile.If controller type is mobile, inputs will be received by BCG_MobileCharacterController component attached to FPS/ TPS Controller UI Canvas.
        if (!BCG_EnterExitSettings.Instance.mobileController) {

            //	X and Y inputs based "Vertical" and "Horizontal" axes.
            inputMovementY = inputs.verticalInput * speed * .02f;
            inputMovementX = inputs.horizonalInput * speed * .02f;

            mouseInputVector += inputs.aim * (.02f * 50f);
            mouseInputVector = new Vector2(mouseInputVector.x, Mathf.Clamp(mouseInputVector.y, -75f, 75f));

        } else {

            //	Receiving X and Y inputs from mobile inputs.
            inputMovementY = BCG_MobileCharacterController.move.y * speed * .02f;
            inputMovementX = BCG_MobileCharacterController.move.x * speed * .02f;

            // Mouse delta
            var mouseDelta = new Vector2(BCG_MobileCharacterController.mouse.x, BCG_MobileCharacterController.mouse.y);
            mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity * smoothing, sensitivity * smoothing));

            // the interpolated float result between the two float values
            smoothV.x = Mathf.Lerp(smoothV.x, mouseDelta.x, 1f / smoothing);
            smoothV.y = Mathf.Lerp(smoothV.y, mouseDelta.y, 1f / smoothing);

            // incrementally add to the camera look
            mouseInputVector += smoothV * Time.deltaTime * 50f;
            mouseInputVector = new Vector3(mouseInputVector.x, Mathf.Clamp(mouseInputVector.y, -75f, 75f));

        }

    }

    private void Controller() {

        // Translating the character with X and Y directions.
        transform.Translate(inputMovementX * Time.deltaTime, 0, inputMovementY * Time.deltaTime);

    }

    private void Camera() {

        //	Setting rotations of the camera.
        characterCamera.transform.localRotation = Quaternion.AngleAxis(-mouseInputVector.y, Vector3.right);
        transform.localRotation = Quaternion.AngleAxis(mouseInputVector.x, transform.up);

    }

    private void OnDisable() {

        inputMovementX = 0f;
        inputMovementY = 0f;

    }

}