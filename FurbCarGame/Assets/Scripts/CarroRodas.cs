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

    private float _l;
    private float _w;
    private float _raioBase = 6f;
    private float _anguloMaxDirecao = 40f;

    private void Start()
    {
        var distanciaEntreEixos = Mathf.Abs(
            rodaColliderFL.transform.localPosition.z - rodaColliderRL.transform.localPosition.z
        );

        var larguraDianteira = Mathf.Abs(
            rodaColliderFL.transform.localPosition.x - rodaColliderFR.transform.localPosition.x
        );

        _l = distanciaEntreEixos;
        _w = larguraDianteira;
    }

    public void AplicarDirecaoVolante(float anguloDirecao)
    {
        if (anguloDirecao == 0)
        {
            if (rodaColliderFL) rodaColliderFL.steerAngle = 0;
            if (rodaColliderFR) rodaColliderFR.steerAngle = 0;
            return;
        }

        // geometria de Ackermann
        var r = _raioBase / Mathf.Abs(anguloDirecao);

        var anguloInterno = Mathf.Rad2Deg * Mathf.Atan(_l / (r - (_w / 2f)));
        var anguloExterno = Mathf.Rad2Deg * Mathf.Atan(_l / (r + (_w / 2f)));

        anguloInterno = Mathf.Clamp(anguloInterno, -_anguloMaxDirecao, _anguloMaxDirecao);
        anguloExterno = Mathf.Clamp(anguloExterno, -_anguloMaxDirecao, _anguloMaxDirecao);

        var virandoParaDireita = anguloDirecao > 0f;

        if (virandoParaDireita)
        {
            if (rodaColliderFL) rodaColliderFL.steerAngle = anguloExterno;
            if (rodaColliderFR) rodaColliderFR.steerAngle = anguloInterno;
        }
        else
        {
            if (rodaColliderFL) rodaColliderFL.steerAngle = -anguloInterno;
            if (rodaColliderFR) rodaColliderFR.steerAngle = -anguloExterno;
        }
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
