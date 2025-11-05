using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuSelecionarCarro : MonoBehaviour
{
    public GameObject[] carros;
    public Button btnEsquerda;
    public Button btnDireita;

    private int _indiceCarroSelecionado;

    private void Start()
    {
        _indiceCarroSelecionado = PlayerPrefs.GetInt("indiceCarroSelecionado");
        DefinirCarroSelecionado();
    }

    private void Update()
    {
        btnEsquerda.interactable = _indiceCarroSelecionado > 0;
        btnDireita.interactable = _indiceCarroSelecionado < carros.Length - 1;
    }

    public void Esquerda()
    {
        _indiceCarroSelecionado--;
        DefinirCarroSelecionado();
        SalvarIndiceCarroSelecionado();
    }

    public void Direita()
    {
        _indiceCarroSelecionado++;
        DefinirCarroSelecionado();
        SalvarIndiceCarroSelecionado();
    }

    private void DefinirCarroSelecionado()
    {
        for (var i = 0; i < carros.Length; i++)
        {
            carros[i].SetActive(i == _indiceCarroSelecionado);
        }
    }

    private void SalvarIndiceCarroSelecionado()
    {
        PlayerPrefs.SetInt("indiceCarroSelecionado", _indiceCarroSelecionado);
        PlayerPrefs.Save();
    }

    public void JogarFase1()
    {
        SceneManager.LoadSceneAsync("Estacionamento1");
    }

    public void JogarFase2()
    {
        SceneManager.LoadSceneAsync("Estacionamento2");
    }

    public void JogarFase3()
    {
        SceneManager.LoadSceneAsync("Estacionamento3");
    }
}
