using UnityEngine;

public class CarroCameras : MonoBehaviour
{
    [Header("Câmeras")]
    public Camera cameraPrimeiraPessoa;
    public Camera cameraTerceiraPessoa;

    private bool _cameraEstaEmPrimeiraPessoa = true;

    public void AlternarCamera()
    {
        _cameraEstaEmPrimeiraPessoa = !_cameraEstaEmPrimeiraPessoa;
        DefinirModoCamera();
    }

    public void DefinirModoCamera()
    {
        cameraPrimeiraPessoa.enabled = _cameraEstaEmPrimeiraPessoa;
        cameraTerceiraPessoa.enabled = !_cameraEstaEmPrimeiraPessoa;
    }
}
