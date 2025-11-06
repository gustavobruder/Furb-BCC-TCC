using UnityEngine;
using UnityEngine.UI;

public class CarroStatus : MonoBehaviour
{
    public Image cintoDeSegurancaImg;
    public Sprite cintoDeSegurancaAtivadoSprite;
    public Sprite cintoDeSegurancaDesativadoSprite;

    public Image motorImg;
    public Sprite motorAtivadoSprite;
    public Sprite motorDesativadoSprite;

    public Image freioDeMaoImg;
    public Sprite freioDeMaoAtivadoSprite;
    public Sprite freioDeMaoDesativadoSprite;

    public CarroVolante carroVolante;

    private void Update()
    {
        UpdateIndicators();
    }

    private void UpdateIndicators()
    {
        cintoDeSegurancaImg.sprite = carroVolante.IsSeatbeltOn() ? cintoDeSegurancaAtivadoSprite : cintoDeSegurancaDesativadoSprite;
        motorImg.sprite = carroVolante.IsEngineOn() ? motorAtivadoSprite : motorDesativadoSprite;
        freioDeMaoImg.sprite = carroVolante.IsHandbrakeOn() ? freioDeMaoDesativadoSprite : freioDeMaoAtivadoSprite;
    }
}
