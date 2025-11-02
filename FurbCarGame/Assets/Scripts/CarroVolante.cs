using System;
using Logitech;
using UnityEngine;

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

    [Header("Câmeras")]
    public Camera cameraFirstPerson;
    public Camera cameraThirdPerson;

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
    [Header("RigidBody")]
    private Rigidbody rb;

    private enum Gear { R = 0, N = 1, G1 = 2, G2 = 3, G3 = 4, G4 = 5, G5 = 6 }
    private Gear currentGear = Gear.N;

    private bool engineOn = false;
    private bool seatbeltOn = false;
    private bool handbrakeEngaged = true;
    private bool inFirstPerson = true;

    private float engineRpm = 0f;

    // -------------------------
    // Estado Logitech
    // -------------------------
    private const int IDX_BTN_SEATBELT = 7;
    private const int IDX_BTN_ENGINE = 23;
    private const int IDX_BTN_GEAR_DOWN = 5;
    private const int IDX_BTN_GEAR_UP = 4;
    private const int IDX_BTN_HANDBRAKE_DOWN = 20;
    private const int IDX_BTN_HANDBRAKE_UP = 19;
    private const int IDX_BTN_CAMERA = 6;

    private byte[] prevButtons = new byte[128];
    private LogitechGSDK.DIJOYSTATE2ENGINES lastRec;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Debug.Log("SteeringInit:" + LogitechGSDK.LogiSteeringInitialize(false));
        engineRpm = idleRpm;
        SetCameraMode(inFirstPerson);
    }

    private void OnApplicationQuit()
    {
        Debug.Log("SteeringShutdown:" + LogitechGSDK.LogiSteeringShutdown());
    }

    private void Update()
    {
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
            var rec = LogitechGSDK.LogiGetStateUnity(0);
            lastRec = rec;
            ProcessInput(rec);
            UpdateWheelVisuals();
            Array.Copy(rec.rgbButtons, prevButtons, Math.Min(prevButtons.Length, rec.rgbButtons.Length));
        }
        else if (!LogitechGSDK.LogiIsConnected(0))
        {
            Debug.LogWarning("PLEASE PLUG IN A STEERING WHEEL OR A FORCE FEEDBACK CONTROLLER");
        }
    }

    private void ProcessInput(LogitechGSDK.DIJOYSTATE2ENGINES rec)
    {
        float NormalizeAxis(int val) => Mathf.Clamp(val / 32767f, -1f, 1f);

        var rawSteer = NormalizeAxis(rec.lX);
        var rawAccel = NormalizeAxis(rec.lY);
        var rawBrake = NormalizeAxis(rec.lRz);
        var rawClutch = 0f;

        if (rec.rglSlider != null && rec.rglSlider.Length > 0)
            rawClutch = Mathf.Clamp(rec.rglSlider[0] / 32767f, -1f, 1f);

        rawAccel *= -1f;
        rawBrake *= -1f;
        rawClutch *= -1f;

        var throttle = Mathf.InverseLerp(-1f, 1f, rawAccel);
        var brake = Mathf.InverseLerp(-1f, 1f, rawBrake);
        var clutch = Mathf.InverseLerp(-1f, 1f, rawClutch);

        var steerNorm = Mathf.Clamp(rawSteer, -1f, 1f);
        var steerAngle = steerNorm * maxSteerAngle;
        ApplySteering(steerAngle);

        var clutchFullyPressed = clutch >= clutchThreshold;

        bool BtnPressed(LogitechGSDK.DIJOYSTATE2ENGINES state, int idx)
        {
            if (idx < 0 || idx >= state.rgbButtons.Length) return false;
            return state.rgbButtons[idx] == 128 && prevButtons[idx] != 128;
        }

        // Cinto
        if (BtnPressed(rec, IDX_BTN_SEATBELT)) ToggleSeatbelt();

        // Ligar/desligar motor 
        if (BtnPressed(rec, IDX_BTN_ENGINE)) TryToggleEngine();

        // Câmbio: diminuir/aumentar marcha
        if (BtnPressed(rec, IDX_BTN_GEAR_DOWN)) TryChangeGear(false, clutchFullyPressed);
        if (BtnPressed(rec, IDX_BTN_GEAR_UP)) TryChangeGear(true, clutchFullyPressed);

        // Freio de mão: soltar/puxar
        if (BtnPressed(rec, IDX_BTN_HANDBRAKE_DOWN)) TrySetHandbrake(false);
        if (BtnPressed(rec, IDX_BTN_HANDBRAKE_UP)) TrySetHandbrake(true);

        // Câmera
        if (BtnPressed(rec, IDX_BTN_CAMERA)) ToggleCamera();

        // POV (olhar)
        HandlePOV(rec.rgdwPOV[0]);

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

    private void ToggleSeatbelt()
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
    private void TryToggleEngine()
    {
        if (!engineOn)
        {
            TryToggleEngineOn();
        }
        else
        {
            TryToggleEngineOff();
        }
    }

    private void TryToggleEngineOn()
    {
        if (!seatbeltOn)
        {
            Debug.Log("Coloque o cinto de segurança para ligar o carro.");
            return;
        }
        var stopped = rb.linearVelocity.magnitude <= stoppedSpeedThreshold;
        if (!stopped)
        {
            Debug.Log("O carro deve estar parado para ligar.");
            return;
        }
        if (currentGear == Gear.N)
        {
            Debug.Log("Engate a marcha 1 ou R para iniciar o movimento após ligar.");
        }

        engineOn = true;
        engineRpm = idleRpm;
        Debug.Log("Carro ligado.");
    }

    private void TryToggleEngineOff()
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
        Debug.Log("Carro desligado. Agora retire o cinto de segurança.");
    }

    private void TryChangeGear(bool up, bool clutchFullyPressed)
    {
        if (!clutchFullyPressed)
        {
            Debug.Log("Aperte completamente a embreagem para trocar a marcha.");
            return;
        }

        var idx = (int)currentGear;
        if (up && idx < gearRatios.Length - 1)
        {
            currentGear = (Gear)(idx + 1);
            Debug.Log("Marcha aumentada -> " + currentGear);
        }
        else if (!up && idx > 0)
        {
            currentGear = (Gear)(idx - 1);
            Debug.Log("Marcha reduzida -> " + currentGear);
        }
    }

    private void TrySetHandbrake(bool up)
    {
        var stopped = rb.linearVelocity.magnitude <= stoppedSpeedThreshold;
        if (!stopped)
        {
            Debug.Log("Car must be stopped to change handbrake.");
            return;
        }

        handbrakeEngaged = up;
        var rearBrake = handbrakeEngaged ? maxBrakeTorque : 0f;
        if (wheelRL) wheelRL.brakeTorque = rearBrake;
        if (wheelRR) wheelRR.brakeTorque = rearBrake;
        Debug.Log("Freio de mão " + (handbrakeEngaged ? "puxado" : "abaixado"));
    }

    private void ToggleCamera()
    {
        inFirstPerson = !inFirstPerson;
        SetCameraMode(inFirstPerson);
        Debug.Log("Câmera -> " + (inFirstPerson ? "1ª pessoa" : "3ª pessoa"));
    }

    private void SetCameraMode(bool firstPerson)
    {
        if (cameraFirstPerson) cameraFirstPerson.enabled = firstPerson;
        if (cameraThirdPerson) cameraThirdPerson.enabled = !firstPerson;
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

        Camera activeCam = inFirstPerson ? cameraFirstPerson : cameraThirdPerson;
        if (activeCam == null) return;

        if (originalCameraLocalRotFirst == Quaternion.identity && cameraFirstPerson != null)
            originalCameraLocalRotFirst = cameraFirstPerson.transform.localRotation;
        if (originalCameraLocalRotThird == Quaternion.identity && cameraThirdPerson != null)
            originalCameraLocalRotThird = cameraThirdPerson.transform.localRotation;

        Quaternion target = Quaternion.Euler(eulerOffset);
        if (inFirstPerson && cameraFirstPerson != null)
            cameraFirstPerson.transform.localRotation = originalCameraLocalRotFirst * target;
        if (!inFirstPerson && cameraThirdPerson != null)
            cameraThirdPerson.transform.localRotation = originalCameraLocalRotThird * target;
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
