using System;
using UnityEngine;

public class CarroMarchas : MonoBehaviour
{
    [Header("Marchas")]
    public Notificacao notificacao;
    public Marcha MarchaAtual { get; private set; } = Marcha.N;
    public float[] RelacaoForcasMarchas { get; private set; } = { -2.9f, 0f, 3.6f, 2.2f, 1.5f, 1.0f, 0.8f };

    private int _quantidadeMarchas = Enum.GetNames(typeof(Marcha)).Length;

    public void AumentarMarcha(bool embreagemPressionada) => TrocarMarcha(true, embreagemPressionada);

    public void ReduzirMarcha(bool embreagemPressionada) => TrocarMarcha(false, embreagemPressionada);

    private void TrocarMarcha(bool aumentarMarcha, bool embreagemPressionada)
    {
        if (!embreagemPressionada)
        {
            notificacao.MostrarNotificacao("Pise na embreagem para trocar de marcha.");
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
