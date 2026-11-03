using UnityEngine;

// Only used in the animation test scene. This doesn't move the gameplay Player.
public class RangerAnimationPreview : MonoBehaviour
{
    public Animator[] characters;
    [Range(0f, 3f)] public float previewSpeed;
    private static readonly int Speed = Animator.StringToHash("Speed");

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) previewSpeed = 0f;
        if (Input.GetKeyDown(KeyCode.Alpha2)) previewSpeed = 1f;
        if (Input.GetKeyDown(KeyCode.Alpha3)) previewSpeed = 2f;
        if (Input.GetKeyDown(KeyCode.Alpha4)) previewSpeed = 3f;

        foreach (Animator character in characters)
        {
            if (character != null) character.SetFloat(Speed, previewSpeed, 0.15f, Time.deltaTime);
        }
    }

    void OnGUI()
    {
        GUI.Box(new Rect(12, 12, 430, 58),
            "Ranger animation test\n1: Idle   2: Walk   3: Jog   4: Sprint");
    }
}
