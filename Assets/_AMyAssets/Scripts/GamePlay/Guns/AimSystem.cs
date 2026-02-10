using Unity.Cinemachine;
using UnityEngine;

public class AimSystem : MonoBehaviour
{
    [Header("Variables FOV Base")]
    public float normalFOV = 70f;
    public float sprintFOV = 85f;
    public float fovSpeedSmooth = 5f;

    [Header("Variables FOV Arma")]
    public float normalGunFOV = 60f; 

    [Header("Referencias")]
    public CinemachineCamera cameraPlayer;
    public Camera cameraGun;
    [Space]
    public PlayerCharacter playerCharacter;
    public PlayerCamera playerCamera;
    public WeaponManager weaponManager;

    float currentAimProgress = 0f;
    float currentBaseFOV;

    void Start()
    {
        currentBaseFOV = normalFOV;
    }

    void Update()
    {
        if (weaponManager == null) return;
        
        if (weaponManager._currentGun == null)
        {
            HandleBaseFOV(); 
            if(cameraPlayer != null) cameraPlayer.Lens.FieldOfView = currentBaseFOV;
            return;
        }

        if (!weaponManager._currentGun.canAim) 
        {
             HandleBaseFOV();
             if(playerCamera != null) playerCamera.SetSensitivityMode(AimType.Normal);
             if(cameraPlayer != null) cameraPlayer.Lens.FieldOfView = currentBaseFOV;
             return;
        }

        bool isAimingRequest = playerCharacter._requestedAim;

        // --- CAMBIO DE SENSIBILIDAD ---
        if (playerCamera != null)
        {
            if (isAimingRequest) playerCamera.SetSensitivityMode(weaponManager._currentGun.aimType);
            else playerCamera.SetSensitivityMode(AimType.Normal);
        }
        
        weaponManager._currentGun.isAiming = isAimingRequest;

        float targetProgress = isAimingRequest ? 1 : 0;
        float step = Time.deltaTime / weaponManager._currentGun.timeToAim;
        currentAimProgress = Mathf.MoveTowards(currentAimProgress, targetProgress, step);

        float curveValue = weaponManager._currentGun.aimCurve.Evaluate(currentAimProgress);

        
        HandleBaseFOV();

        if(cameraPlayer != null)
        {
            cameraPlayer.Lens.FieldOfView = Mathf.Lerp(
                currentBaseFOV, 
                weaponManager._currentGun.aimingFOV, 
                curveValue
            );
        }

        if (cameraGun != null)
        {
            cameraGun.fieldOfView = Mathf.Lerp(
                normalGunFOV, 
                weaponManager._currentGun.gunAimingFOV, 
                curveValue
            );
        }
    }

    void HandleBaseFOV()
    {
        if (playerCharacter == null) return;

        Vector3 velocity = playerCharacter._state.Velocity;
        float speed = new Vector3(velocity.x, 0, velocity.z).magnitude;

        float targetBase = (speed > 6.5f) ? sprintFOV : normalFOV;

        currentBaseFOV = Mathf.Lerp(currentBaseFOV, targetBase, Time.deltaTime * fovSpeedSmooth);
    }
}