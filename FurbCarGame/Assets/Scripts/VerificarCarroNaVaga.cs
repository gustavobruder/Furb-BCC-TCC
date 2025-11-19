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
        var vagaBounds = gerenciadorVagas.BoxColliderVagaEstacionamentoSorteada.bounds;

        var dentroX = carroBounds.min.x >= vagaBounds.min.x && carroBounds.max.x <= vagaBounds.max.x;
        var dentroY = carroBounds.min.y >= vagaBounds.min.y && carroBounds.max.y <= vagaBounds.max.y;
        var dentroZ = carroBounds.min.z >= vagaBounds.min.z && carroBounds.max.z <= vagaBounds.max.z;
        
        return dentroX && dentroY && dentroZ;
    }
}
