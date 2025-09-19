using UnityEngine;

public class PanAndZoomManager : MonoBehaviour
{
    [SerializeField] private float swipeSensitivity = 1f; // Adjust camera movement speed
    [SerializeField] private float pinchSensitivity = 1f; // Adjust zoom speed
    [SerializeField] private float MinZoom = 2f;
    private float MaxZoom = 21f;
    private BoxCollider2D cameraBoundsCollider; // Assigned byy InitCamera
    private float minX, maxX, minY, maxY;
    private Vector2 startTouchPosition;
    private Vector2 lastTouchPosition;
    public static bool isSwiping = false;
    private float initialPinchDistance;
    private bool isPinching = false;
    void Start()
    {
        InitCamera(boxCollider2D);
    }
    [SerializeField] BoxCollider2D boxCollider2D;
    internal void InitCamera(BoxCollider2D cameraBoundsCollider)
    {
        this.cameraBoundsCollider = cameraBoundsCollider;
        this.enabled = true;
        CalculateMaxZoom();
        CalculateCameraBounds();// Recalculate bounds after New MaxZoom has ben set by CalculateMaxZoom
    }
    void CalculateMaxZoom()
    {
        Bounds bounds = cameraBoundsCollider.bounds;
        float aspectRatio = (float)Screen.width / Screen.height;
        float maxVerticalSize = bounds.extents.y;
        float maxHorizontalSize = bounds.extents.x / aspectRatio;

        MaxZoom = Mathf.Min(maxVerticalSize, maxHorizontalSize);
        Camera.main.orthographicSize = MaxZoom;
    }

    void CalculateCameraBounds()
    {
        Bounds bounds = cameraBoundsCollider.bounds;
        float cameraHalfWidth = Camera.main.orthographicSize * Screen.width / Screen.height;
        float cameraHalfHeight = Camera.main.orthographicSize;

        minX = bounds.min.x + cameraHalfWidth;
        maxX = bounds.max.x - cameraHalfWidth;
        minY = bounds.min.y + cameraHalfHeight;
        maxY = bounds.max.y - cameraHalfHeight;
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();  // Allow testing with a mouse
#endif

        HandleTouchInput();  // Works on touchscreen devices
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            lastTouchPosition = startTouchPosition;
            isSwiping = true;
        }
        else if (Input.GetMouseButton(0) && isSwiping) // Move while dragging
        {
            Vector2 swipeVector = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - startTouchPosition;
            Camera.main.transform.position -= swipeSensitivity * new Vector3(swipeVector.x, swipeVector.y, 0);
            lastTouchPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isSwiping = false;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");// Zoom using mouse scroll wheel
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Camera.main.orthographicSize -= scroll * (pinchSensitivity * 5); // Adjust scale for mouse wheel
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, MinZoom, MaxZoom); // Clamp zoom
            CalculateCameraBounds(); // Recalculate bounds when zooming
        }
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 1) // Single-finger gestures (Swipe)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startTouchPosition = Camera.main.ScreenToWorldPoint(touch.position);
                    lastTouchPosition = startTouchPosition;
                    isSwiping = true;
                    break;

                case TouchPhase.Moved: // Move the camera while swiping
                    Vector2 swipeVector = (Vector2)Camera.main.ScreenToWorldPoint(touch.position) - startTouchPosition;
                    Camera.main.transform.position -= swipeSensitivity * new Vector3(swipeVector.x, swipeVector.y, 0);
                    lastTouchPosition = touch.position;
                    break;

                case TouchPhase.Ended:
                    isSwiping = false;
                    break;
            }
        }
        else if (Input.touchCount == 2) // Two-finger pinch gesture
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            float currentDistance = Vector2.Distance(touch1.position, touch2.position);

            if (!isPinching)
            {
                initialPinchDistance = currentDistance;
                isPinching = true;
            }
            else
            {
                float pinchDelta = (currentDistance - initialPinchDistance) / Screen.width; // Normalize zoom speed
                Camera.main.orthographicSize -= pinchDelta * pinchSensitivity * 10f;
                Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, MinZoom, MaxZoom);
                initialPinchDistance = currentDistance;
                CalculateCameraBounds(); // Update bounds after zoom
            }
        }
        else
        {
            isPinching = false;
        }
    }

    void LateUpdate()// Clamp the camera's position inside the BoxCollider2D bounds
    {
        Vector3 clampedPosition = Camera.main.transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
        Camera.main.transform.position = clampedPosition;
    }
}
