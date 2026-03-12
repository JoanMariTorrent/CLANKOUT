using PurrNet;
using UnityEngine;

public class PlayerAnimationHandler : NetworkBehaviour
{
    [Header("Referencias de Holders")]
    [SerializeField] private Transform weaponHolderFP; 
    [SerializeField] private Transform weaponHolder3P; 
    [SerializeField] private GameObject armsMesh;
    [SerializeField] private PlayerCharacter playerCharacter;
    
    [Header("Referencias de Manos 3P")]
    [SerializeField] private Transform leftHind3P;
    [SerializeField] private Transform rightHind3P;
    [SerializeField] private Vector3 leftHindPosCrouched;
    [SerializeField] private Vector3 rightHindPosCrouched;

    [Header("Ajustes")]
    [SerializeField] private WeaponAnimationData defaultStats;
    private WeaponAnimationData _stats;
    public float startWalkSpeedVel = 0.75f;
    public float startRunSpeedVel = 11f;

    private Vector3 _initialPosFP, _initialPos3P;
    private Vector3 _leftHindDefaultPos, _rightHindDefaultPos;
    private Quaternion _initialRotFP, _initialRot3P;
    private float _bobTimer;
    
    // Variables de inclinación separadas para FP
    private float _currentStrafeTiltFP;
    private float _currentForwardTiltFP;
    
    // Variables de inclinación separadas para 3P
    private float _currentStrafeTilt3P;
    private float _currentForwardTilt3P;

    private bool _wasSliding;
    private float _currentSlideDrop;
    private float _currentSlideKick;
    private bool isAiming;
    private Animator weaponAnimator;
    private Vector3 _currentTPOffset;

    void Start()
    {
        if(weaponHolderFP != null) {
            _initialPosFP = weaponHolderFP.localPosition;
            _initialRotFP = weaponHolderFP.localRotation;
        }
        if(weaponHolder3P != null) {
            _initialPos3P = weaponHolder3P.localPosition;
            _initialRot3P = weaponHolder3P.localRotation;
        }

        if(leftHind3P != null) _leftHindDefaultPos = leftHind3P.localPosition;
        if(rightHind3P != null) _rightHindDefaultPos = rightHind3P.localPosition;

        if(playerCharacter == null) playerCharacter = GetComponentInParent<PlayerCharacter>();
        _stats = defaultStats;
    }

    public void RegisterWeaponAnimator(Animator newWeaponAnim, WeaponAnimationData newData)
    {
        weaponAnimator = newWeaponAnim;
        _stats = newData != null ? newData : defaultStats;
    }

    public void UnRegisterWeaponAnimator()
    {
        weaponAnimator = null;
        _stats = defaultStats;
    }

    void Update()
    {
        if(isOwner && armsMesh != null)
        {
            armsMesh.SetActive(weaponAnimator != null);
        }
        
        if (playerCharacter != null)
        {
            HandleProceduralMovement();
        }
    }

