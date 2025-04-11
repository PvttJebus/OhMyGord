using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Serializable data structure for saving and loading levels, including tiles and objects.
/// </summary>
[System.Serializable]
public class LevelData
{
    public long timestamp;
    #region Data Classes

    /// <summary>
    /// List of saved tile data.
    /// </summary>
    public List<TileData> tiles = new List<TileData>();

    /// <summary>
    /// List of saved object data.
    /// </summary>
    public List<ObjectData> objects = new List<ObjectData>();

    /// <summary>
    /// Serializable tile data.
    /// </summary>
    [System.Serializable]
    public class TileData
    {
        public Vector3Int position;
        public string tileName;
    }

    /// <summary>
    /// Serializable object data.
    /// </summary>
    [System.Serializable]
    public class ObjectData
    {
        public int prefabIndex;
        public Vector3 position;
        public Quaternion rotation;
        public Vector2 size;

        /// <summary>
        /// List of serialized editable parameters for this object.
        /// </summary>
        public List<ParameterData> parameters = new List<ParameterData>();
    }

    /// <summary>
    /// Serializable representation of a single editable parameter.
    /// </summary>
    [System.Serializable]
    public class ParameterData
    {
        public string name;
        public string type; // e.g., "float", "bool", "int", "string", "enum"
        public string value; // Store as string for generality; parse as needed
        public int version = 1; // For future-proofing
    }

    #endregion

    #region Capture and Apply

    /// <summary>
    /// Captures the current level state from the editor.
    /// </summary>
    public static LevelData Capture(LevelEditor editor)
    {
        LevelData data = new LevelData();
        data.timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        Tilemap tilemap = editor.GetTilemap();
        foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(pos);
            if (tile != null)
            {
                data.tiles.Add(new TileData
                {
                    position = pos,
                    tileName = tile.name
                });
            }
        }

        foreach (var obj in editor.GetLevelObjects())
        {
            if (obj != null)
            {
                int prefabIndex = GetPrefabIndex(editor.GetPrefabList(), obj.originalPrefabInstanceID);
                if (prefabIndex >= 0)
                {
                    var sr = obj.GetComponent<SpriteRenderer>();
                    var objectData = new ObjectData
                    {
                        prefabIndex = prefabIndex,
                        position = obj.transform.position,
                        rotation = obj.transform.rotation,
                        size = sr != null ? sr.size : Vector2.one
                    };

                    // Export parameters if supported
                    if (obj is IParameterEditable editable)
                    {
                        objectData.parameters = editable.ExportParameters();
                    }

                    data.objects.Add(objectData);
                }
            }
        }

