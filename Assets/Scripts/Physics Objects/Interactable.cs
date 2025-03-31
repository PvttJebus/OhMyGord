// Interactable.cs
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [HideInInspector] public InteractableGroup group;
    public abstract void Toggle();
}