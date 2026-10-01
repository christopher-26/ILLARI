using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema de guardado y persistencia para 'Illari'.
/// Almacena el progreso del jugador: parque/nivel actual, vida y botellas recolectadas.
/// Funciona tanto para guardado manual (desde el Menú de Pausa) como para continuar la partida.
/// </summary>
public static class SistemaGuardado
{
    private const string KEY_HAY_PARTIDA = "Illari_HayPartida";
    private const string KEY_ESCENA = "Illari_EscenaGuardada";
    private const string KEY_VIDA = "Illari_VidaGuardada";
    private const string KEY_BOTELLAS = "Illari_BotellasGuardadas";
    private const string KEY_RESTAURAR_EN_CARGA = "Illari_RestaurarEnCarga";

    public struct DatosPartida
    {
        public bool existe;
        public string escena;
        public int vida;
        public int botellas;
    }

    /// <summary>
    /// Guarda el estado actual del juego.
    /// </summary>
    public static void GuardarPartida(string nombreEscena, int vida, int botellas)
    {
        PlayerPrefs.SetInt(KEY_HAY_PARTIDA, 1);
        PlayerPrefs.SetString(KEY_ESCENA, nombreEscena);
        PlayerPrefs.SetInt(KEY_VIDA, vida);
        PlayerPrefs.SetInt(KEY_BOTELLAS, botellas);
        PlayerPrefs.Save();

        Debug.Log($"<color=#00FF88><b>[GUARDADO]</b> Partida guardada: Escena='{nombreEscena}', Vida={vida}, Botellas={botellas}</color>");
    }

    /// <summary>
    /// Lee los datos de la partida guardada si existen.
    /// </summary>
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

    public static bool ExistePartida()
    {
        return PlayerPrefs.GetInt(KEY_HAY_PARTIDA, 0) == 1;
    }

    /// <summary>
    /// Carga la escena guardada y marca para restaurar los valores de vida y botellas al iniciar.
    /// </summary>
    public static bool CargarPartida()
    {
        if (!ExistePartida()) return false;

        DatosPartida datos = CargarDatos();
        PlayerPrefs.SetInt(KEY_RESTAURAR_EN_CARGA, 1);
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene(datos.escena);
        return true;
    }

    /// <summary>
    /// Verifica si al arrancar la escena se debe restaurar el estado previo cargado.
    /// </summary>
    public static bool DebeRestaurarEstado(out int vida, out int botellas)
    {
        vida = 3;
        botellas = 0;

        if (PlayerPrefs.GetInt(KEY_RESTAURAR_EN_CARGA, 0) == 1)
        {
            DatosPartida datos = CargarDatos();
            vida = datos.vida;
            botellas = datos.botellas;

            // Limpiamos la bandera para no sobreescribir inicios futuros
            PlayerPrefs.SetInt(KEY_RESTAURAR_EN_CARGA, 0);
            PlayerPrefs.Save();
            return true;
        }

        return false;
    }

    public static void BorrarPartida()
    {
        PlayerPrefs.DeleteKey(KEY_HAY_PARTIDA);
        PlayerPrefs.DeleteKey(KEY_ESCENA);
        PlayerPrefs.DeleteKey(KEY_VIDA);
        PlayerPrefs.DeleteKey(KEY_BOTELLAS);
        PlayerPrefs.DeleteKey(KEY_RESTAURAR_EN_CARGA);
        PlayerPrefs.Save();
    }
}
