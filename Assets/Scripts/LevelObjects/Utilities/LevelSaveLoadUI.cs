using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
[RequireComponent(typeof(GraphicRaycaster))]
/// <summary>
/// UI manager for saving, loading, and managing level files.
/// </summary>
public class LevelSaveLoadUI : MonoBehaviour
{
    #region Fields

    [Tooltip("Prefab for each level entry UI element. Must be assigned in the inspector.")]
    public GameObject levelEntryPrefab;

    private Transform levelContainer;
    private TMP_InputField saveNameInput;
    private Button saveAsNewButton;
    private Button closeButton;

    [Header("Assign ScrollRect in Inspector")]
    public ScrollRect scrollRect;

    private string saveDirectory;
    private List<LevelEntry> levelEntries = new List<LevelEntry>();

    private float refreshInterval = 2f;
    private float lastRefreshTime = 0f;
    private int lastFileCount = -1;

    [Header("Configurable Settings")]
    [SerializeField] private float pixelsPerLevel = 500f;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        saveDirectory = Application.persistentDataPath;

        levelContainer = transform.Find("LevelContainer/Levels");
        if (levelContainer == null)
            Debug.LogWarning("LevelSaveLoadUI: Could not find Levels under ScrollRect/LevelContainer/Levels");

        var saveNameObj = transform.Find("SaveNameInput");
        if (saveNameObj != null)
            saveNameInput = saveNameObj.GetComponent<TMP_InputField>();
        else
            Debug.LogWarning("LevelSaveLoadUI: Could not find SaveNameInput");

        var saveAsNewObj = transform.Find("SaveAsNewButton");
        if (saveAsNewObj != null)
            saveAsNewButton = saveAsNewObj.GetComponent<Button>();
        else
            Debug.LogWarning("LevelSaveLoadUI: Could not find SaveAsNewButton");

        var closeObj = transform.Find("CloseButton");
        if (closeObj != null)
            closeButton = closeObj.GetComponent<Button>();
        else
            Debug.LogWarning("LevelSaveLoadUI: Could not find CloseButton");

        saveAsNewButton.onClick.AddListener(SaveAsNew);
        closeButton.onClick.AddListener(Close);
        RefreshSaveList();
    }

    private void Update()
    {
        if (Time.time - lastRefreshTime > refreshInterval)
        {
            lastRefreshTime = Time.time;

            DirectoryInfo dir = new DirectoryInfo(saveDirectory);
            int fileCount = dir.GetFiles("*.json").Length;

            if (fileCount != lastFileCount)
            {
                RefreshSaveList();
                lastFileCount = fileCount;
            }
        }
    }

    #endregion

    #region UI Refresh

    /// <summary>
    /// Refreshes the list of saved levels displayed in the UI.
    /// </summary>
    private void RefreshSaveList()
    {
        // Clear existing UI
        foreach (Transform child in levelContainer)
            Destroy(child.gameObject);
        levelEntries.Clear();

        // Find all saved levels
        DirectoryInfo dir = new DirectoryInfo(saveDirectory);
        FileInfo[] files = dir.GetFiles("*.json");

        System.Array.Sort(files, (a, b) =>
        {
            string aPath = Path.Combine(saveDirectory, a.Name);
            string bPath = Path.Combine(saveDirectory, b.Name);

            LevelData aData = null;
            LevelData bData = null;

            try
            {
                aData = JsonUtility.FromJson<LevelData>(File.ReadAllText(aPath));
            }
            catch { }

            try
            {
                bData = JsonUtility.FromJson<LevelData>(File.ReadAllText(bPath));
            }
            catch { }

            long aTime = aData != null ? aData.timestamp : 0;
            long bTime = bData != null ? bData.timestamp : 0;

            return aTime.CompareTo(bTime); // oldest first, newest last
        });
        foreach (FileInfo file in files)
        {
            string levelName = Path.GetFileNameWithoutExtension(file.Name);
            string previewPath = Path.Combine(saveDirectory, levelName + ".png");

            GameObject entryObj = Instantiate(levelEntryPrefab, levelContainer);
            LevelEntry entry = entryObj.GetComponent<LevelEntry>();
            if (entry != null)
            {
                entry.Initialize(levelName, previewPath, this);
                levelEntries.Add(entry);
            }
            else
            {
                Debug.LogWarning("LevelSaveLoadUI: Instantiated prefab missing LevelEntry component.");
            }
        }

        RectTransform rt = levelContainer.GetComponent<RectTransform>();
        if (rt != null)
        {
            float width = Mathf.Max(pixelsPerLevel * levelEntries.Count, pixelsPerLevel);
            rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);

            rt.pivot = new Vector2(1, 0.5f);
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(0, rt.anchoredPosition.y);
        }
    }

    #endregion

    #region Save/Load Methods

    /// <summary>
    /// Saves the current level as a new file.
    /// </summary>
    public void SaveAsNew()
    {
        string newName = saveNameInput.text.Trim();
        if (string.IsNullOrEmpty(newName))
            return;

        LevelEditor.Instance.SaveLevel(newName);
        RefreshSaveList();
    }

    /// <summary>
    /// Overwrites an existing saved level.
    /// </summary>
    public void OverwriteSave(string levelName)
    {
        LevelEditor.Instance.SaveLevel(levelName);
        RefreshSaveList();
    }

    /// <summary>
    /// Loads a saved level.
    /// </summary>
    public void LoadSave(string levelName)
    {
        LevelEditor.Instance.LoadLevel(levelName);
    }

    /// <summary>
    /// Deletes a saved level and its preview image.
    /// </summary>
    public void DeleteSave(string levelName)
    {
        string jsonPath = Path.Combine(saveDirectory, levelName + ".json");
        string previewPath = Path.Combine(saveDirectory, levelName + ".png");

        if (File.Exists(jsonPath))
            File.Delete(jsonPath);
        if (File.Exists(previewPath))
            File.Delete(previewPath);

        RefreshSaveList();
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Closes the save/load UI panel.
    /// </summary>
    private void Close()
    {
        gameObject.SetActive(false);
    }

    #endregion
}