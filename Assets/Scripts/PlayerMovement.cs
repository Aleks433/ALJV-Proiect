using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam; // Drag Main Camera here in Inspector

    public float walkSpeed = 5f;
    public float mouseSensitivity = 800f;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // --- 1. ROTATION (Mouse) ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate the body left/right
        transform.Rotate(Vector3.up * mouseX);

        // Rotate the camera up/down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // --- 2. MOVEMENT (WASD) ---
        // Ensure these are "Horizontal" and "Vertical", NOT "Mouse X/Y"
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * walkSpeed * Time.deltaTime);
    }
}