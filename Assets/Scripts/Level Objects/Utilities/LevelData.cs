using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class LevelData
{
    public List<TileData> tiles = new List<TileData>();
    public List<ObjectData> objects = new List<ObjectData>();

    [System.Serializable]
    public class TileData
    {
        public Vector3Int position;
        public string tileName;
    }

    [System.Serializable]
    public class ObjectData
    {
        public int prefabIndex;
        public Vector3 position;
    }

    public static void Save(string fileName, Tilemap tilemap, List<LevelObject> levelObjects, GameObject[] prefabList)
    {
        LevelData data = new LevelData();

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
                    data.objects.Add(new ObjectData
                    {
                        prefabIndex = prefabIndex,
                        position = obj.transform.position
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
}