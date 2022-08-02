using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This code is written by me */

public class RainTutorial : TutorialWindow
{
    GameManager GameManager;

    public override void ExtraFunction()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        GameManager.StartCoroutine("AnimateExposure");
    }
}
