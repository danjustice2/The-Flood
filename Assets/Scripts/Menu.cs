using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField] GameObject LoadingWindow;
    [SerializeField] Image LoadingBlackness;
    [SerializeField] GameObject NonLoading;
    [SerializeField] GameObject GameManager;
    [SerializeField] GameObject SettingsMenu;
    float timetofade;

    public void LoadScene(string scene)
    {
        Debug.Log("Showing loading screen.");
        NonLoading.SetActive(false);
        LoadingWindow.SetActive(true);
        /*float starttime = 5;
        timetofade = starttime;

        Time.timeScale = 1f;

        while (timetofade > 0)
        {
            float transparency = timetofade / starttime;
            print(transparency);
            LoadingBlackness.color = new Color(0, 0, 0, transparency);
            timetofade -= Time.deltaTime;
        }*/

        Debug.Log("Loading the scene.");
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }

    public void Settings()
    {
        SettingsMenu.SetActive(true);
    }

    public void ReloadScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        LoadScene(scene.name);
    }

    public void BackToGame()
    {
        GameManager.GetComponent<GameManager>().ToggleEscMenu();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
