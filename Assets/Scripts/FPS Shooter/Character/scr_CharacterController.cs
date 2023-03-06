using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static scr_Models;

// BUG LIST
// Proning and not moving lowers you to 0.90...
// You cant unprone until you move and y increases to 0.95
//
// If running and try to aim in it doesnt aim in 

// Tutorial by Fulled By Caffiene
// Refined by Himay
// Controls Player Movement

public class scr_CharacterController : MonoBehaviour
{

    private CharacterController characterController;
    private DefaultInput defaultInput;

    [HideInInspector]
    // creates a data type with a X and Y value
    public Vector2 input_Movement;

    [HideInInspector]
    public Vector2 input_View;

    private Vector3 newCameraRotation;
    private Vector3 newCharacterRotation; 

    [Header("References")]
    public Transform cameraHolder;
    public Transform feetTransform;
    public Camera cam;
    public LayerMask mask;
    // will have to change to gunInHand later?

    public scr_WeaponController weaponInHand;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    public LayerMask playerMask;
    public LayerMask groundMask;


    public float viewClampYMin = - 70f;
    public float viewClampYMax= 80f;

    [Header("Gravity")]
    public float gravityAmount;
    public float gravityMin;
    private float playerGravity;

    public Vector3 jumpingForce;
    public Vector3 jumpingForceVelocity;

    [Header("Stance")]
    public PlayerStance playerStance;
    public float playerStanceSmoothing;
    public CharacterStance playerStandStance;
    public CharacterStance playerCrouchStance;
    public CharacterStance playerProneStance;

    private float StanceCheckErrorMargin = 0.05f;
    private float cameraHeight;
    private float cameraHeightVelocity;

    private Vector3 stanceCapsuleCenter;
    private Vector3 stanceCapsuleCenterVelocity;

    private float stanceCapsuleHeight;
    private float stanceCapsuleHeightVelocity;

    [HideInInspector]
    public bool isSprinting;

    private Vector3 newMovementSpeed;
    private Vector3 newMovementSpeedVecocity;

    [Header("Weapon")]
    public scr_WeaponController currentWeapon;

    public float weaponAnimationSpeed;

    [HideInInspector]
    public bool isGrounded;
    [HideInInspector]
    public bool isFalling;

    [Header("Aiming In")]
    public bool isAimingIn;

    #region - Awake -
    private void Awake()
    {
        defaultInput = new DefaultInput();
        Cursor.lockState = CursorLockMode.Locked;


        // updates the X and Y value to character movement
        defaultInput.Character.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        defaultInput.Character.View.performed += e => input_View = e.ReadValue<Vector2>();
        defaultInput.Character.Jump.performed += e => Jump();
        defaultInput.Character.Crouch.performed += e => Crouch();
        defaultInput.Character.Prone.performed += e => Prone();
        defaultInput.Character.Sprint.performed += e => ToggleSprint();
        defaultInput.Character.SprintReleased.performed += e => StopSprint();

        // Interact
        defaultInput.Character.Interact.performed += e => BuyAmmo();

        defaultInput.Weapon.Fire2Pressed.performed += e => AimingInPressed();
        defaultInput.Weapon.Fire2Released.performed += e => AimingInReleased();

        // Swap later to advanced
        defaultInput.Weapon.Fire1Pressed.performed += e => RaycastShootingPressed();
        defaultInput.Weapon.Fire1Released.performed += e => RaycastShootingReleased();

        // Reloading
        defaultInput.Weapon.Reload.performed += e => Reload();


        // input is now enabled
        defaultInput.Enable();

        newCameraRotation = cameraHolder.localRotation.eulerAngles;
        newCharacterRotation = transform.localRotation.eulerAngles;

        characterController = GetComponent<CharacterController>();

        cameraHeight = cameraHolder.localPosition.y;

        if (currentWeapon)
        {
            currentWeapon.Initialise(this);
        }
    }

    #endregion

    #region - Update -
    private void Update()
    {
        SetIsGrounded();
        SetIsFalling();

        CalculateMovement();
        CalculateView();
        CalculateJump();
        CalculateStance();
        CalculateAimingIn();

    }
    #endregion

