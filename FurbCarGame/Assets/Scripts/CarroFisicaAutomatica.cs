using UnityEngine;

public class CarroFisicaAutomatica : MonoBehaviour
{
    [Header("Referências")]
    public Rigidbody rb;
    public CarroMotor carroMotor;
    public CarroMarchas carroMarchas;
    public CarroRodas carroRodas;

    [Header("Curva de Torque")]
    public AnimationCurve curvaTorque; 

    [HideInInspector] public float embreagem;
    [HideInInspector] public float freio;
    [HideInInspector] public float acelerador;
    [HideInInspector] public bool trocarMarchasAutomaticamente;

    [Header("Parâmetros do Motor")]
    private float _rpmAtual;
    private float _rpmMinParado = 800f;
    private float _rpmMax = 7000f;
    private float _torqueMax = 450f;
    private float _fatorInerciaMotor = 6f;
    private float _forcaFreioMotor = 1300f;

    [Header("Embreagem")]
    private float _embreagemThreshold = 0.9f;

    [Header("Freio")]
    private float _forcaFreioMax = 3500f;

    [Header("Fisica Automatico")]
    private float _velocidadeTrocaMarcha1 = 12f;
    private float _velocidadeTrocaMarcha2 = 28f;
    private float _velocidadeTrocaMarcha3 = 45f;
    private float _velocidadeTrocaMarcha4 = 62f;

    [Header("Creep (andar sozinho)")]
    private float _creepTorque = 150f;
    private float _creepVelocidadeMax = 6f;

    private void Start()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        _rpmAtual = 0f;
    }

    private void FixedUpdate()
    {
        if (!carroMotor.MotorLigado)
        {
            _rpmAtual = 0f;
            carroRodas.AplicarTorqueMotor(0);
            return;
        }

        AtualizarRpm();
        TrocarMarchasAutomaticamente();
        AplicarTorqueMotor();
        AplicarFreios();
        AplicarFreioMotor();
    }

    private void AtualizarRpm()
    {
        float rpmAlvo;
        var embreagemPressionada = embreagem >= _embreagemThreshold;

        if (carroMarchas.MarchaAtual == Marcha.N || embreagemPressionada)
        {
            rpmAlvo = _rpmMinParado + acelerador * (_rpmMax - _rpmMinParado);
        }
        else
        {
            var velocidade = rb.linearVelocity.magnitude * 3.6f;
            rpmAlvo = Mathf.Lerp(_rpmMinParado, _rpmMax, velocidade / 140f);

            rpmAlvo += acelerador * 2500f;
        }

        rpmAlvo = Mathf.Clamp(rpmAlvo, _rpmMinParado, _rpmMax);

        _rpmAtual = Mathf.Lerp(_rpmAtual, rpmAlvo, Time.fixedDeltaTime * _fatorInerciaMotor);
        carroMotor.AtualizarRpmMotor(_rpmAtual);
    }

    private void TrocarMarchasAutomaticamente()
    {
        if (!trocarMarchasAutomaticamente)
            return;

        var velocidade = rb.linearVelocity.magnitude * 3.6f;
    
        switch (carroMarchas.MarchaAtual)
        {
            case Marcha.M1:
                if (velocidade > _velocidadeTrocaMarcha1)
                    carroMarchas.AumentarMarcha(true);
                break;
    
            case Marcha.M2:
                if (velocidade > _velocidadeTrocaMarcha2)
                    carroMarchas.AumentarMarcha(true);
                else if (velocidade < _velocidadeTrocaMarcha1)
                    carroMarchas.ReduzirMarcha(true);
                break;
    
            case Marcha.M3:
                if (velocidade > _velocidadeTrocaMarcha3)
                    carroMarchas.AumentarMarcha(true);
                else if (velocidade < _velocidadeTrocaMarcha2)
                    carroMarchas.ReduzirMarcha(true);
                break;
    
            case Marcha.M4:
                if (velocidade > _velocidadeTrocaMarcha4)
                    carroMarchas.AumentarMarcha(true);
                else if (velocidade < _velocidadeTrocaMarcha3)
                    carroMarchas.ReduzirMarcha(true);
                break;

            case Marcha.M5:
                if (velocidade < _velocidadeTrocaMarcha4)
                    carroMarchas.ReduzirMarcha(true);
                break;
        }
    }

    private void AplicarTorqueMotor()
    {
        var embreagemPressionada = embreagem >= _embreagemThreshold;

        if (carroMarchas.MarchaAtual == Marcha.N || embreagemPressionada)
        {
            carroRodas.AplicarTorqueMotor(0);
            return;
        }

        var torqueCurva = curvaTorque.Evaluate(_rpmAtual);
        var torqueFinal = torqueCurva * _torqueMax * acelerador;

        // creep (andar sozinho)
        if (acelerador < 0.01f)
        {
            var velocidade = rb.linearVelocity.magnitude * 3.6f;

            if (velocidade < _creepVelocidadeMax)
                torqueFinal = _creepTorque;
            else
                torqueFinal = 0;
        }

        carroRodas.AplicarTorqueMotor(carroMarchas.MarchaAtual == Marcha.R ? -torqueFinal : torqueFinal);
    }

    private void AplicarFreios()
    {
        var freioPedal = freio; 
        var freioAplicado = freioPedal * _forcaFreioMax;

        carroRodas.AplicarFreioDianteiro(freioAplicado);

        var freioTraseiro = carroMotor.carroFreioMao.FreioDeMaoPuxado
            ? _forcaFreioMax
            : freioAplicado;

        carroRodas.AplicarFreioTraseiro(freioTraseiro);
    }

    private void AplicarFreioMotor()
    {
        if (acelerador > 0.15f || freio > 0.1f)
            return;

        var forca = _forcaFreioMotor * (_rpmAtual / _rpmMax);
        rb.AddForce(-transform.forward * forca, ForceMode.Force);
    }
}
