using UnityEngine;

public class CarroSelecionadoSpawner : MonoBehaviour
{
    [Header("Lista de prefabs dos carros")]
    public GameObject[] carrosDisponiveis;

    [Header("Posição inicial do carro na cena")]
    public Vector3 posicaoInicial = new Vector3(-8f, 0f, -20f);

    void Start()
    {
        // Lê o índice salvo no menu
        int indiceCarro = PlayerPrefs.GetInt("indiceCarro", 0);

        // Verifica se o índice é válido
        if (indiceCarro < 0 || indiceCarro >= carrosDisponiveis.Length)
        {
            Debug.LogWarning($"Índice de carro inválido ({indiceCarro}), usando o primeiro carro.");
            indiceCarro = 0;
        }

        // Instancia o carro correspondente
        GameObject carroSelecionado = Instantiate(
            carrosDisponiveis[indiceCarro],
            posicaoInicial,
            Quaternion.identity
        );

        carroSelecionado.name = "CarroSelecionado";
    }
}
