// ============================================================================
// StatBar.cs
// UI bar that goes on each stat bar. Reads hunger, stamina, or health from
// PlayerStats, fills the bar to match, and fades its color from full to low
// as the value empties.
// ============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// goes on each bar, reads values from player stats
public class StatBar : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Configuration
    // ------------------------------------------------------------------------

    public enum StatType { Hunger, Stamina, Health }

    public StatType stat;                   // which stat this bar displays
    public PlayerStats player;
    public Image fillImage;
    public float smoothSpeed = 8f;
    public Color fullColor = Color.green;
    public Color lowColor = Color.red;

    // ------------------------------------------------------------------------
    // Unity Callbacks
    // ------------------------------------------------------------------------

    // Gives the fill image a default sprite if it has none and sets it to horizontal fill.
    void Start()
    {
        if (fillImage.sprite == null)
        {
            Texture2D tex = Texture2D.whiteTexture;
            fillImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
    }

    // Updates the bar's fill amount and color from the current stat value each frame.
    void Update()
    {
        if (player == null)
        {
            return;
        }

        float target = GetPercent();

        // slide smoothly to new value
        fillImage.fillAmount = target;

        // goes red as it empties
        fillImage.color = Color.Lerp(lowColor, fullColor, target);
    }

    // ------------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------------

    // Returns the 0-1 value for the selected stat.
    float GetPercent()
    {
        if (stat == StatType.Hunger)
        {
            return player.GetHungerPercent();
        }
        if (stat == StatType.Stamina)
        {
            return player.GetStaminaPercent();
        }
        return player.GetHealthPercent();
    }
}