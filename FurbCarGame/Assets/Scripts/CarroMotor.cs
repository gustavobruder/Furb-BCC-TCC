using UnityEngine;

public class CarroMotor : MonoBehaviour
{
    [Header("Motor")]
    public Rigidbody rb;
    public CarroCintoSeguranca carroCintoSeguranca;
    public CarroFreioMao carroFreioMao;
    public bool MotorLigado { get; private set; } = false;
    public float RpmMotor { get; private set; } = 0f;

    private float _rpmMotorParado = 800f;
    private float _velocidadeParadaThreshold = 0.5f;

    public void AlternarMotor()
    {
        if (!MotorLigado)
        {
            LigarMotor();
        }
        else
        {
            DesligarMotor();
        }
    }

    private void LigarMotor()
    {
        var carroEmMovimento = rb.linearVelocity.magnitude > _velocidadeParadaThreshold;
        if (carroEmMovimento)
        {
            Debug.Log("O carro deve estar parado para ligar o motor do carro.");
            return;
        }
        if (!carroCintoSeguranca.CintoDeSegurancaColocado)
        {
            Debug.Log("O cinto de segurança deve estar colocado para ligar o motor do carro.");
            return;
        }
        if (!carroFreioMao.FreioDeMaoPuxado)
        {
            Debug.Log("O freio de mão deve estar puxado para ligar o motor do carro.");
            return;
        }

        MotorLigado = true;
        RpmMotor = _rpmMotorParado;
    }

    private void DesligarMotor()
    {
        var carroEmMovimento = rb.linearVelocity.magnitude > _velocidadeParadaThreshold;
        if (carroEmMovimento)
        {
            Debug.Log("O carro deve estar parado para desligar o motor do carro.");
            return;
        }
        if (!carroCintoSeguranca.CintoDeSegurancaColocado)
        {
            Debug.Log("O cinto de segurança deve estar colocado para desligar o motor do carro.");
            return;
        }
        if (!carroFreioMao.FreioDeMaoPuxado)
        {
            Debug.Log("O freio de mão deve estar puxado para desligar o motor do carro.");
            return;
        }

        MotorLigado = false;
        RpmMotor = 0f;
    }

    public void AtualizarRpmMotor(float rpmMotor)
    {
        RpmMotor = rpmMotor;
    }
}
