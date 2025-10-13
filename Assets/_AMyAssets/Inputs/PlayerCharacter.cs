using UnityEngine;
using KinematicCharacterController;


public struct CharacterInput
{
    public Quaternion rotation;
    public Vector2 Move;
}

public class PlayerCharacter : MonoBehaviour, ICharacterController
{ 
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform cameraTarget;
    [Space]
    [SerializeField] private float walkSpeed = 20f;

    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;

    public void Intialize()
    {
        motor.CharacterController = this;
    }

    public void UpdateInput(CharacterInput input)
    {
        _requestedRotation = input.rotation;
        // Pilla el input 2D y crea el movimiento 3D en el vector xz
        _requestedMovement = new Vector3(input.Move.x, 0f, input.Move.y);
        // Clamp del movimiento para que este regulado al pulsar 2 teclas a la vez
        _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f);
        // Orienta el input hacia donde mira el jugador
        _requestedMovement = input.rotation * _requestedMovement;
    }



    public void AfterCharacterUpdate(float deltaTime){}

    public void BeforeCharacterUpdate(float deltaTime){}

    public bool IsColliderValidForCollisions(Collider coll) => false;

    public void OnDiscreteCollisionDetected(Collider hitCollider){}

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport){}

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {}

    public void PostGroundingUpdate(float deltaTime){}

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport){}

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        // actualiza la rotacion del character hacia la misma direccion de la rotacion requerida (camera rotation)

        var forward = Vector3.ProjectOnPlane
       (
            _requestedRotation * Vector3.forward,
            motor.CharacterUp
       );
        currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        var groundedMovement = motor.GetDirectionTangentToSurface
        (
            direction: _requestedMovement,
            surfaceNormal: motor.GroundingStatus.GroundNormal
        ) * _requestedMovement.magnitude;
        currentVelocity = _requestedMovement * walkSpeed;
    }



    public Transform GetCameraTarget() => cameraTarget;
}