    #region - Reload -

    private void Reload()
    {
        currentWeapon.Reload();
    }
    #endregion

    #region - Raycast Shooting -

    private void RaycastShootingPressed()
    {
        if (currentWeapon)
        {
            currentWeapon.isShooting = true;
            isSprinting = false;


        }
    }

    private void RaycastShootingReleased()
    {
        currentWeapon.isShooting = false;

    }

    #endregion

    #region - Advanced Shooting -

    private void ShootingPressed()
    {
        if (currentWeapon)
        {
            currentWeapon.isShooting = true;

        }
    }

    private void ShootingReleased()
    {
        currentWeapon.isShooting = true;
    }

    #endregion

    #region - Aiming In-

    private void AimingInPressed()
    {
        isAimingIn = true;
    }
    
    private void AimingInReleased()
    {
        isAimingIn = false;
    }

    private void CalculateAimingIn()
    {
        if (!currentWeapon)
        {
            return;
        }

        currentWeapon.isAimingIn = isAimingIn;
    }

    #endregion

    #region - IsFalling / IsGrounded

    private void SetIsGrounded()
    {

        isGrounded = Physics.CheckSphere(feetTransform.position, playerSettings.isGroundedRadius, groundMask);
    }

    private void SetIsFalling()
    {

        isFalling = (!isGrounded && characterController.velocity.magnitude > playerSettings.isFallingSpeed);
    }
    #endregion

    #region - View / Movement -

    private void CalculateView()
    {
        // Left and Right View
        newCharacterRotation.y += (isAimingIn ? playerSettings.ViewXSensitivity * playerSettings.AimingSensitivityEffector: playerSettings.ViewXSensitivity) * (playerSettings.ViewXInverted ? -input_View.x : input_View.x) * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(newCharacterRotation);

        // Up and Down View
        newCameraRotation.x += (isAimingIn ? playerSettings.ViewYSensitivity * playerSettings.AimingSensitivityEffector : playerSettings.ViewYSensitivity) * (playerSettings.ViewYInverted ? input_View.y : -input_View.y) * Time.deltaTime;
        newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, viewClampYMin, viewClampYMax);

