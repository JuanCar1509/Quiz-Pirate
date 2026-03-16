using UnityEngine;

[CreateAssetMenu(fileName = "NuevaPregunta", menuName = "Quiz Pirata/Nueva Pregunta")]
public class QuizQuestionData : ScriptableObject
{
    [Header("La Pregunta")]
    [TextArea(2, 5)]
    public string preguntaTexto;

    [Header("Opciones de Respuesta")]
    [Tooltip("Escribe aquí las 4 posibles respuestas")]
    public string[] opciones;

    [Header("Respuesta Correcta")]
    public int indiceCorrecto;

    [Header("Ayudas")]
    [TextArea(2, 2)]
    public string pistaDelLoro; // NUEVO: Aquí escribirás la pista para esta pregunta
}