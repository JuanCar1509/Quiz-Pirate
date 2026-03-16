using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuizManager : MonoBehaviour
{
    [Header("Configuración General")]
    public List<QuizQuestionData> misPreguntas;
    public GameObject panelQuiz;
    public int vidasRestantes = 5;
    public float tiempoPorPregunta = 60f;

    [Header("Referencias UI")]
    public TextMeshProUGUI textoPregunta;
    public TextMeshProUGUI[] textosBotones;
    public Button[] botonesObjetos;
    public TextMeshProUGUI textoVidas;
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoMensajeLoro;
    public TextMeshProUGUI textoBotonLoro;
    public TextMeshProUGUI textoBotonBomba;
    public TextMeshProUGUI textoBotonSalto;

    [Header("Efectos Visuales")]
    public GameObject[] marcasExplosion; // NUEVO: Las imágenes de balazos/quemaduras
    public Color colorTiempoNormal = Color.white; // Color normal del reloj
    private float tamañoFuenteNormal; // Guardará el tamaño original de tu letra

    [Header("Jugador y Mundo")]
    public GameObject jugador;
    public Transform puntoDeSpawn;

    [Header("Audio (Altavoces)")]
    public AudioSource altavozEfectos;
    public AudioSource altavozReloj;

    [Header("Audio (Archivos/Clips)")]
    public AudioClip sonidoBomba;
    public AudioClip sonidoTicTac;

    // Variables internas
    private int preguntaActual = 0;
    private float tiempoActual;
    private bool juegoActivo = false;
    private bool relojSonando = false;

    private int usosLoro = 5;
    private int usosBomba = 5;
    private int usosSalto = 5;

    void Start()
    {
        panelQuiz.SetActive(false);
        juegoActivo = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        ActualizarTextosPowerUps();

        if (textoMensajeLoro != null) textoMensajeLoro.text = "";

        // Guardar el tamaño original del texto del tiempo
        if (textoTiempo != null) tamañoFuenteNormal = textoTiempo.fontSize;
    }

    public void IniciarQuiz()
    {
        panelQuiz.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ControlarMovimientoPersonaje(false);
        ActualizarTextoVidas();
        CargarPregunta();
    }

    void Update()
    {
        if (juegoActivo)
        {
            tiempoActual -= Time.deltaTime;
            ActualizarTextoTiempo();

            // Lógica de los últimos 20 segundos
            if (tiempoActual <= 20f && tiempoActual > 0f)
            {
                // Efecto visual: Rojo y 30% más grande
                if (textoTiempo != null)
                {
                    textoTiempo.color = Color.red;
                    textoTiempo.fontSize = tamañoFuenteNormal * 1.3f;
                }

                // Efecto de sonido
                if (!relojSonando && altavozReloj != null && sonidoTicTac != null)
                {
                    altavozReloj.clip = sonidoTicTac;
                    altavozReloj.Play();
                    relojSonando = true;
                }
            }
            else if (tiempoActual > 20f)
            {
                // Devolver a la normalidad si el tiempo es mayor a 20
                if (textoTiempo != null)
                {
                    textoTiempo.color = colorTiempoNormal;
                    textoTiempo.fontSize = tamañoFuenteNormal;
                }
            }

            if (tiempoActual <= 0)
            {
                GameOverPorTiempo();
            }
        }
    }

    void CargarPregunta()
    {
        // Apagar el reloj si estaba sonando
        if (relojSonando && altavozReloj != null)
        {
            altavozReloj.Stop();
            relojSonando = false;
        }

        // Devolver el reloj a su color y tamaño original
        if (textoTiempo != null)
        {
            textoTiempo.color = colorTiempoNormal;
            textoTiempo.fontSize = tamañoFuenteNormal;
        }

        // Reactivar botones y ocultar las marcas de explosión
        for (int i = 0; i < botonesObjetos.Length; i++)
        {
            if (botonesObjetos[i] != null) botonesObjetos[i].gameObject.SetActive(true);

            if (marcasExplosion.Length > i && marcasExplosion[i] != null)
            {
                marcasExplosion[i].SetActive(false); // Ocultamos el balazo
            }
        }

        if (textoMensajeLoro != null) textoMensajeLoro.text = "";

        if (preguntaActual < misPreguntas.Count)
        {
            QuizQuestionData datos = misPreguntas[preguntaActual];
            textoPregunta.text = datos.preguntaTexto;

            for (int i = 0; i < textosBotones.Length; i++)
            {
                textosBotones[i].text = datos.opciones[i];
            }

            tiempoActual = tiempoPorPregunta;
            juegoActivo = true;
        }
        else
        {
            FinalizarQuiz(true);
        }
    }

    public void Responder(int indiceBoton)
    {
        if (!juegoActivo) return;

        if (indiceBoton == misPreguntas[preguntaActual].indiceCorrecto)
        {
            preguntaActual++;
            CargarPregunta();
        }
        else
        {
            PerderUnaVida();
        }
    }

    public void UsarLoro()
    {
        if (!juegoActivo || usosLoro <= 0) return;
        if (textoMensajeLoro != null) textoMensajeLoro.text = "Loro dice: " + misPreguntas[preguntaActual].pistaDelLoro;
        usosLoro--;
        ActualizarTextosPowerUps();
    }

    public void UsarBomba()
    {
        if (!juegoActivo || usosBomba <= 0) return;
        int correcta = misPreguntas[preguntaActual].indiceCorrecto;
        List<int> incorrectasVisibles = new List<int>();

        for (int i = 0; i < botonesObjetos.Length; i++)
        {
            if (i != correcta && botonesObjetos[i].gameObject.activeSelf)
            {
                incorrectasVisibles.Add(i);
            }
        }

        if (incorrectasVisibles.Count > 0)
        {
            if (altavozEfectos != null && sonidoBomba != null) altavozEfectos.PlayOneShot(sonidoBomba);

            int aEliminar = incorrectasVisibles[Random.Range(0, incorrectasVisibles.Count)];

            // Apagamos el botón
            botonesObjetos[aEliminar].gameObject.SetActive(false);

            // NUEVO: Encendemos la marca de explosión en esa misma posición
            if (marcasExplosion.Length > aEliminar && marcasExplosion[aEliminar] != null)
            {
                marcasExplosion[aEliminar].SetActive(true);
            }

            usosBomba--;
            ActualizarTextosPowerUps();
        }
    }

    public void UsarSalto()
    {
        if (!juegoActivo || usosSalto <= 0) return;
        preguntaActual++;
        CargarPregunta();
        usosSalto--;
        ActualizarTextosPowerUps();
    }

    void ActualizarTextosPowerUps()
    {
        if (textoBotonLoro != null) textoBotonLoro.text = "Pista (" + usosLoro + ")";
        if (textoBotonBomba != null) textoBotonBomba.text = "Bomba (" + usosBomba + ")";
        if (textoBotonSalto != null) textoBotonSalto.text = "Saltar (" + usosSalto + ")";
    }

    void PerderUnaVida()
    {
        vidasRestantes--;
        ActualizarTextoVidas();
        if (vidasRestantes <= 0) FinalizarQuiz(false);
    }

    void GameOverPorTiempo()
    {
        vidasRestantes = 0;
        ActualizarTextoVidas();
        FinalizarQuiz(false);
    }

    void FinalizarQuiz(bool victoria)
    {
        panelQuiz.SetActive(false);
        juegoActivo = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (altavozReloj != null) altavozReloj.Stop();
        relojSonando = false;

        if (!victoria)
        {
            if (puntoDeSpawn != null && jugador != null)
            {
                UnityEngine.CharacterController fisica = jugador.GetComponent<UnityEngine.CharacterController>();
                if (fisica != null) fisica.enabled = false;
                jugador.transform.position = puntoDeSpawn.position;
                jugador.transform.rotation = puntoDeSpawn.rotation;
                if (fisica != null) fisica.enabled = true;
            }
            ReiniciarValoresDelQuiz();
        }
        ControlarMovimientoPersonaje(true);
    }

    void ReiniciarValoresDelQuiz()
    {
        vidasRestantes = 5;
        preguntaActual = 0;
        usosLoro = 5;
        usosBomba = 5;
        usosSalto = 5;
        ActualizarTextosPowerUps();
        ActualizarTextoVidas();

        NPC_Quiz npc = FindFirstObjectByType<NPC_Quiz>();
        if (npc != null) npc.ReiniciarCerebroNPC();
    }

    void ControlarMovimientoPersonaje(bool permitirMovimiento)
    {
        if (jugador != null)
        {
            MonoBehaviour scriptLogica = jugador.GetComponent("ThirdPersonController") as MonoBehaviour;
            if (scriptLogica != null) scriptLogica.enabled = permitirMovimiento;

            MonoBehaviour scriptInputs = jugador.GetComponent("StarterAssetsInputs") as MonoBehaviour;
            if (scriptInputs != null) scriptInputs.enabled = permitirMovimiento;

            MonoBehaviour inputSystem = jugador.GetComponent("PlayerInput") as MonoBehaviour;
            if (inputSystem != null) inputSystem.enabled = permitirMovimiento;

            UnityEngine.CharacterController fisica = jugador.GetComponent<UnityEngine.CharacterController>();
            if (fisica != null) fisica.enabled = permitirMovimiento;
        }
    }

    void ActualizarTextoVidas()
    {
        if (textoVidas != null) textoVidas.text = "Vidas: " + vidasRestantes;
    }

    void ActualizarTextoTiempo()
    {
        if (textoTiempo != null)
        {
            float tiempoAMostrar = Mathf.Max(0, tiempoActual);
            textoTiempo.text = "Tiempo: " + Mathf.CeilToInt(tiempoAMostrar).ToString();
        }
    }
}