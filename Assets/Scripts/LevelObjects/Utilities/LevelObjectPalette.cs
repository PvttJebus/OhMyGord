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

            // Remove existing onClick listeners to avoid double spawn
            Button btn = button.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();

            var trigger = button.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            var pointerDownEntry = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown
            };
            pointerDownEntry.callback.AddListener((eventData) =>
            {
                var fsmController = LevelEditor.Instance.controller;
                if (fsmController != null)
                {
                    fsmController.ChangeToSelectingState();
                }
                SpawnPrefab(index);
            });
            trigger.triggers.Add(pointerDownEntry);

            var pointerUpEntry = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp
            };
            pointerUpEntry.callback.AddListener((eventData) =>
            {
                var fsmController = LevelEditor.Instance.controller;
                if (fsmController != null)
                {
                    fsmController.ChangeToStickySpawningState();
                }
            });
            trigger.triggers.Add(pointerUpEntry);
        }
    }

    void SpawnPrefab(int index)
    {
        LevelEditor.Instance.History.SaveState(LevelEditor.Instance);
        GameObject prefab = objectPrefabs[index];
        // FSM migration: spawning state now handled by FSM context
        Vector3 spawnPos = LevelEditor.Instance.GetSnappedMousePosition();
        GameObject newObj = Instantiate(prefab, spawnPos, Quaternion.identity);
        if (newObj.TryGetComponent(out LevelObject levelObj))
        {
            levelObj.originalPrefabInstanceID = prefab.GetInstanceID();
            LevelEditor.Instance.RegisterLevelObject(levelObj);

            // Enable sticky spawn mode
            var controller = LevelEditor.Instance.controller;
            if (controller == null || controller.context == null)
            {
                Debug.LogError("LevelEditorController or its context is not assigned!");
                return;
                var fsmController = LevelEditor.Instance.controller;
                if (fsmController != null)
                {
                    fsmController.ChangeToSelectingState();
                }
            }
            var ctx = controller.context;
            ctx.isStickySpawning = true;
            ctx.stickyObject = levelObj;
            ctx.suppressNextStickyClick = true;
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

    /// <summary>
    /// Selects a prefab by index for spawning, without instantiating immediately.
    /// </summary>
    public void SelectPrefab(int index)
    {
        if (index < 0 || index >= objectPrefabs.Length || objectPrefabs[index] == null)
        {
            Debug.LogWarning($"Invalid prefab index {index} in SelectPrefab.");
            return;
        }

        // Set the LevelEditor to spawning state with this prefab
        // FSM migration: spawning state now handled by FSM context
        var ctx2 = LevelEditor.Instance.controller.context;
        ctx2.currentTile = null; // Clear tile selection if any
        LevelEditor.Instance.objectPalette = this; // Ensure palette reference is set
        // Optionally, store the selected prefab index if needed
    }
}