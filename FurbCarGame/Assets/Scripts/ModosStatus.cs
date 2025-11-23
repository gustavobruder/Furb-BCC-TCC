using UnityEngine;
using UnityEngine.UI;

public class ModosStatus : MonoBehaviour
{
    [Header("Modo estrada de barro")]
    public Image estradaBarroImg;
    public Sprite estradaBarroAtivadoSprite;
    public Sprite estradaBarroDesativadoSprite;

    [Header("Modo estrada escorregadia")]
    public Image estradaEscorregadiaImg;
    public Sprite estradaEscorregadiaAtivadoSprite;
    public Sprite estradaEscorregadiaDesativadoSprite;

    [Header("Marcha")]
    public Image marchaImg;
    public Sprite marchaManualSprite;
    public Sprite marchaAutomaticaSprite;

    [Header("Carro")]
    public SpawnerCarroSelecionado spawnerCarroSelecionado;
    private CarroJogador _carroJogador;
    private CarroFisicaAutomatica _carroFisicaAutomatica;

    private void Start()
    {
        _carroJogador = spawnerCarroSelecionado.CarrosSelecionado.GetComponent<CarroJogador>();
        _carroFisicaAutomatica = spawnerCarroSelecionado.CarrosSelecionado.GetComponent<CarroFisicaAutomatica>();
    }

    private void Update()
    {
        AtualizarCarroStatus();
    }

    private void AtualizarCarroStatus()
    {
        estradaBarroImg.sprite = ObterSpriteEstradaBarro();
        estradaEscorregadiaImg.sprite = ObterSpriteEstradaEscorregadia();
        marchaImg.sprite = ObterSpriteMarcha();
    }

    private Sprite ObterSpriteEstradaBarro()
    {
        return _carroJogador.EstradaBarroHabilitada
            ? estradaBarroAtivadoSprite
            : estradaBarroDesativadoSprite;
    }

    private Sprite ObterSpriteEstradaEscorregadia()
    {
        return _carroJogador.EstradaEscorregadiaHabilitada
            ? estradaEscorregadiaAtivadoSprite
            : estradaEscorregadiaDesativadoSprite;
    }

    private Sprite ObterSpriteMarcha()
    {
        return _carroFisicaAutomatica.trocarMarchasAutomaticamente
            ? marchaAutomaticaSprite
            : marchaManualSprite;
    }
}
