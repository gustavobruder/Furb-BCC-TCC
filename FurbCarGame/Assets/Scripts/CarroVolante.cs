using UnityEngine;

public class CarroVolante : MonoBehaviour
{
    [Header("Volante")]
    public Transform volante;
    public float anguloMaxVolante = 450f;
    public bool volanteComRotacaoInicial = true;

    private Quaternion _rotacaoInicialVolante;

    private void Start()
    {
        _rotacaoInicialVolante = volante.localRotation;
    }

    public void AtualizarVolante(float anguloVolante)
    {
        if (volanteComRotacaoInicial)
        {
            var rotacaoY = anguloVolante * anguloMaxVolante;
            volante.localRotation = _rotacaoInicialVolante * Quaternion.Euler(0f, rotacaoY, 0f);
        }
        else
        {
            var rotacaoZ = anguloVolante * anguloMaxVolante;
            volante.localRotation = Quaternion.Euler(0f, 0f, -rotacaoZ);
        }
    }
}
