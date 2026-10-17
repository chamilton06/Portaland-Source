using UnityEngine;

// third-person camera that stays behind the player, turns with them,
// and lets you look up and down with the mouse.
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 6f, -8f); // behind/above, in the player's local space

    [Header("Smoothing (lower = slower, floatier camera)")]
    public float followSpeed = 6f;    // how quickly the camera catches up in position
    public float rotateSpeed = 4f;    // how quickly the camera swings round when the player turns

    [Header("Look up / down")]
    public float verticalSensitivity = 2f;
    public float minPitch = -35f;     // how far you can look up (camera dips lower)
    public float maxPitch = 30f;      // how far you can look down (camera rises higher)
    public bool invertY = false;

    public float lookHeight = 1f;     // point on the player the camera aims at

    float currentYaw;
    float pitch;

    void Start()
    {
        if (target != null) currentYaw = target.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity;
        pitch += invertY ? mouseY : -mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        currentYaw = Mathf.LerpAngle(currentYaw, target.eulerAngles.y, rotateSpeed * Time.deltaTime);
        Quaternion rotation = Quaternion.Euler(pitch, currentYaw, 0f);

        Vector3 desired = target.position + rotation * offset;
        transform.position = Vector3.Lerp(transform.position, desired, followSpeed * Time.deltaTime);

        transform.LookAt(target.position + Vector3.up * lookHeight);
    }
}