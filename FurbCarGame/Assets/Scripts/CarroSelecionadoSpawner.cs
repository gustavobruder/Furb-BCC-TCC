using UnityEngine;

public class CarroSelecionadoSpawner : MonoBehaviour
{
    public GameObject[] carrosDisponiveis;
    public Vector3 posicaoInicialCarro = new Vector3(-8f, 0f, -20f);

    private void Start()
    {
        var indiceCarroSelecionado = PlayerPrefs.GetInt("indiceCarroSelecionado", 0);

        if (indiceCarroSelecionado < 0 || indiceCarroSelecionado >= carrosDisponiveis.Length)
        {
            Debug.LogWarning($"Índice de carro inválido ({indiceCarroSelecionado}), usando o primeiro carro.");
            indiceCarroSelecionado = 0;
        }

        var carroSelecionado = Instantiate(
            carrosDisponiveis[indiceCarroSelecionado],
            posicaoInicialCarro,
            Quaternion.identity
        );

        carroSelecionado.name = "CarroSelecionado";
    }
}
