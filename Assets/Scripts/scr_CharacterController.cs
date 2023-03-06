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
 * 
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

    [Header("Settings")]
    public PlayerSettingsModel playerSettings; // Creates an object from "scr_Models"

    [Header("View Clamping")]
    public float viewClampYmin = -70; // -70
    public float viewClampYmax = 80; // 80

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

        // Moves character according to camera direction   
        newMovementSpeed = transform.TransformDirection(newMovementSpeed);

        characterController.Move(newMovementSpeed);


    }
}
