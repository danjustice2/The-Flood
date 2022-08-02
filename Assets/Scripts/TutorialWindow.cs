using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This code is written by me */

public class TutorialWindow : MonoBehaviour
{
    [SerializeField] float Timer = 17f;
    InputMaster controls;

    void Awake()
    {
        controls = new InputMaster();
    }

    void Update()
    {
        if (Timer > 0f) // The standard timer model that I've used so much in this project already :)
        {
            Timer -= Time.deltaTime;
        }

        if (Timer <= 0f || controls.Player.Close.triggered) // If the time has run out or the player presses the close button, close the window.
        {
            gameObject.SetActive(false);
            ExtraFunction();
        }
    }

    public virtual void ExtraFunction()
    {
        // Placeholder. Used by child classes
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
}
