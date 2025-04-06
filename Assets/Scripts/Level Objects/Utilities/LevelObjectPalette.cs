using UnityEngine;
using UnityEngine.UI;

public class LevelObjectPalette : MonoBehaviour
{
    [SerializeField] private GameObject[] objectPrefabs;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform buttonContainer;

    void Start()
    {
        CreatePrefabButtons();
    }

    void CreatePrefabButtons()
    {
        for (int i = 0; i < objectPrefabs.Length; i++)
        {
            if (objectPrefabs[i] == null) continue;
            int index = i;
            GameObject button = Instantiate(buttonPrefab, buttonContainer);
            SpriteRenderer sr = objectPrefabs[i].GetComponent<SpriteRenderer>();
            if (sr != null) button.GetComponent<Image>().sprite = sr.sprite;
            button.GetComponent<Button>().onClick.AddListener(() => SpawnPrefab(index));
        }
    }

    void SpawnPrefab(int index)
    {
        GameObject prefab = objectPrefabs[index];
        LevelEditor.Instance.SetSpawningState();
        Vector3 spawnPos = LevelEditor.Instance.GetSnappedMousePosition();
        GameObject newObj = Instantiate(prefab, spawnPos, Quaternion.identity);
        if (newObj.TryGetComponent(out LevelObject levelObj))
        {
            levelObj.originalPrefabInstanceID = prefab.GetInstanceID();
            LevelEditor.Instance.RegisterLevelObject(levelObj);
            levelObj.editing = true;
        }
    }

    public GameObject GetPrefabByInstanceID(int instanceID)
    {
        foreach (var prefab in ObjectPrefabs)
            if (prefab.GetInstanceID() == instanceID)
                return prefab;
        return null;
    }

    public GameObject[] ObjectPrefabs => objectPrefabs;
}