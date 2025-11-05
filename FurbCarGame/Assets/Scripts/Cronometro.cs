using UnityEngine;
using UnityEngine.UI;

public class Cronometro : MonoBehaviour
{
    public Text textoCronometro;
    public Rigidbody carroRigidbody;
    public CarroVolante carroVolante;
    public VerificarCarroNaVaga verificarVaga;

    private float _tempo = 0f;
    private bool _cronometrando = true;

    private void Update()
    {
        if (_cronometrando)
        {
            _tempo += Time.deltaTime;

            var minutos = Mathf.FloorToInt(_tempo / 60);
            var segundos = Mathf.FloorToInt(_tempo % 60);
            textoCronometro.text = $"{minutos:00}:{segundos:00}";

            if (CarroEstacionadoCorretamente())
            {
                _cronometrando = false;
                Debug.Log("Carro estacionado corretamente! Tempo final: " + textoCronometro.text);
            }
        }
    }

    private bool CarroEstacionadoCorretamente()
    {
        var carroForaDaVaga = !verificarVaga.CarroEstaDentroDaVaga();
        if (carroForaDaVaga) return false;

        var carroEmMovimento = carroRigidbody.linearVelocity.magnitude > 0.1f;
        if (carroEmMovimento) return false;

        var freioMaoSolto = !carroVolante.IsHandbrakeOn();
        if (freioMaoSolto) return false;

        var motorLigado = carroVolante.IsEngineOn();
        if (motorLigado) return false;

        var cintoColocado = carroVolante.IsSeatbeltOn();
        if (cintoColocado) return false;

        return true;
    }
}
