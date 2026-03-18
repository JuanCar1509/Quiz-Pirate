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

    [Header("Sistema de Diálogos")]
    public GameObject panelDialogo;
    public TextMeshProUGUI textoDialogo;

    // Los textos ya están configurados aquí
    private string[] dialogosInicio = new string[] {
        "¡Alto ahí, Stiwart! No des un paso más hacia ese horizonte si aprecias tu pellejo. Ese brillo en tus ojos me dice que buscas a Macabeo, pero déjame decirte algo: ese demonio no se derrota solo con acero y pólvora. Él es una tormenta que devora a los ignorantes.",
        "Para cruzar al siguiente mar y tener una oportunidad contra él, primero debes demostrar que no eres solo carne de cañón. Aquí, en mi 'Juicio de Sal', tus músculos no valen nada si tu cabeza está llena de agua dulce. Te pondré a prueba con diez verdades de estos mares y de nuestra sagrada cultura pirata.",
        "Si fallas, mejor quédate aquí limpiando cubiertas, porque si no conoces el mar, el mar se encargará de que nadie te recuerde. ¿Tienes el salitre suficiente en las venas para empezar, o vas a darte la vuelta como un polizón asustado?"
    };
    private string dialogoVictoria = "Me dejas sin palabras, muchacho. Quizás después de todo sí haya una chispa de capitán en ti. Has demostrado que respetas la historia de estos mares y que tu mente es tan filosa como un abordaje. El camino al siguiente mar está abierto para ti... Ve y demústrale a Macabeo que no eres un simple novato. ¡Buena caza, Stiwart!";
    private string dialogoDerrota = "Lo que me temía... Eres solo un niño jugando a los piratas. Vuelve cuando el rugido de las olas te haya enseñado algo, o terminarás en el fondo del cofre de Davy Jones antes de que puedas decir 'tierra a la vista'. ¡Largo de mi vista hasta que sepas distinguir el ron del agua de mar!";

    [Header("Referencias UI Quiz")]
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
    public GameObject[] marcasExplosion;
    public Color colorTiempoNormal = Color.white;
    private float tamañoFuenteNormal;

    [Header("Jugador y Mundo")]
    public GameObject jugador;
    public Transform puntoDeSpawn;

    [Header("Audio")]
    public AudioSource altavozEfectos;
    public AudioSource altavozReloj;
    public AudioClip sonidoBomba;
    public AudioClip sonidoTicTac;

    // Estados del minijuego
    private enum EstadoJuego { Inactivo, DialogoInicio, Jugando, DialogoFin }
    private EstadoJuego estadoActual = EstadoJuego.Inactivo;
    private int indiceDialogoActual = 0;
    private bool esVictoria = false;

    private int preguntaActual = 0;
    private float tiempoActual;
    private bool relojSonando = false;

    private int usosLoro = 5;
    private int usosBomba = 5;
    private int usosSalto = 5;

    void Start()
    {
        panelQuiz.SetActive(false);
        if (panelDialogo != null) panelDialogo.SetActive(false);

        estadoActual = EstadoJuego.Inactivo;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        ActualizarTextosPowerUps();

        if (textoMensajeLoro != null) textoMensajeLoro.text = "";
        if (textoTiempo != null) tamañoFuenteNormal = textoTiempo.fontSize;
    }

    public void IniciarQuiz()
    {
        ControlarMovimientoPersonaje(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Fase de diálogo en lugar de juego directo
        estadoActual = EstadoJuego.DialogoInicio;
        indiceDialogoActual = 0;
        panelQuiz.SetActive(false);
        panelDialogo.SetActive(true);
        MostrarDialogoActual();
    }

    void MostrarDialogoActual()
    {
        if (estadoActual == EstadoJuego.DialogoInicio)
        {
            if (indiceDialogoActual < dialogosInicio.Length)
            {
                textoDialogo.text = dialogosInicio[indiceDialogoActual];
            }
            else
            {
                // Termina la charla, empieza el quiz
                panelDialogo.SetActive(false);
                EmpezarQuizReal();
            }
        }
        else if (estadoActual == EstadoJuego.DialogoFin)
        {
            textoDialogo.text = esVictoria ? dialogoVictoria : dialogoDerrota;
        }
    }

    // Esta función la llama el botón O la barra espaciadora
    public void AvanzarDialogo()
    {
        if (estadoActual == EstadoJuego.DialogoInicio)
        {
            indiceDialogoActual++;
            MostrarDialogoActual();
        }
        else if (estadoActual == EstadoJuego.DialogoFin)
        {
            CerrarTodo();
        }
    }

    void Update()
    {
        // Detectar barra espaciadora si estamos en los diálogos
        if (estadoActual == EstadoJuego.DialogoInicio || estadoActual == EstadoJuego.DialogoFin)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AvanzarDialogo();
            }
        }

        // Lógica normal del juego si estamos jugando
        if (estadoActual == EstadoJuego.Jugando)
        {
            tiempoActual -= Time.deltaTime;
            ActualizarTextoTiempo();

            if (tiempoActual <= 20f && tiempoActual > 0f)
            {
                if (textoTiempo != null)
                {
                    textoTiempo.color = Color.red;
                    textoTiempo.fontSize = tamañoFuenteNormal * 1.3f;
                }

                if (!relojSonando && altavozReloj != null && sonidoTicTac != null)
                {
                    altavozReloj.clip = sonidoTicTac;
                    altavozReloj.Play();
                    relojSonando = true;
                }
            }
            else if (tiempoActual > 20f)
            {
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

    void EmpezarQuizReal()
    {
        estadoActual = EstadoJuego.Jugando;
        panelQuiz.SetActive(true);

        BarajarPreguntas();
        preguntaActual = 0;

        ActualizarTextoVidas();
        CargarPregunta();
    }

    void CargarPregunta()
    {
        if (relojSonando && altavozReloj != null)
        {
            altavozReloj.Stop();
            relojSonando = false;
        }

        if (textoTiempo != null)
        {
            textoTiempo.color = colorTiempoNormal;
            textoTiempo.fontSize = tamañoFuenteNormal;
        }

        for (int i = 0; i < botonesObjetos.Length; i++)
        {
            if (botonesObjetos[i] != null) botonesObjetos[i].gameObject.SetActive(true);
            if (marcasExplosion.Length > i && marcasExplosion[i] != null) marcasExplosion[i].SetActive(false);
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
        }
        else
        {
            FinalizarQuiz(true);
        }
    }

    public void Responder(int indiceBoton)
    {
        if (estadoActual != EstadoJuego.Jugando) return;

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
        if (estadoActual != EstadoJuego.Jugando || usosLoro <= 0) return;
        if (textoMensajeLoro != null) textoMensajeLoro.text = "Loro dice: " + misPreguntas[preguntaActual].pistaDelLoro;
        usosLoro--;
        ActualizarTextosPowerUps();
    }

    public void UsarBomba()
    {
        if (estadoActual != EstadoJuego.Jugando || usosBomba <= 0) return;
        int correcta = misPreguntas[preguntaActual].indiceCorrecto;
        List<int> incorrectasVisibles = new List<int>();

        for (int i = 0; i < botonesObjetos.Length; i++)
        {
            if (i != correcta && botonesObjetos[i].gameObject.activeSelf) incorrectasVisibles.Add(i);
        }

        if (incorrectasVisibles.Count > 0)
        {
            if (altavozEfectos != null && sonidoBomba != null) altavozEfectos.PlayOneShot(sonidoBomba);
            int aEliminar = incorrectasVisibles[Random.Range(0, incorrectasVisibles.Count)];
            botonesObjetos[aEliminar].gameObject.SetActive(false);
            if (marcasExplosion.Length > aEliminar && marcasExplosion[aEliminar] != null) marcasExplosion[aEliminar].SetActive(true);
            usosBomba--;
            ActualizarTextosPowerUps();
        }
    }

    public void UsarSalto()
    {
        if (estadoActual != EstadoJuego.Jugando || usosSalto <= 0) return;
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

        if (altavozReloj != null) altavozReloj.Stop();
        relojSonando = false;

        // Fase de diálogo de victoria/derrota
        esVictoria = victoria;
        estadoActual = EstadoJuego.DialogoFin;
        panelDialogo.SetActive(true);
        MostrarDialogoActual();
    }

    void CerrarTodo()
    {
        panelDialogo.SetActive(false);
        estadoActual = EstadoJuego.Inactivo;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (!esVictoria)
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
        else
        {
            // Apagar NPC si gana
            NPC_Quiz npc = FindFirstObjectByType<NPC_Quiz>();
            if (npc != null) npc.gameObject.SetActive(false);
        }

        ControlarMovimientoPersonaje(true);
    }

    void ReiniciarValoresDelQuiz()
    {
        vidasRestantes = 5;
        usosLoro = 5;
        usosBomba = 5;
        usosSalto = 5;
        ActualizarTextosPowerUps();
        ActualizarTextoVidas();

        NPC_Quiz npc = FindFirstObjectByType<NPC_Quiz>();
        if (npc != null) npc.ReiniciarCerebroNPC();
    }

    void BarajarPreguntas()
    {
        for (int i = 0; i < misPreguntas.Count; i++)
        {
            QuizQuestionData cartaTemporal = misPreguntas[i];
            int indiceAleatorio = Random.Range(i, misPreguntas.Count);
            misPreguntas[i] = misPreguntas[indiceAleatorio];
            misPreguntas[indiceAleatorio] = cartaTemporal;
        }
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