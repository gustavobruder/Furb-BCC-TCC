using UnityEngine;
using UnityEngine.UI;

public class Speedometer : MonoBehaviour
{
    public Rigidbody rb;
    public Text speedLabel;
    public RectTransform ponteiroVelocimetro;

    public float velocidadeMax = 280f;
    public float velocidade = 0.0f;

    public float anguloVelocidadeMin;
    public float anguloVelocidadeMax;

    private void Update()
    {
        velocidade = rb.linearVelocity.magnitude * 3.6f;
        speedLabel.text = ((int)velocidade) + " km/h";
        ponteiroVelocimetro.localEulerAngles = new Vector3(0, 0,
            Mathf.Lerp(anguloVelocidadeMin, anguloVelocidadeMax, velocidade / velocidadeMax)
        );
    }
}
