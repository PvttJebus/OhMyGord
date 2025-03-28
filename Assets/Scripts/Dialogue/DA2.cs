using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DA2 : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject doorTwo;
    public Collider2D doorCollision;
    public SpriteRenderer doorRender;
    public bool doorIsOpen;
    public bool mouseOverlap;

    void Start()
    {
        doorTwo = GameObject.Find("Door A2");
        doorCollision = doorTwo.GetComponent<Collider2D>();
        doorRender = doorTwo.GetComponent<SpriteRenderer>();
         
    }

    // Update is called once per frame
    void Update()
    {
        if (doorIsOpen)
        {
            doorCollision.enabled = false;
            doorRender.color = new Color(doorRender.color.r, doorRender.color.g, doorRender.color.b, 0.5f);
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
