using UnityEngine;

//moving the mouse left/right turns the player,
// and Camerafollow swings round to stay behind them.
public class PlayerTurn : MonoBehaviour
{
    public float mouseSensitivity = 3f;
    public bool lockCursor = true;

    void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0f, mouseX, 0f);

        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}