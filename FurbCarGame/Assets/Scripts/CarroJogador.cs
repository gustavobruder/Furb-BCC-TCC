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
    private float _rpmMotorParado = 800f;
    private float _rpmMotorMax = 7000f;
    private float _torqueMotor = 400f;
    private float _torqueMotorMax = 1500f;
    private float _direcaoFinal = 3.42f;
    private float _freioMax = 3000f;
    private float _embreagemThreshold = 0.9f;

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
    private bool[] _btnsPressionados = new bool[128];

    private void Start()
    {
        Debug.Log("SteeringInit:" + LogitechGSDK.LogiSteeringInitialize(false));
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
        var rawSteer = NormalizarEixo(logiState.lX);
        var currentSteerNorm = Mathf.Clamp(rawSteer, -1f, 1f);

        var rawAccel = NormalizarEixo(logiState.lY);
        var rawBrake = NormalizarEixo(logiState.lRz);
        var rawClutch = 0f;

        if (logiState.rglSlider != null && logiState.rglSlider.Length > 0)
            rawClutch = Mathf.Clamp(logiState.rglSlider[0] / 32767f, -1f, 1f);

        rawAccel *= -1f;
        rawBrake *= -1f;
        rawClutch *= -1f;

        var throttle = Mathf.InverseLerp(-1f, 1f, rawAccel);
        var brake = Mathf.InverseLerp(-1f, 1f, rawBrake);
        var clutch = Mathf.InverseLerp(-1f, 1f, rawClutch);

        var clutchFullyPressed = clutch >= _embreagemThreshold;

        carroVolante.AtualizarVolante(currentSteerNorm);
        carroRodas.AplicarDirecaoVolante(currentSteerNorm);

        if (BtnPressionado(logiState, INDICE_BTN_CINTO_DE_SEGURANCA)) carroCintoSeguranca.AlternarCintoDeSeguranca();
        if (BtnPressionado(logiState, INDICE_BTN_MOTOR)) carroMotor.AlternarMotor();
        if (BtnPressionado(logiState, INDICE_BTN_FREIO_DE_MAO_BAIXO)) carroFreioMao.AlternarFreioDeMao(false);
        if (BtnPressionado(logiState, INDICE_BTN_FREIO_DE_MAO_CIMA)) carroFreioMao.AlternarFreioDeMao(true);
        if (BtnPressionado(logiState, INDICE_BTN_MARCHA_BAIXO)) carroMarchas.ReduzirMarcha(clutchFullyPressed);
        if (BtnPressionado(logiState, INDICE_BTN_MARCHA_CIMA)) carroMarchas.AumentarMarcha(clutchFullyPressed);
        if (BtnPressionado(logiState, INDICE_BTN_CAMERA)) carroCameras.AlternarCamera();
        if (BtnPressionado(logiState, INDICE_BTN_PLAYSTATION)) SceneManager.LoadSceneAsync("MenuPrincipal");

        // POV (olhar)
        HandlePOV(logiState.rgdwPOV[0]);

        // Física e motor
        UpdateEngineAndDrive(throttle, brake, clutchFullyPressed);
    }

    private static float NormalizarEixo(int eixo)
    {
        return Mathf.Clamp(eixo / 32767f, -1f, 1f);
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

    public void UpdateEngineAndDrive(float throttle, float brakeInput, bool clutchFullyPressed)
    {
        var brakeTorque = brakeInput * _freioMax;

        carroRodas.AplicarFreioDianteiro(brakeTorque);
        carroRodas.AplicarFreioTraseiro(carroFreioMao.FreioDeMaoPuxado ? _freioMax : brakeTorque);

        if (carroMotor.MotorLigado)
        {
            var targetRpm = _rpmMotorParado + throttle * (_rpmMotorMax - _rpmMotorParado);
            carroMotor.AtualizarRpmMotor(Mathf.Lerp(carroMotor.RpmMotor, targetRpm, Time.deltaTime * 8f));
        }
        else
        {
            carroRodas.AplicarTorqueMotor(0f);
            return;
        }

        var inNeutral = carroMarchas.MarchaAtual == Marcha.N;
        var clutchPressed = clutchFullyPressed;

        if (inNeutral || clutchPressed)
        {
            carroRodas.AplicarTorqueMotor(0f);
            return;
        }

        var gearIdx = (int)carroMarchas.MarchaAtual;
        var gearRatio = carroMarchas.RelacaoForcasMarchas[gearIdx];
        var torque = throttle * _torqueMotor * gearRatio * _direcaoFinal;

        carroRodas.AplicarTorqueMotor(torque);
    }

    // ---------------------------
    // POV look handling
    // ---------------------------
    private Quaternion originalCameraLocalRotFirst;
    private Quaternion originalCameraLocalRotThird;
    private float povLookAngle = 20f;

    private void HandlePOV(uint pov)
    {
        Vector3 eulerOffset = Vector3.zero;
        switch (pov)
        {
            case 0: eulerOffset = new Vector3(-povLookAngle, 0f, 0f); break;
            case 9000: eulerOffset = new Vector3(0f, povLookAngle, 0f); break;
            case 18000: eulerOffset = new Vector3(povLookAngle, 0f, 0f); break;
            case 27000: eulerOffset = new Vector3(0f, -povLookAngle, 0f); break;
            default: eulerOffset = Vector3.zero; break;
        }

        // todo: ajustar
        // Camera activeCam = cameraEstaEmPrimeiraPessoa ? cameraPrimeiraPessoa : cameraTerceiraPessoa;
        // if (activeCam == null) return;
        //
        // if (originalCameraLocalRotFirst == Quaternion.identity && cameraPrimeiraPessoa != null)
        //     originalCameraLocalRotFirst = cameraPrimeiraPessoa.transform.localRotation;
        // if (originalCameraLocalRotThird == Quaternion.identity && cameraTerceiraPessoa != null)
        //     originalCameraLocalRotThird = cameraTerceiraPessoa.transform.localRotation;
        //
        // Quaternion target = Quaternion.Euler(eulerOffset);
        // if (cameraEstaEmPrimeiraPessoa && cameraPrimeiraPessoa != null)
        //     cameraPrimeiraPessoa.transform.localRotation = originalCameraLocalRotFirst * target;
        // if (!cameraEstaEmPrimeiraPessoa && cameraTerceiraPessoa != null)
        //     cameraTerceiraPessoa.transform.localRotation = originalCameraLocalRotThird * target;
    }
}
