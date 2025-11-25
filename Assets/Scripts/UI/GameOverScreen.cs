using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    public string gameOverText = "You Died";
    void Start()
    {
        // Find the TextMeshPro UGUI component in children and update its text.
        TextMeshProUGUI gameOverTextComponent = GameObject.FindWithTag("GameOverMSG").GetComponent<TextMeshProUGUI>();
        gameOverTextComponent.text = gameOverText;
    }
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
