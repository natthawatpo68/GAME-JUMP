using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public sealed class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 6.5f;
    [SerializeField] private float height = 2.2f;
    [SerializeField] private float mouseSensitivity = 0.12f;
    [SerializeField] private float followSmoothness = 14f;
    [SerializeField] private float minPitch = -25f;
    [SerializeField] private float maxPitch = 55f;

    private float yaw;
    private float pitch = 15f;

    private void Start()
    {
        yaw = target != null ? target.eulerAngles.y : transform.eulerAngles.y;
        LockCursor();
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Cursor.lockState = CursorLockMode.None;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) LockCursor();
#else
        if (Input.GetKeyDown(KeyCode.Escape)) Cursor.lockState = CursorLockMode.None;
        if (Input.GetMouseButtonDown(0)) LockCursor();
#endif

        Vector2 look = ReadLookInput();
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            yaw += look.x * mouseSensitivity;
            pitch = Mathf.Clamp(pitch - look.y * mouseSensitivity, minPitch, maxPitch);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;
        Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focusPoint = target.position + Vector3.up * height;
        Vector3 desiredPosition = focusPoint + orbit * new Vector3(0f, 0f, -distance);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, 1f - Mathf.Exp(-followSmoothness * Time.deltaTime));
        transform.rotation = Quaternion.LookRotation(focusPoint - transform.position);
    }

    private static Vector2 ReadLookInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) return Mouse.current.delta.ReadValue();
#endif
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    private static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
