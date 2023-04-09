using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

// Implemented By Himay
// First Edit: March 31 2023
// Last Edit: April 1 2023
// Requirements:
// Runs off unity's input system. Class is called "PlayerInput"
/// <summary>
/// Naming conventions
/// classes / methods - PascalCase
/// variables - camelCase
/// constants - CAPITAL
/// </summary>

public class scr_PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    private PlayerInput input;

    #region - Movement Values -

    [Header("Movement")]
    [SerializeField] private float forwardSpeed;
    [SerializeField] private float backwardSpeed;
    [SerializeField] private float sidewaySpeed;
    [Space]
    public float aimingForwardSpeed = 6;
    public float aimingBackwardSpeed = 3;
    public float aimingSidewaySpeed = 5;
    [Space]
    public float walkingForwardSpeed = 6;
    public float walkingBackwardSpeed = 3;
    public float walkingSidewaySpeed = 5;
    [Space]
    public float sprintingForwardSpeed = 10;
    public float sprintingBackwardSpeed = 5;
    public float sprintingSidewaySpeed = 7;
    private bool sprintPressed;


    private Vector2 movementInput;
    private Vector3 movementVelocity; // movement with direction

    [Header("Jump")]
    public float jumpHeight = 1.5f;
    public float jumpSpeed = 1f;
    public float fallSpeed = 1f;
    public float gravity = -9.81f; // Gravity value, default is -9.81 (earth gravity)
    private float verticalVelocity; // Vertical velocity for jumping and gravity
    private bool jumpPressed;

    [Header("States")]
    private bool isWalking;
    public bool isSprinting;
    private bool isJumping;



    #endregion

    public enum PlayerState
    {
        Aiming,
        Walking,
        Sprinting,
        Jumping,
    }


    private void Awake()
    {
        input = new PlayerInput();

        input.Player.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += ctx => movementInput = ctx.ReadValue<Vector2>();

        input.Player.Jump.performed += ctx => jumpPressed = ctx.ReadValueAsButton();

        input.Player.Sprinting.performed += ctx => isSprinting = ctx.ReadValueAsButton();


        characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        input.Player.Enable();
    }

    private void OnDisable()
    {
        input.Player.Disable();

    }

    void Update()
    {
        CalculateMovement();
        Jump();
    }

    // TODO implement advanced jump
    /// <summary>
    /// calculates needed velocity to reach jumpHeight under earths gravity and returns time to reach
    /// changes gravity and velocity needed to reach
    /// </summary>
    private void CalculateMovement()
    {
        // Apply gravity
        if (characterController.isGrounded && !isJumping)
        {
            verticalVelocity = 0;
            isJumping = false;
        }
        else
        {
            float speedMultiplier = (verticalVelocity > 0) ? jumpSpeed : fallSpeed;
            verticalVelocity += gravity * speedMultiplier * Time.deltaTime;
        }

        // TODO calculate PlayerState

        // TODO get forward/backward/sideway speed based off player state speed.

        // Calculate separate forward/backward and horizontal movements
        Vector3 forwardMovement = transform.forward * movementInput.y;
        Vector3 horizontalMovement = transform.right * movementInput.x;

        // Apply different speeds for forward, backward, and horizontal movement
        // Check if player is sprinting and adjust speeds accordingly
        if (isSprinting)
        {
            if (movementInput.y > 0)
            {
                forwardMovement *= sprintingForwardSpeed;
            }
            else
            {
                forwardMovement *= sprintingBackwardSpeed;
            }
            horizontalMovement *= sprintingSidewaySpeed;
        }
        else
        {
            if (movementInput.y > 0)
            {
                forwardMovement *= walkingForwardSpeed;
            }
            else
            {
                forwardMovement *= walkingBackwardSpeed;
            }
            horizontalMovement *= walkingSidewaySpeed;
        }

        // Combine forward/backward and horizontal movement
        movementVelocity = forwardMovement + horizontalMovement;

        // Add vertical movement (jumping and gravity)
        movementVelocity.y = verticalVelocity;

        // Move the character
        characterController.Move(movementVelocity * Time.deltaTime);
    }

    private void Jump()
    {
        if (characterController.isGrounded && jumpPressed)
        {
            isJumping = true;
            verticalVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight) * jumpSpeed;
        }
    }



}
