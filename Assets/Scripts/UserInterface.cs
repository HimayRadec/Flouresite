using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    [Header("References")]
    public Slider healthBarSlider;
    public Gradient healthBarGradient;
    public Image healthBarFill;

    public HealthController health;

    public Text roundNumber;
    public Text pointsDisplay;

    public Text maxAmmo;
    public Text currentAmmo;

    public Text weaponName;
    public Gradient weaponBackgroundColor;


    #region - Health -
    public void SetMaxHealth(int health)
    {

        healthBarSlider.maxValue = health;
        healthBarSlider.value = health;

        healthBarFill.color = healthBarGradient.Evaluate(1f);
    }

    public void SetHealth(float health)
    {
        healthBarSlider.value = health;

        healthBarFill.color = healthBarGradient.Evaluate(healthBarSlider.normalizedValue);
    }

    #endregion

    #region - Points
    public void SetPoints(int points)
    {
        pointsDisplay.text = points.ToString();
    }
    #endregion

    #region - Ammo -
    public void SetAmmo(int currAmmo)
    {
        currentAmmo.text = currAmmo.ToString();
    }

    public void SetMaxAmmo(int ammo)
    {
        maxAmmo.text = ammo.ToString();
    }

    #endregion

    #region - Round -
    public void SetRound(int round)
    {
        roundNumber.text = round.ToString();
    }

    #endregion

    #region - Weapon -
    public void SetWeapon(string weapon)
    {
        weaponName.text = weapon;
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        SetHealth(health.currentPlayerHealth);
    }
}
