using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float panSpeed = 20f; // The speed at which the camera will pan.

    [SerializeField]
    private float panBorder = 10f; // The border in pixels that will cause the camera to pan if the mouse is outside of.

    private void OnEnable() {
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
    }

    // Update is called once per frame
    void Update()
    {
        // Don't allow camera movement when paused.
        if (GameController.Instance.IsPaused)
            return;

        // If LMB is being held down, do not allow the camera to move.
        if (Input.GetMouseButton(0))
            return;

        // Move the camera with mouse and keyboard input
        Vector3 camPos = transform.position;

        // Up
        if (Input.GetKey(KeyCode.UpArrow) || Input.mousePosition.y >= Screen.height - panBorder) {
            camPos.z += panSpeed * Time.deltaTime;
        }

        // Down
        if(Input.GetKey(KeyCode.DownArrow) || Input.mousePosition.y <= panBorder) {
            camPos.z -= panSpeed * Time.deltaTime;
        }

        // Left
        if (Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x <= panBorder) {
            camPos.x -= panSpeed * Time.deltaTime;
        }

        // Right
        if (Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x >= Screen.width - panBorder) {
            camPos.x += panSpeed * Time.deltaTime;
        }

        transform.position = camPos;
    }
}
