using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine;

public class TilesetManager : MonoBehaviour
{
    [SerializeField] RuleTile[] tilePalette;
    [SerializeField] GameObject buttonPrefab;
    [SerializeField] Transform buttonContainer;

    void Start()
    {
        CreateTileButtons();
    }

    void CreateTileButtons()
    {
        foreach (RuleTile tile in tilePalette)
        {
            GameObject button = Instantiate(buttonPrefab, buttonContainer);
            button.GetComponent<Image>().sprite = tile.m_DefaultSprite;

            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                var ctx = LevelEditor.Instance.controller.context;
                ctx.currentTile = tile;
                LevelEditor.Instance.controller.ChangeToDrawingState();
            });
        }
    }

    public RuleTile[] TilePalette => tilePalette;
}