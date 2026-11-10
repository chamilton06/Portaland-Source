using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// start screen buttons
public class MainMenu : MonoBehaviour
{
    public string gameScene = "UI Update";   // scene new game loads
    public Button continueButton;
    public Button quitButton;
    public GameObject settingsPanel;

    void Start()
    {
        Time.timeScale = 1f;   // in case we came back from a paused game
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // no saves yet so continue stays greyed out
        if (continueButton != null) continueButton.interactable = false;

        // quit does nothing in a browser, so hide it in webgl
        if (quitButton != null) quitButton.gameObject.SetActive(Application.platform != RuntimePlatform.WebGLPlayer);

        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void NewGame()
    {
        SceneManager.LoadScene(gameScene);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }
}

