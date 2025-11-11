using UnityEngine;

public class CarroVolante : MonoBehaviour
{
    [Header("Volante")]
    public Transform volante;
    public float anguloMaxVolante = 450f;

    private Quaternion _rotacaoInicialVolante;

    private void Start()
    {
        _rotacaoInicialVolante = volante.localRotation;
    }

    public void AtualizarVolante(float anguloVolante)
    {
        var rotacaoY = anguloVolante * anguloMaxVolante;
        volante.localRotation = _rotacaoInicialVolante * Quaternion.Euler(0f, rotacaoY, 0f);
    }
}
