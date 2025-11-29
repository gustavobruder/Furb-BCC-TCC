using UnityEngine;

public class CarroFreioMao : MonoBehaviour
{
    [Header("Freio de Mão")]
    public Rigidbody rb;
    public CarroCintoSeguranca carroCintoSeguranca;
    public CarroMotor carroMotor;
    public CarroMarchas carroMarchas;
    public CarroRodas carroRodas;
    public Notificacao notificacao;
    public bool FreioDeMaoPuxado { get; private set; } = true;

    private float _velocidadeParadaThreshold = 0.5f;
    private float _freioMax = 3000f;

    public void AlternarFreioDeMao(bool puxarFreioDeMao)
    {
        var carroEmMovimento = rb.linearVelocity.magnitude > _velocidadeParadaThreshold;
        if (carroEmMovimento)
        {
            notificacao.MostrarNotificacaoAviso($"O carro deve estar parado para {(FreioDeMaoPuxado ? "abaixar" : "puxar")} o freio de mão.");
            return;
        }
        if (!carroCintoSeguranca.CintoDeSegurancaColocado)
        {
            notificacao.MostrarNotificacaoAviso($"O cinto de segurança deve estar colocado para {(FreioDeMaoPuxado ? "abaixar" : "puxar")} o freio de mão.");
            return;
        }
        if (!carroMotor.MotorLigado)
        {
            notificacao.MostrarNotificacaoAviso($"O motor do carro deve estar ligado para {(FreioDeMaoPuxado ? "abaixar" : "puxar")} o freio de mão.");
            return;
        }
        if (carroMarchas.MarchaAtual != Marcha.N)
        {
            notificacao.MostrarNotificacaoAviso($"A marcha deve estar em neutra para {(FreioDeMaoPuxado ? "abaixar" : "puxar")} o freio de mão.");
            return;
        }

        FreioDeMaoPuxado = puxarFreioDeMao;
        var freioTraseiro = FreioDeMaoPuxado ? _freioMax : 0f;
        carroRodas.AplicarFreioTraseiro(freioTraseiro);
    }
}
