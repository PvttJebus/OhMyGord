using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlatTestScript : MonoBehaviour
{

    public GameObject platform;
    public float movement = 1f;
    // Start is called before the first frame update
    void Start()
    {
        platform = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 currentPosition = platform.transform.position;

            Vector3 newPosition = new Vector3(currentPosition.x, currentPosition.y + movement, currentPosition.z);

            platform.transform.position = newPosition;
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 currentPosition = platform.transform.position;

            Vector3 newPosition = new Vector3(currentPosition.x, currentPosition.y - movement, currentPosition.z);

            platform.transform.position = newPosition;
        }
    }
}
