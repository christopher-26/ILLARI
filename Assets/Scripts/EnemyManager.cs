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

    public enum TipoEnemigo { Patrullero, Tirador, Volador, Blindado }

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

    [Header("Configuración Volador (Acechador Alado)")]
    [SerializeField] private float velocidadVuelo = 2.4f;
    [SerializeField] private float rangoPicadaX = 4.5f;
    [SerializeField] private float velocidadPicada = 6.5f;
    [SerializeField] private float cadenciaPicada = 3.8f;
    [SerializeField] private float distanciaPatrullaVuelo = 6.0f;

    [Header("Configuración Blindado / Tanque")]
    [SerializeField] private float velocidadEmbestida = 4.8f;
    [SerializeField] private float cadenciaEmbestida = 5.0f;
    [SerializeField] private float duracionBloqueo = 1.4f;

    [Header("Sprites de Animación Flipbook")]
    [SerializeField] private Sprite[] spritesCaminar;
    [SerializeField] private Sprite[] spritesReposo;
    [SerializeField] private Sprite[] spritesDisparo;
    [SerializeField] private Sprite[] spritesDanio;
    [SerializeField] private Sprite[] spritesMuerte;

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer spriteRenderer;
    private Transform transformJugador;

    private EstadoPatrulla estadoActual = EstadoPatrulla.Patrullando;
    private float direccionX = 1f;
    private float tiempoProximoDisparo = 0f;
    private Vector3 escalaInicial;

    private Transform transVisual;
    private Animator animator;
    private bool estaMuerto = false;
    private bool estaDisparando = false;
    private bool estaEnDanio = false;
    private float timerAnim = 0f;
    private int indiceFrameAnim = 0;

    // Estado Volador
    private Vector2 posOrigenVuelo;
    private float altitudBaseY;
    private bool estaEnPicada = false;
    private float tiempoProximaPicada = 0f;

    // Estado Blindado y Patrulla
    private Vector2 posOrigenPatrulla;
    private float ultimoCambioDireccionTiempo = 0f;
    private float ultimoDisparoEjecutadoTiempo = -10f;
    private bool estaBloqueando = false;
    private bool estaEmvistiendo = false;
    private float tiempoProximaEmbestida = 0f;
    private float tiempoFinBloqueo = 0f;
    private float tiempoFinEmbestida = 0f;
    private bool tieneParametroChargeAttack = false;
    private bool tieneParametroTakeDamage = false;
    private bool tieneParametroPurify = false;
    private bool tieneParametroIsDiving = false;
    private bool tieneParametroIsBlocking = false;
    private bool tieneParametroShoulderBash = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null && animator.GetComponent<EnemyAnimationEvents>() == null)
        {
            animator.gameObject.AddComponent<EnemyAnimationEvents>();
        }

        if (spriteRenderer != null)
        {
            if (colorEnemigo != Color.white)
            {
                spriteRenderer.color = colorEnemigo;
            }
            spriteRenderer.sortingLayerName = "Props_Front";
            spriteRenderer.sortingOrder = 50;
        }

        transVisual = transform.Find("Visual");
        if (transVisual != null)
        {
            transVisual.localPosition = Vector3.zero;
        }
        escalaInicial = (transVisual != null) ? transVisual.localScale : transform.localScale;

        BoxCollider2D boxCol = col as BoxCollider2D;
        if (boxCol != null)
        {
            boxCol.offset = new Vector2(0f, 0.95f);
            if (boxCol.size.y < 1.6f)
            {
                boxCol.size = new Vector2(1.5f, 1.85f);
            }
        }

        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        posOrigenPatrulla = transform.position;

#if UNITY_EDITOR
        if (prefabBalaEnemiga == null)
        {
            prefabBalaEnemiga = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BalaBarro.prefab");
        }
