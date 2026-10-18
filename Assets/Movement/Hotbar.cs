using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// fast equip slots, keys 1-4 pick a slot
public class Hotbar : MonoBehaviour
{
    public Image[] slots;   // the 4 squares
    public Color normalColor = new Color(0f, 0f, 0f, 0.5f);
    public Color selectedColor = new Color(1f, 1f, 1f, 0.8f);

    private int selected = 0;

    void Start()
    {
        SelectSlot(0);
    }

    void Update()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slots.Length)
        {
            return;
        }

        selected = index;

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].color = (i == selected) ? selectedColor : normalColor;
        }

        // equip item in this slot here (inventory part)
    }

    public int GetSelectedSlot()
    {
        return selected;
    }
}
