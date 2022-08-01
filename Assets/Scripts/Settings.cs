using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using TMPro;

public class Settings : MonoBehaviour
{
    [SerializeField] GameObject VolumeSlider;
    [SerializeField] GameObject VolumeLabel;
    [SerializeField] GameObject SensitivitySlider;
    [SerializeField] GameObject SensitivityLabel;
    [SerializeField] GameObject FOVSlider;
    [SerializeField] GameObject FOVLabel;

    [SerializeField] GameObject GameManager;

    [SerializeField] GameObject settingsMenu;
    [SerializeField] AudioMixer Mixer;


    [Range(0.0001f, 1.0f)] public float volume;
    [Range(1f, 200f)] public float sensitivity;
    [Range(50f, 120f)] public float fov;

    int friendlyVolume;

    [Range(0.0001f, 1.0f)] [SerializeField] float defaultVolume = 1.0f;
    [Range(1f, 200f)] [SerializeField] float defaultSensitivity = 100f;
    [Range(50f, 120f)] [SerializeField] float defaultFOV = 70f;


    private void Awake()
    {
        volume = PlayerPrefs.GetFloat("Volume", defaultVolume);
        friendlyVolume = (int)Mathf.Round(volume * 100);
        sensitivity = PlayerPrefs.GetFloat("Sensitivity", defaultSensitivity);
        fov = PlayerPrefs.GetFloat("FOV", defaultFOV);
        Debug.Log("Values set");

        applySettings();
    }

    private void Start()
    {
        VolumeLabel.GetComponent<TextMeshProUGUI>().text = "Volume: " + friendlyVolume + "%";
        VolumeSlider.GetComponent<Slider>().value = volume;

        SensitivityLabel.GetComponent<TextMeshProUGUI>().text = "Mouse Sensitivity: " + sensitivity + "%";
        SensitivitySlider.GetComponent<Slider>().value = sensitivity;

        FOVLabel.GetComponent<TextMeshProUGUI>().text = "FOV: " + fov + "°";
        FOVSlider.GetComponent<Slider>().value = fov;
    }

    public void changeVolume()
    {
        // The volume changes in real time as the player changes it. This should make it easier to get the wished volume level
        setValues();
        applySettings();

        friendlyVolume = (int)Mathf.Round(volume * 100);

        VolumeLabel.GetComponent<TextMeshProUGUI>().text = "Volume: " + friendlyVolume + "%";
    }

    public void changeSensitivity()
    {
        sensitivity = SensitivitySlider.GetComponent<Slider>().value;
        SensitivityLabel.GetComponent<TextMeshProUGUI>().text = "Mouse Sensitivity: " + sensitivity + "%";
    }

    public void changeFOV()
    {
        fov = FOVSlider.GetComponent<Slider>().value;
        FOVLabel.GetComponent<TextMeshProUGUI>().text = "FOV: " + fov + "°";
    }

    void setValues()
    {
        volume = VolumeSlider.GetComponent<Slider>().value;
        fov = FOVSlider.GetComponent<Slider>().value;
        sensitivity = SensitivitySlider.GetComponent<Slider>().value;
    }

    void saveSettings()
    {
        setValues();
        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.SetFloat("Sensitivity", sensitivity);
        PlayerPrefs.SetFloat("FOV", fov);
        PlayerPrefs.Save();
        Debug.Log("Data saved");

        applySettings();
    }

    void applySettings()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "mainMenu")
        {
            GameManager.GetComponent<GameManager>().applySettings(sensitivity, fov, volume);
        }
        else
        {
            // If we're in the main menu, we will just set the volume value manually since we don't have the GameManager to help with that.
            // This is so the user can get live feedback when they change the volume.
            Mixer.SetFloat("Volume", Mathf.Log10(volume) * 20); 
        }
    }

    public void back()
    {
        settingsMenu.SetActive(false);
        saveSettings();
    }

    public void resetDefaults()
    {
        SensitivitySlider.GetComponent<Slider>().value = defaultSensitivity;
        FOVSlider.GetComponent<Slider>().value = defaultFOV;
        VolumeSlider.GetComponent<Slider>().value = defaultVolume;

        saveSettings();
    }
}
