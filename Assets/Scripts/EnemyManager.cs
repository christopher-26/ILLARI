using UnityEngine;



/// <summary>

/// Controlador versátil de enemigos para 'Iyari'.

/// Arquetipos:

/// 1. Patrullero: Patrulla con detección de obstáculos. Si ve al jugador frente a él,

///    entra en modo PERSECUCIÓN corriendo hacia él, respetando los bordes para no caer al vacío.

/// 2. Tirador: Estático, apunta y dispara ráfagas cada 2 segundos al entrar en rango.

/// </summary>

[RequireComponent(typeof(Rigidbody2D))]

[RequireComponent(typeof(Collider2D))]

public class EnemyManager : MonoBehaviour

{

    public enum TipoEnemigo { Patrullero, Tirador }

    public enum EstadoPatrulla { Patrullando, Persiguiendo }



    [Header("Tipo de Comportamiento")]

    [SerializeField] private TipoEnemigo tipo = TipoEnemigo.Patrullero;



    [Header("Color Prototipo")]

    [SerializeField] private Color colorEnemigo = Color.white;



    [Header("Salud y Recompensas")]

    [SerializeField] private int salud = 2;

    [SerializeField] private GameObject prefabEfectoMuerte;

    [SerializeField] private GameObject prefabBotellaDrop;



    [Header("Configuración Patrullero y Persecución")]

    [SerializeField] private float velocidadPatrulla = 1.8f;

    [SerializeField] private float velocidadPersecucion = 3.2f;

    [SerializeField] private float rangoVisionPersecucion = 7.0f;

    [SerializeField] private float distanciaPared = 0.5f;

    [SerializeField] private float distanciaVacio = 1.0f;

    [SerializeField] private LayerMask capaObstaculosSuelo;

    [SerializeField] private string tagSuelo = "Ground";

    [SerializeField] private int danioContacto = 1;



    [Header("Configuración Tirador (Bolas de Barro)")]

    [SerializeField] private float rangoVisionTirador = 8.5f;

    [SerializeField] private float cadenciaDisparo = 2.5f;

    [SerializeField] private GameObject prefabBalaEnemiga;

    [SerializeField] private Transform puntoDisparo;



    private Rigidbody2D rb;

    private Collider2D col;

    private SpriteRenderer spriteRenderer;

    private Transform transformJugador;



    private EstadoPatrulla estadoActual = EstadoPatrulla.Patrullando;

    private float direccionX = 1f;

    private float tiempoProximoDisparo = 0f;

    private Vector3 escalaInicial;



    private Transform transVisual;
    private bool estaMuerto = false;



    private void Awake()

    {

        rb = GetComponent<Rigidbody2D>();

        col = GetComponent<Collider2D>();

        spriteRenderer = GetComponent<SpriteRenderer>();



        if (spriteRenderer == null)

        {

            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        }



        if (spriteRenderer != null && colorEnemigo != Color.white)

        {

            spriteRenderer.color = colorEnemigo;

        }



        transVisual = transform.Find("Visual");



        escalaInicial = (transVisual != null) ? transVisual.localScale : transform.localScale;

        rb.freezeRotation = true;

    }



    private void Start()

    {

        GameObject jugadorObj = GameObject.FindWithTag("Player");

        if (jugadorObj != null)

        {

            transformJugador = jugadorObj.transform;

        }



        if (tipo == TipoEnemigo.Tirador)

        {

            rb.bodyType = RigidbodyType2D.Kinematic;

            rb.linearVelocity = Vector2.zero;

        }

    }



    private void Update()

    {

        if (estaMuerto) return;



        if (tipo == TipoEnemigo.Tirador)

        {

            ComportamientoTirador();

        }

        else

        {

            VerificarPersecucion();

        }



        AnimarCaminataLodo();

    }



    /// <summary>

    /// Animación orgánica de caminata para los monstruos de barro:

    /// Aplica deformación viscosa (squash & stretch) y balanceo al dar pasos en el lodo.

    /// </summary>

    private void AnimarCaminataLodo()

    {

        bool enMovimiento = (tipo == TipoEnemigo.Patrullero) && Mathf.Abs(rb.linearVelocity.x) > 0.05f;





        if (transVisual == null) return;



        float signoX = direccionX > 0 ? 1f : -1f;



        if (enMovimiento)

        {

            // Pasos viscosos: squash & stretch rítmico y ligero tambaleo

            float paso = Mathf.Sin(Time.time * 8.5f);

            float deformacionY = paso * 0.08f;

            float deformacionX = -paso * 0.06f;

            float bamboleoZ = paso * 3.5f;



            transVisual.localScale = new Vector3(

                signoX * Mathf.Abs(escalaInicial.x) * (1f + deformacionX),

                escalaInicial.y * (1f + deformacionY),

                escalaInicial.z

            );

            transVisual.localRotation = Quaternion.Euler(0f, 0f, bamboleoZ);

        }

        else

        {

            // Respiración lenta de lodo en reposo

            float respiracion = Mathf.Sin(Time.time * 2.5f) * 0.025f;

            transVisual.localScale = new Vector3(

                signoX * Mathf.Abs(escalaInicial.x) * (1f - respiracion),

                escalaInicial.y * (1f + respiracion),

                escalaInicial.z

            );

            transVisual.localRotation = Quaternion.identity;

        }

    }



    private void FixedUpdate()

    {

        if (tipo == TipoEnemigo.Patrullero)

        {

            ComportamientoPatrullero();

        }

    }



    /// <summary>

    /// Detecta si el jugador está en su campo de visión para iniciar o cancelar persecución.

    /// </summary>

