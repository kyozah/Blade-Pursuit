using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Position - FIXED")]
    public float distance = 5f;
    public float height = 2f;
    public float sideOffset = 0f;

    [Header("Camera Rotation - MOUSE ONLY")]
    public float mouseSensitivity = 2f;
    public float minPitch = -30f;
    public float maxPitch = 60f;

    [Header("Collision")]
    public LayerMask collisionLayers;
    public float collisionRadius = 0.3f;
    public float collisionBuffer = 0.2f;

    private float currentYaw = 0f;
    private float currentPitch = 15f;

    private PlayerInputActions inputActions;
    private Vector2 lookInput;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void OnEnable()  { inputActions.Player.Enable(); }
    void OnDisable() { inputActions.Player.Disable(); }

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("❌ Target not assigned!");
            return;
        }
        currentYaw = target.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ✅ NetworkPlayerSync gọi để gán đúng player
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
            currentYaw = target.eulerAngles.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;
        HandleInput();
        UpdateCameraPosition();
    }

    void HandleInput()
    {
        lookInput = inputActions.Player.Look.ReadValue<Vector2>();
        currentYaw   += lookInput.x * mouseSensitivity * 0.02f;
        currentPitch -= lookInput.y * mouseSensitivity * 0.02f;
        currentPitch  = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    void UpdateCameraPosition()
    {
        Quaternion rotation  = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 focusPoint   = target.position + Vector3.up * height;
        Vector3 direction    = rotation * Vector3.back;
        Vector3 rightOffset  = rotation * Vector3.right * sideOffset;
        Vector3 desiredPos   = focusPoint + direction * distance + rightOffset;
        Vector3 finalPos     = CheckCameraCollision(focusPoint, desiredPos);

        transform.position = finalPos;
        transform.LookAt(focusPoint);
    }

    Vector3 CheckCameraCollision(Vector3 fromPos, Vector3 toPos)
    {
        Vector3 direction = toPos - fromPos;
        RaycastHit hit;
        if (Physics.SphereCast(fromPos, collisionRadius, direction.normalized,
                               out hit, direction.magnitude, collisionLayers))
        {
            float safeDistance = hit.distance - collisionRadius - collisionBuffer;
            return fromPos + direction.normalized * Mathf.Max(safeDistance, 0.5f);
        }
        return toPos;
    }

    public float GetCameraYaw() => currentYaw;

    void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(target.position + Vector3.up * height, 0.3f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, target.position + Vector3.up * height);
    }
}