    private void HandleProceduralMovement()
    {
        float dt = Time.deltaTime;

        // --- 1. LEER LA RED, NO LO LOCAL ---
        CharacterState currentState = isOwner ? playerCharacter._state : playerCharacter.syncedState.value;

        Vector3 vel = currentState.Velocity;
        float speed = new Vector3(vel.x, 0, vel.z).magnitude;
        bool isGrounded = currentState.Grounded;
        bool isCrouched = currentState.Stance == Stance.Crouch;
        bool isRunning = speed > startRunSpeedVel && isGrounded;
        bool isWalking = speed > startWalkSpeedVel && isGrounded;
        bool isSliding = currentState.Stance == Stance.Slide;
        bool isOnWall = currentState.Stance == Stance.Wall;
        
        // (El isAiming lo leemos de tu SyncVar en PlayerCharacter, si no lo tienes como SyncVar, los demás no lo verán)
        isAiming = currentState.IsAiming; 
        
        float tiltAimReduct = isAiming ? 0f : 1f;
        bool canApplyStateRotation = !isAiming && isGrounded;

        if (isSliding && !_wasSliding) {
            _currentSlideDrop = _stats.slideDropAmount;
            _currentSlideKick = _stats.slideKickRotation;
        }
        _wasSliding = isSliding;
        _currentSlideDrop = Mathf.Lerp(_currentSlideDrop, 0f, dt * _stats.slideRecoverySpeed);
        _currentSlideKick = Mathf.Lerp(_currentSlideKick, 0f, dt * _stats.slideRecoverySpeed);

        // --- 2. EL TRUCO PARA EL TILT EN MULTIJUGADOR ---
        float inputX = 0f;
        float inputY = 0f;

        if (isOwner) 
        {
            inputX = Input.GetAxisRaw("Horizontal");
            inputY = Input.GetAxisRaw("Vertical");
        }
        else 
        {
            Vector3 localVel = playerCharacter.transform.InverseTransformDirection(vel);
            inputX = Mathf.Clamp(localVel.x / 5f, -1f, 1f); 
            inputY = Mathf.Clamp(localVel.z / 5f, -1f, 1f);
        }

        // 3. DETERMINAR LA ROTACIÓN BASE POR ESTADO
        Quaternion targetStateRotFP = _initialRotFP;
        Quaternion targetStateRot3P = _initialRot3P;

        if (canApplyStateRotation)
        {
            if (isRunning || isSliding || isOnWall)
            {
                targetStateRotFP = Quaternion.Euler(_stats.fp_runningTilt) * _initialRotFP;
                targetStateRot3P = Quaternion.Euler(_stats.tp_runningTilt) * _initialRot3P;
            }
            else if (isCrouched)
            {
                targetStateRotFP = Quaternion.Euler(_stats.fp_runningTilt) * _initialRotFP;
                targetStateRot3P = Quaternion.Euler(_stats.tp_runningTilt) * _initialRot3P;
            }
        }
        
        // --- APLICACIÓN FP ---
        if (isOwner && weaponHolderFP != null) {
            Vector3 fp_Bob = CalculateBob(dt, isWalking, isRunning, isSliding, isOnWall, _stats.fp_walkBobAmount, _stats.fp_runBobMultiplier, isAiming ? 0.1f : 1f, _stats.fp_idleAmount);
            
            _currentStrafeTiltFP = Mathf.Lerp(_currentStrafeTiltFP, -inputX * _stats.fp_strafeTiltAmount * tiltAimReduct, dt * _stats.tiltSpeed);
            _currentForwardTiltFP = Mathf.Lerp(_currentForwardTiltFP, -inputY * _stats.fp_forwardTiltAmount * tiltAimReduct, dt * _stats.tiltSpeed);
            Quaternion fp_Tilt = Quaternion.Euler(_currentForwardTiltFP, 0, _currentStrafeTiltFP);
            
            float mX = Input.GetAxisRaw("Mouse X");
            float mY = Input.GetAxisRaw("Mouse Y");
            Vector3 fp_Sway = new Vector3(Mathf.Clamp(-mX * _stats.fp_swayAmount * (isAiming ? 0.1f : 1f), -_stats.fp_maxSway, _stats.fp_maxSway),
                                         Mathf.Clamp(-mY * _stats.fp_swayAmount * (isAiming ? 0.1f : 1f), -_stats.fp_maxSway, _stats.fp_maxSway), 0);

            Vector3 targetBasePosFP = isAiming ? _stats.fp_aimPosition : _stats.fp_hipPosition;
            Vector3 targetFinalPosFP = targetBasePosFP + fp_Sway + fp_Bob + new Vector3(0, _currentSlideDrop, 0);

            float currentFPRotSpeed = isAiming ? _stats.fp_aimSpeed : _stats.runTransitionSpeed;

            // Fíjate que aquí usamos targetStateRotFP directamente
            weaponHolderFP.localPosition = Vector3.Lerp(weaponHolderFP.localPosition, targetFinalPosFP, dt * _stats.fp_aimSpeed);
            weaponHolderFP.localRotation = Quaternion.Lerp(weaponHolderFP.localRotation, targetStateRotFP * Quaternion.Euler(_currentSlideKick * tiltAimReduct, 0, 0) * fp_Tilt, dt * currentFPRotSpeed);
        }

        // --- APLICACIÓN 3P ---
        if (weaponHolder3P != null) {
            Vector3 targetOffset3P = Vector3.zero;
            if (!isAiming) {
                if (isSliding || isRunning) targetOffset3P = _stats.tp_runningOffset;
                else if (isCrouched) targetOffset3P = _stats.tp_crouchOffset;
            }

            if (leftHind3P != null && rightHind3P != null) {
                Vector3 targetLeft = isCrouched ? leftHindPosCrouched : _leftHindDefaultPos;
                Vector3 targetRight = isCrouched ? rightHindPosCrouched : _rightHindDefaultPos;

                float hindLerpSpeed = dt * _stats.runTransitionSpeed;

                leftHind3P.localPosition = Vector3.Lerp(leftHind3P.localPosition, targetLeft, hindLerpSpeed);
                rightHind3P.localPosition = Vector3.Lerp(rightHind3P.localPosition, targetRight, hindLerpSpeed);    
            }

            _currentTPOffset = Vector3.Lerp(_currentTPOffset, targetOffset3P, dt * _stats.runTransitionSpeed);

            Vector3 tp_Bob = CalculateBob(dt, isWalking, isRunning, isSliding, isOnWall, _stats.tp_walkBobAmount, _stats.tp_runBobMultiplier, isAiming ? 0.2f : 1f, _stats.tp_idleAmount);
            
            _currentStrafeTilt3P = Mathf.Lerp(_currentStrafeTilt3P, -inputX * _stats.tp_strafeTiltAmount * tiltAimReduct, dt * _stats.tiltSpeed);
            _currentForwardTilt3P = Mathf.Lerp(_currentForwardTilt3P, -inputY * _stats.tp_forwardTiltAmount * tiltAimReduct, dt * _stats.tiltSpeed);
            Quaternion tp_Tilt = Quaternion.Euler(_currentForwardTilt3P, 0, _currentStrafeTilt3P);
            
            Vector3 targetBasePos3P = isAiming ? _stats.tp_aimPosition : _stats.tp_hipPosition;
            Vector3 targetFinalPos3P = targetBasePos3P + (tp_Bob * _stats.tp_bobMultiplier) + _currentTPOffset;

            float currentTPRotSpeed = isAiming ? _stats.tp_aimSpeed : _stats.runTransitionSpeed;
            
            
            weaponHolder3P.localPosition = Vector3.Lerp(weaponHolder3P.localPosition, targetFinalPos3P, dt * _stats.tp_aimSpeed);
            weaponHolder3P.localRotation = Quaternion.Lerp(weaponHolder3P.localRotation, targetStateRot3P * Quaternion.Euler(_currentSlideKick * tiltAimReduct, 0, 0) * tp_Tilt, dt * currentTPRotSpeed);
        }
    }

    private Vector3 CalculateBob(float dt, bool walk, bool run, bool slide, bool wall, float amt, float mult, float reduct, float idleAmt) {
        if(walk || slide || (wall && !isAiming)) {
            float speedMult = run || wall ? 1.3f : 1f;
            _bobTimer += dt * _stats.walkBobSpeed * speedMult;
            float currentAmp = (run || wall ? amt * mult : amt) * reduct;
            return new Vector3(Mathf.Cos(_bobTimer/2)*currentAmp, Mathf.Sin(_bobTimer)*currentAmp, run ? Mathf.Sin(_bobTimer)*(currentAmp*0.5f) : 0);
        }
        return new Vector3(0, Mathf.Sin(Time.time * _stats.idleSpeed) * (idleAmt * reduct), 0);
    }

    public void TriggerReload() { if (weaponAnimator != null) weaponAnimator.SetTrigger("Reload"); }
    public void TriggerShoot() { if (weaponAnimator != null) weaponAnimator.SetTrigger("Shoot"); }
}