using PurrNet;
using UnityEngine;

public class BallisticProjectile : NetworkBehaviour
{
    [Header("Balística")]
    [SerializeField] private float speed = 250f;
    [SerializeField] private float gravityScale = 1f;
    [SerializeField] private float maxLifetime = 5f;
    [SerializeField] private LayerMask hitLayers;

    private Vector3 _velocity;
    private int _damage;
    private PlayerID _ownerID;
    private bool _isActive = false;

    public void Initialize(int damage, PlayerID ownerID, Vector3 direction)
    {
        _damage = damage;
        _ownerID = ownerID;
        _velocity = direction * speed;
        _isActive = true;
        
        if (isServer) Invoke(nameof(Despawn), maxLifetime);
    }

    private void Update()
    {
        if (!isServer || !_isActive) return;

        MoveBullet(Time.deltaTime);
    }

    private void MoveBullet(float deltaTime)
    {
        // 1. Calcular donde estara la bala en este frame
        Vector3 currentPos = transform.position;
        
        // 2. gravedad
        _velocity += Physics.gravity * gravityScale * deltaTime;
        
        // 3. Calculamos el desplazamiento
        Vector3 displacement = _velocity * deltaTime;
        Vector3 nextPos = currentPos + displacement;

        // 4. Comprobar si chocamos con algo en ese trayecto
        if (Physics.Linecast(currentPos, nextPos, out RaycastHit hit, hitLayers))
        {
            // --- IMPACTO ---
            HandleImpact(hit);
        }
        else
        {
            // --- SIN IMPACTO: Mover la bala ---
            transform.position = nextPos;
            
            // Rotar la bala para que mire hacia donde cae
            if (_velocity != Vector3.zero) transform.forward = _velocity.normalized;
        }
    }

    private void HandleImpact(RaycastHit hit)
    {
        // Ignorar colisión con el dueño 
        if (hit.collider.TryGetComponent(out PlayerHealth ph))
        {
            if (ph.owner.Value == _ownerID) return;
        }

        if(hit.collider.TryGetComponent(out HealthObject oh))
        {
            oh.ChangeHealth(-_damage, transform.position);
        }

        // Lógica de Daño
        if (ph != null)
        {
            ph.ChangeHealth(-_damage, _ownerID);
            if(InstanceHandler.TryGetInstance(out ScoreManager sm))
                sm.AddDamageServerRpc(ph.PlayerID, _ownerID, _damage);
        }
        else if (hit.collider.TryGetComponent(out HealthObject obj))
        {
            obj.ChangeHealth(-_damage, hit.point);
        }

        // Efectos visuales

        Despawn();
    }

    private void Despawn()
    {
        _isActive = false;
        if (gameObject != null) Destroy(gameObject);
    }
}