#endif
    }

    private void Start()
    {
        BuscarJugadorSiFalta();
        posOrigenPatrulla = transform.position;

        if (tipo == TipoEnemigo.Tirador)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2.5f;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            rb.linearVelocity = Vector2.zero;
            rangoVisionTirador = Mathf.Max(rangoVisionTirador, 11.5f);
        }
        else if (tipo == TipoEnemigo.Volador)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.linearVelocity = Vector2.zero;
            posOrigenVuelo = transform.position;
            altitudBaseY = transform.position.y;
            tiempoProximaPicada = Time.time + 2.2f;
            rangoVisionTirador = Mathf.Max(rangoVisionTirador, 11.5f);
        }
        else if (tipo == TipoEnemigo.Blindado)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2.5f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            salud = Mathf.Max(salud, 4);
            tiempoProximaEmbestida = Time.time + 2.5f;
        }

        DetectarParametrosAnimator();
    }

    private void BuscarJugadorSiFalta()
    {
        if (transformJugador != null) return;
        GameObject jugadorObj = GameObject.FindWithTag("Player");
        if (jugadorObj != null)
        {
            transformJugador = jugadorObj.transform;
        }
        else
        {
            var illari = Object.FindFirstObjectByType<IllariPlatformer>();
            if (illari != null) transformJugador = illari.transform;
        }
    }

    private void DetectarParametrosAnimator()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        if (animator == null || animator.runtimeAnimatorController == null) return;

        tieneParametroChargeAttack = false;
        tieneParametroTakeDamage = false;
        tieneParametroPurify = false;
        tieneParametroIsDiving = false;
        tieneParametroIsBlocking = false;
        tieneParametroShoulderBash = false;

        foreach (var p in animator.parameters)
        {
            if (p.name == "ChargeAttack") tieneParametroChargeAttack = true;
            if (p.name == "TakeDamage") tieneParametroTakeDamage = true;
            if (p.name == "Purify") tieneParametroPurify = true;
            if (p.name == "IsDiving") tieneParametroIsDiving = true;
            if (p.name == "IsBlocking") tieneParametroIsBlocking = true;
            if (p.name == "ShoulderBash") tieneParametroShoulderBash = true;
        }
    }

    private void Update()
    {
        if (estaMuerto) return;

        BuscarJugadorSiFalta();

        switch (tipo)
        {
            case TipoEnemigo.Tirador:
                ComportamientoTirador();
                break;
            case TipoEnemigo.Volador:
                ComportamientoVolador();
                break;
            case TipoEnemigo.Blindado:
                ComportamientoBlindado();
                break;
            case TipoEnemigo.Patrullero:
                VerificarPersecucion();
                break;
        }

        AnimarCaminataLodo();
        ActualizarFlipbookSprites();
    }

    /// <summary>
    /// Animación orgánica de caminata para los monstruos de barro:
    /// Aplica deformación viscosa (squash & stretch) y balanceo al dar pasos en el lodo.
    /// </summary>
    private void AnimarCaminataLodo()
    {
        bool enMovimiento = (tipo == TipoEnemigo.Patrullero) && Mathf.Abs(rb.linearVelocity.x) > 0.05f;

        if (transVisual == null) return;

        // Si cuenta con Animator configurado, la animación de sprites manda
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            ActualizarEscalaVisual();
            return;
        }

        float signoX = direccionX > 0 ? 1f : -1f;

        if (enMovimiento)
        {
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
        if (estaMuerto) return;

        if (tipo == TipoEnemigo.Patrullero)
        {
            ComportamientoPatrullero();
        }
        else if (tipo == TipoEnemigo.Blindado)
        {
            FisicasBlindado();
        }
        else if (tipo == TipoEnemigo.Volador)
        {
            FisicasVolador();
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
            bool mirandoAlJugador = (dirAlJugadorX * direccionX) > 0f;
            if (distancia <= rangoVisionPersecucion && mirandoAlJugador && Mathf.Abs(transformJugador.position.y - transform.position.y) < 2.5f)
            {
                estadoActual = EstadoPatrulla.Persiguiendo;
            }
        }
        else if (estadoActual == EstadoPatrulla.Persiguiendo)
        {
            if (distancia > rangoVisionPersecucion * 1.5f)
            {
                estadoActual = EstadoPatrulla.Patrullando;
            }
            else
            {
                if (dirAlJugadorX > 0.1f) direccionX = 1f;
                else if (dirAlJugadorX < -0.1f) direccionX = -1f;
                ActualizarEscalaVisual();
            }
        }
    }

    private bool ExisteSueloEnRayo(Vector2 origen, Vector2 direccion, float distancia)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origen, direccion, distancia);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D c = hits[i].collider;
            if (c == null || c == col || c.isTrigger) continue;
            if (c.CompareTag(tagSuelo)) return true;
        }
        return false;
    }

    private void ComportamientoPatrullero()
    {
        float velActual = (estadoActual == EstadoPatrulla.Persiguiendo) ? velocidadPersecucion : velocidadPatrulla;

        float radioX = (col != null && col.bounds.extents.x > 0.1f) ? col.bounds.extents.x : 0.6f;
        float radioY = (col != null && col.bounds.extents.y > 0.1f) ? col.bounds.extents.y : 0.8f;
        Vector2 centro = (col != null && col.bounds.extents.y > 0.1f) ? (Vector2)col.bounds.center : (Vector2)transform.position + new Vector2(0f, 0.8f);

        // Detección de muro frontal ignorando el propio collider
        Vector2 origenFrente = centro + new Vector2(direccionX * (radioX + 0.1f), 0.1f);
        bool hayMuro = ExisteSueloEnRayo(origenFrente, Vector2.right * direccionX, distanciaPared);

        // Detección de precipicio un paso adelante hacia abajo
        Vector2 origenVacio = new Vector2(centro.x + direccionX * (radioX + 0.25f), centro.y);
        bool haySueloAdelante = ExisteSueloEnRayo(origenVacio, Vector2.down, radioY + distanciaVacio + 0.8f);
        bool hayVacio = !haySueloAdelante;

        // Límite de patrulla horizontal alrededor del origen (para que patrulle de lado a lado aunque el suelo sea largo)
        float despDesdeOrigen = transform.position.x - posOrigenPatrulla.x;
        bool excedeRangoPatrulla = (estadoActual == EstadoPatrulla.Patrullando) &&
                                   ((direccionX > 0f && despDesdeOrigen > 6.5f) || (direccionX < 0f && despDesdeOrigen < -6.5f));

        if (estadoActual == EstadoPatrulla.Persiguiendo)
        {
            if (hayVacio || hayMuro)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return;
            }
        }
        else
        {
            if ((hayMuro || hayVacio || excedeRangoPatrulla) && (Time.time - ultimoCambioDireccionTiempo > 0.35f))
            {
                ultimoCambioDireccionTiempo = Time.time;
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
        Transform transVis = transform.Find("Visual");
        Transform targetTrans = (transVis != null) ? transVis : transform;
        targetTrans.localScale = new Vector3(direccionX * Mathf.Abs(escalaInicial.x), escalaInicial.y, escalaInicial.z);
    }

    private void ComportamientoTirador()
    {
        if (transformJugador == null) return;

        float distancia = Vector2.Distance(transform.position, transformJugador.position);

        if (distancia <= rangoVisionTirador)
        {
            float dirAlJugadorX = transformJugador.position.x - transform.position.x;
            direccionX = dirAlJugadorX >= 0f ? 1f : -1f;
            ActualizarEscalaVisual();

            if (Time.time >= tiempoProximoDisparo)
            {
                tiempoProximoDisparo = Time.time + cadenciaDisparo;

                if (animator != null && tieneParametroChargeAttack)
                {
                    animator.SetTrigger("ChargeAttack");
                    StartCoroutine(RutinaDisparoSincronizado(0.25f));
                }
                else
                {
                    DispararBolaLodo();
                }
            }
        }
    }

    private System.Collections.IEnumerator RutinaDisparoSincronizado(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!estaMuerto)
        {
            DispararBolaLodo();
        }
    }

    /// <summary>
    /// Evento de animación o llamada directa para expulsar la bola de lodo por la boca.
    /// </summary>
    public void DispararBolaLodo()
    {
        if (estaMuerto) return;
        if (Time.time - ultimoDisparoEjecutadoTiempo < 0.45f) return;

        if (transformJugador != null)
        {
            float dx = transformJugador.position.x - transform.position.x;
            direccionX = dx >= 0f ? 1f : -1f;
            ActualizarEscalaVisual();
        }

        Vector2 dirDisparo = new Vector2(direccionX, 0f);
        if (tipo == TipoEnemigo.Volador && transformJugador != null)
        {
            Vector2 origenVuelo = (Vector2)transform.position + new Vector2(direccionX * 0.55f, -0.25f);
            Vector2 objetivoJugador = (Vector2)transformJugador.position + new Vector2(0f, 0.55f);
            dirDisparo = (objetivoJugador - origenVuelo).normalized;
        }

        EjecutarDisparoBala(dirDisparo);
    }

    // ==========================================
    // COMPORTAMIENTOS ESPECÍFICOS: VOLADOR & BLINDADO
    // ==========================================

    private void ComportamientoVolador()
    {
        if (transformJugador == null) return;

        float dx = transformJugador.position.x - transform.position.x;
        float dy = transform.position.y - transformJugador.position.y;
        float distancia = Vector2.Distance(transform.position, transformJugador.position);

        // El Fangoso Volador dispara bolas de lodo hacia Illari cuando está en su rango de visión
        if (distancia <= rangoVisionTirador && Time.time >= tiempoProximoDisparo)
        {
            tiempoProximoDisparo = Time.time + cadenciaDisparo;
            direccionX = dx >= 0f ? 1f : -1f;
            ActualizarEscalaVisual();
            DispararBolaLodo();
        }

        if (estaEnPicada) return;

        // Si Illari está justo debajo, además realiza su ataque en picada
        if (dy > 0.4f && dy < 7.0f && Mathf.Abs(dx) < rangoPicadaX && Time.time >= tiempoProximaPicada)
        {
            StartCoroutine(RutinaPicadaAerea());
        }
    }

    private void FisicasVolador()
    {
        if (estaEnPicada) return;

        // Vuelo oscilatorio suave
        float desplazamientoX = transform.position.x - posOrigenVuelo.x;
        if ((direccionX > 0f && desplazamientoX >= distanciaPatrullaVuelo) ||
            (direccionX < 0f && desplazamientoX <= -distanciaPatrullaVuelo))
        {
            direccionX = -Mathf.Sign(desplazamientoX);
            ActualizarEscalaVisual();
        }

        float nuevoY = altitudBaseY + Mathf.Sin(Time.time * 2.8f) * 0.35f;
        rb.linearVelocity = new Vector2(direccionX * velocidadVuelo, (nuevoY - transform.position.y) * 4f);
    }

    private System.Collections.IEnumerator RutinaPicadaAerea()
    {
        estaEnPicada = true;
        tiempoProximaPicada = Time.time + cadenciaPicada;

        if (transformJugador != null)
        {
            direccionX = (transformJugador.position.x > transform.position.x) ? 1f : -1f;
            ActualizarEscalaVisual();
        }

        if (animator != null && tieneParametroIsDiving)
        {
            animator.SetBool("IsDiving", true);
        }

        Vector3 objetivo = (transformJugador != null) ? transformJugador.position : (transform.position + Vector3.down * 3f);
        Vector2 dirPicada = ((Vector2)objetivo - (Vector2)transform.position).normalized;

        float tiempoPicada = 0.65f;
        float t = 0f;
        while (t < tiempoPicada && !estaMuerto)
        {
            rb.linearVelocity = dirPicada * velocidadPicada;
            t += Time.deltaTime;
            yield return null;
        }

        if (animator != null && tieneParametroIsDiving)
        {
            animator.SetBool("IsDiving", false);
        }

        float tiempoRetorno = 0.85f;
        t = 0f;
        while (t < tiempoRetorno && !estaMuerto)
        {
            float targetY = altitudBaseY;
            float vy = Mathf.Clamp((targetY - transform.position.y) * 5f, -velocidadVuelo, velocidadVuelo * 1.5f);
            rb.linearVelocity = new Vector2(direccionX * velocidadVuelo * 0.7f, vy);
            t += Time.deltaTime;
            yield return null;
        }

        estaEnPicada = false;
    }

    private void ComportamientoBlindado()
    {
        if (estaBloqueando)
        {
            if (Time.time >= tiempoFinBloqueo)
            {
                estaBloqueando = false;
                if (animator != null && tieneParametroIsBlocking) animator.SetBool("IsBlocking", false);
            }
            return;
        }

        if (estaEmvistiendo)
        {
            if (Time.time >= tiempoFinEmbestida)
            {
                estaEmvistiendo = false;
            }
            return;
        }

        if (transformJugador != null)
        {
            float dist = Vector2.Distance(transform.position, transformJugador.position);
            float diffY = Mathf.Abs(transformJugador.position.y - transform.position.y);
            float dirAlJugadorX = transformJugador.position.x - transform.position.x;
            bool mirandoAlJugador = (dirAlJugadorX * direccionX) > 0f;

            if (mirandoAlJugador && diffY < 1.8f && dist >= 1.8f && dist <= 7.5f && Time.time >= tiempoProximaEmbestida)
            {
                IniciarEmbestidaBlindado();
            }
        }
    }

    private void IniciarEmbestidaBlindado()
    {
        estaEmvistiendo = true;
        tiempoFinEmbestida = Time.time + 0.9f;
        tiempoProximaEmbestida = Time.time + cadenciaEmbestida;

        if (animator != null && tieneParametroShoulderBash)
        {
            animator.SetTrigger("ShoulderBash");
        }
    }

    private void FisicasBlindado()
    {
        if (estaBloqueando)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (estaEmvistiendo)
        {
            rb.linearVelocity = new Vector2(direccionX * velocidadEmbestida, rb.linearVelocity.y);
            return;
        }

        ComportamientoPatrullero();
    }

    public void ActivarBloqueoBlindado()
    {
        estaBloqueando = true;
        tiempoFinBloqueo = Time.time + duracionBloqueo;
        if (animator != null && tieneParametroIsBlocking)
        {
            animator.SetBool("IsBlocking", true);
        }
    }

    private void ActualizarFlipbookSprites()
    {
        if (animator != null && animator.runtimeAnimatorController != null) return;
        if (spriteRenderer == null || estaMuerto || estaEnDanio || estaDisparando) return;

        bool enMovimiento = (tipo == TipoEnemigo.Patrullero) && Mathf.Abs(rb.linearVelocity.x) > 0.05f;
        Sprite[] grupoActivo = (enMovimiento && spritesCaminar != null && spritesCaminar.Length > 0) ? spritesCaminar : spritesReposo;

        if (grupoActivo != null && grupoActivo.Length > 0)
        {
            float intervalo = enMovimiento ? 0.11f : 0.18f;
            timerAnim += Time.deltaTime;
            if (timerAnim >= intervalo)
            {
                timerAnim = 0f;
                indiceFrameAnim = (indiceFrameAnim + 1) % grupoActivo.Length;
                spriteRenderer.sprite = grupoActivo[indiceFrameAnim];
            }
        }
    }

    private void DispararBalaEnemiga(Vector2 direccion)
    {
        if (estaMuerto) return;

        if (spritesDisparo != null && spritesDisparo.Length > 0)
        {
            StartCoroutine(RutinaDisparoVisual(direccion));
        }
        else
        {
            EjecutarDisparoBala(direccion);
        }
    }

    private System.Collections.IEnumerator RutinaDisparoVisual(Vector2 direccion)
    {
        estaDisparando = true;
        int frameDisparo = Mathf.Max(1, spritesDisparo.Length / 2);

        for (int i = 0; i < spritesDisparo.Length; i++)
        {
            if (estaMuerto) yield break;
            if (spriteRenderer != null) spriteRenderer.sprite = spritesDisparo[i];
            if (i == frameDisparo)
            {
                EjecutarDisparoBala(direccion);
            }
            yield return new WaitForSeconds(0.08f);
        }

        estaDisparando = false;
    }

    private void EjecutarDisparoBala(Vector2 direccion)
    {
        if (estaMuerto) return;
        if (Time.time - ultimoDisparoEjecutadoTiempo < 0.45f) return;
        ultimoDisparoEjecutadoTiempo = Time.time;

        Vector2 dirNorm = direccion.sqrMagnitude > 0.001f ? direccion.normalized : new Vector2(direccionX, 0f);

        // Posición de salida a la altura de la boca (para no chocar con el suelo en pivots Bottom-Center)
        Vector2 offsetBoca = (tipo == TipoEnemigo.Volador)
            ? new Vector2(direccionX * 0.65f, 0.70f)
            : new Vector2(direccionX * 0.85f, 0.95f);

        Vector2 origen = (puntoDisparo != null) ? (Vector2)puntoDisparo.position : (Vector2)transform.position + offsetBoca;

        GameObject balaObj = null;
        if (prefabBalaEnemiga != null)
        {
            balaObj = Instantiate(prefabBalaEnemiga, origen, Quaternion.identity);
        }
        else
        {
            balaObj = new GameObject("BalaBarro_Runtime");
            balaObj.transform.position = origen;
            var rbB = balaObj.AddComponent<Rigidbody2D>();
            rbB.gravityScale = 0f;
            var colB = balaObj.AddComponent<CircleCollider2D>();
            colB.isTrigger = true;
            colB.radius = 0.2f;
            var srB = balaObj.AddComponent<SpriteRenderer>();
            srB.sortingLayerName = "Props_Front";
            srB.sortingOrder = 95;
            balaObj.AddComponent<Bala>();
        }

        Bala bala = balaObj.GetComponent<Bala>();
        if (bala != null)
        {
            bala.Inicializar(dirNorm, true);
        }
    }

    private void IntentarDanarJugador(GameObject obj)
    {
        if (estaMuerto || obj == null) return;

        IllariPlatformer jugador = obj.GetComponent<IllariPlatformer>();
        if (jugador == null) jugador = obj.GetComponentInParent<IllariPlatformer>();

        if (jugador != null)
        {
            jugador.RecibirDanio(danioContacto);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        IntentarDanarJugador(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        IntentarDanarJugador(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IntentarDanarJugador(other.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        IntentarDanarJugador(other.gameObject);
    }

    public void RecibirDanio(int cantidad = 1, Vector2 direccionImpacto = default)
    {
        if (estaMuerto) return;

        // Comportamiento especial de bloqueo para Blindado frente a proyectiles frontales
        if (tipo == TipoEnemigo.Blindado && direccionImpacto != Vector2.zero)
        {
            bool impactoFrontal = (direccionImpacto.x * direccionX) < 0f;
            if (impactoFrontal)
            {
                ActivarBloqueoBlindado();
                salud -= 1; // Impacto amortiguado por el escudo de rocas
                StartCoroutine(RutinaFlashBloqueo());

                if (salud <= 0)
                {
                    estaMuerto = true;
                    if (animator != null && tieneParametroPurify)
                    {
                        if (tieneParametroIsBlocking) animator.SetBool("IsBlocking", false);
                        animator.SetTrigger("Purify");
                        StartCoroutine(RutinaMuerteAnimator());
                    }
                    else
                    {
                        FinalizarMuerte();
                    }
                }
                return;
            }
        }

        salud -= cantidad;
        AudioManager.Instancia?.ReproducirSFX("danio");

        if (salud <= 0)
        {
            estaMuerto = true;
            if (animator != null && tieneParametroPurify)
            {
                if (tieneParametroIsBlocking) animator.SetBool("IsBlocking", false);
                if (tieneParametroIsDiving) animator.SetBool("IsDiving", false);
                animator.SetTrigger("Purify");
                StartCoroutine(RutinaMuerteAnimator());
            }
            else if (spritesMuerte != null && spritesMuerte.Length > 0)
            {
                StartCoroutine(RutinaMuerte());
            }
            else
            {
                FinalizarMuerte();
            }
        }
        else
        {
            if (animator != null && tieneParametroTakeDamage)
            {
                animator.SetTrigger("TakeDamage");
            }
            else if (spritesDanio != null && spritesDanio.Length > 0)
            {
                StartCoroutine(RutinaDanioVisual());
            }
            else
            {
                StartCoroutine(RutinaFlashDanioGeneral());
            }
        }
    }

    private System.Collections.IEnumerator RutinaFlashDanioGeneral()
    {
        estaEnDanio = true;
        if (spriteRenderer != null)
        {
            Color colOriginal = spriteRenderer.color;
            spriteRenderer.color = new Color(1f, 0.4f, 0.4f, 1f);
            yield return new WaitForSeconds(0.12f);
            if (spriteRenderer != null) spriteRenderer.color = colOriginal;
        }
        estaEnDanio = false;
    }

    private System.Collections.IEnumerator RutinaFlashBloqueo()
    {
        if (spriteRenderer != null)
        {
            Color colOriginal = spriteRenderer.color;
            spriteRenderer.color = new Color(1f, 0.9f, 0.3f, 1f);
            yield return new WaitForSeconds(0.12f);
            if (spriteRenderer != null) spriteRenderer.color = colOriginal;
        }
    }

    private System.Collections.IEnumerator RutinaMuerteAnimator()
    {
        estaMuerto = true;
        if (col != null) col.enabled = false;

        if (tipo == TipoEnemigo.Volador)
        {
            // El acechador alado cae al piso mientras se disuelve en agua limpia y suelta la botella
            rb.gravityScale = 2.5f;
            rb.linearVelocity = new Vector2(0f, -3.5f);
            yield return new WaitForSeconds(0.6f);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(0.55f);
        }

        FinalizarMuerte();
    }

    private System.Collections.IEnumerator RutinaDanioVisual()
    {
        estaEnDanio = true;
        Color colOriginal = (spriteRenderer != null) ? spriteRenderer.color : Color.white;

        for (int i = 0; i < spritesDanio.Length; i++)
        {
            if (estaMuerto) yield break;
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = spritesDanio[i];
                spriteRenderer.color = new Color(1f, 0.4f, 0.4f, 1f);
            }
            yield return new WaitForSeconds(0.12f);
        }

        if (spriteRenderer != null) spriteRenderer.color = colOriginal;
        estaEnDanio = false;
    }

    private System.Collections.IEnumerator RutinaMuerte()
    {
        estaMuerto = true;
        rb.linearVelocity = Vector2.zero;
        if (col != null) col.enabled = false;

        for (int i = 0; i < spritesMuerte.Length; i++)
        {
            if (spriteRenderer != null) spriteRenderer.sprite = spritesMuerte[i];
            yield return new WaitForSeconds(0.08f);
        }

        FinalizarMuerte();
    }

    private void FinalizarMuerte()
    {
        if (prefabBotellaDrop != null)
        {
            GameObject objBotella = Instantiate(prefabBotellaDrop, transform.position + Vector3.up * 0.4f, Quaternion.identity);
            BotellaVidrio bv = objBotella.GetComponent<BotellaVidrio>();
            if (bv != null)
            {
                bv.IniciarDropFisico(new Vector2(Random.Range(-0.8f, 0.8f), 4.2f));
            }
            else
            {
                Rigidbody2D rbDrop = objBotella.GetComponent<Rigidbody2D>();
                if (rbDrop != null)
                {
                    rbDrop.bodyType = RigidbodyType2D.Dynamic;
                    rbDrop.linearVelocity = new Vector2(Random.Range(-0.8f, 0.8f), 4.2f);
                }
            }
        }

        if (prefabEfectoMuerte != null)
        {
            Instantiate(prefabEfectoMuerte, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

}

