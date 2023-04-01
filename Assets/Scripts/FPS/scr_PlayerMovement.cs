using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

// Implemented By Himay
// First Edit: March 31 2023
// Last Edit: April 1 2023
// Requirements:
// Runs off unity's input system. Class is called "PlayerInput"

public class scr_PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    private PlayerInput input;

    #region - Movement Values -

    [Header("Movement")]
    public float forwardSpeed = 6;
    public float backwardSpeed = 3;
    public float horizontalSpeed = 5;

    private Vector2 movementInput;
    private Vector3 movementVelocity; // movement with direction

    [Header("Jump")]
    public float jumpHeight;
    public float jumpSpeed = 1f;
    public float fallSpeed = 1f;
    public float gravity = -9.81f; // Gravity value, default is -9.81 (earth gravity)
    private float verticalVelocity; // Vertical velocity for jumping and gravity
    private bool isJumping;

    #endregion


    private void Awake()
    {
        input = new PlayerInput();

        input.Player.Move.performed += e => movementInput = e.ReadValue<Vector2>();
        input.Player.Move.canceled += e => movementInput = e.ReadValue<Vector2>();
        input.Player.Jump.performed += e => Jump();

        input.Enable();

        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Debug.Log(movementDirection);
        CalculateMovement();
    }


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

        // Calculate separate forward/backward and horizontal movements
        Vector3 forwardMovement = transform.forward * movementInput.y;
        Vector3 horizontalMovement = transform.right * movementInput.x;

        // Apply different speeds for forward, backward, and horizontal movement
        if (movementInput.y > 0)
        {
            forwardMovement *= forwardSpeed;
        }
        else
        {
            forwardMovement *= backwardSpeed;
        }
        horizontalMovement *= horizontalSpeed;

        // Combine forward/backward and horizontal movement
        movementVelocity = forwardMovement + horizontalMovement;

        // Add vertical movement (jumping and gravity)
        movementVelocity.y = verticalVelocity;

        // Move the character
        characterController.Move(movementVelocity * Time.deltaTime);
    }

    private void Jump()
    {
        Debug.Log("Jump Pressed");
        if (characterController.isGrounded)
        {
            isJumping = true;
            verticalVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight) * jumpSpeed;
        }
    }



}
