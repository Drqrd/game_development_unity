using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobeCameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private bool mouseEnabled = true;
    [SerializeField] private bool keyboardEnabled = true;
    [SerializeField] [Range(0.1f,1f)] private float scrollSensitivity;
    [SerializeField] [Range(1f,10f)] private float mouseSensitivity;
    [SerializeField] [Range(1f,10f)] private float keyboardSensitivity;
    [SerializeField] [Range(0.5f, 10f)] private float zoomSpeed;
    [SerializeField] private KeyCode up;
    [SerializeField] private KeyCode down;
    [SerializeField] private KeyCode left;
    [SerializeField] private KeyCode right;

    [SerializeField] private float distanceFromTarget;
    [SerializeField] private float minimumDistance;
    [SerializeField] private float maximumDistance;
    
    private Camera cam;
    float rotX, rotY, lerpVal, previousPosition, lerpDistance;
    bool lerping;
    void Start()
    {
        cam = GetComponent<Camera>();     

        rotX = 0f;
        rotY = 0f;
        lerpVal = 0f;
        previousPosition = distanceFromTarget;
        lerpDistance = distanceFromTarget;
    }

    // Update is called once per frame
    void Update()
    {   transform.position = target.position;
        lerping = lerpDistance != distanceFromTarget;
        if (lerping) {
            lerpVal = Mathf.Clamp01(lerpVal + Time.deltaTime * zoomSpeed);
            lerpDistance = Mathf.Lerp(previousPosition, distanceFromTarget, lerpVal);
        }
        else lerpVal = 0f;

        transform.Translate(0f,0f,-lerpDistance);

        if (mouseEnabled) HandleMouseInput();
        if (keyboardEnabled) HandleKeyboardInput();
        HandleScrollInput();

        rotY = Mathf.Clamp(rotY, -60f, 60f);
        target.localRotation = Quaternion.Euler(-rotY, rotX, 0f);
    }

    private void HandleMouseInput() {
        
        if (Input.GetMouseButton(0)) {
            rotX += Input.GetAxis("Mouse X") * mouseSensitivity;
            rotY += Input.GetAxis("Mouse Y") * mouseSensitivity;
        }
    }

    private void HandleKeyboardInput() {

        if (Input.GetKey(up)) rotY -= 1f * keyboardSensitivity;
        if (Input.GetKey(down)) rotY += 1f * keyboardSensitivity;
        if (Input.GetKey(left)) rotX += 1f * keyboardSensitivity;
        if (Input.GetKey(right)) rotX -= 1f * keyboardSensitivity;
    }

    private void HandleScrollInput() {
        float previousEndPoint = -1f;
        if (Input.mouseScrollDelta.magnitude > 0) {
            if (!lerping) previousPosition = distanceFromTarget;
            else previousEndPoint = distanceFromTarget;
            distanceFromTarget += -Input.mouseScrollDelta.y * scrollSensitivity;
            distanceFromTarget = Mathf.Clamp(distanceFromTarget, minimumDistance, maximumDistance);
            if (previousEndPoint > 0f) lerpVal = lerpVal * Mathf.Abs(previousEndPoint - previousPosition) / Mathf.Abs(previousPosition - distanceFromTarget);
        }
    }
}
