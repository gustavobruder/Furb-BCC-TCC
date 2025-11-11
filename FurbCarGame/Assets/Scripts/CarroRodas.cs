using UnityEngine;

public class CarroRodas : MonoBehaviour
{
    [Header("Rodas Colliders")]
    public WheelCollider rodaColliderFL;
    public WheelCollider rodaColliderFR;
    public WheelCollider rodaColliderRL;
    public WheelCollider rodaColliderRR;

    [Header("Rodas Meshs")]
    public Transform rodaMeshFL;
    public Transform rodaMeshFR;
    public Transform rodaMeshRL;
    public Transform rodaMeshRR;

    private float _anguloMaxDirecao = 35f;

    public void AplicarDirecaoVolante(float anguloDirecao)
    {
        if (rodaColliderFL) rodaColliderFL.steerAngle = anguloDirecao * _anguloMaxDirecao;
        if (rodaColliderFR) rodaColliderFR.steerAngle = anguloDirecao * _anguloMaxDirecao;
    }

    public void AplicarTorqueMotor(float torqueMotor)
    {
        // tração dianteira (FWD)
        if (rodaColliderFL) rodaColliderFL.motorTorque = torqueMotor;
        if (rodaColliderFR) rodaColliderFR.motorTorque = torqueMotor;
        if (rodaColliderRL) rodaColliderRL.motorTorque = 0f;
        if (rodaColliderRR) rodaColliderRR.motorTorque = 0f;
    }

    public void AplicarFreioDianteiro(float freioDianteiro)
    {
        if (rodaColliderFL) rodaColliderFL.brakeTorque = freioDianteiro;
        if (rodaColliderFR) rodaColliderFR.brakeTorque = freioDianteiro;
    }

    public void AplicarFreioTraseiro(float freioTraseiro)
    {
        if (rodaColliderRL) rodaColliderRL.brakeTorque = freioTraseiro;
        if (rodaColliderRR) rodaColliderRR.brakeTorque = freioTraseiro;
    }

    public void AtualizarRodas()
    {
        AtualizarRoda(rodaColliderFL, rodaMeshFL);
        AtualizarRoda(rodaColliderFR, rodaMeshFR);
        AtualizarRoda(rodaColliderRL, rodaMeshRL);
        AtualizarRoda(rodaColliderRR, rodaMeshRR);
    }

    private static void AtualizarRoda(WheelCollider rodaCollider, Transform rodaMesh)
    {
        Vector3 posicao;
        Quaternion rotacao;
        rodaCollider.GetWorldPose(out posicao, out rotacao);
        rodaMesh.position = posicao;
        rodaMesh.rotation = rotacao;
    }
}
