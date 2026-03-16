using UnityEngine;

public class ColisionOceano : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        string nombre = collision.gameObject.name;
        Debug.Log($"?? El océano fue tocado por: {nombre}");

        // Reacción personalizada:
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("?? El jugador tocó el océano");
            // Aquí puedes activar partículas, sonido, daño, etc.
        }

        if (collision.gameObject.CompareTag("Barco"))
        {
            Debug.Log("?? Un barco tocó el océano");
            // Aquí puedes aplicar efectos de flotación, movimiento, etc.
        }

        if (collision.gameObject.CompareTag("Isla"))
        {
            Debug.Log("?? Una isla colisionó con el agua");
        }
    }
}