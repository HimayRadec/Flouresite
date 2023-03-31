using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static scr_Models;

/* Developed By Himay
 * First Edit: March 3 2023
 * Last Edit: March 30 2023
 * 
 * Other Resources:
 * - FUELLED BY CAFFEINE: https://www.youtube.com/playlist?list=PLW3-6V9UKVh2T0wIqYWC1qvuCk2LNSG5c
 * 
 * 
 * IMPLEMENTATION
 * 
 * Requirements:
 * scr_models - for the player settings
 * DefaultInput - Input controller class script using unity's input system 
 * 
 * NOTES
 */

//TODO: Simplify jumping mechanics

public class NEWscr_CharacterController : MonoBehaviour
{
    // Accesses the input actions
    private CharacterController characterController;
    private DefaultInput defaultInput;

    // Movement inputs
    private Vector2 input_Movement;
    private Vector2 input_View;

    // Rotations
    private Vector3 newCameraRotation;
    private Vector3 newCharacterRotation;

    [Header("References")]
    public Transform cameraHolder; // Should reference main camera parent 
    public Transform feetTransform; // Place at players feet

    [Header("Settings")] // Refer to "scr_Models"
    public PlayerSettingsModel playerSettings; // Creates an object from a class declared in "scr_Models"
    public float viewClampYmin = -70; // -70
    public float viewClampYmax = 80; // 80
    public LayerMask playerMask; // Apply a "Player" mask to player and select everything but "Player"


    [Header("Gravity")]
    public float gravityAmount;
    public float gravityMin;
    private float playerGravity;


    public Vector3 jumpingForce;
    public Vector3 jumpingForceVelocity;

    [Header("Stance")]
    public PlayerStance playerStance;
    public float playerStanceSmoothing; //TODO: Replace to playerStanceTransitionTime | in seconds
    public CharacterStance playerStandStance;
    public CharacterStance playerCrouchStance;
    public CharacterStance playerProneStance;
    private float stanceCheckErrorMargin = 0.05f;
    private float cameraHeight;
    private float cameraHeightVelocity; // ? camera height transition time

    public Vector3 stanceCapsuleCenterVelocity; // ? capsule center transition time
    private float stanceCapsuleHeightVelocity; // ? capsule height transition time 

    private bool isSprinting;

    private Vector3 newMovementSpeed;
    private Vector3 newMovementSpeedVelocity;


    private void Awake()
    {
        defaultInput = new DefaultInput();

        // connect actions to functions
        defaultInput.Character.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        defaultInput.Character.View.performed += e => input_View = e.ReadValue<Vector2>();
        defaultInput.Character.Jump.performed += e => Jump();
        defaultInput.Character.Crouch.performed += e => Crouch();
        defaultInput.Character.Prone.performed += e => Prone();
        defaultInput.Character.Sprint.performed += e => ToggleSprint();
        defaultInput.Character.SprintReleased.performed += e => StopSprint();

        defaultInput.Enable();

        newCameraRotation = cameraHolder.localRotation.eulerAngles;
        newCharacterRotation = transform.localRotation.eulerAngles;

        characterController = GetComponent<CharacterController>();

        cameraHeight = cameraHolder.localPosition.y;

    }
    void Start()
    {
        
    }
    void Update()
    {
        CalculateView();
        CalculateMovement();
        CalculateJump();
        CalculateStance();
    }
    private void CalculateView()
    {
        newCharacterRotation.y += playerSettings.ViewXSensitivity * (playerSettings.ViewXInverted ? -input_View.x : input_View.x) * Time.deltaTime;
        transform.rotation = Quaternion.Euler(newCharacterRotation);

        newCameraRotation.x += playerSettings.ViewYSensitivity * (playerSettings.ViewYInverted ? input_View.y : -input_View.y) * Time.deltaTime;
        newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, viewClampYmin, viewClampYmax);

