using System;
using UnityEngine;

public class CarroMarchas : MonoBehaviour
{
    [Header("Marchas")]
    public CarroCintoSeguranca carroCintoSeguranca;
    public CarroMotor carroMotor;
    public CarroFreioMao carroFreioMao;
    public Notificacao notificacao;
    public Marcha MarchaAtual { get; private set; } = Marcha.N;

    private int _quantidadeMarchas = Enum.GetNames(typeof(Marcha)).Length;

    public void AumentarMarcha(bool embreagemPressionada) => TrocarMarcha(true, embreagemPressionada);

    public void ReduzirMarcha(bool embreagemPressionada) => TrocarMarcha(false, embreagemPressionada);

    private void TrocarMarcha(bool aumentarMarcha, bool embreagemPressionada)
    {
        if (!carroCintoSeguranca.CintoDeSegurancaColocado)
        {
            notificacao.MostrarNotificacaoAviso("O cinto de segurança deve estar colocado para trocar de marcha.");
            return;
        }
        if (!carroMotor.MotorLigado)
        {
            notificacao.MostrarNotificacaoAviso("O motor do carro deve estar ligado para trocar de marcha.");
            return;
        }
        if (carroFreioMao.FreioDeMaoPuxado)
        {
            notificacao.MostrarNotificacaoAviso("O freio de mão deve estar solto para trocar de marcha.");
            return;
        }
        if (!embreagemPressionada)
        {
            notificacao.MostrarNotificacaoAviso("Pise na embreagem para trocar de marcha.");
            return;
        }

        var indiceMarcha = (int)MarchaAtual;
        if (aumentarMarcha && indiceMarcha < _quantidadeMarchas - 1)
        {
            MarchaAtual = (Marcha)(indiceMarcha + 1);
        }
        else if (!aumentarMarcha && indiceMarcha > 0)
        {
            MarchaAtual = (Marcha)(indiceMarcha - 1);
        }
    }
}

public enum Marcha
{
    R = 0,
    N = 1,
    M1 = 2,
    M2 = 3,
    M3 = 4,
    M4 = 5,
    M5 = 6,
}
