using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneInputHandler : MonoBehaviour
{
    public PauseMenuUI pauseMenu;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenu.IsSettingsPanelActive())
                pauseMenu.CloseSettings();
            else
                pauseMenu.Toggle();
        }
    }
}
