using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    GameObject player;
    private Camera cam;
    private Vector3 velocity = Vector3.zero;

    [Range(0, 1)] public float followStrength = 0.3f;
    public float smoothTime = 0.3f;
    [Range(0, 0.5f)] public float edgeThreshold = 0.1f;

    [Range(0, 0.5f)] public float horizontalMargin = 0.2f;
    [Range(0, 0.5f)] public float verticalMargin = 0.2f;
    public float zoomLevel;
    public float maxZoom = 7.5f;
    public float minZoom = 2.5f;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        Vector3 desiredCamPos;

        
        if (Input.mouseScrollDelta.y < 0 && cam.orthographicSize <= maxZoom)
        {
            cam.orthographicSize += 0.5f;

            
        }

        else if (Input.mouseScrollDelta.y > 0 && cam.orthographicSize >= minZoom)
        {
            cam.orthographicSize -= 0.5f;
        }
        
        
        //if (Input.GetMouseButton(0))
        //{
        //    // Freeze camera position when mouse is held down
        //    desiredCamPos = transform.position;
        //    velocity = Vector3.zero; // Reset velocity for immediate stop
        //}
        
        
            // Normal camera behavior when mouse is not held down
            Vector3 playerPos = player.transform.position;
            Vector3 mouseViewportPos = cam.ScreenToViewportPoint(Input.mousePosition);

            float horizontalInfluence = CalculateEdgeInfluence(mouseViewportPos.x);
            float verticalInfluence = CalculateEdgeInfluence(mouseViewportPos.y);

            float camHeight = 2f * cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            Vector3 targetOffset = new Vector3(
                horizontalInfluence * (0.5f - horizontalMargin) * camWidth,
                verticalInfluence * (0.5f - verticalMargin) * camHeight,
                0
            ) * followStrength;

            desiredCamPos = playerPos + targetOffset;
            desiredCamPos.z = transform.position.z;
        

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredCamPos,
            ref velocity,
            smoothTime
        );
    }

    private float CalculateEdgeInfluence(float axisPosition)
    {
        if (axisPosition < edgeThreshold)
            return (axisPosition / edgeThreshold) - 1;
        if (axisPosition > 1 - edgeThreshold)
            return (axisPosition - (1 - edgeThreshold)) / edgeThreshold;
        return 0;
    }
}