using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuSelecionarCarro : MonoBehaviour
{
    public GameObject[] carros;
    public Button esquerda;
    public Button direita;
    int indice;

    void Start()
    {
        indice = PlayerPrefs.GetInt("indiceCarro");

        for (int i = 0; i < carros.Length; i++)
        {
            carros[i].SetActive(false);
            carros[indice].SetActive(true);
        }
    }

    void Update()
    {
        if (indice >= carros.Length - 1)
        {
            direita.interactable = false;
        }
        else
        {
            direita.interactable = true;
        }

        if (indice <= 0)
        {
            esquerda.interactable = false;
        }
        else
        {
            esquerda.interactable = true;
        }
    }

    public void Esquerda()
    {
        indice--;

        for (int i = 0; i < carros.Length; i++)
        {
            carros[i].SetActive(false);
            carros[indice].SetActive(true);
        }

        PlayerPrefs.SetInt("indiceCarro", indice);
        PlayerPrefs.Save();
    }

    public void Direita()
    {
        indice++;

        for (int i = 0; i < carros.Length; i++)
        {
            carros[i].SetActive(false);
            carros[indice].SetActive(true);
        }

        PlayerPrefs.SetInt("indiceCarro", indice);
        PlayerPrefs.Save();
    }

    public void Jogar1()
    {
        SceneManager.LoadSceneAsync("Estacionamento1");
    }

    public void Jogar2()
    {
        SceneManager.LoadSceneAsync("Estacionamento2");
    }

    public void Jogar3()
    {
        SceneManager.LoadSceneAsync("Estacionamento3");
    }
}
