using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// goes on each bar, reads values from player stats
public class StatBar : MonoBehaviour
{
    public enum StatType { Hunger, Stamina, Health }

    public StatType stat;
    public PlayerStats player;
    public Image fillImage;
    public float smoothSpeed = 8f;
    public Color fullColor = Color.green;
    public Color lowColor = Color.red;

    private float shownFill = 1f;

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

    void Update()
    {
        if (player == null)
        {
            return;
        }

        float target = GetPercent();

        // slide smoothly to new value
        shownFill = Mathf.Lerp(shownFill, target, smoothSpeed * Time.deltaTime);
        fillImage.fillAmount = shownFill;

        // goes red as it empties
        fillImage.color = Color.Lerp(lowColor, fullColor, target);
    }

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