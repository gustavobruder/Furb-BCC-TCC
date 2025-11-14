using UnityEngine;

public class CarroCameras : MonoBehaviour
{
    [Header("Câmeras")]
    public Camera cameraPrimeiraPessoa;
    public Camera cameraTerceiraPessoa;

    private Quaternion _cameraPrimeiraPessoaRotacaoInicial;
    private Quaternion _cameraTerceiraPessoaRotacaoInicial;

    private bool _cameraEstaEmPrimeiraPessoa = true;

    private float _anguloPov = 10f;
    private float _tempoSuavizacao = 0.15f;

    private float _offsetX = 0f;
    private float _offsetY = 0f;

    private float _offsetXSuavizado = 0f;
    private float _offsetYSuavizado = 0f;

    private float _velocidadeX = 0f;             
    private float _velocidadeY = 0f;

    private void Start()
    {
        _cameraPrimeiraPessoaRotacaoInicial = cameraPrimeiraPessoa.transform.localRotation;
        _cameraTerceiraPessoaRotacaoInicial = cameraTerceiraPessoa.transform.localRotation;
    }

    public Camera ObterCamera()
    {
        return _cameraEstaEmPrimeiraPessoa ? cameraPrimeiraPessoa : cameraTerceiraPessoa;
    }

    private Quaternion ObterRotacaoInicialCamera()
    {
        return _cameraEstaEmPrimeiraPessoa ? _cameraPrimeiraPessoaRotacaoInicial : _cameraTerceiraPessoaRotacaoInicial;
    }

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

    public void ControlarPov(uint pov)
    {
        var cameraCarro = ObterCamera();
        var cameraRotacaoInicial = ObterRotacaoInicialCamera();

        switch (pov)
        {
            case 0: // CIMA
                _offsetY = -_anguloPov;
                _offsetX = 0f;
                break;
            case 4500: // CIMA-DIREITA
                _offsetY = -_anguloPov;
                _offsetX = _anguloPov;
                break;
            case 9000: // DIREITA
                _offsetY = 0f;
                _offsetX = _anguloPov;
                break;
            case 13500: // BAIXO-DIREITA
                _offsetY = _anguloPov;
                _offsetX = _anguloPov;
                break;
            case 18000: // BAIXO
                _offsetY = _anguloPov;
                _offsetX = 0f;
                break;
            case 22500: // BAIXO-ESQUERDA
                _offsetY = _anguloPov;
                _offsetX = -_anguloPov;
                break;
            case 27000: // ESQUERDA
                _offsetY = 0f;
                _offsetX = -_anguloPov;
                break;
            case 31500: // CIMA-ESQUERDA
                _offsetY = -_anguloPov;
                _offsetX = -_anguloPov;
                break;
            default: // CENTRO
                _offsetX = 0f;
                _offsetY = 0f;
                break;
        }

        _offsetXSuavizado = Mathf.SmoothDamp(_offsetXSuavizado, _offsetX, ref _velocidadeX, _tempoSuavizacao);
        _offsetYSuavizado = Mathf.SmoothDamp(_offsetYSuavizado, _offsetY, ref _velocidadeY, _tempoSuavizacao);

        cameraCarro.transform.localRotation = Quaternion.Euler(_offsetYSuavizado, _offsetXSuavizado, 0f) * cameraRotacaoInicial;
    }
}