        return data;
    }

    /// <summary>
    /// Applies saved level data to the editor.
    /// </summary>
    public static void Apply(LevelEditor editor, LevelData data)
    {
        Tilemap tilemap = editor.GetTilemap();
        var levelObjects = editor.GetLevelObjects();
        var tilesetManager = editor.GetTilesetManager();
        var prefabList = editor.GetPrefabList();

        tilemap.ClearAllTiles();

        foreach (var obj in new List<LevelObject>(levelObjects))
        {
            if (obj != null)
                Object.Destroy(obj.gameObject);
        }
        levelObjects.Clear();

        foreach (var tileData in data.tiles)
        {
            TileBase tile = GetTileByName(tileData.tileName, tilesetManager);
            if (tile != null)
            {
                tilemap.SetTile(tileData.position, tile);
            }
        }

        foreach (var objData in data.objects)
        {
            if (objData.prefabIndex >= 0 && objData.prefabIndex < prefabList.Length && prefabList[objData.prefabIndex] != null)
            {
                GameObject newObj = Object.Instantiate(prefabList[objData.prefabIndex], objData.position, objData.rotation);
                if (newObj.TryGetComponent(out LevelObject levelObj))
                {
                    levelObj.originalPrefabInstanceID = prefabList[objData.prefabIndex].GetInstanceID();
                    levelObjects.Add(levelObj);

                    var sr = newObj.GetComponent<SpriteRenderer>();
                    if (sr != null)
                        sr.size = objData.size;

                    // Import parameters if supported
                    if (levelObj is IParameterEditable editable)
                    {
                        editable.ImportParameters(objData.parameters);
                    }
                }
            }
        }
    }

    #endregion

    #region Serialization

    public static string ToJson(LevelData data)
    {
        return JsonUtility.ToJson(data);
    }

    public static LevelData FromJson(string json)
    {
        return JsonUtility.FromJson<LevelData>(json);
    }

    #endregion

    #region Save and Load

    /// <summary>
    /// Finds the prefab index in the list by instance ID.
    /// </summary>
    private static int GetPrefabIndex(GameObject[] prefabList, int instanceID)
    {
        for (int i = 0; i < prefabList.Length; i++)
        {
            if (prefabList[i] != null && prefabList[i].GetInstanceID() == instanceID)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Saves the level data to a JSON file.
    /// </summary>
    public static void Save(string fileName, Tilemap tilemap, List<LevelObject> levelObjects, GameObject[] prefabList)
    {
        LevelData data = new LevelData();
        data.timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(pos);
            if (tile != null)
            {
                data.tiles.Add(new TileData
                {
                    position = pos,
                    tileName = tile.name
                });
            }
        }

        foreach (var obj in levelObjects)
        {
            if (obj != null)
            {
                int prefabIndex = -1;
                for (int i = 0; i < prefabList.Length; i++)
                {
                    if (prefabList[i] != null && prefabList[i].GetInstanceID() == obj.originalPrefabInstanceID)
                    {
                        prefabIndex = i;
                        break;
                    }
                }
                if (prefabIndex >= 0)
                {
                    var sr = obj.GetComponent<SpriteRenderer>();
                    data.objects.Add(new ObjectData
                    {
                        prefabIndex = prefabIndex,
                        position = obj.transform.position,
                        rotation = obj.transform.rotation,
                        size = sr != null ? sr.size : Vector2.one
                    });
                }
                else
                {
                    Debug.LogWarning($"Prefab index for {obj.name} not found in prefab list during save.");
                }
            }
        }

        string json = JsonUtility.ToJson(data);
        string path = Path.Combine(Application.persistentDataPath, fileName + ".json");
        File.WriteAllText(path, json);
        Debug.Log($"Level saved to: {path}");
    }

    /// <summary>
    /// Saves a PNG preview image of the level.
    /// </summary>
    public static void SaveLevelPreview(string fileName, Camera previewCamera)
    {
        if (previewCamera == null)
        {
            Debug.LogWarning("No preview camera assigned, skipping preview generation.");
            return;
        }

        int width = 256;
        int height = 256;
        RenderTexture rt = new RenderTexture(width, height, 24);
        previewCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);

        previewCamera.Render();

        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();

        previewCamera.targetTexture = null;
        RenderTexture.active = null;
        Object.Destroy(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        string path = Path.Combine(Application.persistentDataPath, fileName + ".png");
        File.WriteAllBytes(path, bytes);
        Debug.Log($"Preview saved to: {path}");
    }

    /// <summary>
    /// Loads level data from a JSON file.
    /// </summary>
    public static void Load(string fileName, Tilemap tilemap, List<LevelObject> levelObjects, TilesetManager tilesetManager, GameObject[] prefabList)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName + ".json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            LevelData data = JsonUtility.FromJson<LevelData>(json);

            tilemap.ClearAllTiles();
            foreach (var obj in levelObjects)
            {
                if (obj != null) Object.Destroy(obj.gameObject);
            }
            levelObjects.Clear();

            foreach (var tileData in data.tiles)
            {
                TileBase tile = GetTileByName(tileData.tileName, tilesetManager);
                if (tile != null)
                {
                    tilemap.SetTile(tileData.position, tile);
                }
                else
                {
                    Debug.LogWarning($"Tile '{tileData.tileName}' not found in tileset.");
                }
            }

            foreach (var objData in data.objects)
            {
                if (objData.prefabIndex >= 0 && objData.prefabIndex < prefabList.Length && prefabList[objData.prefabIndex] != null)
                {
                    GameObject newObj = Object.Instantiate(prefabList[objData.prefabIndex], objData.position, Quaternion.identity);
                    if (newObj.TryGetComponent(out LevelObject levelObj))
                    {
                        levelObj.originalPrefabInstanceID = prefabList[objData.prefabIndex].GetInstanceID();
                        levelObjects.Add(levelObj);

                        var sr = newObj.GetComponent<SpriteRenderer>();
                        if (sr != null)
                            sr.size = objData.size;
                    }
                }
                else
                {
                    Debug.LogWarning($"Invalid prefab index {objData.prefabIndex} during load.");
                }
            }

            Debug.Log($"Level loaded from: {path}");
        }
        else
        {
            Debug.LogError($"No save file found at: {path}");
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Finds a tile by name in the tileset manager.
    /// </summary>
    private static TileBase GetTileByName(string name, TilesetManager tilesetManager)
    {
        if (tilesetManager != null)
        {
            foreach (var tile in tilesetManager.TilePalette)
            {
                if (tile != null && tile.name == name)
                {
                    return tile;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the next available level file name.
    /// </summary>
    public static string GetNextLevelFileName()
    {
        int levelNumber = 1;
        string fileName;
        do
        {
            fileName = $"L{levelNumber}";
            string path = Path.Combine(Application.persistentDataPath, fileName + ".json");
            if (!File.Exists(path))
            {
                return fileName;
            }
            levelNumber++;
        } while (true);
    }

    /// <summary>
    /// Updates the list of saved level files.
    /// </summary>
    private static List<string> UpdateSavedLevelFiles()
    {
        List<string> savedLevelFiles = new List<string>();
        string[] files = Directory.GetFiles(Application.persistentDataPath, "L*.json");
        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            savedLevelFiles.Add(fileName);
        }
        savedLevelFiles.Sort();
        return savedLevelFiles;
    }

    /// <summary>
    /// Gets the next saved level file name, cycling through the list.
    /// </summary>
    public static string GetNextSavedLevel(ref int currentLevelIndex)
    {
        List<string> savedLevelFiles = UpdateSavedLevelFiles();

        if (savedLevelFiles.Count == 0)
        {
            return null;
        }

        currentLevelIndex = (currentLevelIndex + 1) % savedLevelFiles.Count;
        return savedLevelFiles[currentLevelIndex];
    }

    #endregion
}