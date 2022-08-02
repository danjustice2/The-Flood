using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/* 

This code is written by me with some help from:
Blackthornprod (2018). "How to make UI in UNITY - EASY TUTORIAL." from https://www.youtube.com/watch?v=_RIsfVOqTaE.
Unity Technologies Unity Scripting Reference.
	
*/

public class Menu : MonoBehaviour
{
    [SerializeField] GameObject LoadingWindow;
    [SerializeField] Image LoadingBlackness;
    [SerializeField] GameObject NonLoading;
    [SerializeField] GameObject GameManager;
    [SerializeField] GameObject SettingsMenu;
    float timetofade;

    public void LoadScene(string scene) // Load the scene with the specified name.
    {
        Debug.Log("Showing loading screen.");
        NonLoading.SetActive(false); // This disables all UI elements other than the loading screen. I did this before I understood that I could have just placed the loading screen on top of everything else.
        LoadingWindow.SetActive(true); // Activate the loading window. This goes away when the scene is loaded since it isn't enabled in the new scene.

        Debug.Log("Loading the scene.");
        SceneManager.LoadScene(scene, LoadSceneMode.Single); // Load the scene.
    }

    public void Settings() // Open the settings menu
    {
        SettingsMenu.SetActive(true);
    }

    public void ReloadScene() // Reload the current scene
    {
        Scene scene = SceneManager.GetActiveScene();
        LoadScene(scene.name);
    }

    public void BackToGame() // Toggle the escape menu
    {
        GameManager.GetComponent<GameManager>().ToggleEscMenu();
    }

    public void Quit() // Quit the game
    {
        Application.Quit();
    }
}
