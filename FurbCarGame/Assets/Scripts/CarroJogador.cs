using Logitech;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class CarroJogador : MonoBehaviour
{
    // -------------------------
    // Parâmetros da física do carro
    // -------------------------
    [Header("Engine / RPM")]
    private Rigidbody _rb;
    private float _volante = 0.0f;
    private float _embreagem = 0.0f;
    private float _freio = 0.0f;
    private float _acelerador = 0.0f;

    private float _rpmMotorParado = 800f;
    private float _rpmMotorMax = 7000f;
    private float _torqueMotor = 400f;
    private float _torqueMotorMax = 1500f;
    private float _direcaoFinal = 3.42f;
    private float _freioMax = 3000f;
    private float _embreagemThreshold = 0.9f;
    private float _velocidade = 0.0f;

    // -------------------------
    // Estado interno
    // -------------------------
    public CarroCintoSeguranca carroCintoSeguranca;
    public CarroFreioMao carroFreioMao;
    public CarroMotor carroMotor;
    public CarroRodas carroRodas;
    public CarroVolante carroVolante;
    public CarroMarchas carroMarchas;
    public CarroCameras carroCameras;
    public Notificacao notificacao;

    // -------------------------
    // Estado Logitech
    // -------------------------
    private const int INDICE_BTN_CINTO_DE_SEGURANCA = 7;
    private const int INDICE_BTN_MOTOR = 23;
    private const int INDICE_BTN_FREIO_DE_MAO_BAIXO = 20;
    private const int INDICE_BTN_FREIO_DE_MAO_CIMA = 19;
    private const int INDICE_BTN_MARCHA_BAIXO = 5;
    private const int INDICE_BTN_MARCHA_CIMA = 4;
    private const int INDICE_BTN_CAMERA = 6;
    private const int INDICE_BTN_PLAYSTATION = 24; // btn options = 9
    private const int INDICE_BTN_QUADRADO = 1;
    private const int INDICE_BTN_BOLA = 2;
    private bool[] _btnsPressionados = new bool[128];

    private bool _estradaBarroHabilitada = false;
    private bool _estradaEscorregadiaHabilitada = false;

    private void Start()
    {
        Debug.Log("SteeringInit:" + LogitechGSDK.LogiSteeringInitialize(false));
        _rb = GetComponent<Rigidbody>();
        carroCameras.DefinirModoCamera();
    }

    private void OnApplicationQuit()
    {
        Debug.Log("SteeringShutdown:" + LogitechGSDK.LogiSteeringShutdown());
    }

    private void Update()
    {
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
            var logiState = LogitechGSDK.LogiGetStateUnity(0);
            ProcessInput(logiState);
            carroRodas.AtualizarRodas();
        }
        else if (!LogitechGSDK.LogiIsConnected(0))
        {
            Debug.LogWarning("PLEASE PLUG IN A STEERING WHEEL OR A FORCE FEEDBACK CONTROLLER");
        }
    }

    private void ProcessInput(LogitechGSDK.DIJOYSTATE2ENGINES logiState)
    {
        _volante = NormalizarVolante(logiState.lX);
        _embreagem = NormalizarPedal(logiState.rglSlider[0]);
        _freio = NormalizarPedal(logiState.lRz);
        _acelerador = NormalizarPedal(logiState.lY);

        var embreagemPressionada = _embreagem >= _embreagemThreshold;

        carroVolante.AtualizarVolante(_volante);
        carroRodas.AplicarDirecaoVolante(_volante);

        if (BtnPressionado(logiState, INDICE_BTN_CINTO_DE_SEGURANCA)) carroCintoSeguranca.AlternarCintoDeSeguranca();
        if (BtnPressionado(logiState, INDICE_BTN_MOTOR)) carroMotor.AlternarMotor();
        if (BtnPressionado(logiState, INDICE_BTN_FREIO_DE_MAO_BAIXO)) carroFreioMao.AlternarFreioDeMao(false);
        if (BtnPressionado(logiState, INDICE_BTN_FREIO_DE_MAO_CIMA)) carroFreioMao.AlternarFreioDeMao(true);
        if (BtnPressionado(logiState, INDICE_BTN_MARCHA_BAIXO)) carroMarchas.ReduzirMarcha(embreagemPressionada);
        if (BtnPressionado(logiState, INDICE_BTN_MARCHA_CIMA)) carroMarchas.AumentarMarcha(embreagemPressionada);
        if (BtnPressionado(logiState, INDICE_BTN_CAMERA)) carroCameras.AlternarCamera();
        if (BtnPressionado(logiState, INDICE_BTN_PLAYSTATION)) SceneManager.LoadSceneAsync("MenuPrincipal");
        if (BtnPressionado(logiState, INDICE_BTN_QUADRADO))
        {
            _estradaBarroHabilitada = !_estradaBarroHabilitada;
            notificacao.MostrarNotificacaoAviso($"Modo estrada de barro está {(_estradaBarroHabilitada ? "habilitado" : "desabilitado")}");
        }
        if (BtnPressionado(logiState, INDICE_BTN_BOLA))
        {
            _estradaEscorregadiaHabilitada = !_estradaEscorregadiaHabilitada;
            notificacao.MostrarNotificacaoAviso($"Modo estrada escorregadia está {(_estradaEscorregadiaHabilitada ? "habilitado" : "desabilitado")}");
        }

        carroCameras.ControlarPov(logiState.rgdwPOV[0]);

        AplicarEfeitosVolante();

        AtualizarFisicaMotorCarro(embreagemPressionada);
    }

    private static float NormalizarVolante(int volante)
    {
        return Mathf.Clamp(volante / 32767f, -1f, 1f);
    }

    private static float NormalizarPedal(float pedal)
    {
        return (32767f - pedal) / 65535f;
    }

    private bool BtnPressionado(LogitechGSDK.DIJOYSTATE2ENGINES logiState, int indiceBtn)
    {
        if (indiceBtn < 0 || indiceBtn >= logiState.rgbButtons.Length)
            return false;

        var btnNaoEstavaPressionadoAntes = !_btnsPressionados[indiceBtn];

        if (logiState.rgbButtons[indiceBtn] == 128)
            _btnsPressionados[indiceBtn] = true;
        else
            _btnsPressionados[indiceBtn] = false;

        var btnEstaPressionadoAgora = _btnsPressionados[indiceBtn];

        return btnNaoEstavaPressionadoAntes && btnEstaPressionadoAgora;
    }

    private void AtualizarFisicaMotorCarro(bool embreagemPressionada)
    {
        var freioAplicado = _freio * _freioMax;
        carroRodas.AplicarFreioDianteiro(freioAplicado);
        carroRodas.AplicarFreioTraseiro(carroFreioMao.FreioDeMaoPuxado ? _freioMax : freioAplicado);

        if (carroMotor.MotorLigado)
        {
            var rpmAlvo = _rpmMotorParado + _acelerador * (_rpmMotorMax - _rpmMotorParado);
            carroMotor.AtualizarRpmMotor(Mathf.Lerp(carroMotor.RpmMotor, rpmAlvo, Time.deltaTime * 8f));
        }
        else
        {
            carroRodas.AplicarTorqueMotor(0f);
            return;
        }

        var marchaNeutra = carroMarchas.MarchaAtual == Marcha.N;
        if (marchaNeutra || embreagemPressionada)
        {
            carroRodas.AplicarTorqueMotor(0f);
            return;
        }

        var indiceMarchaAtual = (int)carroMarchas.MarchaAtual;
        var relacaoForcaMarchaAtual = carroMarchas.RelacaoForcasMarchas[indiceMarchaAtual];
        var torque = _acelerador * _torqueMotor * relacaoForcaMarchaAtual * _direcaoFinal;
        carroRodas.AplicarTorqueMotor(torque);
    }

    private void AplicarEfeitosVolante()
    {
        _velocidade = _rb.linearVelocity.magnitude * 3.6f;
        var velocidade = Mathf.Clamp01(_velocidade / 100f);

        AplicarEfeitosVolante(velocidade);
        AplicarEfeitoEstradaBarroVolante(velocidade);
        AplicarEfeitoEstradaEscorregadiaVolante(velocidade);
    }

    private void AplicarEfeitosVolante(float velocidade)
    {
        if (carroMotor.MotorLigado)
        {
            var forcaEfeito = Mathf.RoundToInt(Mathf.Lerp(10, 75, velocidade));
            LogitechGSDK.LogiStopSpringForce(0);
            LogitechGSDK.LogiPlayDamperForce(0, forcaEfeito);
        }
        else
        {
            LogitechGSDK.LogiPlaySpringForce(0,0, 100, 100);
            LogitechGSDK.LogiStopDamperForce(0);
        }
    }

    private void AplicarEfeitoEstradaBarroVolante(float velocidade)
    {
        if (_estradaBarroHabilitada)
        {
            var forcaEfeito = Mathf.RoundToInt(Mathf.Lerp(25, 75, velocidade));
            LogitechGSDK.LogiPlayDirtRoadEffect(0, forcaEfeito);
        }
        else
        {
            LogitechGSDK.LogiStopDirtRoadEffect(0);
        }
    }

    private void AplicarEfeitoEstradaEscorregadiaVolante(float velocidade)
    {
        if (_estradaEscorregadiaHabilitada)
        {
            var forcaEfeito = Mathf.RoundToInt(Mathf.Lerp(25, 75, velocidade));
            LogitechGSDK.LogiPlaySlipperyRoadEffect(0, forcaEfeito);
        }
        else
        {
            LogitechGSDK.LogiStopSlipperyRoadEffect(0);
        }
    }
}
