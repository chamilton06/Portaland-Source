// ============================================================================
// Billboard.cs
// Ensures text on an NPC always faces the camera (is always readable).
//
// To use, drag script onto any NPC Canvas
// ============================================================================

using UnityEngine;

public class Billboard : MonoBehaviour {
    Transform cam;

    void Start() => cam = Camera.main.transform;

    void LateUpdate() {
        transform.forward = cam.forward;
    }
}