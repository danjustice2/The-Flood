using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This code is written by me */

public class EasterEgg : Interactable
{
    GameManager GameManager;

    public override void Interact()
    {
        FeedbackMessage();
        gameObject.SetActive(false);
    }

    void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        ItemName = "Easter Egg";
    }
}
