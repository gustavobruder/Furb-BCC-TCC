using UnityEngine;
using UnityEngine.UI;

public class Cronometro : MonoBehaviour
{
    public Text textoCronometro;

    private float _tempo = 0f;
    private bool _cronometrando = true;

    private void Update()
    {
        if (_cronometrando)
        {
            _tempo += Time.deltaTime;
            AtualizarCronometro();
        }
    }

    private void AtualizarCronometro()
    {
        var minutos = Mathf.FloorToInt(_tempo / 60);
        var segundos = Mathf.FloorToInt(_tempo % 60);
        textoCronometro.text = $"{minutos:00}:{segundos:00}";
    }

    public void PararCronometro()
    {
        if (_cronometrando) _cronometrando = false;
    }
}
