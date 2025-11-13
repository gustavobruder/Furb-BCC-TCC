using UnityEngine;

public class CarroCintoSeguranca : MonoBehaviour
{
    [Header("Cinto de Segurança")]
    public Rigidbody rb;
    public CarroFreioMao carroFreioMao;
    public CarroMotor carroMotor;
    public Notificacao notificacao;
    public bool CintoDeSegurancaColocado { get; private set; } = false;

    private float _velocidadeParadaThreshold = 0.5f;

    public void AlternarCintoDeSeguranca()
    {
        var carroEmMovimento = rb.linearVelocity.magnitude > _velocidadeParadaThreshold;
        if (carroEmMovimento)
        {
            notificacao.MostrarNotificacaoAviso($"O carro deve estar parado para {(CintoDeSegurancaColocado ? "retirar" : "colocar")} o cinto de segurança.");
            return;
        }
        if (carroMotor.MotorLigado)
        {
            notificacao.MostrarNotificacaoAviso($"O motor do carro deve estar desligado para {(CintoDeSegurancaColocado ? "retirar" : "colocar")} o cinto de segurança.");
            return;
        }
        if (!carroFreioMao.FreioDeMaoPuxado)
        {
            notificacao.MostrarNotificacaoAviso($"O freio de mão deve estar puxado para {(CintoDeSegurancaColocado ? "retirar" : "colocar")} o cinto de segurança.");
            return;
        }

        CintoDeSegurancaColocado = !CintoDeSegurancaColocado;
    }
}
