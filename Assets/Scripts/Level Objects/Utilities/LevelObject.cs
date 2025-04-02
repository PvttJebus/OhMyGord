using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelObject : MonoBehaviour
{
    public bool editing = false;

    // Modify must be described in the level object itself if applicable 
    public abstract void Modify();
}
