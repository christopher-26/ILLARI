using System.Collections;
using UnityEngine;

/// <summary>
/// Controlador de plataformas y parkour 2D para 'Iyari'.
/// Físicas en Unity 6 con linearVelocity, gravedad estándar (gravityScale = 3.2),
/// salto tradicional con Coyote Time, Jump Buffer y Doble Salto acrobático.
/// Salto gestionado vía Animator (parámetro bool 'estaEnAire') SIN rotar el Transform por código,
/// asegurando disparo 100% horizontal y certero en todo momento.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class IllariPlatformer : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 9f;

    [Header("Salto y Físicas Parkour")]
    [SerializeField] private float fuerzaSalto = 15f;
    [SerializeField] private float fuerzaDobleSalto = 13.5f;
    [SerializeField] private bool permitirDobleSalto = true;
    [SerializeField] private float gravedadEstandar = 3.2f;
    [SerializeField] private float tiempoCoyote = 0.15f;
    [SerializeField] private float tiempoBufferSalto = 0.12f;
    [SerializeField] private float distanciaRaycastSuelo = 0.25f;
    [SerializeField] private LayerMask capaSuelo;
    [SerializeField] private string tagSuelo = "Ground";

    [Header("Animación y Visual")]
    [SerializeField] private Transform visualTransform;
    [SerializeField] private Animator animator;

    [Header("Sistema de Disparo (Proyectil de Agua)")]
    [SerializeField] private GameObject prefabProyectilAgua;
    [SerializeField] private Transform puntoDisparo;
    [SerializeField] private float cadenciaDisparo = 0.22f;

    [Header("Salud e Invulnerabilidad")]
    [SerializeField] private float tiempoInvulnerabilidad = 1.0f;
    [SerializeField] private int parpadeosInvulnerabilidad = 5;
    [SerializeField] private float limiteCaidaVacioY = -12f;

    [Header("Color Prototipo")]
    [SerializeField] private Color colorJugador = Color.white; // Blanco por defecto si tiene sprite propio

    public enum TipoPoder
    {
        Normal,
        DisparoTriple,
        LaserAgua,
        BombaAgua
    }

    [Header("Sistema de Poderes y Mejoras")]
    [SerializeField] private TipoPoder poderActivo = TipoPoder.Normal;
    [SerializeField] private int disparosPoderRestantes = 0;
    [SerializeField] private bool tieneEscudo = false;
    [SerializeField] private float tiempoSuperVelocidadRestante = 0f;
    private float tiempoCargaDisparo = 0f;
    private const float TIEMPO_CARGA_MEGA = 0.70f;
    private GameObject objEscudoVisual;
    private GameObject objAuraCargaVisual;

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private SpriteRenderer visualSpriteRenderer;

    private float inputHorizontal = 0f;
    private float inputVertical = 0f;
    private bool apuntandoArriba = false;
    private bool mirandoDerecha = true;
    private bool isGrounded = false;
    private float contadorCoyote = 0f;
    private float contadorBufferSalto = 0f;
    private bool puedeDobleSalto = false;
    private bool esInvulnerable = false;
    private float tiempoProximoDisparo = 0f;
    private float velocidadAnimSmooth = 0f;
    private Vector3 escalaVisualOriginal;
    private bool estaMuerta = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();

        if (visualTransform == null)
        {
            Transform hijoVisual = transform.Find("Visual");
            visualTransform = (hijoVisual != null) ? hijoVisual : transform;
        }

        visualSpriteRenderer = visualTransform.GetComponent<SpriteRenderer>();
        if (visualSpriteRenderer == null)
        {
            visualSpriteRenderer = visualTransform.gameObject.AddComponent<SpriteRenderer>();
        }

        AsegurarCapasRenderizado();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null) animator = visualTransform.GetComponent<Animator>();
        }

        escalaVisualOriginal = visualTransform.localScale;

        if (visualTransform != null && visualTransform != transform)
        {
            // Garantizar centrado horizontal absoluto del transform visual con respecto al BoxCollider2D
            visualTransform.localPosition = new Vector3(0f, visualTransform.localPosition.y, visualTransform.localPosition.z);
        }

        // Físicas Unity 6
        rb.gravityScale = gravedadEstandar;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;

        if (col != null && col.sharedMaterial == null)
        {
            var matFriccion = Resources.Load<PhysicsMaterial2D>("Frictionless2D");
            if (matFriccion == null)
            {
                matFriccion = new PhysicsMaterial2D("Illari_CeroFriccion") { friction = 0f, bounciness = 0f };
            }
            col.sharedMaterial = matFriccion;
        }

        // Garantizar que capaSuelo detecte tanto la capa 'Ground' como 'Default' (para objetos con tag Ground)
        int capaGroundIdx = LayerMask.NameToLayer("Ground");
        if (capaGroundIdx != -1)
        {
            capaSuelo |= (1 << capaGroundIdx);
        }
        capaSuelo |= (1 << 0); // Capa 0 (Default)
    }

    public void AsegurarCapasRenderizado()
    {
        if (visualSpriteRenderer != null)
        {
            visualSpriteRenderer.sortingLayerName = "Props_Front";
            visualSpriteRenderer.sortingOrder = 100;
        }
        else if (visualTransform != null)
        {
            var sr = visualTransform.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                visualSpriteRenderer = sr;
                sr.sortingLayerName = "Props_Front";
                sr.sortingOrder = 100;
            }
        }
    }

    private void Start()
    {
        if (visualTransform != null)
        {
            escalaVisualOriginal = visualTransform.localScale;
        }
        AsegurarCapasRenderizado();
    }

    public void ConfigurarEscalaVisual(Vector3 escala)
    {
        escalaVisualOriginal = escala;
        if (visualTransform != null)
        {
            float signoX = mirandoDerecha ? 1f : -1f;
            visualTransform.localScale = new Vector3(signoX * Mathf.Abs(escalaVisualOriginal.x), escalaVisualOriginal.y, escalaVisualOriginal.z);
        }
    }

    private void Update()
    {
        if (estaMuerta) return;

        if (tiempoSuperVelocidadRestante > 0f)
        {
            tiempoSuperVelocidadRestante -= Time.deltaTime;
            if (tiempoSuperVelocidadRestante <= 0f && GameManagerIyari.Instancia != null)
            {
                tiempoSuperVelocidadRestante = 0f;
                string hudNom = (poderActivo == TipoPoder.Normal) ? "NORMAL" : (poderActivo == TipoPoder.DisparoTriple ? "TRIPLE" : (poderActivo == TipoPoder.LaserAgua ? "LASER" : "BOMBA"));
                GameManagerIyari.Instancia.ActualizarPoderHUD(hudNom, disparosPoderRestantes);
            }
        }

        ProcesarEntrada();
        VerificarSuelo();
        GestionarSaltoParkour();
        GestionarAnimacion();
        GestionarOrientacion();
        GestionarDisparo();
        VerificarCaidaVacio();
    }

    private void FixedUpdate()
    {
        if (estaMuerta)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float velEfectiva = (tiempoSuperVelocidadRestante > 0f) ? (velocidad * 1.35f) : velocidad;
        rb.linearVelocity = new Vector2(inputHorizontal * velEfectiva, rb.linearVelocity.y);
    }

    private void ProcesarEntrada()
    {
        float horizontal = 0f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            horizontal = 1f;
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            horizontal = -1f;
        }
        inputHorizontal = horizontal;

        float vertical = 0f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            vertical = 1f;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            vertical = -1f;
        }
        inputVertical = vertical;

        // Detectar si el jugador presiona botón de salto (Espacio, W, Flecha Arriba o botón configurado Jump)
        bool saltoPresionado = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetButtonDown("Jump");
        bool saltoSoltado = Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetButtonUp("Jump");

        // Detectar si está apuntando hacia arriba con teclado (solo si no es un intento de salto)
        bool teclaArribaPresionada = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        apuntandoArriba = teclaArribaPresionada && !saltoPresionado && isGrounded && Mathf.Abs(inputHorizontal) < 0.1f;

        if (inputHorizontal > 0.01f) mirandoDerecha = true;
        else if (inputHorizontal < -0.01f) mirandoDerecha = false;

        if (saltoPresionado)
        {
            contadorBufferSalto = tiempoBufferSalto;
        }
        else
        {
            contadorBufferSalto -= Time.deltaTime;
        }

        if (saltoSoltado && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.55f);
        }
    }

    private void VerificarSuelo()
    {
        Bounds bounds = col.bounds;
        Vector2 centro = bounds.center;
        float anchoMitad = bounds.extents.x * 0.78f;

        Vector2 origenIzq = new Vector2(centro.x - anchoMitad, bounds.min.y);
        Vector2 origenCentro = new Vector2(centro.x, bounds.min.y);
        Vector2 origenDer = new Vector2(centro.x + anchoMitad, bounds.min.y);

        float distanciaChequeo = Mathf.Max(distanciaRaycastSuelo, 0.35f);

        bool tocando = RaycastDetectaSuelo(origenIzq, distanciaChequeo) ||
                       RaycastDetectaSuelo(origenCentro, distanciaChequeo) ||
                       RaycastDetectaSuelo(origenDer, distanciaChequeo) ||
                       VerificarColisionDirectaSuelo();

        isGrounded = tocando;

        if (isGrounded)
        {
            contadorCoyote = tiempoCoyote;
            puedeDobleSalto = true;
        }
        else
        {
            contadorCoyote -= Time.deltaTime;
        }
    }

    private bool RaycastDetectaSuelo(Vector2 origen, float distancia)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origen, Vector2.down, distancia, capaSuelo);
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit2D hit = hits[i];
            if (hit.collider == null || hit.collider.gameObject == gameObject || hit.collider.isTrigger)
                continue;

            if (hit.collider.CompareTag(tagSuelo)) return true;

            int capaGround = LayerMask.NameToLayer("Ground");
            if (capaGround != -1 && hit.collider.gameObject.layer == capaGround) return true;

            if (hit.collider.GetComponent<PlatformEffector2D>() != null) return true;
        }
        return false;
    }

    private bool VerificarColisionDirectaSuelo()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = capaSuelo;
        filter.useTriggers = false;
        RaycastHit2D[] castHits = new RaycastHit2D[8];
        int count = col.Cast(Vector2.down, filter, castHits, 0.18f);
        for (int i = 0; i < count; i++)
        {
            var hCol = castHits[i].collider;
            if (hCol == null || hCol.gameObject == gameObject || hCol.isTrigger) continue;
            if (hCol.CompareTag(tagSuelo)) return true;
            int capaGround = LayerMask.NameToLayer("Ground");
            if (capaGround != -1 && hCol.gameObject.layer == capaGround) return true;
            if (hCol.GetComponent<PlatformEffector2D>() != null) return true;
        }
        return false;
    }

    private void GestionarSaltoParkour()
    {
        if (contadorBufferSalto > 0f && contadorCoyote > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
            contadorBufferSalto = 0f;
            contadorCoyote = 0f;
            AudioManager.Instancia?.ReproducirSFX("salto");
            if (animator != null)
            {
                animator.SetBool("estaEnAire", true);
                animator.Play("Iyari_Jump", -1, 0f);
            }
        }
        else if (contadorBufferSalto > 0f && permitirDobleSalto && puedeDobleSalto && !isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaDobleSalto);
            puedeDobleSalto = false;
            contadorBufferSalto = 0f;
            AudioManager.Instancia?.ReproducirSFX("salto");
            if (animator != null)
            {
                animator.SetBool("estaEnAire", true);
                animator.SetTrigger("dobleJump");
                animator.Play("Iyari_DoubleJump", -1, 0f);
            }
        }
    }

    /// <summary>
    /// Comunica al Animator el estado en aire mediante el parámetro 'estaEnAire'.
    /// El Transform NUNCA se rota por código para garantizar disparo recto.
    /// </summary>
    private void GestionarAnimacion()
    {
        if (animator != null)
        {
            animator.SetBool("estaEnAire", !isGrounded);
            animator.SetBool("apuntandoArriba", apuntandoArriba);
            float velTarget = Mathf.Max(Mathf.Abs(inputHorizontal), Mathf.Abs(rb.linearVelocity.x) / Mathf.Max(0.1f, velocidad));
            velocidadAnimSmooth = Mathf.Lerp(velocidadAnimSmooth, velTarget, Time.deltaTime * 14f);
            animator.SetFloat("velocidadX", velocidadAnimSmooth);

            // Postura de apuntado vertical en reposo (solo si no está saltando y está firme en suelo)
            if (isGrounded && apuntandoArriba && Mathf.Abs(inputHorizontal) < 0.1f && contadorBufferSalto <= 0f && !animator.GetCurrentAnimatorStateInfo(0).IsName("Iyari_ShootUp"))
            {
                animator.Play("Iyari_ShootUp", 0, 0f);
            }
        }

        // Mantenemos la rotación fija en 0°
        if (visualTransform != null)
        {
            visualTransform.localRotation = Quaternion.identity;
        }
    }

    private void GestionarOrientacion()
    {
        if (visualTransform == null) return;

        float signoX = mirandoDerecha ? 1f : -1f;
        visualTransform.localScale = new Vector3(signoX * Mathf.Abs(escalaVisualOriginal.x), escalaVisualOriginal.y, escalaVisualOriginal.z);
    }

    private void GestionarDisparo()
    {
        // Solo consideramos bloqueo de ratón si el cursor está sobre un elemento UI
        bool mouseSobreUI = false;
        if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            mouseSobreUI = true;
        }

        bool presionoTeclado = Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X) ||
                               Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.F) ||
                               Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);
        bool presionoMouse = !mouseSobreUI && (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetButtonDown("Fire1"));
        bool disparoPresionado = presionoTeclado || presionoMouse;

        bool sosteniendoTeclado = Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.X) ||
                                  Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.F) ||
                                  Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool sosteniendoMouse = !mouseSobreUI && (Input.GetKey(KeyCode.Mouse0) || Input.GetButton("Fire1"));
        bool sosteniendo = sosteniendoTeclado || sosteniendoMouse;

        bool soltoTeclado = Input.GetKeyUp(KeyCode.J) || Input.GetKeyUp(KeyCode.Z) || Input.GetKeyUp(KeyCode.X) ||
                            Input.GetKeyUp(KeyCode.C) || Input.GetKeyUp(KeyCode.F) ||
                            Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl);
        bool soltoMouse = Input.GetKeyUp(KeyCode.Mouse0) || Input.GetButtonUp("Fire1");
        bool solto = soltoTeclado || soltoMouse;

        // 1. Disparo inmediato reactivo al pulsar
        if (disparoPresionado && Time.time >= tiempoProximoDisparo)
        {
            tiempoProximoDisparo = Time.time + cadenciaDisparo;
            DispararProyectil();
        }

        // 2. Carga y disparo continuo mientras se mantiene presionado
        if (sosteniendo)
        {
            tiempoCargaDisparo += Time.deltaTime;
            if (tiempoCargaDisparo >= TIEMPO_CARGA_MEGA)
            {
                ActualizarVisualCarga(true);
            }
            else if (Time.time >= tiempoProximoDisparo && tiempoCargaDisparo < TIEMPO_CARGA_MEGA * 0.45f)
            {
                tiempoProximoDisparo = Time.time + cadenciaDisparo;
                DispararProyectil();
            }
        }

        // 3. Liberación de Mega Disparo
        if (solto)
        {
            if (tiempoCargaDisparo >= TIEMPO_CARGA_MEGA)
            {
                DispararMegaBala();
            }
            tiempoCargaDisparo = 0f;
            ActualizarVisualCarga(false);
        }
    }

    private (Vector2 direccion, bool haciaArriba, bool esDiagonal) CalcularDireccionDisparo()
    {
        bool disparoMouse = Input.GetKey(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.Mouse0) || Input.GetButton("Fire1") || Input.GetButtonDown("Fire1");
        if (disparoMouse && Camera.main != null)
        {
            Vector3 mouseMundo = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 deltaMouse = (Vector2)mouseMundo - (Vector2)transform.position;

            if (deltaMouse.x > 0.15f) mirandoDerecha = true;
            else if (deltaMouse.x < -0.15f) mirandoDerecha = false;

            if (deltaMouse.sqrMagnitude > 0.1f)
            {
                bool haciaArriba = deltaMouse.y > 0.35f;
                bool esDiagonal = haciaArriba && Mathf.Abs(deltaMouse.x) > 0.25f;
                return (deltaMouse.normalized, haciaArriba, esDiagonal);
            }
        }

        bool teclaArribaPresionada = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || apuntandoArriba;
        if (teclaArribaPresionada)
        {
            if (Mathf.Abs(inputHorizontal) > 0.1f)
            {
                return (new Vector2(mirandoDerecha ? 0.707f : -0.707f, 0.707f).normalized, true, true);
            }
            return (Vector2.up, true, false);
        }

        return (mirandoDerecha ? Vector2.right : Vector2.left, false, false);
    }

    private Vector2 ObtenerPuntoSpawnDisparo(bool haciaArriba = false, bool esDiagonal = false)
    {
        if (haciaArriba)
        {
            if (esDiagonal)
            {
                float xOffDiag = mirandoDerecha ? 0.42f : -0.42f;
                return (Vector2)transform.position + new Vector2(xOffDiag, 1.05f);
            }
            float xOff = mirandoDerecha ? 0.12f : -0.12f;
            return (Vector2)transform.position + new Vector2(xOff, 1.25f);
        }

        if (puntoDisparo != null)
        {
            return (Vector2)transform.position + new Vector2(mirandoDerecha ? Mathf.Abs(puntoDisparo.localPosition.x) : -Mathf.Abs(puntoDisparo.localPosition.x), puntoDisparo.localPosition.y);
        }
        return (Vector2)transform.position + new Vector2(mirandoDerecha ? 0.7f : -0.7f, 0.65f);
    }

    private void DispararProyectil()
    {
        if (prefabProyectilAgua == null) return;
        AudioManager.Instancia?.ReproducirSFX("disparo");

        var dirInfo = CalcularDireccionDisparo();
        bool haciaArriba = dirInfo.haciaArriba;
        bool esDiagonal = dirInfo.esDiagonal;
        Vector2 direccion = dirInfo.direccion;

        Vector2 puntoSpawn = ObtenerPuntoSpawnDisparo(haciaArriba, esDiagonal);

        if (poderActivo == TipoPoder.DisparoTriple && disparosPoderRestantes > 0)
        {
            disparosPoderRestantes--;
            DispararTriple(direccion, puntoSpawn);

            if (disparosPoderRestantes <= 0)
            {
                poderActivo = TipoPoder.Normal;
                if (GameManagerIyari.Instancia != null) GameManagerIyari.Instancia.ActualizarPoderHUD("NORMAL", 0);
            }
            else if (GameManagerIyari.Instancia != null)
            {
                GameManagerIyari.Instancia.ActualizarPoderHUD("TRIPLE", disparosPoderRestantes);
            }
        }
        else if (poderActivo == TipoPoder.LaserAgua && disparosPoderRestantes > 0)
        {
            disparosPoderRestantes--;
            GameObject balaObj = Instantiate(prefabProyectilAgua, puntoSpawn, Quaternion.identity);
            Bala bala = balaObj.GetComponent<Bala>();
            if (bala != null) bala.InicializarLaser(direccion, 2);

            if (disparosPoderRestantes <= 0)
            {
                poderActivo = TipoPoder.Normal;
                if (GameManagerIyari.Instancia != null) GameManagerIyari.Instancia.ActualizarPoderHUD("NORMAL", 0);
            }
            else if (GameManagerIyari.Instancia != null)
            {
                GameManagerIyari.Instancia.ActualizarPoderHUD("LASER", disparosPoderRestantes);
            }
        }
        else if (poderActivo == TipoPoder.BombaAgua && disparosPoderRestantes > 0)
        {
            disparosPoderRestantes--;
            GameObject balaObj = Instantiate(prefabProyectilAgua, puntoSpawn, Quaternion.identity);
            Bala bala = balaObj.GetComponent<Bala>();
            if (bala != null) bala.InicializarGranada(direccion, 3, 2.6f);

            if (disparosPoderRestantes <= 0)
            {
                poderActivo = TipoPoder.Normal;
                if (GameManagerIyari.Instancia != null) GameManagerIyari.Instancia.ActualizarPoderHUD("NORMAL", 0);
            }
            else if (GameManagerIyari.Instancia != null)
            {
                GameManagerIyari.Instancia.ActualizarPoderHUD("BOMBA", disparosPoderRestantes);
            }
        }
        else
        {
            GameObject balaObj = Instantiate(prefabProyectilAgua, puntoSpawn, Quaternion.identity);
            Bala bala = balaObj.GetComponent<Bala>();
            if (bala != null)
            {
                bala.Inicializar(direccion, false);
            }
        }

        DispararAnimacion(haciaArriba);
        StartCoroutine(RutinaRetrocesoDisparo(false, haciaArriba));
    }

    private void DispararTriple(Vector2 dirBase, Vector2 pos)
    {
        float[] angulos = new float[] { -15f, 0f, 15f };
        for (int i = 0; i < angulos.Length; i++)
        {
            float angRad = angulos[i] * Mathf.Deg2Rad;
            Vector2 dirMod = new Vector2(
                dirBase.x * Mathf.Cos(angRad) - dirBase.y * Mathf.Sin(angRad),
                dirBase.x * Mathf.Sin(angRad) + dirBase.y * Mathf.Cos(angRad)
            );

            GameObject balaObj = Instantiate(prefabProyectilAgua, pos, Quaternion.identity);
            Bala bala = balaObj.GetComponent<Bala>();
            if (bala != null)
            {
                bala.InicializarEspecial(dirMod.normalized, 1, false, false);
            }
        }
    }

    private void DispararMegaBala()
    {
        if (prefabProyectilAgua == null) return;
        AudioManager.Instancia?.ReproducirSFX("disparo");

        var dirInfo = CalcularDireccionDisparo();
        Vector2 direccion = dirInfo.direccion;
        bool haciaArriba = dirInfo.haciaArriba;
        bool esDiagonal = dirInfo.esDiagonal;

        Vector2 puntoSpawn = ObtenerPuntoSpawnDisparo(haciaArriba, esDiagonal);

        GameObject balaObj = Instantiate(prefabProyectilAgua, puntoSpawn, Quaternion.identity);
        Bala bala = balaObj.GetComponent<Bala>();
        if (bala != null)
        {
            // Daño masivo 3, escala enorme, atraviesa enemigos y neutraliza bolas de barro
            bala.InicializarEspecial(direccion, 3, true, true);
        }

        if (GameManagerIyari.Instancia != null)
        {
            GameManagerIyari.Instancia.MostrarMensajeAviso("¡MEGA CHORRO PURIFICADOR DISPARADO!");
        }

        DispararAnimacion(haciaArriba);
        StartCoroutine(RutinaRetrocesoDisparo(true, haciaArriba));
    }

    private void DispararAnimacion(bool haciaArriba = false)
    {
        if (animator != null)
        {
            animator.SetTrigger("disparar");
            if (haciaArriba)
            {
                if (!isGrounded)
                {
                    animator.Play("Iyari_ShootAirUp", 0, 0f);
                }
                else
                {
                    animator.Play("Iyari_ShootUp", 0, 0f);
                }
            }
            else
            {
                if (!isGrounded)
                {
                    animator.Play("Iyari_ShootAir", 0, 0f);
                }
                else
                {
                    animator.Play("Iyari_Shoot", 0, 0f);
                }
            }
        }
    }

    private void ActualizarVisualCarga(bool cargado)
    {
        if (objAuraCargaVisual == null)
        {
            objAuraCargaVisual = new GameObject("Visual_Aura_Carga");
            objAuraCargaVisual.transform.SetParent(visualTransform != null ? visualTransform : transform, false);
            objAuraCargaVisual.transform.localPosition = new Vector3(0.5f, 0.65f, 0f);

            var sr = objAuraCargaVisual.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("UI/IconoBotellaHUD");
            sr.color = new Color(0f, 1f, 0.95f, 0.85f);
            sr.sortingLayerName = "Props_Front";
            sr.sortingOrder = 106;
        }

        objAuraCargaVisual.SetActive(cargado);
        if (cargado)
        {
            float pulso = Mathf.Sin(Time.time * 16f) * 0.25f + 1f;
            objAuraCargaVisual.transform.localScale = new Vector3(pulso * 0.9f, pulso * 0.9f, 1f);
        }
    }

    public void ActivarPoderDisparoTriple(int cantidad = 30)
    {
        poderActivo = TipoPoder.DisparoTriple;
        disparosPoderRestantes = cantidad;
        if (GameManagerIyari.Instancia != null)
        {
            GameManagerIyari.Instancia.ActualizarPoderHUD("TRIPLE", disparosPoderRestantes);
        }
    }

    public void ActivarPoderLaserAgua(int cantidad = 25)
    {
        poderActivo = TipoPoder.LaserAgua;
        disparosPoderRestantes = cantidad;
        if (GameManagerIyari.Instancia != null)
        {
            GameManagerIyari.Instancia.ActualizarPoderHUD("LASER", disparosPoderRestantes);
        }
    }

    public void ActivarPoderBombaAgua(int cantidad = 15)
    {
        poderActivo = TipoPoder.BombaAgua;
        disparosPoderRestantes = cantidad;
        if (GameManagerIyari.Instancia != null)
        {
            GameManagerIyari.Instancia.ActualizarPoderHUD("BOMBA", disparosPoderRestantes);
        }
    }

    public void ActivarSuperVelocidad(float duracion = 18f)
    {
        tiempoSuperVelocidadRestante = duracion;
        permitirDobleSalto = true;
        if (GameManagerIyari.Instancia != null)
        {
            GameManagerIyari.Instancia.ActualizarPoderHUD("TURBO", (int)duracion);
        }
    }

    public void ActivarEscudoAcuatico()
    {
        tieneEscudo = true;
        GarantizarVisualEscudo(true);
        if (GameManagerIyari.Instancia != null)
        {
            GameManagerIyari.Instancia.ActualizarPoderHUD("ESCUDO", 1);
        }
    }

    public void ActivarDobleSalto()
    {
        permitirDobleSalto = true;
    }

    public bool TieneEscudo() => tieneEscudo;

    private void GarantizarVisualEscudo(bool activo)
    {
        if (objEscudoVisual == null)
        {
            objEscudoVisual = new GameObject("Visual_Escudo_Burbuja");
            objEscudoVisual.transform.SetParent(transform, false);
            objEscudoVisual.transform.localPosition = new Vector3(0f, 0.68f, 0f);
            objEscudoVisual.transform.localScale = new Vector3(1.75f, 1.75f, 1f);

            var sr = objEscudoVisual.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("UI/IconoBotellaHUD");
            sr.color = new Color(0.1f, 0.85f, 1f, 0.6f);
            sr.sortingLayerName = "Props_Front";
            sr.sortingOrder = 104;
        }

        objEscudoVisual.SetActive(activo);
    }

    private IEnumerator RutinaRetrocesoDisparo(bool esMega = false, bool haciaArriba = false)
    {
        if (visualTransform == null) yield break;

        Vector3 posOriginal = Vector3.zero;
        Vector3 posRetroceso;
        if (haciaArriba)
        {
            // Retroceso hacia abajo por la fuerza de reacción del chorro ascendente
            float fuerzaY = esMega ? -0.22f : -0.10f;
            posRetroceso = new Vector3(0f, fuerzaY, 0f);
        }
        else
        {
            float fuerzaRetroceso = esMega ? 0.28f : 0.12f;
            posRetroceso = new Vector3(mirandoDerecha ? -fuerzaRetroceso : fuerzaRetroceso, esMega ? 0.06f : 0.02f, 0f);
        }
        visualTransform.localPosition = posRetroceso;

        float duracion = esMega ? 0.16f : 0.09f;
        float elapsed = 0f;
        while (elapsed < duracion)
        {
            elapsed += Time.deltaTime;
            visualTransform.localPosition = Vector3.Lerp(posRetroceso, posOriginal, elapsed / duracion);
            yield return null;
        }
        visualTransform.localPosition = posOriginal;
    }

    public void RecibirDanio(int cantidad)
    {
        if (esInvulnerable || estaMuerta) return;

        // Si tiene Escudo Acuático activo, absorbe completamente el impacto
        if (tieneEscudo)
        {
            tieneEscudo = false;
            GarantizarVisualEscudo(false);

            if (GameManagerIyari.Instancia != null)
            {
                GameManagerIyari.Instancia.MostrarMensajeAviso("¡EL ESCUDO ACUÁTICO ABSORBIÓ EL IMPACTO!");
                GameManagerIyari.Instancia.ActualizarPoderHUD("NORMAL", 0);
            }

            StartCoroutine(RutinaInvulnerabilidad());
            return;
        }

        if (GameManagerIyari.Instancia != null)
        {
            GameManagerIyari.Instancia.ModificarVida(-cantidad);
            if (GameManagerIyari.Instancia.ObtenerVida() <= 0)
            {
                Morir();
                return;
            }
        }

        if (animator != null)
        {
            animator.SetTrigger("recibirDanio");
        }

        StartCoroutine(RutinaInvulnerabilidad());
    }

    public void Morir()
    {
        if (estaMuerta) return;
        estaMuerta = true;
        rb.linearVelocity = Vector2.zero;
        AudioManager.Instancia?.ReproducirSFX("danio");

        if (animator != null)
        {
            animator.SetTrigger("morir");
            animator.Play("Iyari_Death", -1, 0f);
        }
    }

    private void VerificarCaidaVacio()
    {
        if (transform.position.y < limiteCaidaVacioY && !estaMuerta)
        {
            Morir();
            if (GameManagerIyari.Instancia != null)
            {
                GameManagerIyari.Instancia.ModificarVida(-999);
            }
        }
    }

    private IEnumerator RutinaInvulnerabilidad()
    {
        esInvulnerable = true;
        float tiempoPaso = tiempoInvulnerabilidad / (parpadeosInvulnerabilidad * 2);

        if (visualSpriteRenderer != null)
        {
            Color colorOriginal = visualSpriteRenderer.color;
            Color colorDanio = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, 0.25f);

            for (int i = 0; i < parpadeosInvulnerabilidad; i++)
            {
                visualSpriteRenderer.color = colorDanio;
                yield return new WaitForSeconds(tiempoPaso);
                visualSpriteRenderer.color = colorOriginal;
                yield return new WaitForSeconds(tiempoPaso);
            }

            visualSpriteRenderer.color = colorOriginal;
        }
        else
        {
            yield return new WaitForSeconds(tiempoInvulnerabilidad);
        }

        esInvulnerable = false;
    }
}
