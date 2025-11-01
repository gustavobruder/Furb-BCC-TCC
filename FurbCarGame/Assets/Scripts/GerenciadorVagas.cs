using System.Collections.Generic;
using UnityEngine;

public class GerenciadorVagas : MonoBehaviour
{
    [Header("Referências")]
    public List<Transform> parkingSpots;

    public List<GameObject> carPrefabs;

    public GameObject arrowIndicatorPrefab;

    [Header("Configuração Interna")]
    private Transform targetSpot; // vaga sorteada para estacionar
    private int targetSpotIndex;

    void Start()
    {
        ChooseTargetSpot();     // define qual vaga será o objetivo
        GenerateParkingCars();  // gera os carros nas demais vagas
        SpawnIndicator();       // cria o indicador visual sobre a vaga alvo
    }

    // Sorteia uma vaga que será o destino do jogador
    void ChooseTargetSpot()
    {
        targetSpotIndex = Random.Range(0, parkingSpots.Count);
        targetSpot = parkingSpots[targetSpotIndex];

        // Remove qualquer carro que possa estar nessa vaga
        // foreach (Transform child in targetSpot)
        // {
        //     Destroy(child.gameObject);
        // }

        Debug.Log("Vaga sorteada para estacionar: " + targetSpot.name);
    }

    // Gera carros aleatoriamente nas outras vagas
    void GenerateParkingCars()
    {
        for (int i = 0; i < parkingSpots.Count; i++)
        {
            // pula a vaga alvo (deve ficar vazia)
            if (i == targetSpotIndex)
                continue;

            int randomChoice = Random.Range(0, 6); // 0-4 = carros, 5 = vazio

            if (randomChoice < 5)
            {
                Transform spot = parkingSpots[i];
                GameObject carPrefab = carPrefabs[randomChoice];

                // Rotação adicional de +90° no eixo Y
                Quaternion rotation = spot.rotation * Quaternion.Euler(0, 90, 0);

                GameObject car = Instantiate(carPrefab, spot.position, rotation);
                car.name = "Car_" + (randomChoice + 1);
                car.transform.parent = spot; // para manter organizado na hierarquia
            }
        }
    }

    // Cria a seta flutuante sobre a vaga alvo
    void SpawnIndicator()
    {
        if (arrowIndicatorPrefab == null)
        {
            Debug.LogWarning("Nenhum prefab de indicador foi atribuído ao ParkingManager!");
            return;
        }

        Vector3 spawnPos = targetSpot.position + Vector3.up * 4f; // altura da seta
        GameObject arrow = Instantiate(arrowIndicatorPrefab, spawnPos, Quaternion.identity);

        arrow.name = "TargetIndicator";
        arrow.transform.parent = targetSpot;

        // Adiciona comportamento de flutuação simples
        FloatingIndicator floating = arrow.AddComponent<FloatingIndicator>();
        floating.amplitude = 0.25f;
        floating.frequency = 2f;
    }
}

// Classe auxiliar para dar um efeito de flutuação
public class FloatingIndicator : MonoBehaviour
{
    public float amplitude = 0.25f; // altura do movimento
    public float frequency = 2f;    // velocidade
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = startPos + new Vector3(0, offset, 0);
        transform.Rotate(Vector3.up * 30 * Time.deltaTime); // rotação leve contínua
    }
}
