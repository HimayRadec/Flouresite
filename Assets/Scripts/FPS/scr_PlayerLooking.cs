using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class scr_PlayerLooking : MonoBehaviour
{
    private PlayerInput input;

    [SerializeField]
    public Transform cameraHolder;

    [Header("Settings")]
    public float xSensitivity = 50f;
    public float ySensitivity = 50f;
    public bool xViewInverted;
    public bool yViewInverted;
    public float viewClampYMax = 80;
    public float viewClampYMin = -70;

    private Vector2 lookInput;
    private Vector2 previousMousePosition;
    private Vector3 cameraRotation; 
    private Vector3 characterRotation;

    private void Awake()
    {
        input = new PlayerInput();
        Cursor.lockState = CursorLockMode.Locked;

        input.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();

        cameraRotation = cameraHolder.localRotation.eulerAngles;
        characterRotation = transform.localRotation.eulerAngles;

        // previousMousePosition = Mouse.current.position.ReadValue();
    }

    private void OnEnable()
    {
        input.Player.Enable();

    }

    private void OnDisable()
    {
        input.Player.Disable();

    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }
    void Update()
    {
        CalculateLook();
    }

    private void CalculateLook()
    {
        Vector2 currentMousePosition = Mouse.current.position.ReadValue(); // get current mouse position
        Vector2 mouseDelta = currentMousePosition - previousMousePosition; // calculate mouse delta movement

        // check if the mouse has moved
        if (mouseDelta != Vector2.zero)
        {
            lookInput = new Vector2(lookInput.x + mouseDelta.x, lookInput.y + mouseDelta.y);
        }
        else
        {
            lookInput = Vector2.zero;
        }

        // update previous mouse position
        previousMousePosition = currentMousePosition;

        // rotates character left and right
        characterRotation.y += xSensitivity * (xViewInverted ? -lookInput.x : lookInput.x) * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(characterRotation);

        // rotates camera up and down
        cameraRotation.x += ySensitivity * (yViewInverted ? -lookInput.y : lookInput.y) * Time.deltaTime; // (If ? True : False)
        cameraRotation.x = Mathf.Clamp(cameraRotation.x, viewClampYMin, viewClampYMax);

        cameraHolder.localRotation = Quaternion.Euler(cameraRotation);
    }
}
