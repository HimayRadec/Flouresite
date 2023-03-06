using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class HealthController : MonoBehaviour
{
    //public Canvas image1;
    //public Canvas image2;

    /// <summary>
    /// This script is in corrilation with the Health Canvas. For
    /// the visuals of blood splatter which is the health indicator 
    /// of damage taken; current hits taken effect. Using two png/
    /// img I made from photoshop.
    /// 
    /// Attach this script to an Empty GameObject. Set Regen Rate: 30.
    /// Drag and drop the two Images of Blood splatter & Radiant Blood
    /// into the Hurt Img:__ & Blood Splatter Img:__ .
    /// </summary>

    // change to global later on
    [Header("Player Health Amount")]
    public float currentPlayerHealth = 150f;
    [SerializeField] private float maxPlayerHealth = 150f;

    [SerializeField] private int regenRate = 1;
    private bool canRegen = false;

    [Header("Add the Splatter image here")]
    [SerializeField] private Image redSplatterImage = null;

    [Header("Hurt Image Flash")]
    [SerializeField] private Image hurtImage = null;
    [SerializeField] private float hurtTimer = 0.1f;

    [Header("Heal Timer")]
    [SerializeField] private float healCoolDown = 3.0f;
    [SerializeField] private float maxHealCoolDown = 3.0f;
    [SerializeField] private bool startCoolDown = false;

    /*
    [Header("Audio Name")]
    [SerializeField] private AudioClip hurtAudio = null;
    private AudioSource healthAudioSource;
    */

    void UpdateHealth()
    {
        Color splatterAlpha = redSplatterImage.color;
        splatterAlpha.a = 1 - (currentPlayerHealth / maxPlayerHealth);
        redSplatterImage.color = splatterAlpha;
    }

    IEnumerator HurtFlash()
    {
        hurtImage.enabled = true;
        yield return new WaitForSeconds(hurtTimer);
        hurtImage.enabled = false;
    }

    // KC I added this param damage given from EnemyAiFollowTest
    public void TakeDamage()
    {
        if (currentPlayerHealth >= 0)
        {
            canRegen = false;
            StartCoroutine(HurtFlash());
            UpdateHealth();
            healCoolDown = maxHealCoolDown;
            startCoolDown = true;
        }
    }

    private void Update()
    {
        if (currentPlayerHealth <= 0)
        {
            Application.Quit();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

        }
        if (currentPlayerHealth != 0)
        {
            if (startCoolDown)
            {
                healCoolDown -= Time.deltaTime;
                if (healCoolDown <= 0)
                {
                    canRegen = true;
                    startCoolDown = false;
                }
            }

            if (canRegen)
            {
                if (currentPlayerHealth <= maxPlayerHealth - 0.01)
                {
                    currentPlayerHealth += Time.deltaTime * regenRate;
                    UpdateHealth();
                }
                else
                {
                    currentPlayerHealth = maxPlayerHealth;
                    healCoolDown = maxHealCoolDown;
                    canRegen = false;
                }
            }
        }
        else
        {
            Die();
        }
    }

    private void Die()
    {
        if (currentPlayerHealth == 0)
        {
            // End Credits or something before the game quits or restarts, your choice.

            Debug.Log("Player Has DIED!! Quitting application.");
            Application.Quit();
        }
    }
}
