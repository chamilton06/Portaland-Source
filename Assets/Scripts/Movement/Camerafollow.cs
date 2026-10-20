// ============================================================================
// CameraFollow.cs
// Third-person camera that stays behind the player and turns with them.
// Mouse up/down tilts the camera; looking is disabled while the inventory
// is open.
// ============================================================================

using UnityEngine;

// third person camera, stays behind the player and turns with them
// mouse up/down tilts the camera
public class CameraFollow : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Configuration
    // ------------------------------------------------------------------------

    public Transform target;
    public Vector3 offset = new Vector3(0f, 6f, -8f); // behind/above the player
    public float lookHeight = 1f;     // point on the player the camera aims at

    // smoothing, lower = slower camera
    public float followSpeed = 6f;    // catching up in position
    public float rotateSpeed = 4f;    // swinging round when player turns

    // look up/down
    public float verticalSensitivity = 2f;
    public float minPitch = -35f;     // how far up you can look
    public float maxPitch = 30f;      // how far down you can look
    public bool invertY = false;

    // ------------------------------------------------------------------------
    // State
    // ------------------------------------------------------------------------

    public bool canLook = true;       // off while inventory is open
    private PlayerCombat combat;      // inventory sets inventoryOpen on this

    private float currentYaw;
    private float pitch;

    // ------------------------------------------------------------------------
    // Unity Callbacks
    // ------------------------------------------------------------------------

    // Initializes yaw from the target and caches its PlayerCombat.
    void Start()
    {
        if (target != null)
        {
            currentYaw = target.eulerAngles.y;
            combat = target.GetComponent<PlayerCombat>();
        }
    }

    // Applies mouse tilt, then smoothly follows and aims at the player.
    void LateUpdate()
    {
        if (target == null) return;

        // no looking while inventory is open
        canLook = combat == null || !combat.inventoryOpen;

        // mouse up/down tilts camera
        if (canLook)
        {
            float mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity;
            pitch += invertY ? mouseY : -mouseY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // follow player facing smoothly
        currentYaw = Mathf.LerpAngle(currentYaw, target.eulerAngles.y, rotateSpeed * Time.deltaTime);
        Quaternion rotation = Quaternion.Euler(pitch, currentYaw, 0f);

        // move camera behind player
        Vector3 desired = target.position + rotation * offset;
        transform.position = Vector3.Lerp(transform.position, desired, followSpeed * Time.deltaTime);

        transform.LookAt(target.position + Vector3.up * lookHeight);
    }
}