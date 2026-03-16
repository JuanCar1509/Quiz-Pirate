using UnityEngine;

public class NPC_Quiz : MonoBehaviour
{
    [Header("Conexión")]
    public QuizManager managerDelQuiz;
    public GameObject mensajeInteraccion;

    private bool jugadorCerca = false;
    private bool quizYaComenzado = false;

    void Start()
    {
        if (mensajeInteraccion != null) mensajeInteraccion.SetActive(false);
    }

    void Update()
    {
        if (jugadorCerca && !quizYaComenzado && Input.GetKeyDown(KeyCode.E))
        {
            managerDelQuiz.IniciarQuiz();
            quizYaComenzado = true;

            if (mensajeInteraccion != null) mensajeInteraccion.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !quizYaComenzado)
        {
            jugadorCerca = true;
            if (mensajeInteraccion != null) mensajeInteraccion.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            quizYaComenzado = false; // SEGURIDAD: Si el jugador se aleja (o es teletransportado), se reinicia
            if (mensajeInteraccion != null) mensajeInteraccion.SetActive(false);
        }
    }

    // NUEVO: Permite al Game Manager "despertar" al NPC cuando pierdes
    public void ReiniciarCerebroNPC()
    {
        quizYaComenzado = false;
        jugadorCerca = false;
    }
}