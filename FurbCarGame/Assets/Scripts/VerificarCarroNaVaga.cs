using UnityEngine;

public class VerificarCarroNaVaga : MonoBehaviour
{
    public BoxCollider carroCollider;
    public BoxCollider vagaCollider;

    private bool _carroEstaDentroDaVagaTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other == vagaCollider)
            _carroEstaDentroDaVagaTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == vagaCollider)
            _carroEstaDentroDaVagaTrigger = false;
    }

    private void Update()
    {
        if (CarroEstaDentroDaVaga())
        {
            Debug.Log("✅ Carro completamente dentro da vaga!");
        }
    }

    public bool CarroEstaDentroDaVaga()
    {
        if (!_carroEstaDentroDaVagaTrigger)
            return false;

        return PontosDoCarroEstaoTodosDentroDaVaga();
    }

    private bool PontosDoCarroEstaoTodosDentroDaVaga()
    {
        var carroBounds = carroCollider.bounds;
        var min = carroBounds.min;
        var max = carroBounds.max;

        var pontos = new Vector3[8];
        pontos[0] = new Vector3(min.x, min.y, min.z);
        pontos[1] = new Vector3(max.x, min.y, min.z);
        pontos[2] = new Vector3(min.x, max.y, min.z);
        pontos[3] = new Vector3(max.x, max.y, min.z);
        pontos[4] = new Vector3(min.x, min.y, max.z);
        pontos[5] = new Vector3(max.x, min.y, max.z);
        pontos[6] = new Vector3(min.x, max.y, max.z);
        pontos[7] = new Vector3(max.x, max.y, max.z);

        foreach (var ponto in pontos)
        {
            if (!EstaDentroDaVaga(ponto))
                return false;
        }

        return true;
    }

    private bool EstaDentroDaVaga(Vector3 ponto)
    {
        var pontoMaisProximo = vagaCollider.ClosestPoint(ponto);
        return Vector3.Distance(ponto, pontoMaisProximo) < 0.001f;
    }
}
