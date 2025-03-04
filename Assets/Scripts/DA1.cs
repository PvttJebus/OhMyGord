using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class DA1 : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject doorOne;
    public Collider2D doorCollision;
    public SpriteRenderer doorRender;
    public bool doorIsOpen;
    public bool mouseOverlap;

    void Start()
    {
        doorOne = GameObject.Find("Door A1");
        doorCollision = doorOne.GetComponent<Collider2D>();
        doorRender = doorOne.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (doorIsOpen)
        {
            doorCollision.enabled = false;
            doorRender.color = new Color(doorRender.color.r,doorRender.color.g,doorRender.color.b,0.5f);
        }

        else 
        {
            doorCollision.enabled = true;
            doorRender.color = new Color(doorRender.color.r, doorRender.color.g, doorRender.color.b, 1f);
        }
    }

    private void OnMouseEnter()
    {
        mouseOverlap = true;
    }

    private void OnMouseExit()
    {
        mouseOverlap = false;
    }
}
