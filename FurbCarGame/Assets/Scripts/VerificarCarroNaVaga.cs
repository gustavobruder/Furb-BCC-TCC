using UnityEngine;

public class VerificarCarroNaVaga : MonoBehaviour
{
    public Rigidbody rb;
    public BoxCollider carroCollider;
    public GerenciadorVagas gerenciadorVagas;
    public CarroCintoSeguranca carroCintoSeguranca;
    public CarroMotor carroMotor;
    public CarroFreioMao carroFreioMao;
    public Cronometro cronometro;
    public Notificacao notificacao;

    private bool _carroEstaDentroDaVagaTrigger = false;
    private bool _carroEstacionadoCorretamente = false;
    private float _velocidadeParadaThreshold = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other == gerenciadorVagas.BoxColliderVagaEstacionamentoSorteada)
            _carroEstaDentroDaVagaTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == gerenciadorVagas.BoxColliderVagaEstacionamentoSorteada)
            _carroEstaDentroDaVagaTrigger = false;
    }

    private void Update()
    {
        if (_carroEstacionadoCorretamente)
            return;

        if (CarroEstacionadoCorretamente())
        {
            _carroEstacionadoCorretamente = true;
            cronometro.PararCronometro();
            notificacao.MostrarNotificacaoSucesso("Carro estacionado com sucesso! Volte para o menu clicando no botão PlayStation!");
        }
    }

    private bool CarroEstacionadoCorretamente()
    {
        var carroForaDaVaga = !CarroEstaDentroDaVaga();
        if (carroForaDaVaga) return false;

        var carroEmMovimento = rb.linearVelocity.magnitude > _velocidadeParadaThreshold;
        if (carroEmMovimento) return false;

        var freioDeMaoSolto = !carroFreioMao.FreioDeMaoPuxado;
        if (freioDeMaoSolto) return false;

        var motorLigado = carroMotor.MotorLigado;
        if (motorLigado) return false;

        var cintoDeSegurancaColocado = carroCintoSeguranca.CintoDeSegurancaColocado;
        if (cintoDeSegurancaColocado) return false;

        return true;
    }

    private bool CarroEstaDentroDaVaga()
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
            if (!PontoEstaDentroDaVaga(ponto))
                return false;
        }

        return true;
    }

    private bool PontoEstaDentroDaVaga(Vector3 ponto)
    {
        var pontoMaisProximo = gerenciadorVagas.BoxColliderVagaEstacionamentoSorteada.ClosestPoint(ponto);
        return Vector3.Distance(ponto, pontoMaisProximo) < 0.001f;
    }
}
