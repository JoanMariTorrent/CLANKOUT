using UnityEngine;

public class MovimientoBucle : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private Vector3 direccion = Vector3.right;
    [SerializeField] private float distancia = 5f;     
    [SerializeField] private float velocidad = 2f; 

    private Vector3 _posicionInicial;

    void Start()
    {
        _posicionInicial = transform.position;
    }

    void Update()
    {
        float movimiento = Mathf.PingPong(Time.time * velocidad, distancia);
        
        transform.position = _posicionInicial + (direccion.normalized * movimiento);
    }
}