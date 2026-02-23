using UnityEngine;

public class Proyectil : MonoBehaviour
{
    public float velocidad = 15f;
    public float distanciaMaxima = 15f; // Cuánto viaja antes de desaparecer

    private Vector3 _inicio;

    void Start()
    {
        _inicio = transform.position;
        
        // Asegurar que no caiga por gravedad
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if(rb != null) rb.gravityScale = 0;
    }

    void Update()
    {
        // Moverse siempre hacia "su" derecha (que será la izquierda si el ninja estaba girado)
        transform.Translate(Vector2.right * velocidad * Time.deltaTime);

        // Destruir si llega al límite
        if (Vector3.Distance(_inicio, transform.position) > distanciaMaxima)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemigo"))
        {
            Destroy(other.gameObject); // Matar enemigo
            Destroy(gameObject);       // Destruir dardo
        }
        else if (other.CompareTag("Suelo"))
        {
            Destroy(gameObject);       // Choca con pared
        }
    }
}