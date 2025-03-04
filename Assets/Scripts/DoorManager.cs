using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorManager : MonoBehaviour
{

    public DA1 doorOne;
    public DA2 doorTwo;
    // Start is called before the first frame update
    void Start()
    {
        GameObject d1 = GameObject.Find("Door A1");
        doorOne = d1.GetComponent<DA1>();
        GameObject d2 = GameObject.Find("Door A2");
        doorTwo = d2.GetComponent<DA2>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (doorOne.doorIsOpen == false && doorOne.mouseOverlap == true)
            {
                doorOne.doorIsOpen = true;
                doorTwo.doorIsOpen = false;
                return;
            }

            else if (doorTwo.doorIsOpen == false && doorTwo.mouseOverlap == true)
            {
                doorTwo.doorIsOpen = true;
                doorOne.doorIsOpen = false;
                return;
            }

            else
            {
                Debug.Log("This door shit is broken");
            }
        }
    }
}
