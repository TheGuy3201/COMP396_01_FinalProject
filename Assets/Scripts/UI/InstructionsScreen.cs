using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InstructionsScreen : MonoBehaviour
{
    // Called when Return To Main Menu button is pressed
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OpenMainMenu();
        }
    }

    public void OpenMainMenu()
    {
        SceneManager.LoadScene("TitleScreen");
    }
}
