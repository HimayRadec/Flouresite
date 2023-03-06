using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class scr_Models
{
    #region - Player -

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

        [Header("Movement")]
        public float WalkingForwardSpeed; // 8
        public float WalkingStrafeSpeed; // 5
        public float WalkingBackwardSpeed; // 4
    }
    #endregion
}
