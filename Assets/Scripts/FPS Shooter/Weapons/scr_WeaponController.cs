using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static scr_Models;

// Tutorial by Fulled By Caffiene
// Refined by Himay
// Has Weapon Movement, Animations, and Settings
public class scr_WeaponController : MonoBehaviour
{

    private scr_CharacterController characterController;


    [Header("References")]
    public Animator weaponAnimator;
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public AudioSource bulletShot;
    public AudioSource emptyShot;
    public AudioSource magazineReload;

    // Delete later
    public UserInterface UI;
        // ^

    // for ray casting
    public Camera fpsCam;
    // switch to the bullet spawn later?
    public ParticleSystem muzzleFlash;
    public GameObject fleshImpact;
    public GameObject nonFleshImpact;

    [Header("Settings")]
    public string weaponName;
    public WeaponSettingsModel settings;
    public bool isInitialised;

    Vector3 newWeaponRotation;
    Vector3 newWeaponRotationVelocity;
    Vector3 targetWeaponRotation;
    Vector3 targetWeaponRotationVelocity;
    
    Vector3 newWeaponMovementRotation;
    Vector3 newWeaponMovementRotationVelocity;
    Vector3 targetWeaponMovementRotation;
    Vector3 targetWeaponMovementRotationVelocity;

    private bool isGroundedTrigger;

    private float fallingDelay;

    [Header("Weapon Sway")]
    public Transform weaponSwayObject;

    public float swayAmountA = 1;
    public float swayAmountB = 2;
    public float swayScale = 400f;
    public float swayLerpSpeed = 14f;
    float swayTime;
    Vector3 swayPosition;


    [Header("Sights")]
    public Transform sightTarget;
    public float sightOffset;
    public float aimingInTime;
    private Vector3 weaponSwayPosition; 
    private Vector3 weaponSwayPositionVelocity; 
    [HideInInspector]
    public bool isAimingIn;

    [Header("Shooting")]
    public float damage;
    public float range;

    public float fireRate;
    private float currentFireRate;
    public List<WeaponFireType> allowedFireType;
    public WeaponFireType currentFireType;
    [HideInInspector]
    public bool isShooting;
    [HideInInspector]
    private float nextTimeToFire;
    [HideInInspector]
    public bool isReloading = false;

    [Header("Ammo")]
    public int magazineSize;
    public int ammoInMagazine;
    public int totalAmmo;
    public float reloadingTime;
    private int bulletsNeeded = 0;

    #region - Start / Update -
    private void Start()
    {
        newWeaponRotation = transform.localRotation.eulerAngles;
        currentFireType = allowedFireType.First();
        UI.SetWeapon(weaponName);

    }   

    private void Update()
    {
        if (!isInitialised)
        {
            return; 
        }

        CalculateWeaponRotation();
        SetWeaponAnimations();
        CalculateWeaponSway();
        CalculateAimingIn();
        CalculateShooting();

        UI.SetAmmo(ammoInMagazine);
        UI.SetMaxAmmo(totalAmmo);


    }
    #endregion

    #region - Shooting -

    private void CalculateShooting()
    {
        // what is Time.time ??
        if (isShooting && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            if (ammoInMagazine > 0)
            {
                if (!isReloading)
                {
                    RaycastShoot();
                }
            } 
            else if (ammoInMagazine == 0 && !isReloading)
            {
                emptyShot.Play();
            }

            /* 
             * MORE ADVANCED Shooting
            if (currentFireType == WeaponFireType.SemiAuto)
            {
                isShooting = false;
            }
            */
        }
    }

    private void RaycastShoot()
    {
        bulletShot.Play();
        muzzleFlash.Play();
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            //Enemy enemy = hit.transform.GetComponent<Enemy>();
            //if (hit.transform.tag == "Enemy")
            //{
            //    Debug.Log("ENEMY TAG");
            //}
            //if (enemy != null)
            //{
            //    Debug.Log("enemy hit");
            //    enemy.TakeDamage(damage);
            //    Instantiate(fleshImpact, hit.point, Quaternion.LookRotation(hit.normal));
            //    return;
            //}
            Instantiate(nonFleshImpact, hit.point, Quaternion.LookRotation(hit.normal));

        }
        // this is for the more advanced later on
        // var bullet = Instantiate(bulletPrefab, bulletSpawn);

        // Load bullet settings
        ammoInMagazine--;
        
