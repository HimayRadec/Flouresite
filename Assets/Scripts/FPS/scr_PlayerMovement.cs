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
    public float movementSpeed; // Comes out to units per second
    private Vector2 movementInput;
    private Vector3 movementDirection;
    public Vector3 movementVelocity;

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

        movementDirection = new Vector3(movementInput.x, 0, movementInput.y);

        // Move relative to players view
        movementVelocity = transform.TransformDirection(movementDirection) * movementSpeed / 100;

        characterController.Move(movementVelocity);
    }
}
