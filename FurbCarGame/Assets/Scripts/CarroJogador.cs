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

    private float _embreagemThreshold = 0.9f;
    private float _velocidade = 0.0f;

    // -------------------------
    // Estado interno
    // -------------------------
    public CarroFisicaAutomatica carroFisicaAutomatica;
    public CarroCintoSeguranca carroCintoSeguranca;
    public CarroFreioMao carroFreioMao;
    public CarroMotor carroMotor;
    public CarroRodas carroRodas;
    public CarroVolante carroVolante;
    public CarroMarchas carroMarchas;
    public CarroCameras carroCameras;
    public Notificacao notificacao;
    public ContadorColisoes contadorColisoes;
    public GameObject menuMapeamentoBotoes;

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
    private const int INDICE_BTN_OPTIONS = 9;
    private const int INDICE_BTN_PLAYSTATION = 24;
    private const int INDICE_BTN_QUADRADO = 1;
    private const int INDICE_BTN_BOLA = 2;
    private const int INDICE_BTN_TRIANGULO = 3;
    private bool[] _btnsPressionados = new bool[128];

    public bool EstradaBarroHabilitada { get; private set; } = false;
    public bool EstradaEscorregadiaHabilitada { get; private set; } = false;

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
            ProcessarEntradasVolantePedais(logiState);
        }
        else if (!LogitechGSDK.LogiIsConnected(0))
        {
            Debug.LogWarning("PLEASE PLUG IN A STEERING WHEEL OR A FORCE FEEDBACK CONTROLLER");
        }
    }

    private void ProcessarEntradasVolantePedais(LogitechGSDK.DIJOYSTATE2ENGINES logiState)
    {
        _volante = NormalizarVolante(logiState.lX);
        _embreagem = NormalizarPedal(logiState.rglSlider[0]);
        _freio = NormalizarPedal(logiState.lRz);
        _acelerador = NormalizarPedal(logiState.lY);

        carroFisicaAutomatica.embreagem = _embreagem;
        carroFisicaAutomatica.freio = _freio;
        carroFisicaAutomatica.acelerador = _acelerador;

        var embreagemPressionada = _embreagem >= _embreagemThreshold;

        ProcessarBotoesVolante(logiState, embreagemPressionada);

        AplicarEfeitosVolante();

        carroVolante.AtualizarVolante(_volante);

        carroRodas.AplicarDirecaoVolante(_volante);
        carroRodas.AtualizarRodas();

        carroCameras.ControlarPov(logiState.rgdwPOV[0]);
    }

    private static float NormalizarVolante(int volante)
    {
        return Mathf.Clamp(volante / 32767f, -1f, 1f);
    }

    private static float NormalizarPedal(float pedal)
    {
        return (32767f - pedal) / 65535f;
    }

    private void ProcessarBotoesVolante(LogitechGSDK.DIJOYSTATE2ENGINES logiState, bool embreagemPressionada)
    {
        if (BtnPressionado(logiState, INDICE_BTN_CINTO_DE_SEGURANCA)) carroCintoSeguranca.AlternarCintoDeSeguranca();
        if (BtnPressionado(logiState, INDICE_BTN_MOTOR)) carroMotor.AlternarMotor();
        if (BtnPressionado(logiState, INDICE_BTN_FREIO_DE_MAO_BAIXO)) carroFreioMao.AlternarFreioDeMao(false);
        if (BtnPressionado(logiState, INDICE_BTN_FREIO_DE_MAO_CIMA)) carroFreioMao.AlternarFreioDeMao(true);
        if (BtnPressionado(logiState, INDICE_BTN_MARCHA_BAIXO)) carroMarchas.ReduzirMarcha(embreagemPressionada);
        if (BtnPressionado(logiState, INDICE_BTN_MARCHA_CIMA)) carroMarchas.AumentarMarcha(embreagemPressionada);
        if (BtnPressionado(logiState, INDICE_BTN_CAMERA)) carroCameras.AlternarCamera();
        if (BtnPressionado(logiState, INDICE_BTN_OPTIONS)) menuMapeamentoBotoes.SetActive(!menuMapeamentoBotoes.activeSelf);
        if (BtnPressionado(logiState, INDICE_BTN_PLAYSTATION)) SceneManager.LoadSceneAsync("MenuPrincipal");
        if (BtnPressionado(logiState, INDICE_BTN_QUADRADO))
        {
            EstradaBarroHabilitada = !EstradaBarroHabilitada;
            notificacao.MostrarNotificacaoAviso($"Modo estrada de barro está {(EstradaBarroHabilitada ? "habilitado" : "desabilitado")}");
        }
        if (BtnPressionado(logiState, INDICE_BTN_BOLA))
        {
            EstradaEscorregadiaHabilitada = !EstradaEscorregadiaHabilitada;
            notificacao.MostrarNotificacaoAviso($"Modo estrada escorregadia está {(EstradaEscorregadiaHabilitada ? "habilitado" : "desabilitado")}");
        }
        if (BtnPressionado(logiState, INDICE_BTN_TRIANGULO))
        {
            carroFisicaAutomatica.trocarMarchasAutomaticamente = !carroFisicaAutomatica.trocarMarchasAutomaticamente;
            notificacao.MostrarNotificacaoAviso($"Modo troca de marchas automaticas está {(carroFisicaAutomatica.trocarMarchasAutomaticamente ? "habilitado" : "desabilitado")}");
        }
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
        if (!contadorColisoes.TemColisaoAtiva())
        {
            if (EstradaBarroHabilitada)
            {
                var forcaEfeito = Mathf.RoundToInt(Mathf.Lerp(25, 75, velocidade));
                LogitechGSDK.LogiPlayDirtRoadEffect(0, forcaEfeito);
            }
            else
            {
                LogitechGSDK.LogiStopDirtRoadEffect(0);
            }
        }
    }

    private void AplicarEfeitoEstradaEscorregadiaVolante(float velocidade)
    {
        if (EstradaEscorregadiaHabilitada)
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
