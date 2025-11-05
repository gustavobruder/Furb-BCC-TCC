using System.Collections.Generic;
using UnityEngine;

public class GerenciadorVagas : MonoBehaviour
{
    public List<Transform> vagasEstacionamento;
    public List<GameObject> carrosPrefabs;
    public GameObject indicadorObjetivoPrefab;

    private Transform _vagaEstacionamentoSorteada;
    private int _indiceVagaEstacionamentoSorteada;

    private void Start()
    {
        SortearVagaParaEstacionar();
        GerarCarrosEstacionados();
        GerarIndicadorObjetivo();
    }

    private void SortearVagaParaEstacionar()
    {
        _indiceVagaEstacionamentoSorteada = Random.Range(0, vagasEstacionamento.Count);
        _vagaEstacionamentoSorteada = vagasEstacionamento[_indiceVagaEstacionamentoSorteada];
        Debug.Log("Vaga sorteada para estacionar: " + _vagaEstacionamentoSorteada.name);
    }

    private void GerarCarrosEstacionados()
    {
        for (var i = 0; i < vagasEstacionamento.Count; i++)
        {
            if (i == _indiceVagaEstacionamentoSorteada)
                continue; // ignora a vaga sorteada para estacionar

            var indiceAleatorio = Random.Range(0, 6); // 0-4 = carros, 5 = vazio

            if (indiceAleatorio < 5)
            {
                var vagaEstacionamento = vagasEstacionamento[i];
                var carroPrefab = carrosPrefabs[indiceAleatorio];

                // rotação adicional de +90° no eixo Y, devido a orientação dos modelos de carros e vagas 
                var rotacao = vagaEstacionamento.rotation * Quaternion.Euler(0, 90, 0);

                var carroObstaculo = Instantiate(carroPrefab, vagaEstacionamento.position, rotacao);
                carroObstaculo.name = "CarroObstaculo" + (indiceAleatorio + 1);
                carroObstaculo.transform.parent = vagaEstacionamento;
            }
        }
    }

    private void GerarIndicadorObjetivo()
    {
        var posicaoInicial = _vagaEstacionamentoSorteada.position + Vector3.up * 4f;
        var indicadorObjetivo = Instantiate(indicadorObjetivoPrefab, posicaoInicial, Quaternion.identity);

        indicadorObjetivo.name = "IndicadorObjetivo";
        indicadorObjetivo.transform.parent = _vagaEstacionamentoSorteada;

        // comportamento de flutuação
        var indicadorFlutuante = indicadorObjetivo.AddComponent<IndicadorFlutuante>();
        indicadorFlutuante.amplitude = 0.25f;
        indicadorFlutuante.frequencia = 2f;
    }
}

public class IndicadorFlutuante : MonoBehaviour
{
    public float amplitude = 0.25f;
    public float frequencia = 2f;

    private Vector3 _posicaoInicialIndicador;

    private void Start()
    {
        _posicaoInicialIndicador = transform.position;
    }

    private void Update()
    {
        var offset = Mathf.Sin(Time.time * frequencia) * amplitude;
        transform.position = _posicaoInicialIndicador + new Vector3(0, offset, 0);
        transform.Rotate(Vector3.up * 30 * Time.deltaTime);
    }
}
