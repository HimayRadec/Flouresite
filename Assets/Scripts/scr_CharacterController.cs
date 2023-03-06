using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static scr_Models;

/* IMPLEMENTATION
 * 
 * Requirements
 * scr_models for the player settings
 * Input controller class using unity's input system named "DefaultInput"
 * 
 * 
 */

public class scr_CharacterController : MonoBehaviour
{
    // Accesses the input actions
    private CharacterController characterController;
    private DefaultInput defaultInput;

    public Vector2 input_Movement;
    public Vector2 input_View;

    private Vector3 newCameraRotation;
    private Vector3 newCharacterRotation;

    [Header("References")]
    public Transform cameraHolder;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    public float viewClampYmin = -70;
    public float viewClampYmax = 80;

    private void Awake()
    {
        defaultInput = new DefaultInput();

        defaultInput.Character.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        defaultInput.Character.View.performed += e => input_View = e.ReadValue<Vector2>();

        defaultInput.Enable();

        // -- UNSURE --
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
        CalculateMovment();
    }

    private void CalculateView()
    {
        newCharacterRotation.y += playerSettings.ViewXSensitivity * (playerSettings.ViewXInverted ? -input_View.x : input_View.x) * Time.deltaTime;
        transform.rotation = Quaternion.Euler(newCharacterRotation);

        newCameraRotation.x += playerSettings.ViewYSensitivity * (playerSettings.ViewYInverted ? input_View.y : -input_View.y) * Time.deltaTime;
        newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, viewClampYmin, viewClampYmax);

        cameraHolder.localRotation = Quaternion.Euler(newCameraRotation);
    }

    private void CalculateMovment()
    {
        var verticalSpeed = playerSettings.WalkingForwardSpeed * input_Movement.y * Time.deltaTime;
        var horizonstalSpeed = playerSettings.WalkingStrafeSpeed * input_Movement.x * Time.deltaTime;
        var newMovementSpeed = new Vector3(horizonstalSpeed, 0, verticalSpeed);

        // Makes movement according to camera   
        newMovementSpeed = transform.TransformDirection(newMovementSpeed);

        characterController.Move(newMovementSpeed);


    }
}
