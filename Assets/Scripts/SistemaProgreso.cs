using UnityEngine;

/// <summary>
/// Sistema de Progreso de Niveles para 'Illari' (Huancayo 2026).
/// Maneja qué niveles están desbloqueados, cuál fue el último completado y la lógica de desbloqueo progresivo.
/// Los datos se persisten en PlayerPrefs de forma robusta.
/// </summary>
public static class SistemaProgreso
{
    // Claves de PlayerPrefs
    private const string KEY_NIVEL_DESBLOQUEADO = "Illari_NivelDesbloqueado_";
    private const string KEY_NIVEL_MAX_ALCANZADO = "Illari_NivelMaxAlcanzado";
    private const string KEY_HAY_PARTIDA = "Illari_HayPartida";
    private const string KEY_ESCENA = "Illari_EscenaGuardada";
    private const string KEY_VIDA = "Illari_VidaGuardada";
    private const string KEY_BOTELLAS = "Illari_BotellasGuardadas";
    private const string KEY_RESTAURAR_EN_CARGA = "Illari_RestaurarEnCarga";

    public const int TOTAL_NIVELES = 9;

    /// <summary>
    /// Mapeo de índice de nivel (1..9) a nombre de escena.
    /// </summary>
    public static readonly string[] ESCENAS_NIVELES = new string[]
    {
        "",                                      // índice 0 vacío
        "NivelParqueColonial",                   // Nivel 1
        "Nivel2_CerritoLibertad",                // Nivel 2
        "Nivel3_ParqueTupacAmaru",               // Nivel 3
        "Nivel4_ParqueSombreros",                // Nivel 4
        "Nivel5_PlazaConstitucion",              // Nivel 5
        "Nivel6_BosqueDorado",                   // Nivel 6
        "Nivel7_RioShullcas",                    // Nivel 7
        "Nivel8_TorreTorre",                     // Nivel 8
        "Nivel9_NevadoHuaytapallana"             // Nivel 9
    };

    /// <summary>
    /// Nombres visuales de los niveles para la pantalla de selección.
    /// </summary>
    public static readonly string[] NOMBRES_NIVELES = new string[]
    {
        "",
        "Parque Identidad Huanca",
        "Cerrito de la Libertad",
        "Parque Túpac Amaru",
        "Parque de los Sombreros",
        "Plaza Constitución",
        "Bosque Dorado de Paccha",
        "Ribera Río Shullcas",
        "Formaciones Torre Torre",
        "Nevado Huaytapallana"
    };

    /// <summary>
    /// Emojis / símbolos de cada nivel para la selección.
    /// </summary>
    public static readonly string[] ICONOS_NIVELES = new string[]
    {
        "",
        "🌺", "🏔", "🦆", "🎩",
        "⛪", "🌿", "🌊", "🗿", "❄"
    };

    /// <summary>
    /// El nivel 1 siempre está desbloqueado. Los demás se desbloquean al completar el anterior.
    /// </summary>
    public static bool EsNivelDesbloqueado(int nivel)
    {
        if (nivel < 1 || nivel > TOTAL_NIVELES) return false;
        if (nivel == 1) return true;
        return PlayerPrefs.GetInt(KEY_NIVEL_DESBLOQUEADO + nivel, 0) == 1;
    }

    /// <summary>
    /// Desbloquea el siguiente nivel al completar 'nivelCompletado'.
    /// </summary>
    public static void DesbloquearSiguienteNivel(int nivelCompletado)
    {
        int siguiente = nivelCompletado + 1;
        if (siguiente > TOTAL_NIVELES) return;

        PlayerPrefs.SetInt(KEY_NIVEL_DESBLOQUEADO + siguiente, 1);

        int maxAlcanzado = PlayerPrefs.GetInt(KEY_NIVEL_MAX_ALCANZADO, 1);
        if (siguiente > maxAlcanzado)
        {
            PlayerPrefs.SetInt(KEY_NIVEL_MAX_ALCANZADO, siguiente);
        }
        PlayerPrefs.Save();
        Debug.Log($"<color=#00FF88><b>[PROGRESO]</b> Nivel {siguiente} desbloqueado: {NOMBRES_NIVELES[siguiente]}</color>");
    }

    /// <summary>
    /// Obtiene el índice de nivel a partir del nombre de escena activo.
    /// </summary>
    public static int ObtenerIndiceNivelActual()
    {
        string escena = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        for (int i = 1; i <= TOTAL_NIVELES; i++)
        {
            if (ESCENAS_NIVELES[i] == escena) return i;
        }
        return 0;
    }

    /// <summary>
    /// Retorna el nivel máximo alcanzado (al menos 1).
    /// </summary>
    public static int NivelMaxAlcanzado()
    {
        return PlayerPrefs.GetInt(KEY_NIVEL_MAX_ALCANZADO, 1);
    }

    // ===================== Sistema de guardado =====================

    public static void GuardarPartida(string nombreEscena, int vida, int botellas)
    {
        PlayerPrefs.SetInt(KEY_HAY_PARTIDA, 1);
        PlayerPrefs.SetString(KEY_ESCENA, nombreEscena);
        PlayerPrefs.SetInt(KEY_VIDA, vida);
        PlayerPrefs.SetInt(KEY_BOTELLAS, botellas);
        PlayerPrefs.Save();
        Debug.Log($"<color=#00FF88><b>[GUARDADO]</b> Partida guardada: Escena='{nombreEscena}', Vida={vida}, Botellas={botellas}</color>");
    }

    public static bool ExistePartida()
    {
        return PlayerPrefs.GetInt(KEY_HAY_PARTIDA, 0) == 1;
    }

    public struct DatosPartida
    {
        public bool existe;
        public string escena;
        public int vida;
        public int botellas;
    }

    public static DatosPartida CargarDatos()
    {
        DatosPartida datos = new DatosPartida();
        datos.existe = PlayerPrefs.GetInt(KEY_HAY_PARTIDA, 0) == 1;
        if (datos.existe)
        {
            datos.escena = PlayerPrefs.GetString(KEY_ESCENA, "NivelParqueColonial");
            datos.vida = PlayerPrefs.GetInt(KEY_VIDA, 3);
            datos.botellas = PlayerPrefs.GetInt(KEY_BOTELLAS, 0);
        }
        return datos;
    }

    public static bool CargarPartida()
    {
        if (!ExistePartida()) return false;
        DatosPartida datos = CargarDatos();
        PlayerPrefs.SetInt(KEY_RESTAURAR_EN_CARGA, 1);
        PlayerPrefs.Save();
        UnityEngine.Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(datos.escena);
        return true;
    }

    public static bool DebeRestaurarEstado(out int vida, out int botellas)
    {
        vida = 3;
        botellas = 0;
        if (PlayerPrefs.GetInt(KEY_RESTAURAR_EN_CARGA, 0) == 1)
        {
            DatosPartida datos = CargarDatos();
            vida = datos.vida;
            botellas = datos.botellas;
            PlayerPrefs.SetInt(KEY_RESTAURAR_EN_CARGA, 0);
            PlayerPrefs.Save();
            return true;
        }
        return false;
    }

    public static void BorrarTodoElProgreso()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("<color=#FF5555><b>[PROGRESO]</b> Todo el progreso borrado.</color>");
    }
}
