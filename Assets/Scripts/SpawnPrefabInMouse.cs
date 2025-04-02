using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnGrabbedPrefab : MonoBehaviour
{
    public GameObject grabbedPrefab;

    public void Spawn()
    {
        if (grabbedPrefab != null)
        {
            // Instantiate the prefab at the position of the mouse click
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Set z to 0 to place it in the 2D plane
            Instantiate(grabbedPrefab, mousePosition, Quaternion.identity);
        }
    }
}
