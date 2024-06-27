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

public class BCG_InputManager : RCCP_Singleton<BCG_InputManager> {

    private BCG_Inputs inputs = new BCG_Inputs();

#if BCG_NEWINPUTSYSTEM
    private static BCG_InputActions inputActions;
#endif

    public delegate void onInteract();
    public static event onInteract OnInteract;

    private void Awake() {

        gameObject.hideFlags = HideFlags.HideInHierarchy;

        //  Creating inputs.
        inputs = new BCG_Inputs();

    }

    private void Update() {

        //  Creating inputs.
        if (inputs == null)
            inputs = new BCG_Inputs();

        //  Receive inputs from the controller.
        inputs = Inputs();

    }

    private BCG_Inputs Inputs() {

#if BCG_NEWINPUTSYSTEM

        if (inputActions == null) {

            inputActions = new BCG_InputActions();
            inputActions.Enable();

            inputActions.Character.Interact.performed += Interact_performed;

        }

        if (!BCG_EnterExitSettings.Instance.mobileController) {

            inputs.horizonalInput = inputActions.Character.Movement.ReadValue<Vector2>().x;
            inputs.verticalInput = inputActions.Character.Movement.ReadValue<Vector2>().y;
            inputs.aim = inputActions.Character.Aim.ReadValue<Vector2>();

        }

#else

        if (!BCG_EnterExitSettings.Instance.mobileController) {

            inputs.horizonalInput = Input.GetAxis("Horizontal");
            inputs.verticalInput = Input.GetAxis("Vertical");
            inputs.aim = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            if (Input.GetKeyDown(KeyCode.Return))
                Interact_performed();

        }

#endif

        return inputs;

    }

    public BCG_Inputs GetInputs() {

        return inputs;

    }

    private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {

        if (OnInteract != null)
            OnInteract();

    }

    private void Interact_performed() {

        if (OnInteract != null)
            OnInteract();

    }

}
