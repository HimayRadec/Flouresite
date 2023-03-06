using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettings : MonoBehaviour
{

    [Header("References")]
    public UserInterface UI;

    [Header("Player Stats")]
    public int health;
    public int maxHealth;
    public int points = 0;

    public void Awake()
    {


    }
    // FOR EFFECIENCY DO NOT UPDATE EVERY FRAME 
    void Update()
    {
        UI.SetHealth(health);
        UI.SetPoints(points);
    }
}
