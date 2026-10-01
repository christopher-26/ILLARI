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
        DisparoTriple
    }

    [Header("Sistema de Poderes y Mejoras")]
    [SerializeField] private TipoPoder poderActivo = TipoPoder.Normal;
    [SerializeField] private int disparosPoderRestantes = 0;
    [SerializeField] private bool tieneEscudo = false;
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

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null) animator = visualTransform.GetComponent<Animator>();
        }

        escalaVisualOriginal = visualTransform.localScale;

        // Físicas Unity 6
        rb.gravityScale = gravedadEstandar;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        if (visualTransform != null)
        {
            escalaVisualOriginal = visualTransform.localScale;
        }
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

        rb.linearVelocity = new Vector2(inputHorizontal * velocidad, rb.linearVelocity.y);
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

        // Detectar si está apuntando hacia arriba (W, Flecha Arriba o cursor apuntando alto)
        apuntandoArriba = inputVertical > 0.4f;
        if (!apuntandoArriba && Camera.main != null && (Input.GetKey(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.Mouse0)))
        {
            Vector3 mouseMundo = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 deltaMouse = (Vector2)mouseMundo - (Vector2)transform.position;
            if (deltaMouse.y > 0.9f && Mathf.Abs(deltaMouse.x) < deltaMouse.y * 0.95f)
            {
                apuntandoArriba = true;
            }
        }

        if (inputHorizontal > 0.01f) mirandoDerecha = true;
        else if (inputHorizontal < -0.01f) mirandoDerecha = false;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            contadorBufferSalto = tiempoBufferSalto;
        }
        else
        {
            contadorBufferSalto -= Time.deltaTime;
        }

        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.55f);
        }
    }

    private void VerificarSuelo()
    {
        Bounds bounds = col.bounds;
        Vector2 centro = bounds.center;
        float anchoMitad = bounds.extents.x * 0.8f;

        Vector2 origenIzq = new Vector2(centro.x - anchoMitad, bounds.min.y);
        Vector2 origenDer = new Vector2(centro.x + anchoMitad, bounds.min.y);

        RaycastHit2D hitIzq = Physics2D.Raycast(origenIzq, Vector2.down, distanciaRaycastSuelo, capaSuelo);
        RaycastHit2D hitDer = Physics2D.Raycast(origenDer, Vector2.down, distanciaRaycastSuelo, capaSuelo);

        bool tocando = (hitIzq.collider != null && hitIzq.collider.CompareTag(tagSuelo)) ||
                       (hitDer.collider != null && hitDer.collider.CompareTag(tagSuelo));

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

    private void GestionarSaltoParkour()
    {
        if (contadorBufferSalto > 0f && contadorCoyote > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
            contadorBufferSalto = 0f;
            contadorCoyote = 0f;
            if (animator != null)
            {
                animator.SetBool("estaEnAire", true);
            }
        }
        else if (contadorBufferSalto > 0f && permitirDobleSalto && puedeDobleSalto && !isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaDobleSalto);
            puedeDobleSalto = false;
            contadorBufferSalto = 0f;
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

            // Postura de apuntado vertical en reposo
            if (isGrounded && apuntandoArriba && Mathf.Abs(inputHorizontal) < 0.1f && !animator.GetCurrentAnimatorStateInfo(0).IsName("Iyari_ShootUp"))
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
        // Si el puntero está sobre la interfaz gráfica (ej: botón de pausa), ignorar clic para no disparar
        if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            tiempoCargaDisparo = 0f;
            ActualizarVisualCarga(false);
            return;
        }

        bool sosteniendo = Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.Z);
        bool solto = Input.GetKeyUp(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.J) || Input.GetKeyUp(KeyCode.Z);

        if (sosteniendo)
        {
            tiempoCargaDisparo += Time.deltaTime;
            if (tiempoCargaDisparo >= TIEMPO_CARGA_MEGA)
            {
                ActualizarVisualCarga(true);
            }
        }

        if (solto)
        {
            if (tiempoCargaDisparo >= TIEMPO_CARGA_MEGA)
            {
                DispararMegaBala();
            }
            else if (Time.time >= tiempoProximoDisparo)
            {
                tiempoProximoDisparo = Time.time + cadenciaDisparo;
                DispararProyectil();
            }
            tiempoCargaDisparo = 0f;
            ActualizarVisualCarga(false);
        }
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

        bool haciaArriba = apuntandoArriba;
        bool esDiagonal = haciaArriba && Mathf.Abs(inputHorizontal) > 0.2f;

        Vector2 direccion;
        if (haciaArriba)
        {
            if (esDiagonal)
            {
                direccion = new Vector2(mirandoDerecha ? 0.707f : -0.707f, 0.707f).normalized;
            }
            else
            {
                direccion = Vector2.up;
            }
        }
        else
        {
            direccion = mirandoDerecha ? Vector2.right : Vector2.left;
        }

        Vector2 puntoSpawn = ObtenerPuntoSpawnDisparo(haciaArriba, esDiagonal);

        if (poderActivo == TipoPoder.DisparoTriple && disparosPoderRestantes > 0)
        {
            disparosPoderRestantes--;
            DispararTriple(direccion, puntoSpawn);

            if (disparosPoderRestantes <= 0)
            {
                poderActivo = TipoPoder.Normal;
                if (GameManagerIyari.Instancia != null)
                {
                    GameManagerIyari.Instancia.ActualizarPoderHUD("NORMAL", 0);
                }
            }
            else
            {
                if (GameManagerIyari.Instancia != null)
                {
                    GameManagerIyari.Instancia.ActualizarPoderHUD("TRIPLE", disparosPoderRestantes);
                }
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

        bool haciaArriba = apuntandoArriba;
        bool esDiagonal = haciaArriba && Mathf.Abs(inputHorizontal) > 0.2f;

        Vector2 direccion;
        if (haciaArriba)
        {
            if (esDiagonal)
            {
                direccion = new Vector2(mirandoDerecha ? 0.707f : -0.707f, 0.707f).normalized;
            }
            else
            {
                direccion = Vector2.up;
            }
        }
        else
        {
            direccion = mirandoDerecha ? Vector2.right : Vector2.left;
        }

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
            GameManagerIyari.Instancia.MostrarMensajeAviso("🌊 ¡MEGA CHORRO PURIFICADOR DISPARADO!");
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
            sr.sortingOrder = 9;
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
            sr.sortingOrder = 8;
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
                GameManagerIyari.Instancia.MostrarMensajeAviso("🛡 ¡EL ESCUDO ACUÁTICO ABSORBIÓ EL IMPACTO!");
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
