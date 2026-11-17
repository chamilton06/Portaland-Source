// ============================================================================
// Quest1.cs
// Dialogue with the village blacksmith
//
// 
// ============================================================================

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfferPopup : MonoBehaviour
{
    public static OfferPopup Instance { get; private set; }

    public GameObject panel;
    public TMP_Text messageText;
    public Button acceptButton;
    public Button rejectButton;

    public Behaviour[] disableWhileOpen;

    public bool IsOpen => panel.activeSelf;

    Action onAccept, onReject, onClosed;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
        acceptButton.onClick.AddListener(() => Close(onAccept));
        rejectButton.onClick.AddListener(() => Close(onReject));
    }

    public void Show(string message, Action accept = null, Action reject = null, Action closed = null)
    {
        messageText.text = message;
        onAccept = accept;
        onReject = reject;
        onClosed = closed;

        panel.SetActive(true);
        foreach (var b in disableWhileOpen) b.enabled = false;
        Cursor.lockState = CursorLockMode.None;     // unlock cursor
        Cursor.visible = true;
    }

    void Close(Action result)
    {
        panel.SetActive(false);
        foreach (var b in disableWhileOpen) b.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;         // re-lock cursor

        result?.Invoke();
        onClosed?.Invoke();             // prompt comes back after closing dialogue
    }
}