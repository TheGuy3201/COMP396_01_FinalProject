using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameplayScene;
    [SerializeField] private string optionsScene;
    [SerializeField] private string instructionsScene;

    // Called when Start button is pressed
    public void StartGame()
    {
        SceneManager.LoadScene(gameplayScene);
    }

    // Called when Options button is pressed
    public void OpenOptions()
    {
        SceneManager.LoadScene(optionsScene);
    }

    // Called when Instructions button is pressed
    public void OpenInstructions()
    {
        SceneManager.LoadScene(instructionsScene);
    }

    // Called when Quit button is pressed
    public void QuitGame()
    {
        Application.Quit();
    }

    void Start()
    {
        AudioManager.Play("MainMenuMusic");
    }
}
