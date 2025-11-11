using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InstructionsScreen : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string mainMenuScene;


    // Called when Return To Main Menu button is pressed
    public void OpenMainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    void Start()
    {
        AudioManager.Play("MainMenuMusic");
    }
}
