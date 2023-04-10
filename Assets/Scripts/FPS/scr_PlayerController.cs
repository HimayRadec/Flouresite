using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_PlayerController : MonoBehaviour 
{
    enum CharacterState
    {
        Idle,
        WalkingForward,
        WalkingBackward,
        SprintingForward,
        SprintingBackward,
        Jumping,
        Standing,
        Crouching,
        Proning,
        Aiming,
        Shooting

    }
}
