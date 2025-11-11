using UnityEngine;
using UnityEngine.UI;

public class CarroStatus : MonoBehaviour
{
    [Header("Cinto de Segurança")]
    public Image cintoDeSegurancaImg;
    public Sprite cintoDeSegurancaAtivadoSprite;
    public Sprite cintoDeSegurancaDesativadoSprite;

    [Header("Motor")]
    public Image motorImg;
    public Sprite motorAtivadoSprite;
    public Sprite motorDesativadoSprite;

    [Header("Freio de Mão")]
    public Image freioDeMaoImg;
    public Sprite freioDeMaoAtivadoSprite;
    public Sprite freioDeMaoDesativadoSprite;

    [Header("Marcha")]
    public Image marchaImg;
    public Sprite marchaRSprite;
    public Sprite marchaNSprite;
    public Sprite marchaM1Sprite;
    public Sprite marchaM2Sprite;
    public Sprite marchaM3Sprite;
    public Sprite marchaM4Sprite;
    public Sprite marchaM5Sprite;

    [Header("Carro")]
    public SpawnerCarroSelecionado spawnerCarroSelecionado;
    private CarroCintoSeguranca _carroCintoSeguranca;
    private CarroMotor _carroMotor;
    private CarroFreioMao _carroFreioMao;
    private CarroMarchas _carroMarchas;

    private void Start()
    {
        _carroCintoSeguranca = spawnerCarroSelecionado.CarrosSelecionado.GetComponent<CarroCintoSeguranca>();
        _carroMotor = spawnerCarroSelecionado.CarrosSelecionado.GetComponent<CarroMotor>();
        _carroFreioMao = spawnerCarroSelecionado.CarrosSelecionado.GetComponent<CarroFreioMao>();
        _carroMarchas = spawnerCarroSelecionado.CarrosSelecionado.GetComponent<CarroMarchas>();
    }

    private void Update()
    {
        AtualizarCarroStatus();
    }

    private void AtualizarCarroStatus()
    {
        cintoDeSegurancaImg.sprite = ObterSpriteCintoDeSeguranca();
        motorImg.sprite = ObterSpriteMotor();
        freioDeMaoImg.sprite = ObterSpriteFreioDeMao();
        marchaImg.sprite = ObterSpriteMarcha();
    }

    private Sprite ObterSpriteCintoDeSeguranca()
    {
        return _carroCintoSeguranca.CintoDeSegurancaColocado
            ? cintoDeSegurancaAtivadoSprite
            : cintoDeSegurancaDesativadoSprite;
    }

    private Sprite ObterSpriteMotor()
    {
        return _carroMotor.MotorLigado
            ? motorAtivadoSprite
            : motorDesativadoSprite;
    }

    private Sprite ObterSpriteFreioDeMao()
    {
        return _carroFreioMao.FreioDeMaoPuxado
            ? freioDeMaoDesativadoSprite
            : freioDeMaoAtivadoSprite;
    }

    private Sprite ObterSpriteMarcha()
    {
        return _carroMarchas.MarchaAtual switch
        {
            Marcha.R => marchaRSprite,
            Marcha.N => marchaNSprite,
            Marcha.M1 => marchaM1Sprite,
            Marcha.M2 => marchaM2Sprite,
            Marcha.M3 => marchaM3Sprite,
            Marcha.M4 => marchaM4Sprite,
            Marcha.M5 => marchaM5Sprite,
            _ => marchaNSprite,
        };
    }
}
