using System;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarControllerLogitechSDK : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;

    [Header("Visual Wheel Meshes (optional)")]
    public Transform meshFL;
    public Transform meshFR;
    public Transform meshRL;
    public Transform meshRR;

    [Header("Steering")]
    public float maxSteerAngle = 30f;

    [Header("Drive / Brakes")]
    public float[] gearRatios = new float[] { -3f, 0f, 3f, 2.2f, 1.5f, 1.0f, 0.8f };
    public int currentGearIndex = 1;
    public float[] torquePerGear = new float[] { 0f, 0f, 400f, 350f, 300f, 250f, 200f };
    public float brakeTorque = 1500f;
    public float handbrakeTorque = 3000f;
    public bool handbrakeEngaged = true;

    [Header("Engine RPM")]
    public float engineRPM = 800f;
    public float idleRPM = 800f;
    public float maxRPM = 8000f;
    public float rpmRiseRate = 3000f;
    public float rpmDropRate = 2000f;
    public float minRPMForShift = 1500f;
    public bool engineOn = false;
    private bool isStalled = false;

    [Header("Clutch / Shift")]
    [Range(0f,1f)]
    public float clutchPressedThreshold = 0.9f;

    [Header("Buttons (Logitech SDK)")]
    public int btnGearUp = 3;      // botão 4 no jogo
    public int btnGearDown = 4;    // botão 5
    public int btnIgnition = 22;   // botão 23
    public int btnHandbrakeUp = 18;// botão 19
    public int btnHandbrakeDown = 19;// botão 20
    public int btnCameraToggle = 2; // botão 3 (triângulo)

    [Header("Cameras")]
    public Camera camFirstPerson;
    public Camera camThirdPerson;
    private bool camFirst = true;

    private Rigidbody rb;

    // Logitech SDK import (ajuste conforme seu wrapper)
    private const int LOGI_DEVICETYPE_DRIVING = 3;  // valor da enumeração (exemplo)
    [DllImport("LogitechSteeringWheelEnginesWrapper", CallingConvention=CallingConvention.Cdecl)]
    private static extern bool LogiSteeringInitialize(bool ignoreXInputControllers);
    [DllImport("LogitechSteeringWheelEnginesWrapper", CallingConvention=CallingConvention.Cdecl)]
    private static extern bool LogiSteeringShutdown();
    [DllImport("LogitechSteeringWheelEnginesWrapper", CallingConvention=CallingConvention.Cdecl)]
    private static extern bool LogiUpdate();
    [DllImport("LogitechSteeringWheelEnginesWrapper", CallingConvention=CallingConvention.Cdecl)]
    private static extern bool LogiIsConnected(int index);
    [DllImport("LogitechSteeringWheelEnginesWrapper", CallingConvention=CallingConvention.Cdecl)]
    private static extern IntPtr LogiGetState(int index);
    // Defina um struct que corresponda ao STATE retornado pela SDK (exemplo simplificado)
    [StructLayout(LayoutKind.Sequential)]
    private struct LogiControllerState
    {
        public UInt32 version;
        public UInt32 size;
        public UInt32 packetNumber;
        public UInt32 buttons;     // cada bit = botão pressionado
        public UInt32 pov;         // direcional
        public float steering;     // valor de -1.0f a +1.0f
        public float throttle;     // valor de 0.0f a 1.0f
        public float brake;        // valor de 0.0f a 1.0f
        public float clutch;       // valor de 0.0f a 1.0f
        // … outros campos se necessário
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Inicializa SDK
        if (!LogiSteeringInitialize(false))
        {
            Debug.LogError("[CarController] Falha ao inicializar Logitech SDK!");
        }
        else
        {
            Debug.Log("[CarController] Logitech SDK iniciado.");
        }

        UpdateCameraState();
    }

    void OnDestroy()
    {
        LogiSteeringShutdown();
    }

    void FixedUpdate()
    {
        if (!LogiUpdate()){
            Debug.Log("[CarController] LogiUpdate not!");
            return;
        }

        // assumimos index 0 para primeiro volante
        if (!LogiIsConnected(0)){
            Debug.Log("[CarController] LogiIsConnected not!");
            return;
        }

        IntPtr statePtr = LogiGetState(0);
        LogiControllerState state = (LogiControllerState)Marshal.PtrToStructure(statePtr, typeof(LogiControllerState));

        // Leitura dos eixos
        float steerInput = state.steering;      // -1..+1
        float throttle = state.throttle;        // 0..1
        float brakeInput = state.brake;         // 0..1
        float clutch = state.clutch;            // 0..1

        Debug.Log($"[steerInput] {steerInput}");
        Debug.Log($"[throttle] {throttle}");
        Debug.Log($"[brakeInput] {brakeInput}");
        Debug.Log($"[clutch] {clutch}");

        // Leitura de botões – verificamos bits
        bool btnUpPressed   = (state.buttons & (1u << btnGearUp)) != 0;
        bool btnDownPressed = (state.buttons & (1u << btnGearDown)) != 0;
        bool btnIgnPressed  = (state.buttons & (1u << btnIgnition)) != 0;
        bool btnHbUpPressed   = (state.buttons & (1u << btnHandbrakeUp)) != 0;
        bool btnHbDownPressed = (state.buttons & (1u << btnHandbrakeDown)) != 0;
        bool btnCamTogglePressed = (state.buttons & (1u << btnCameraToggle)) != 0;

        // Talvez você queira detectar *pressionamento* (transição) – adicionar lógica de estado anterior se quiser only-once

        // Trocas de marcha
        if (btnUpPressed)   TryShiftUp(clutch);
        if (btnDownPressed) TryShiftDown(clutch);

        // Ignição
        if (btnIgnPressed) ToggleIgnition();

        // Handbrake
        if (btnHbUpPressed)   TryEngageHandbrake();
        if (btnHbDownPressed) TryReleaseHandbrake();

        // Câmera
        if (btnCamTogglePressed) ToggleCamera();

        // Movimento
        ApplySteering(steerInput);
        ApplyDriveAndBrakes(throttle, brakeInput, clutch);
        UpdateEngineRPM(throttle, clutch);
        UpdateWheelMeshes();
    }

    #region Steering / drive
    void ApplySteering(float steerInput)
    {
        float angle = steerInput * maxSteerAngle;
        wheelFL.steerAngle = angle;
        wheelFR.steerAngle = angle;
    }

    void ApplyDriveAndBrakes(float throttle, float brakeInput, float clutch)
    {
        if (!engineOn || isStalled)
        {
            ApplyBrake(brakeInput * brakeTorque);
            ApplyMotorTorque(0f);
            return;
        }

        bool isNeutral = (currentGearIndex == 1);
        float forwardMultiplier = (currentGearIndex == 0) ? -1f : 1f;

        float gearTorque = torquePerGear.Length > currentGearIndex ? torquePerGear[currentGearIndex] : 0f;
        float motorTorque = 0f;

        if (!isNeutral && Mathf.Abs(gearRatios[currentGearIndex]) > 0.01f)
        {
            motorTorque = gearTorque * throttle * forwardMultiplier;
        }

        ApplyMotorTorque(motorTorque);

        float appliedBrake = brakeInput * brakeTorque;
        if (handbrakeEngaged)
        {
            appliedBrake = handbrakeTorque;
            ApplyMotorTorque(0f);
        }
        ApplyBrake(appliedBrake);
    }

    void ApplyMotorTorque(float torque)
    {
        wheelRL.motorTorque = torque;
        wheelRR.motorTorque = torque;
    }

    void ApplyBrake(float brake)
    {
        wheelFL.brakeTorque = brake;
        wheelFR.brakeTorque = brake;
        wheelRL.brakeTorque = brake;
        wheelRR.brakeTorque = brake;
    }

    void UpdateWheelMeshes()
    {
        UpdateWheelMesh(wheelFL, meshFL);
        UpdateWheelMesh(wheelFR, meshFR);
        UpdateWheelMesh(wheelRL, meshRL);
        UpdateWheelMesh(wheelRR, meshRR);
    }

    void UpdateWheelMesh(WheelCollider wc, Transform mesh)
    {
        if (wc == null || mesh == null) return;
        Vector3 pos;
        Quaternion rot;
        wc.GetWorldPose(out pos, out rot);
        mesh.position = pos;
        mesh.rotation = rot;
    }
    #endregion

    #region Gears, clutch, handbrake, ignition
    void TryShiftUp(float clutch)
    {
        if (clutch < clutchPressedThreshold) return;
        int next = Mathf.Min(currentGearIndex + 1, gearRatios.Length - 1);
        AttemptShift(next);
    }

    void TryShiftDown(float clutch)
    {
        if (clutch < clutchPressedThreshold) return;
        int next = Mathf.Max(currentGearIndex - 1, 0);
        AttemptShift(next);
    }

    void AttemptShift(int nextGear)
    {
        if (nextGear == currentGearIndex) return;

        if (engineRPM < minRPMForShift)
        {
            isStalled = true;
            engineOn = false;
            Debug.Log("[CarController] Motor morreu: troca abaixo do RPM mínimo!");
            return;
        }

        currentGearIndex = nextGear;
        Debug.Log($"[CarController] Marcha alterada para índice {currentGearIndex}");
    }

    void ToggleIgnition()
    {
        if (rb.linearVelocity.magnitude <= 0.1f && currentGearIndex == 1)
        {
            engineOn = !engineOn;
            isStalled = false;
            if (engineOn) engineRPM = Mathf.Max(engineRPM, idleRPM);
            Debug.Log("[CarController] Ignição: " + (engineOn ? "Ligado":"Desligado"));
        }
    }

    void TryEngageHandbrake()
    {
        if (rb.linearVelocity.magnitude <= 0.1f)
        {
            handbrakeEngaged = true;
            Debug.Log("[CarController] Freio de mão engatado.");
        }
    }

    void TryReleaseHandbrake()
    {
        if (rb.linearVelocity.magnitude <= 0.1f)
        {
            handbrakeEngaged = false;
            Debug.Log("[CarController] Freio de mão liberado.");
        }
    }
    #endregion

    #region Engine RPM simulation
    void UpdateEngineRPM(float throttle, float clutch)
    {
        if (!engineOn)
        {
            engineRPM = Mathf.MoveTowards(engineRPM, 0f, rpmDropRate * Time.fixedDeltaTime);
            return;
        }
        if (isStalled)
        {
            engineRPM = 0f;
            return;
        }

        if (throttle > 0f)
        {
            engineRPM = Mathf.MoveTowards(engineRPM, Mathf.Max(idleRPM, throttle * maxRPM), rpmRiseRate * Time.fixedDeltaTime);
        }
        else
        {
            engineRPM = Mathf.MoveTowards(engineRPM, idleRPM, rpmDropRate * Time.fixedDeltaTime);
        }

        engineRPM = Mathf.Clamp(engineRPM, 0f, maxRPM);
    }
    #endregion

    #region Camera
    void ToggleCamera()
    {
        camFirst = !camFirst;
        UpdateCameraState();
    }

    void UpdateCameraState()
    {
        if (camFirstPerson != null) camFirstPerson.enabled = camFirst;
        if (camThirdPerson != null) camThirdPerson.enabled = !camFirst;
    }
    #endregion
}
