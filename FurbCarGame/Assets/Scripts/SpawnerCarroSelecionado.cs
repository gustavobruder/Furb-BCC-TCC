using UnityEngine;

public class SpawnerCarroSelecionado : MonoBehaviour
{
    public GameObject[] carrosDisponiveis;
    public Vector3 posicaoInicialCarro = new Vector3(-8f, 0f, -20f);
    public GameObject CarrosSelecionado { get; private set; }

    private void Awake()
    {
        var indiceCarroSelecionado = PlayerPrefs.GetInt("indiceCarroSelecionado", 0);

        if (indiceCarroSelecionado < 0 || indiceCarroSelecionado >= carrosDisponiveis.Length)
        {
            Debug.LogWarning($"Índice de carro inválido ({indiceCarroSelecionado}), usando o primeiro carro.");
            indiceCarroSelecionado = 0;
        }

        for (var i = 0; i < carrosDisponiveis.Length; i++)
        {
            if (i != indiceCarroSelecionado)
            {
                Destroy(carrosDisponiveis[i]);
            }
            else
            {
                CarrosSelecionado = carrosDisponiveis[i];
                CarrosSelecionado.SetActive(true);
            }
        }
    }
}
