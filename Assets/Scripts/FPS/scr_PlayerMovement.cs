using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private Vector3 movementVelocity;

    #endregion


    private void Awake()
    {
        input = new PlayerInput();

        input.Player.Move.performed += e => movementInput = e.ReadValue<Vector2>();
        input.Player.Move.canceled += e => movementInput = e.ReadValue<Vector2>();

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

        // Move the character
        characterController.Move(movementVelocity * Time.deltaTime);
    }

}
