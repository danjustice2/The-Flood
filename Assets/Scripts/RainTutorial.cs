using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainTutorial : TutorialWindow
{
    GameManager GameManager;

    public override void ExtraFunction()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        GameManager.StartCoroutine("AnimateExposure");
    }
}
