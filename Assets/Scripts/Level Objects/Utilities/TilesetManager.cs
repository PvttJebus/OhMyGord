using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine;

public class TilesetManager : MonoBehaviour
{
    [SerializeField] TileBase[] tilePalette;
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
        foreach (TileBase tile in tilePalette)
        {
            GameObject button = Instantiate(buttonPrefab, buttonContainer);
            if (tile is Tile baseTile)
                button.GetComponent<Image>().sprite = baseTile.sprite;

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
}