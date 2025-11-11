using UnityEngine;

public class CarroCintoSeguranca : MonoBehaviour
{
    [Header("Cinto de Segurança")]
    public Rigidbody rb;
    public CarroFreioMao carroFreioMao;
    public CarroMotor carroMotor;
    public bool CintoDeSegurancaColocado { get; private set; } = false;

    private float _velocidadeParadaThreshold = 0.5f;

    public void AlternarCintoDeSeguranca()
    {
        var carroEmMovimento = rb.linearVelocity.magnitude > _velocidadeParadaThreshold;
        if (carroEmMovimento)
        {
            Debug.Log($"O carro deve estar parado para {(CintoDeSegurancaColocado ? "retirar" : "colocar")} o cinto de segurança.");
            return;
        }
        if (carroMotor.MotorLigado)
        {
            Debug.Log($"O motor do carro deve estar desligado para {(CintoDeSegurancaColocado ? "retirar" : "colocar")} o cinto de segurança.");
            return;
        }
        if (!carroFreioMao.FreioDeMaoPuxado)
        {
            Debug.Log($"O freio de mão deve estar puxado para {(CintoDeSegurancaColocado ? "retirar" : "colocar")} o cinto de segurança.");
            return;
        }

        CintoDeSegurancaColocado = !CintoDeSegurancaColocado;
    }
}
