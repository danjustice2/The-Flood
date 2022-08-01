using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloodWarning : TutorialWindow
{
    GameManager GameManager;
    [SerializeField] GameObject Player;

    public override void ExtraFunction()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        if (GameManager != null) { GameManager.StartTutorial(GameManager.FloodWarningTutorial); }
        else { Debug.LogError("GameManager is null."); }

        if (Player != null) { Player.GetComponent<PlayerController>().IncreaseMoveSpeed(); }
        else { Debug.LogError("Player is null."); }
    }
}
