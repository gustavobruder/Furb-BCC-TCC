using UnityEngine;
using UnityEngine.UI;

public class Velocimetro : MonoBehaviour
{
    public Text textoRpm;
    public RectTransform ponteiroVelocimetro;
    public SpawnerCarroSelecionado spawnerCarroSelecionado;

    private Rigidbody _rb;
    private CarroMotor _carroMotor;

    private float _velocidadeMax = 280f;
    private float _velocidade = 0.0f;
    private float _anguloVelocidadeMin = 187.5f;
    private float _anguloVelocidadeMax = -82.5f;

    private void Start()
    {
        _rb = spawnerCarroSelecionado.CarrosSelecionado.GetComponent<Rigidbody>();
        _carroMotor = spawnerCarroSelecionado.CarrosSelecionado.GetComponent<CarroMotor>();
    }

    private void Update()
    {
        DefinirTextoRpm();
        AtualizarVelocidade();
        AtualizarPonteiroVelocimetro();
    }

    private void DefinirTextoRpm()
    {
        textoRpm.text = Mathf.RoundToInt(_carroMotor.RpmMotor) + " RPM";
    }

    private void AtualizarVelocidade()
    {
        _velocidade = _rb.linearVelocity.magnitude * 3.6f;
    }

    private void AtualizarPonteiroVelocimetro()
    {
        ponteiroVelocimetro.localEulerAngles = new Vector3(0, 0,
            Mathf.Lerp(_anguloVelocidadeMin, _anguloVelocidadeMax, _velocidade / _velocidadeMax)
        );
    }
}