        cameraHolder.localRotation = Quaternion.Euler(newCameraRotation);
    }
    private void CalculateMovement()
    {

        if (input_Movement.y <= 0.2f)
        {
            isSprinting = false;
        }

        var verticalSpeed = playerSettings.WalkingForwardSpeed; // TODO: rename to forwardSpeed
        var horizontalSpeed = playerSettings.WalkingStrafeSpeed; // TODO: rename to strafeSpeed

        if (isSprinting)
        {
            verticalSpeed = playerSettings.RunningForwardSpeed;
            horizontalSpeed = playerSettings.RunningStrafeSpeed;
        }

        newMovementSpeed = Vector3.SmoothDamp(newMovementSpeed, new Vector3(horizontalSpeed * input_Movement.x * Time.deltaTime, 0, verticalSpeed * input_Movement.y * Time.deltaTime), ref newMovementSpeedVelocity, playerSettings.MovementSmoothing);
        var movementSpeed = transform.TransformDirection(newMovementSpeed); // Moves character according to camera direction   


        // Gravity

        if (playerGravity > gravityMin)
        {
            playerGravity -= gravityAmount * Time.deltaTime;

        }


        if (playerGravity < -0.1f && characterController.isGrounded)
        {
            playerGravity = -0.1f;
        }

        movementSpeed.y += playerGravity;
        movementSpeed += jumpingForce * Time.deltaTime;


        characterController.Move(newMovementSpeed);

    }
    private void CalculateJump()
    {
        jumpingForce = Vector3.SmoothDamp(jumpingForce, Vector3.zero, ref jumpingForceVelocity, playerSettings.JumpingFalloff);
    }
    private void CalculateStance()
    {
        /// <summary>
        /// currentStance is set to equal a CharacterStance data type (found in scr_models)
        /// each CharacterStance has a unique cameraHeight and capsuleCollider values associated with it.
        /// </summary>

        var currentStance = playerStandStance;

        if (playerStance == PlayerStance.Crouch)
        {
            currentStance = playerCrouchStance;
        }
        else if (playerStance == PlayerStance.Prone) 
        {
            currentStance = playerProneStance;
        }


        cameraHeight = Mathf.SmoothDamp(cameraHolder.localPosition.y, currentStance.CameraHeight, ref cameraHeightVelocity, playerStanceSmoothing);
        cameraHolder.localPosition = new Vector3(cameraHolder.localPosition.x, cameraHeight, cameraHolder.localPosition.z);

        characterController.height = Mathf.SmoothDamp(characterController.height, currentStance.StanceCollider.height, ref stanceCapsuleHeightVelocity, playerStanceSmoothing);
        characterController.center = Vector3.SmoothDamp(characterController.center, currentStance.StanceCollider.center, ref stanceCapsuleCenterVelocity, playerStanceSmoothing);


    }
    private void Jump()
    {
        if (!characterController.isGrounded || playerStance == PlayerStance.Prone) // TODO: make it so that if you are proned/crouched and try to jump you stand up
        {
            return;
        }

        if (playerStance == PlayerStance.Crouch)
        {
            playerStance = PlayerStance.Stand;
            return;
        }

        jumpingForce = Vector3.up * playerSettings.JumpingHeight;
        playerGravity = 0;
    }
    private void Crouch()
    {
        if (playerStance == PlayerStance.Crouch) 
        {
            if (StanceCheck(playerStandStance.StanceCollider.height)) // crouch to stand not viable
            {
                return;
            }

            playerStance = PlayerStance.Stand;
            return;
        }

        if (StanceCheck(playerCrouchStance.StanceCollider.height)) // prone to crouch not viable
        {
            return;
        }

        playerStance = PlayerStance.Crouch;
    }
    private void Prone()
    {
        playerStance = PlayerStance.Prone;

    }
    private bool StanceCheck(float stanceCheckHeight)
    {
        // Checks if the stance will collide
        var start = new Vector3(feetTransform.position.x, feetTransform.position.y + stanceCheckErrorMargin + characterController.radius, feetTransform.position.z);
        var end = new Vector3(feetTransform.position.x, feetTransform.position.y - stanceCheckErrorMargin - characterController.radius + stanceCheckHeight, feetTransform.position.z);
        
        return Physics.CheckCapsule(start, end, characterController.radius, playerMask);
    }
    private void ToggleSprint()
    {
        if (input_Movement.y <= 0.2f)
        {
            isSprinting = false;
        }

        isSprinting = !isSprinting;

    }
    private void StopSprint()
    {
        if (playerSettings.SprintingHold)
        {
            isSprinting = false;
        }
    }
}

