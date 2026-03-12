using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponAnimData", menuName = "Game/AnimationDataBase")]
public class WeaponAnimationData : ScriptableObject
{
    [Header("--- 0. TIEMPOS Y VELOCIDADES (Global) ---")]
    [Tooltip("Velocidad de la respiración")] public float idleSpeed = 1f;
    [Tooltip("Velocidad del paso")] public float walkBobSpeed = 10f;
    [Tooltip("Velocidad de transición entre estados")] public float runTransitionSpeed = 6f;
    [Tooltip("Velocidad de recuperación de inclinación")] public float tiltSpeed = 5f;

    [Header("--- 1. IDLE (Respiración) ---")]
    public float fp_idleAmount = 0.02f;
    public float tp_idleAmount = 0.01f;

    [Header("--- 2. WALK (Caminar) ---")]
    public float fp_walkBobAmount = 0.02f;
    public float tp_walkBobAmount = 0.01f;

    [Header("--- 3. RUN / CORRER ---")]
    public Vector3 fp_runningTilt = new Vector3(15f, -45f, 0f);
    public Vector3 tp_runningTilt = new Vector3(5f, -10f, 0f);
    [Space]
    public float fp_runBobMultiplier = 5f;
    public float tp_runBobMultiplier = 2f;

    [Header("--- 4. SLIDE (Deslizamiento) ---")]
    public float slideDropAmount = -0.15f;
    public float slideKickRotation = -10f;
    public float slideRecoverySpeed = 8f;

    [Header("--- 5. STRAFE & TILT (Inclinación lateral) ---")]
    public float fp_strafeTiltAmount = 4f;
    public float tp_strafeTiltAmount = 1.5f;
    [Space]
    public float fp_forwardTiltAmount = 3f;
    public float tp_forwardTiltAmount = 1f;

    [Header("--- 6. SWAY (Inercia de ratón - SOLO FP) ---")]
    public float fp_swayAmount = 0.02f;
    public float fp_maxSway = 0.06f;

    [Header("--- 7. POSICIONES FP (1.ª Persona) ---")]
    public Vector3 fp_hipPosition = new Vector3(0.2f, -0.2f, 0f); 
    public Vector3 fp_aimPosition = new Vector3(0f, -0.1f, 0f);   
    public float fp_aimSpeed = 12.5f;

    [Header("--- 8. POSICIONES 3P (3.ª Persona) ---")]
    public Vector3 tp_hipPosition = Vector3.zero;
    public Vector3 tp_aimPosition = Vector3.zero; 
    [Space]
    public Vector3 tp_runningOffset = new Vector3(0.1f, -0.05f, 0f); 
    public Vector3 tp_crouchOffset = new Vector3(0f, 0.05f, -0.1f);
    [Space]
    public float tp_aimSpeed = 12.5f;
    public float tp_bobMultiplier = 0.4f;
}