        cameraHolder.localRotation = Quaternion.Euler(newCameraRotation);
    }
    
    private void CalculateMovement()
    {
        // if you let go of W sprinting will reset
        if (input_Movement.y <= 0.2f)
        {
            isSprinting = false;
        }

        var horizontalSpeed = playerSettings.WalkingStrafeSpeed;
        var verticalSpeed = playerSettings.WalkingForwardSpeed;

        // updates movespeed if you are sprinting   
        if (isSprinting)
        {
            verticalSpeed = playerSettings.RunningForwardSpeed;
            horizontalSpeed = playerSettings.RunningStrafeSpeed;
        }

        // Effectors

        if (!isGrounded)
        {
            playerSettings.SpeedEffector = playerSettings.FallingSpeedEffector;
        }
        else if (playerStance == PlayerStance.Crouch)
        {
            playerSettings.SpeedEffector = playerSettings.CrouchSpeedEffector;
        }
        else if (playerStance == PlayerStance.Prone)
        {
            playerSettings.SpeedEffector = playerSettings.ProneSpeedEffector;
        }
        else if (isAimingIn)
        {
            playerSettings.SpeedEffector = playerSettings.AimingSpeedEffector;
        }
        else
        {
            playerSettings.SpeedEffector = 1;
        }

        weaponAnimationSpeed = characterController.velocity.magnitude / (playerSettings.WalkingForwardSpeed * playerSettings.SpeedEffector);

        if (weaponAnimationSpeed > 1)
        {
            weaponAnimationSpeed = 1;
        }

        verticalSpeed *= playerSettings.SpeedEffector;
        horizontalSpeed *= playerSettings.SpeedEffector;


        // Smooths movement when switching speeds
        newMovementSpeed = Vector3.SmoothDamp(newMovementSpeed, new Vector3(horizontalSpeed * input_Movement.x * Time.deltaTime, 0, verticalSpeed * input_Movement.y * Time.deltaTime), ref newMovementSpeedVecocity, isGrounded ? playerSettings.MovementSmoothing : playerSettings.FallingSmoothing);

        // Move relative to players view
        var movementSpeed = transform.TransformDirection(newMovementSpeed);

        // Gravity

        if (playerGravity > gravityMin)
        {
            playerGravity -= gravityAmount * Time.deltaTime;
        }
        

        if (playerGravity < -0.1f && isGrounded)
        {
            playerGravity = -0.1f;
        }

        movementSpeed.y += playerGravity;
        movementSpeed += jumpingForce * Time.deltaTime;

        characterController.Move(movementSpeed);
    }

    #endregion

    #region - Jumping - 

    private void CalculateJump()
    {
        jumpingForce = Vector3.SmoothDamp(jumpingForce, Vector3.zero, ref jumpingForceVelocity, playerSettings.JumpingFalloff);
    }


    private void Jump()
    {

        // could make it so if you are in prone you just crouch and then stand instead.
        if (!isGrounded || playerStance == PlayerStance.Prone)
        {
            return;  
        }

        // if you are crouched you will stand instead of crouch jumping
        if (playerStance == PlayerStance.Crouch)
        {
            if (StanceCheck(playerStandStance.StanceCollider.height))
            {
                return;
            }

            playerStance = PlayerStance.Stand;
            return;
        }

        // Jump
        jumpingForce = Vector3.up * playerSettings.JumpingHeight;
        playerGravity = 0;
        currentWeapon.TriggerJump();
    }

    #endregion

    #region - Stance -
    private void CalculateStance()
    {

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

    private void Crouch()
    {
        if (playerStance == PlayerStance.Crouch)
        {
            if (StanceCheck(playerStandStance.StanceCollider.height))
            {
                return;
            }

            playerStance = PlayerStance.Stand;
            return;
        }

        if (StanceCheck(playerCrouchStance.StanceCollider.height))
        {
            return;
        }

        playerStance = PlayerStance.Crouch;
    }

    private void Prone()
    {
        if (!(playerStance == PlayerStance.Prone))
        {
            playerStance = PlayerStance.Prone;
            return;
        } else if (!StanceCheck(playerStandStance.StanceCollider.height))
        {
            playerStance = PlayerStance.Stand;
            return;

        }
    }

    private bool StanceCheck(float stanceCheckHeight)
    {
        var start = new Vector3(feetTransform.position.x, feetTransform.position.y + characterController.radius + StanceCheckErrorMargin, feetTransform.position.z);
        var end = new Vector3(feetTransform.position.x, feetTransform.position.y - characterController.radius - StanceCheckErrorMargin + stanceCheckHeight, feetTransform.position.z);


        // If it won't collide with anything in playerMask the nit can uncrouch
        return Physics.CheckCapsule(start, end, characterController.radius, playerMask);
    }

    #endregion

    #region - Sprinting -

    private void ToggleSprint()
    {
        if (input_Movement.y <= 0.2f)
        {
            isSprinting = false;
            return;
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

    #endregion

    #region - Interacting -

    public void BuyAmmo()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        RaycastHit hitInfo;
        //if (Physics.Raycast(ray, out hitInfo, 5f, mask))
        //{
            

        //    if (hitInfo.collider.GetComponent<scr_ammoBuy>() != null)
        //    {
        //        Interactable interactable = hitInfo.collider.GetComponent<Interactable>();
        //        //playerUI.UpdateText(interactable.promptMessage);

        //        interactable.BaseInteract();


        //    } 
        //    else if (hitInfo.collider.GetComponent<scr_BuyDoor>() != null)
        //    {
        //        Debug.Log("BUY DOOR");
        //        Interactable interactable = hitInfo.collider.GetComponent<Interactable>();
        //        //playerUI.UpdateText(interactable.promptMessage);

        //        interactable.BaseInteract();


        //    }
        //}
    }

    #endregion

    #region - Gizmos -
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(feetTransform.position, playerSettings.isGroundedRadius);
    }

    #endregion
}
