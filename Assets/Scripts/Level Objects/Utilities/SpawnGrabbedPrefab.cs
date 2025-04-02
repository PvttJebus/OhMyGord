using UnityEngine;

public class SpawnGrabbedPrefab : MonoBehaviour
{
    public GameObject grabbedPrefab;

    public void Spawn()
    {
        if (!grabbedPrefab) return;

        LevelEditor.Instance.SetSpawningState();
        Vector3 spawnPos = LevelEditor.Instance.GetSnappedMousePosition();

        GameObject newObj = Instantiate(grabbedPrefab, spawnPos, Quaternion.identity);
        if (newObj.TryGetComponent(out LevelObject levelObj))
        {
            LevelEditor.Instance.RegisterLevelObject(levelObj);
            levelObj.editing = true;
        }
    }
}