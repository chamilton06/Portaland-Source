using System.Collections.Generic;
using UnityEngine;

public static class MenuState
{
    private static readonly HashSet<Object> openMenus = new HashSet<Object>();

    public static bool AnyOpen
    {
        get { return openMenus.Count > 0; }
    }

    public static void SetOpen(Object menu, bool open)
    {
        if (open)
        {
            openMenus.Add(menu);
        }
        else
        {
            openMenus.Remove(menu);
        }
    }

    // reset on restart
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Clear()
    {
        openMenus.Clear();
    }
}
