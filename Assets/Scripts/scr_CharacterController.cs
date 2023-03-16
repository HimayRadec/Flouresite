using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static scr_Models;

/* Developed By Himay
 * First Edit: March 3 2023
 * Last Edit: March 3 2023
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

public class scr_CharacterController : MonoBehaviour
{
    // Accesses the input actions
    private CharacterController characterController;
    private DefaultInput defaultInput;

    // Movement inputs
    public Vector2 input_Movement;
    public Vector2 input_View;

    // Rotations
    private Vector3 newCameraRotation;
    private Vector3 newCharacterRotation;

    [Header("References")]
    public Transform cameraHolder; // Should reference main camera empty parent object

    [Header("Settings")] // Refer to "scr_Models"
    public PlayerSettingsModel playerSettings; // Creates an object from a class declared in "scr_Models"

    [Header("View Clamping")]
    public float viewClampYmin = -70; // -70
    public float viewClampYmax = 80; // 80

    [Header("Gravity")]
    public float gravityAmount;
    public float gravityMin;
    public float playerGravity;

    private void Awake()
    {
        defaultInput = new DefaultInput();

        defaultInput.Character.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        defaultInput.Character.View.performed += e => input_View = e.ReadValue<Vector2>();

        defaultInput.Enable();

        newCameraRotation = cameraHolder.localRotation.eulerAngles;
        newCharacterRotation = transform.localRotation.eulerAngles;

        characterController = GetComponent<CharacterController>();

    }
    void Start()
    {
        
    }
    void Update()
    {
        CalculateView();
        CalculateMovement();
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
        var verticalSpeed = playerSettings.WalkingForwardSpeed * input_Movement.y * Time.deltaTime;
        var horizonstalSpeed = playerSettings.WalkingStrafeSpeed * input_Movement.x * Time.deltaTime;
        var newMovementSpeed = new Vector3(horizonstalSpeed, 0, verticalSpeed);

        
        newMovementSpeed = transform.TransformDirection(newMovementSpeed); // Moves character according to camera direction   

        characterController.Move(newMovementSpeed);

        // Gravity

        if (playerGravity > gravityMin)
        {
            playerGravity -= gravityAmount * Time.deltaTime;

        }

        if (playerGravity < -1 && characterController.isGrounded)
        {
            playerGravity = -1;
        }
          
    }
}
