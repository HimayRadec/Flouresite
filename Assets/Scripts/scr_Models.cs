using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class scr_Models
{
    #region - Player -

    public enum PlayerStance
    {
        Stand,
        Crouch,
        Prone
    }

    // Serializing a class makes all its properties editable in the unity editor
    [Serializable]
    public class PlayerSettingsModel
    {
        // Recommended values are commeneted
        [Header("View Settings")]
        public float ViewXSensitivity; // 50
        public float ViewYSensitivity; // 50

        public bool ViewXInverted;
        public bool ViewYInverted;

        [Header("Movement Settings")]
        public bool SprintingHold;
        public float MovementSmoothing; // Transition between movements
        [Header("Movement - Running")] // TODO: replace running with sprinting
        public float RunningForwardSpeed;
        public float RunningStrafeSpeed;

        [Header("Movement - Walking")]
        public float WalkingForwardSpeed; // 8
        public float WalkingStrafeSpeed; // 5
        public float WalkingBackwardSpeed; // 4

        [Header("Jumping")]
        public float JumpingHeight;
        public float JumpingFalloff;

    }

    [Serializable]
    public class CharacterStance
    {
        public float CameraHeight;
        public CapsuleCollider StanceCollider; // link with a size appropriate Capsule Collider
    }
    #endregion

}
