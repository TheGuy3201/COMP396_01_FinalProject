using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    void Start()
    {
        // Find the TextMeshPro UGUI component in children and update its text.
        TextMeshProUGUI gameOverTextComponent = GetComponentInChildren<TextMeshProUGUI>();
        gameOverTextComponent.text = gameOverText;
    }

    public string gameOverText = "You Died";
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

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Level (1)");
    }
}