        if (ammoInMagazine <= 0)
        {
            Reload();
        }
    }
    #endregion

    #region - Reload -
    public void Reload()
    {
        if (!isReloading && totalAmmo != 0)
        {
            // CALCULATE BULLETS NEEDED
            bulletsNeeded = magazineSize - ammoInMagazine;
            isReloading = true;

            if (bulletsNeeded > totalAmmo)
            {
                
                Debug.Log("Reload One");
                StartCoroutine(delayReloadOne());
            } 
            else if (bulletsNeeded <= totalAmmo)
            {
                
                Debug.Log("Reload Two");
                StartCoroutine(delayReloadTwo());
            }
        }
        
    }

    #endregion

    #region - Initialise -
    public void Initialise(scr_CharacterController CharacterController)
    {
        characterController = CharacterController;
        isInitialised = true;
    }

    #endregion

    #region - Aiming In -
    private void CalculateAimingIn()
    {
        var targetPosition = transform.position;
        if (isAimingIn)
        {
            targetPosition = characterController.cameraHolder.transform.position + (weaponSwayObject.transform.position - sightTarget.position) + (characterController.cameraHolder.transform.forward * sightOffset);
        }

        weaponSwayPosition = weaponSwayObject.transform.position;
        weaponSwayPosition = Vector3.SmoothDamp(weaponSwayPosition, targetPosition, ref weaponSwayPositionVelocity, aimingInTime);
        weaponSwayObject.transform.position = weaponSwayPosition + swayPosition;
    }
    #endregion

    #region - Jumping -
    public void TriggerJump()
    {
        isGroundedTrigger = false;
        weaponAnimator.SetTrigger("Jump");
    }
    #endregion

    #region - Rotation -
    private void CalculateWeaponRotation()
    {

        // Weapon Horizontal and Vertical Sway Movement
        targetWeaponRotation.y += (isAimingIn ? settings.SwayAmount / 10 : settings.SwayAmount) * (settings.SwayXInverted ? -characterController.input_View.x : characterController.input_View.x) * Time.deltaTime;
        targetWeaponRotation.x += (isAimingIn ? settings.SwayAmount / 10 : settings.SwayAmount) * (settings.SwayYInverted ? characterController.input_View.y : -characterController.input_View.y) * Time.deltaTime;

        // Weapon Sway Clamping
        targetWeaponRotation.x = Mathf.Clamp(targetWeaponRotation.x, -settings.SwayClampX, settings.SwayClampX);
        targetWeaponRotation.y = Mathf.Clamp(targetWeaponRotation.y, -settings.SwayClampY, settings.SwayClampY);
        targetWeaponRotation.z = isAimingIn ? 0 : targetWeaponRotation.y;

        // Weapon Returns To Original Position
        targetWeaponRotation = Vector3.SmoothDamp(targetWeaponRotation, Vector3.zero, ref targetWeaponRotationVelocity, settings.SwayResetSmoothing);
        newWeaponRotation = Vector3.SmoothDamp(newWeaponRotation, targetWeaponRotation, ref newWeaponRotationVelocity, settings.SwaySmoothing);

        // WASD Movement Swaying
        targetWeaponMovementRotation.z = (isAimingIn ? settings.MovementSwayX / 150 : settings.MovementSwayX) * (settings.MovementSwayXInverted ? -characterController.input_Movement.x : characterController.input_Movement.x);
        targetWeaponMovementRotation.x = (isAimingIn ? settings.MovementSwayY / 105 : settings.MovementSwayY) * (settings.MovementSwayYInverted ? -characterController.input_Movement.y : characterController.input_Movement.y);

        targetWeaponMovementRotation = Vector3.SmoothDamp(targetWeaponMovementRotation, Vector3.zero, ref targetWeaponMovementRotationVelocity, settings.MovementSwaySmoothing);
        newWeaponMovementRotation = Vector3.SmoothDamp(newWeaponMovementRotation, targetWeaponMovementRotation, ref newWeaponMovementRotationVelocity, settings.MovementSwaySmoothing);

        transform.localRotation = Quaternion.Euler(newWeaponRotation + newWeaponMovementRotation);
    }
    #endregion

    #region - Animations -
    private void SetWeaponAnimations()
    {
        if (isGroundedTrigger)
        {
            fallingDelay = 0f;

        }
        else
        {
            fallingDelay += Time.deltaTime;
        }

        if (characterController.isGrounded && !isGroundedTrigger && fallingDelay > 0.1f)
        {
            weaponAnimator.SetTrigger("Land");
            isGroundedTrigger = true;
        }
        else if (!characterController.isGrounded && isGroundedTrigger)
        {
            weaponAnimator.SetTrigger("Falling");
            isGroundedTrigger = false;
        }

        weaponAnimator.SetBool("isSprinting", characterController.isSprinting);
        weaponAnimator.SetFloat("WeaponAnimationSpeed", characterController.weaponAnimationSpeed);
    }
    #endregion

    #region - Sway -
    private void CalculateWeaponSway()
    {
        var targetPosition = LissajousCurve(swayTime, swayAmountA, swayAmountB) / (isAimingIn ? swayScale * 15 : swayScale);
        swayPosition = Vector3.Lerp(swayPosition, targetPosition, Time.smoothDeltaTime * swayLerpSpeed);
        swayTime += Time.deltaTime;

        if (swayTime > 6.3f)
        {
            swayTime = 0;
        }

        // make it so the less stamina you have the more sway you have and vice versa
    }
    // Weapon Sway Function
    private Vector3 LissajousCurve(float Time, float A, float B)
    {
        return new Vector3(Mathf.Sin(Time), A * Mathf.Sin(B * Time + Mathf.PI));
    }
    #endregion

    IEnumerator delayReloadOne()
    {
        magazineReload.Play();
        yield return new WaitForSeconds(2);

        ammoInMagazine += totalAmmo;
        totalAmmo = 0;

        isReloading = false;

    }

    IEnumerator delayReloadTwo()
    {
        magazineReload.Play();
        yield return new WaitForSeconds(2);

        ammoInMagazine += bulletsNeeded;
        totalAmmo -= bulletsNeeded;
        Debug.Log(totalAmmo);

        isReloading = false;

    }

}
