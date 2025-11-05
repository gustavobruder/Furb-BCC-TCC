using UnityEngine;
using UnityEngine.UI;

public class ContadorColisoes : MonoBehaviour
{
    public Text textoColisoes;
    private int _quantidadeColisoes = 0;

    private void OnCollisionEnter(Collision collision)
    {
        _quantidadeColisoes++;

        Debug.Log($"Colisão detectada com {collision.gameObject.name}. Total: {_quantidadeColisoes}");

        DefinirTextoColisoes();
    }

    private void DefinirTextoColisoes()
    {
        textoColisoes.text = "Colisões: " + _quantidadeColisoes;
    }
}
