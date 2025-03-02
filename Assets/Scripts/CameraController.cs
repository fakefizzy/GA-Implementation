using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float zoomSpeed = 1.0f;
    public float minZoom;
    public float maxZoom;

    public float panSpeed = 15.0f;
    private bool isPanning = false;
    private Vector3 lastMousePosition;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
    }

    void Update()
    {
        HandleZoom();
        HandlePan();
    }

    void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0)
        {
            float newSize = mainCamera.orthographicSize - scrollInput * zoomSpeed * 5;

            mainCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }

    void HandlePan()
    {
        if (Input.GetMouseButtonDown(2))
        {
            isPanning = true;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(2))
        {
            isPanning = false;
        }

        if (isPanning)
        {
            Vector3 currentMousePosition = Input.mousePosition;
            Vector3 mouseDelta = currentMousePosition - lastMousePosition;

            float panMultiplier = mainCamera.orthographicSize / 10.0f;

            Vector3 moveDirection = new(
                -mouseDelta.x * panSpeed * panMultiplier * Time.deltaTime,
                -mouseDelta.y * panSpeed * panMultiplier * Time.deltaTime,
                0
            );

            transform.Translate(moveDirection);

            lastMousePosition = currentMousePosition;
        }
    }
}