    private void VerificarPersecucion()

    {

        if (transformJugador == null) return;



        float distancia = Vector2.Distance(transform.position, transformJugador.position);

        float dirAlJugadorX = transformJugador.position.x - transform.position.x;



        if (estadoActual == EstadoPatrulla.Patrullando)

        {

            // Mira hacia el jugador y está dentro del rango

            bool mirandoAlJugador = (dirAlJugadorX * direccionX) > 0f;

            if (distancia <= rangoVisionPersecucion && mirandoAlJugador && Mathf.Abs(transformJugador.position.y - transform.position.y) < 2.5f)

            {

                estadoActual = EstadoPatrulla.Persiguiendo;

            }

        }

        else if (estadoActual == EstadoPatrulla.Persiguiendo)

        {

            // Si el jugador escapa lejos, vuelve a patrullar

            if (distancia > rangoVisionPersecucion * 1.5f)

            {

                estadoActual = EstadoPatrulla.Patrullando;

            }

            else

            {

                // Actualiza dirección hacia el jugador

                if (dirAlJugadorX > 0.1f) direccionX = 1f;

                else if (dirAlJugadorX < -0.1f) direccionX = -1f;



                ActualizarEscalaVisual();

            }

        }

    }



    private void ComportamientoPatrullero()

    {

        float velActual = (estadoActual == EstadoPatrulla.Persiguiendo) ? velocidadPersecucion : velocidadPatrulla;



        float radioX = (col != null) ? col.bounds.extents.x : 0.5f;

        float radioY = (col != null) ? col.bounds.extents.y : 0.65f;



        // Detección de muro (frente al collider)

        Vector2 origenFrente = (Vector2)transform.position + new Vector2(direccionX * (radioX + 0.15f), 0f);

        RaycastHit2D hitMuro = Physics2D.Raycast(origenFrente, Vector2.right * direccionX, distanciaPared, capaObstaculosSuelo);

        bool hayMuro = hitMuro.collider != null && hitMuro.collider.CompareTag(tagSuelo);



        // Detección de precipicio (un paso adelante proyectando hacia abajo desde el borde)

        Vector2 origenVacio = (Vector2)transform.position + new Vector2(direccionX * (radioX + 0.25f), -radioY + 0.25f);

        RaycastHit2D hitSueloBorde = Physics2D.Raycast(origenVacio, Vector2.down, distanciaVacio + 0.4f, capaObstaculosSuelo);

        bool hayVacio = hitSueloBorde.collider == null || !hitSueloBorde.collider.CompareTag(tagSuelo);



        if (estadoActual == EstadoPatrulla.Persiguiendo)

        {

            // En persecución, si llega a un precipicio o muro, se detiene para no caerse

            if (hayVacio || hayMuro)

            {

                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

                return;

            }

        }

        else

        {

            // En patrulla normal, al topar muro o vacío da media vuelta

            if (hayMuro || hayVacio)

            {

                direccionX *= -1f;

                ActualizarEscalaVisual();

            }

        }



        rb.linearVelocity = new Vector2(direccionX * velActual, rb.linearVelocity.y);

    }



    public void ConfigurarEscalaVisual(Vector3 escala)

    {

        escalaInicial = escala;

        ActualizarEscalaVisual();

    }



    private void ActualizarEscalaVisual()

    {

        Transform transVisual = transform.Find("Visual");

        Transform targetTrans = (transVisual != null) ? transVisual : transform;

        targetTrans.localScale = new Vector3(direccionX * Mathf.Abs(escalaInicial.x), escalaInicial.y, escalaInicial.z);

    }



    private void ComportamientoTirador()

    {

        if (transformJugador == null) return;



        float distancia = Vector2.Distance(transform.position, transformJugador.position);



        if (distancia <= rangoVisionTirador)

        {

            float dirAlJugadorX = transformJugador.position.x - transform.position.x;

            direccionX = dirAlJugadorX > 0 ? 1f : -1f;

            ActualizarEscalaVisual();



            if (Time.time >= tiempoProximoDisparo)

            {

                tiempoProximoDisparo = Time.time + cadenciaDisparo;

                DispararBalaEnemiga(dirAlJugadorX > 0 ? Vector2.right : Vector2.left);

            }

        }

    }



    private void DispararBalaEnemiga(Vector2 direccion)

    {

        if (estaMuerto) return;





        if (prefabBalaEnemiga == null) return;



        Vector2 origen = (puntoDisparo != null) ? (Vector2)puntoDisparo.position : (Vector2)transform.position + direccion * 0.7f;

        GameObject balaObj = Instantiate(prefabBalaEnemiga, origen, Quaternion.identity);



        Bala bala = balaObj.GetComponent<Bala>();

        if (bala != null)

        {

            bala.Inicializar(direccion, true);

        }

    }



    private void OnCollisionEnter2D(Collision2D collision)

    {

        if (tipo == TipoEnemigo.Patrullero && collision.gameObject.CompareTag("Player"))

        {

            IllariPlatformer jugador = collision.gameObject.GetComponent<IllariPlatformer>();

            if (jugador != null)

            {

                jugador.RecibirDanio(danioContacto);

            }

        }

    }



    public void RecibirDanio(int cantidad)

    {

        salud -= cantidad;



        if (salud <= 0)

        {

            if (prefabBotellaDrop != null)

            {

                Instantiate(prefabBotellaDrop, transform.position + Vector3.up * 0.4f, Quaternion.identity);

            }



            if (prefabEfectoMuerte != null)

            {

                Instantiate(prefabEfectoMuerte, transform.position, Quaternion.identity);

            }



            Destroy(gameObject);

        }

    }

}

