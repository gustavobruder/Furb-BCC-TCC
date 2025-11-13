using System.Collections;
using UnityEngine;
using TMPro;

public class Notificacao : MonoBehaviour
{
    public GameObject painelNotificacao;
    public TextMeshProUGUI textoNotificacao;
    public float duracaoNotificacao = 3f;

    private Coroutine _coroutineAtiva;

    private void Awake()
    {
        painelNotificacao.SetActive(false);
    }

    public void MostrarNotificacao(string mensagem)
    {
        if (_coroutineAtiva != null)
            StopCoroutine(_coroutineAtiva);

        _coroutineAtiva = StartCoroutine(MostrarNotificacaoCoroutine(mensagem));
    }

    private IEnumerator MostrarNotificacaoCoroutine(string mensagem)
    {
        textoNotificacao.text = mensagem;
        painelNotificacao.SetActive(true);

        var canvasGroup = painelNotificacao.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = painelNotificacao.AddComponent<CanvasGroup>();

        for (float t = 0; t < 0.3f; t += Time.deltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / 0.3f);
            yield return null;
        }
        canvasGroup.alpha = 1;

        yield return new WaitForSeconds(duracaoNotificacao);

        for (float t = 0; t < 0.3f; t += Time.deltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / 0.3f);
            yield return null;
        }
        canvasGroup.alpha = 0;

        painelNotificacao.SetActive(false);
        _coroutineAtiva = null;
    }
}
