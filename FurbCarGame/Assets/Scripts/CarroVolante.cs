using System;
using Logitech;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class CarroVolante : MonoBehaviour
{
    [Header("Wheel Visuals")]
    public Transform meshFL;
    public Transform meshFR;
    public Transform meshRL;
    public Transform meshRR;

    [Header("Wheel Colliders")]
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;

    [Header("Steering Wheel")]
    public Transform steeringWheel;
    public float maxSteeringWheelAngle = 450f;
    private Quaternion volanteBaseRotation;

    [Header("Câmeras")]
    public Camera cameraPrimeiraPessoa;
    public Camera cameraTerceiraPessoa;

    // -------------------------
    // Parâmetros da física do carro
    // -------------------------
    [Header("Steering / Drive")]
    public float maxSteerAngle = 35f;
    public float maxMotorTorque = 1500f;
    public float maxBrakeTorque = 3000f;

    [Header("Engine / RPM")]
    public float idleRpm = 800f;
    public float maxRpm = 7000f;
    public float engineTorque = 400f;
    public float finalDrive = 3.42f;
    public float clutchThreshold = 0.9f;

    [Header("Gears (R, N, 1, 2, 3, 4, 5)")]
    public float[] gearRatios = new float[] { -2.9f, 0f, 3.6f, 2.2f, 1.5f, 1.0f, 0.8f };

    [Header("Checks")]
    public float stoppedSpeedThreshold = 0.5f;

    // -------------------------
    // Estado interno
    // -------------------------
    private Rigidbody rb;

    private enum Gear { R = 0, N = 1, G1 = 2, G2 = 3, G3 = 4, G4 = 5, G5 = 6 }
    private Gear currentGear = Gear.N;

    private bool engineOn = false;
    private bool seatbeltOn = false;
    private bool handbrakeEngaged = true;
    private bool cameraEstaEmPrimeiraPessoa = true;

    private float engineRpm = 0f;
    private float currentSteerNorm = 0f;

    // -------------------------
    // Estado Logitech
    // -------------------------
    private const int INDICE_BTN_CINTO_DE_SEGURANCA = 7;
    private const int INDICE_BTN_MOTOR = 23;
    private const int INDICE_BTN_MARCHA_BAIXO = 5;
    private const int INDICE_BTN_MARCHA_CIMA = 4;
    private const int INDICE_BTN_FREIO_DE_MAO_BAIXO = 20;
    private const int INDICE_BTN_FREIO_DE_MAO_CIMA = 19;
    private const int INDICE_BTN_CAMERA = 6;
    private const int INDICE_BTN_PLAYSTATION = 24; // btn options = 9
    private bool[] _btnsPressionados = new bool[128];

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Debug.Log("SteeringInit:" + LogitechGSDK.LogiSteeringInitialize(false));
        engineRpm = idleRpm;
        DefinirModoCamera();
        volanteBaseRotation = steeringWheel.localRotation;
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
            UpdateWheelVisuals();
        }
        else if (!LogitechGSDK.LogiIsConnected(0))
        {
            Debug.LogWarning("PLEASE PLUG IN A STEERING WHEEL OR A FORCE FEEDBACK CONTROLLER");
        }
    }

    private void ProcessInput(LogitechGSDK.DIJOYSTATE2ENGINES logiState)
    {
        float NormalizarEixo(int eixo) => Mathf.Clamp(eixo / 32767f, -1f, 1f);

        var rawSteer = NormalizarEixo(logiState.lX);
        currentSteerNorm = Mathf.Clamp(rawSteer, -1f, 1f);

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

        var steerAngle = currentSteerNorm * maxSteerAngle;
        ApplySteering(steerAngle);
        UpdateVolanteVisual();

        var clutchFullyPressed = clutch >= clutchThreshold;

        bool BtnPressionado(LogitechGSDK.DIJOYSTATE2ENGINES logiState, int indiceBtn)
        {
            if (indiceBtn < 0 || indiceBtn >= logiState.rgbButtons.Length)
                return false;

            if (!_btnsPressionados[indiceBtn] && logiState.rgbButtons[indiceBtn] == 128)
                _btnsPressionados[indiceBtn] = true;
            else if (logiState.rgbButtons[indiceBtn] != 128)
                _btnsPressionados[indiceBtn] = false;

            return _btnsPressionados[indiceBtn];
        }

        if (BtnPressionado(logiState, INDICE_BTN_CINTO_DE_SEGURANCA)) AlternarCintoDeSeguranca();
        if (BtnPressionado(logiState, INDICE_BTN_MOTOR)) AlternarMotor();
        if (BtnPressionado(logiState, INDICE_BTN_MARCHA_BAIXO)) TrocarMarcha(false, clutchFullyPressed);
        if (BtnPressionado(logiState, INDICE_BTN_MARCHA_CIMA)) TrocarMarcha(true, clutchFullyPressed);
        if (BtnPressionado(logiState, INDICE_BTN_FREIO_DE_MAO_BAIXO)) AlternarFreioDeMao(false);
        if (BtnPressionado(logiState, INDICE_BTN_FREIO_DE_MAO_CIMA)) AlternarFreioDeMao(true);
        if (BtnPressionado(logiState, INDICE_BTN_CAMERA)) AlternarCamera();
        if (BtnPressionado(logiState, INDICE_BTN_PLAYSTATION)) SceneManager.LoadSceneAsync("MenuPrincipal");

        // POV (olhar)
        HandlePOV(logiState.rgdwPOV[0]);

        // Física e motor
        UpdateEngineAndDrive(throttle, brake, clutchFullyPressed, clutch);
    }

    private void ApplySteering(float steerAngle)
    {
        if (wheelFL) wheelFL.steerAngle = steerAngle;
        if (wheelFR) wheelFR.steerAngle = steerAngle;
    }

    private void UpdateWheelVisuals()
    {
        void UpdateMesh(WheelCollider wc, Transform mesh)
        {
            if (wc == null || mesh == null) return;
            Vector3 pos;
            Quaternion rot;
            wc.GetWorldPose(out pos, out rot);
            mesh.position = pos;
            mesh.rotation = rot;
        }

        UpdateMesh(wheelFL, meshFL);
        UpdateMesh(wheelFR, meshFR);
        UpdateMesh(wheelRL, meshRL);
        UpdateMesh(wheelRR, meshRR);
    }

    private void UpdateVolanteVisual()
    {
        var rotationY = currentSteerNorm * maxSteeringWheelAngle;
        steeringWheel.localRotation = volanteBaseRotation * Quaternion.Euler(0f, rotationY, 0f);
    }

    // ---------------------------
    // Motor, câmbio e tração (FWD)
    // ---------------------------
    private void UpdateEngineAndDrive(float throttle, float brakeInput, bool clutchFullyPressed, float rawClutchNormalized)
    {
        var brakeTorque = brakeInput * maxBrakeTorque;

        if (handbrakeEngaged)
        {
            if (wheelRL) wheelRL.brakeTorque = Mathf.Max(wheelRL.brakeTorque, maxBrakeTorque);
            if (wheelRR) wheelRR.brakeTorque = Mathf.Max(wheelRR.brakeTorque, maxBrakeTorque);
        }

        if (wheelFL) wheelFL.brakeTorque = brakeTorque;
        if (wheelFR) wheelFR.brakeTorque = brakeTorque;
        if (wheelRL && !handbrakeEngaged) wheelRL.brakeTorque = brakeTorque;
        if (wheelRR && !handbrakeEngaged) wheelRR.brakeTorque = brakeTorque;

        var targetRpm = idleRpm + throttle * (maxRpm - idleRpm);
        engineRpm = Mathf.Lerp(engineRpm, targetRpm, Time.deltaTime * 8f);

        if (!engineOn)
        {
            ApplyMotorTorqueToDriveWheels(0f);
            return;
        }

        var inNeutral = currentGear == Gear.N;
        var clutchPressed = clutchFullyPressed;

        if (inNeutral || clutchPressed)
        {
            ApplyMotorTorqueToDriveWheels(0f);
            return;
        }

        var gearIdx = (int)currentGear;
        var gearRatio = gearRatios[gearIdx];
        var torque = throttle * engineTorque * gearRatio * finalDrive;

        ApplyMotorTorqueToDriveWheels(torque);
    }

    private void ApplyMotorTorqueToDriveWheels(float torque)
    {
        // Tração dianteira (FWD)
        if (wheelFL) wheelFL.motorTorque = torque;
        if (wheelFR) wheelFR.motorTorque = torque;
        if (wheelRL) wheelRL.motorTorque = 0f;
        if (wheelRR) wheelRR.motorTorque = 0f;
    }

    private void AlternarCintoDeSeguranca()
    {
        if (rb.linearVelocity.magnitude > stoppedSpeedThreshold && seatbeltOn)
        {
            Debug.Log("Não é possível retirar o cinto em movimento.");
            return;
        }

        seatbeltOn = !seatbeltOn;
        Debug.Log("Cinto de segurança -> " + (seatbeltOn ? "Colocado" : "Retirado"));
    }

    // ---------------------------
    // Ligar/desligar motor
    // ---------------------------
    private void AlternarMotor()
    {
        if (!engineOn)
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
        if (!seatbeltOn)
        {
            Debug.Log("Coloque o cinto de segurança para ligar o carro.");
            return;
        }
        var stopped = rb.linearVelocity.magnitude <= stoppedSpeedThreshold;
        if (!stopped)
        {
            Debug.Log("O carro deve estar parado para ligar o motor.");
            return;
        }
        if (currentGear == Gear.N)
        {
            Debug.Log("Engate a marcha 1 ou R para iniciar o movimento após ligar.");
        }

        engineOn = true;
        engineRpm = idleRpm;
    }

    private void DesligarMotor()
    {
        if (currentGear != Gear.N)
        {
            Debug.Log("Engate a marcha N para desligar o carro.");
            return;
        }
        if (!handbrakeEngaged)
        {
            Debug.Log("Puxe o freio de mão antes de desligar o carro.");
            return;
        }

        engineOn = false;
        engineRpm = 0f;
    }

    private void TrocarMarcha(bool aumentarMarcha, bool embreagemCompletamentePressionada)
    {
        if (!embreagemCompletamentePressionada)
        {
            Debug.Log("Pise completamente na embreagem para trocar de marcha.");
            return;
        }

        var indiceMarcha = (int)currentGear;
        if (aumentarMarcha && indiceMarcha < gearRatios.Length - 1)
        {
            currentGear = (Gear)(indiceMarcha + 1);
        }
        else if (!aumentarMarcha && indiceMarcha > 0)
        {
            currentGear = (Gear)(indiceMarcha - 1);
        }
    }

    private void AlternarFreioDeMao(bool puxarFreioDeMao)
    {
        var carroParado = rb.linearVelocity.magnitude <= stoppedSpeedThreshold;
        if (!carroParado)
        {
            Debug.Log("O carro deve estar parado para acionar o freio de mão.");
            return;
        }

        handbrakeEngaged = puxarFreioDeMao;
        var freioTraseiro = handbrakeEngaged ? maxBrakeTorque : 0f;
        if (wheelRL) wheelRL.brakeTorque = freioTraseiro;
        if (wheelRR) wheelRR.brakeTorque = freioTraseiro;
    }

    private void AlternarCamera()
    {
        cameraEstaEmPrimeiraPessoa = !cameraEstaEmPrimeiraPessoa;
        DefinirModoCamera();
    }

    private void DefinirModoCamera()
    {
        cameraPrimeiraPessoa.enabled = cameraEstaEmPrimeiraPessoa;
        cameraTerceiraPessoa.enabled = !cameraEstaEmPrimeiraPessoa;
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

        Camera activeCam = cameraEstaEmPrimeiraPessoa ? cameraPrimeiraPessoa : cameraTerceiraPessoa;
        if (activeCam == null) return;

        if (originalCameraLocalRotFirst == Quaternion.identity && cameraPrimeiraPessoa != null)
            originalCameraLocalRotFirst = cameraPrimeiraPessoa.transform.localRotation;
        if (originalCameraLocalRotThird == Quaternion.identity && cameraTerceiraPessoa != null)
            originalCameraLocalRotThird = cameraTerceiraPessoa.transform.localRotation;

        Quaternion target = Quaternion.Euler(eulerOffset);
        if (cameraEstaEmPrimeiraPessoa && cameraPrimeiraPessoa != null)
            cameraPrimeiraPessoa.transform.localRotation = originalCameraLocalRotFirst * target;
        if (!cameraEstaEmPrimeiraPessoa && cameraTerceiraPessoa != null)
            cameraTerceiraPessoa.transform.localRotation = originalCameraLocalRotThird * target;
    }

    // ---------------------------
    // Utilitários públicos
    // ---------------------------
    public string GetGearString()
    {
        return currentGear switch
        {
            Gear.R => "R",
            Gear.N => "N",
            Gear.G1 => "1",
            Gear.G2 => "2",
            Gear.G3 => "3",
            Gear.G4 => "4",
            Gear.G5 => "5",
            _ => "?"
        };
    }

    public float GetEngineRpm() => engineRpm;
    public bool IsEngineOn() => engineOn;
    public bool IsSeatbeltOn() => seatbeltOn;
    public bool IsHandbrakeOn() => handbrakeEngaged;
    public string GetGearName() => GetGearString();
}
