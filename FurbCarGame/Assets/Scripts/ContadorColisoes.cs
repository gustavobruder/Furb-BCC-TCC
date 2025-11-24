using System.Collections;
using Logitech;
using UnityEngine;
using UnityEngine.UI;

public class ContadorColisoes : MonoBehaviour
{
    public Text textoColisoes;
    private int _quantidadeColisoes = 0;
    private Coroutine _coroutineAtiva;

    private void OnCollisionEnter(Collision collision)
    {
        _quantidadeColisoes++;

        DefinirTextoColisoes();

        AplicarEfeitoColisao();
    }

    public bool TemColisaoAtiva()
    {
        return _coroutineAtiva != null;
    }

    private void DefinirTextoColisoes()
    {
        textoColisoes.text = "Colisões: " + _quantidadeColisoes;
    }

    private void AplicarEfeitoColisao()
    {
        if (_coroutineAtiva != null)
            StopCoroutine(_coroutineAtiva);

        _coroutineAtiva = StartCoroutine(AplicarEfeitoColisaoCoroutine());
    }

    private IEnumerator AplicarEfeitoColisaoCoroutine()
    {
        LogitechGSDK.LogiPlayDirtRoadEffect(0, 50);

        yield return new WaitForSeconds(0.5f);

        LogitechGSDK.LogiStopDirtRoadEffect(0);

        _coroutineAtiva = null;
    }
}
