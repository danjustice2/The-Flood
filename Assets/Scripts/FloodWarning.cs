using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This code is written by me */

// This code is attached to the flood warning UI element.

public class FloodWarning : TutorialWindow
{
    GameManager GameManager;
    [SerializeField] GameObject Player;

    public override void ExtraFunction() // Called when the player presses Q to close the window
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        /* The if statements below are for error handling in case I forgot to assign the serialized field or the GameManager wasn't found by the line above */

        // After the warning is closed, we ask the Game Manager to open the flood warning tutorial.
        // We don't just open them both at once because that would likely be overwhelming for the player
        if (GameManager != null) { GameManager.StartTutorial(GameManager.FloodWarningTutorial); } 
        else { Debug.LogError("GameManager is null."); }

        // The movement speed is increased after the flood warning. Do that now
        if (Player != null) { Player.GetComponent<PlayerController>().IncreaseMoveSpeed(); }
        else { Debug.LogError("Player is null."); }
    }
}
