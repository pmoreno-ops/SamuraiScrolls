using UnityEngine;

public class NinjaController : MonoBehaviour
{
    [Header("--- Configuración de Movimiento ---")]
    [SerializeField] private float velocidadCaminar = 4f;
    [SerializeField] private float velocidadCorrer = 8f;
    [SerializeField] private float fuerzaSalto = 12f;

    [Header("--- Configuración de Combate Melee ---")]
    [SerializeField] private Transform puntoAtaque; // El punto desde donde sale el golpe
    [SerializeField] private float rangoAtaque = 0.5f;
    [SerializeField] private LayerMask capaEnemigos; // Para saber qué es un enemigo
    
    [Header("--- Configuración de Disparo (Ranged) ---")]
    [SerializeField] private Transform puntoDisparo; // Punto en la boca/mano
    [SerializeField] private GameObject prefabDardo; // El prefab del dardo

    [Header("--- Detección de Suelo ---")]
    [SerializeField] private Transform puntoSuelo; // Pies del ninja
    [SerializeField] private float radioSuelo = 0.2f;
    [SerializeField] private LayerMask capaSuelo; // Para saber qué es suelo

    // Referencias a componentes
    private Rigidbody2D _rb;
    private Animator _animator; 
    private bool _estaEnSuelo;
    private bool _estaDefendiendo;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 1. Detección de suelo
        _estaEnSuelo = Physics2D.OverlapCircle(puntoSuelo.position, radioSuelo, capaSuelo);

        // 2. SISTEMA DE DEFENSA (Tecla S)
        // Si pulsas S y estás en el suelo, defiendes y NO te mueves.
        if (Input.GetKey(KeyCode.S) && _estaEnSuelo)
        {
            _estaDefendiendo = true;
            _rb.linearVelocity = Vector2.zero; // Frenar en seco (Unity 6)
            
            if (_animator != null) _animator.SetBool("Defender", true);
            return; // ¡IMPORTANTE! Al hacer return, el código de abajo no se ejecuta.
        }
        else
        {
            _estaDefendiendo = false;
            if (_animator != null) _animator.SetBool("Defender", false);
        }

        // 3. MOVIMIENTO (Flechas + Tecla Shift para Correr)
        float inputX = 0;
        if (Input.GetKey(KeyCode.RightArrow)) inputX = 1;
        else if (Input.GetKey(KeyCode.LeftArrow)) inputX = -1;

        float velocidadActual = velocidadCaminar;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            velocidadActual = velocidadCorrer; // Metemos el turbo
        }
        
        // Aplicar velocidad (usando la velocidad actual, ya sea caminar o correr)
        _rb.linearVelocity = new Vector2(inputX * velocidadActual, _rb.linearVelocity.y);

        // Girar al personaje (Flip) respetando su tamaño original en el Inspector
        if (inputX != 0) 
        {
            Vector3 escala = transform.localScale;
            if (inputX > 0) escala.x = Mathf.Abs(escala.x); // Mira a la derecha (positivo)
            else if (inputX < 0) escala.x = -Mathf.Abs(escala.x); // Mira a la izquierda (negativo)
            transform.localScale = escala;
        }

        // 4. SALTO (Barra Espaciadora)
        if (Input.GetKeyDown(KeyCode.Space) && _estaEnSuelo)
        {
            Saltar();
        }

        // 5. COMBATE (Teclas Z, X, C, A)
        if (Input.GetKeyDown(KeyCode.Z)) Atacar("Ataque1"); // Rápido
        if (Input.GetKeyDown(KeyCode.X)) Atacar("Ataque2"); // Fuerte
        if (Input.GetKeyDown(KeyCode.C)) Atacar("Ataque3"); // Especial
        if (Input.GetKeyDown(KeyCode.A)) Disparar();        // Distancia
        
        // Actualizar Animator
        if (_animator != null)
        {
            // Multiplicamos el input por la velocidad para que el Animator sepa si anda o corre
            _animator.SetFloat("Velocidad", Mathf.Abs(inputX * velocidadActual));
            _animator.SetBool("EnSuelo", _estaEnSuelo);
        }
    }

    void Saltar()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0); 
        _rb.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);

        // Activamos la animación de salto
        if (_animator != null) _animator.SetTrigger("Saltar");
    }

    void Atacar(string triggerAnimacion)
    {
        if (_animator != null) _animator.SetTrigger(triggerAnimacion);

        // Detectar enemigos en el rango
        Collider2D[] enemigosGolpeados = Physics2D.OverlapCircleAll(puntoAtaque.position, rangoAtaque, capaEnemigos);

        foreach (Collider2D enemigo in enemigosGolpeados)
        {
            // Aquí destruimos al enemigo
            Destroy(enemigo.gameObject); 
        }
    }

    void Disparar()
    {
        if (_animator != null) _animator.SetTrigger("Disparar");

        if (prefabDardo != null && puntoDisparo != null)
        {
            // Instanciar el dardo
            GameObject dardo = Instantiate(prefabDardo, puntoDisparo.position, Quaternion.identity);
            
            // Hacer que el dardo mire hacia donde mira el ninja
            dardo.transform.localScale = transform.localScale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (puntoAtaque != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(puntoAtaque.position, rangoAtaque);
        }
        if (puntoSuelo != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(puntoSuelo.position, radioSuelo);
        }
    }
}