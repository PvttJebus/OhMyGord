using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine;

public class TilesetManager : MonoBehaviour
{
    [SerializeField] RuleTile[] tilePalette;
    [SerializeField] GameObject buttonPrefab;
    [SerializeField] Transform buttonContainer;
    [SerializeField] Sprite eraseSprite;

    void Start()
    {
        CreateTileButtons();
        CreateEraserButton();
    }

    void CreateTileButtons()
    {
        foreach (RuleTile tile in tilePalette)
        {
            GameObject button = Instantiate(buttonPrefab, buttonContainer);
            button.GetComponent<Image>().sprite = tile.m_DefaultSprite;

            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                LevelEditor.Instance.SetDrawingState(tile);
            });
        }
    }

    void CreateEraserButton()
    {
        GameObject button = Instantiate(buttonPrefab, buttonContainer);
        button.GetComponent<Image>().sprite = eraseSprite;
        button.GetComponent<Button>().onClick.AddListener(() =>
        {
            LevelEditor.Instance.SetEraserState();
        });
    }
    public RuleTile[] TilePalette => tilePalette;
}