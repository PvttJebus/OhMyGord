using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEntry : MonoBehaviour
{
    public Image previewImage;
    public TMP_Text levelNameText;
    public Button loadButton;
    public Button overwriteButton;
    public Button deleteButton;

    private string levelName;
    private LevelSaveLoadUI manager;

    public void Initialize(string levelName, string previewPath, LevelSaveLoadUI manager)
    {
        this.levelName = levelName;
        this.manager = manager;
        levelNameText.text = levelName;

        if (File.Exists(previewPath))
        {
            byte[] pngData = File.ReadAllBytes(previewPath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(pngData);
            previewImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            previewImage.sprite = null;
        }

        loadButton.onClick.AddListener(() => manager.LoadSave(levelName));
        overwriteButton.onClick.AddListener(() => manager.OverwriteSave(levelName));
        deleteButton.onClick.AddListener(() => manager.DeleteSave(levelName));
    }
}