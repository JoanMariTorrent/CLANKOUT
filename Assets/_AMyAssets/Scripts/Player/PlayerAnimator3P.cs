using PurrNet;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator3P : NetworkBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerCharacter playerCharacter;

    [Header("Ajustes de suavizado")]
    [SerializeField] private float dampTime = 0.1f;

    private readonly int _velXHash = Animator.StringToHash("VelX");
    private readonly int _velZHash = Animator.StringToHash("VelZ");
    private readonly int _jumpHash = Animator.StringToHash("Jump");
    private readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");
    private readonly int _isCrouchingHash = Animator.StringToHash("IsCrouching");
    private readonly int _isSlidingHash = Animator.StringToHash("IsSliding");
    private readonly int _isWallRunningHash = Animator.StringToHash("IsWallRuning");
    private readonly int _isClimbingHash = Animator.StringToHash("IsClimbing");
    private readonly int _WallSideHash = Animator.StringToHash("WallSide");



    private void Start()
    {
        if(animator == null) animator = GetComponent<Animator>();
        if(playerCharacter == null) playerCharacter = GetComponentInParent<PlayerCharacter>();
    }


    private void Update()
    {
        if(playerCharacter == null || animator == null) return;

        UpdateLocomotion();
        UpdateStances();
    }

    private void UpdateLocomotion()
    {
        // LEEMOS DE LA SYNCVAR
        Vector3 vel = playerCharacter.syncedState.value.Velocity;
        Vector3 localVelocity = transform.InverseTransformDirection(vel);

        float normX = localVelocity.x / 13f;
        float normZ = localVelocity.z / 13f;

        animator.SetFloat(_velXHash, normX, dampTime, Time.deltaTime);
        animator.SetFloat(_velZHash, normZ, dampTime, Time.deltaTime);
    }

 
    private void UpdateStances()
    {
        CharacterState state = isOwner ? playerCharacter._state : playerCharacter.syncedState.value;

        animator.SetBool(_isGroundedHash, state.Grounded);
        animator.SetBool(_isCrouchingHash, state.Stance == Stance.Crouch);
        animator.SetBool(_isSlidingHash, state.Stance == Stance.Slide);
        animator.SetBool(_isWallRunningHash, state.Stance == Stance.Wall);
        animator.SetBool(_isClimbingHash, state.Stance == Stance.Climb);

        float wallSideValue = 0f;
        if(state.Stance == Stance.Wall)
        {
            if(playerCharacter.GetWallSide == WallSide.Left) wallSideValue = -1f;
            else if (playerCharacter.GetWallSide == WallSide.Right) wallSideValue = 1f;
        }

        animator.SetFloat(_WallSideHash, wallSideValue, 0.1f, Time.deltaTime);
    }
}
