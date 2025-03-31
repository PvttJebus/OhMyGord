using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Camera cam;

    [Header("Follow Settings")]
    [SerializeField][Range(0, 1)] private float followStrength = 0.3f;
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField][Range(0, 0.5f)] private float edgeThreshold = 0.1f;

    [Header("Margins")]
    [SerializeField][Range(0, 0.5f)] private float horizontalMargin = 0.2f;
    [SerializeField][Range(0, 0.5f)] private float verticalMargin = 0.2f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float maxZoom = 7.5f;
    [SerializeField] private float minZoom = 2.5f;

    private Vector3 velocity = Vector3.zero;
    private float targetZoom;

    private void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null) Debug.LogError("PlayerCamera: Player transform not found!");
        }

        if (cam == null) cam = GetComponent<Camera>();
        targetZoom = cam.orthographicSize;
    }

    private void LateUpdate()
    {
        if (playerTransform == null || cam == null) return;

        HandleZoomInput();
        UpdateCameraPosition();
    }

    private void HandleZoomInput()
    {
        float scrollInput = -Input.GetAxis("Mouse ScrollWheel");
        targetZoom += scrollInput * zoomSpeed;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
    }

    private void UpdateCameraPosition()
    {
        Vector3 mouseViewportPos = cam.ScreenToViewportPoint(Input.mousePosition);
        Vector3 playerPosition = playerTransform.position;

        Vector3 targetOffset = CalculateCameraOffset(mouseViewportPos, playerPosition);
        Vector3 desiredPosition = playerPosition + targetOffset;
        desiredPosition.z = transform.position.z;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime * (1 - followStrength)
        );
    }

    private Vector3 CalculateCameraOffset(Vector3 mouseViewportPos, Vector3 playerPosition)
    {
        float horizontalInfluence = CalculateEdgeInfluence(mouseViewportPos.x);
        float verticalInfluence = CalculateEdgeInfluence(mouseViewportPos.y);

        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        return new Vector3(
            horizontalInfluence * (0.5f - horizontalMargin) * camWidth,
            verticalInfluence * (0.5f - verticalMargin) * camHeight,
            0
        ) * followStrength;
    }

    private float CalculateEdgeInfluence(float axisPosition)
    {
        if (axisPosition < edgeThreshold)
        {
            return Mathf.SmoothStep(-1f, 0f, axisPosition / edgeThreshold);
        }

        if (axisPosition > 1 - edgeThreshold)
        {
            return Mathf.SmoothStep(0f, 1f, (axisPosition - (1 - edgeThreshold)) / edgeThreshold);
        }

        return 0;
    }
}