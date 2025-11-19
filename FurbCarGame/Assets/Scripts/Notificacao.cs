using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Notificacao : MonoBehaviour
{
    public GameObject painelNotificacao;
    public Image imagemNotificacao;
    public TextMeshProUGUI textoNotificacao;
    public float duracaoNotificacao = 3f;

    private Color _corPainelNotificacaoAviso = new Color(0f, 0f, 0f, 0.627451f);
    private Color _corPainelNotificacaoSucesso = new Color(0.3019608f, 1f, 0.6784314f, 0.627451f);
    private Color _corTextoNotificacaoAviso = new Color(1f, 1f, 1f, 255f);
    private Color _corTextoNotificacaoSucesso = new Color(0f, 0f, 0f, 255f);

    private CanvasGroup _canvasGroup;
    private Coroutine _coroutineAtiva;

    private void Awake()
    {
        painelNotificacao.SetActive(false);
    }

    private void Start()
    {
        _canvasGroup = painelNotificacao.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = painelNotificacao.AddComponent<CanvasGroup>();
    }

    public void MostrarNotificacaoAviso(string mensagem)
    {
        MostrarNotificacao(mensagem, _corPainelNotificacaoAviso, _corTextoNotificacaoAviso);
    }

    public void MostrarNotificacaoSucesso(string mensagem)
    {
        MostrarNotificacao(mensagem, _corPainelNotificacaoSucesso, _corTextoNotificacaoSucesso);
    }

    private void MostrarNotificacao(string mensagem, Color corPainel, Color corTexto)
    {
        if (_coroutineAtiva != null)
            StopCoroutine(_coroutineAtiva);

        _coroutineAtiva = StartCoroutine(MostrarNotificacaoCoroutine(mensagem, corPainel, corTexto));
    }

    private IEnumerator MostrarNotificacaoCoroutine(string mensagem, Color corPainel, Color corTexto)
    {
        imagemNotificacao.color = corPainel;
        textoNotificacao.text = mensagem;
        textoNotificacao.color = corTexto;
        painelNotificacao.SetActive(true);

        for (float t = 0; t < 0.3f; t += Time.deltaTime)
        {
            _canvasGroup.alpha = Mathf.Lerp(0, 1, t / 0.3f);
            yield return null;
        }
        _canvasGroup.alpha = 1;

        yield return new WaitForSeconds(duracaoNotificacao);

        for (float t = 0; t < 0.3f; t += Time.deltaTime)
        {
            _canvasGroup.alpha = Mathf.Lerp(1, 0, t / 0.3f);
            yield return null;
        }
        _canvasGroup.alpha = 0;

        painelNotificacao.SetActive(false);
        _coroutineAtiva = null;
    }
}
