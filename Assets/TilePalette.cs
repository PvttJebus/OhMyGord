using UnityEngine;

// Optional but helps validate palette structure
public class TilePalette : MonoBehaviour
{
    [Header("Child objects must have SpriteRenderer components")]
    [SerializeField] bool validateChildren = true;

    void OnValidate()
    {
        if (!validateChildren) return;

        foreach (Transform child in transform)
        {
            if (child.GetComponent<SpriteRenderer>() == null)
            {
                Debug.LogError($"TilePalette child {child.name} is missing SpriteRenderer!", this);
            }
        }
    }
}