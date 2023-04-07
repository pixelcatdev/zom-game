using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector3 dragOrigin;

    public float dragSpeed = 10f;
    public float zoomSpeed = 1f;
    public float minZoomSize = 2f;
    public float maxZoomSize = 10f;

    void Update()
    {
        // Check if right mouse button is held down
        if (Input.GetMouseButtonDown(1))
        {
            // Set the drag origin to the current mouse position
            dragOrigin = Input.mousePosition;
        }

        // Check if right mouse button is being held down and the mouse is moving
        if (Input.GetMouseButton(1) && dragOrigin != Vector3.zero)
        {
            // Calculate the drag distance
            Vector3 mousePos = Input.mousePosition;
            Vector3 dragDistance = Camera.main.ScreenToViewportPoint(mousePos - dragOrigin);
            dragDistance *= dragSpeed;

            // Move the camera based on the drag distance
            transform.Translate(-dragDistance.x, -dragDistance.y, -dragDistance.z, Space.Self);

            // Update the drag origin to the current mouse position
            dragOrigin = mousePos;
        }

        // Check if mouse wheel is being scrolled
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            // Zoom the camera in or out based on the scroll amount
            float size = Camera.main.orthographicSize;
            float zoomAmount = scroll * zoomSpeed;
            size = Mathf.Clamp(size - zoomAmount, minZoomSize, maxZoomSize);
            Camera.main.orthographicSize = size;
        }
    }
}
