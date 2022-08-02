using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This code is written by me */

public class FrontDoor : Interactable
{
    [SerializeField] GameObject MaybeStayHome;
    [SerializeField] GameObject PlayerController;
    GameManager GameManager;

    public override void Interact()
    {
        if (GameManager.FloodFinished) // If the flood is finished, the player is allowed to open the door
        {
            // Give the success feedback message
            FeedbackMessage(); 

            // Give the player a higher moving speed so they can explore the terrain easier.
            PlayerController.GetComponent<PlayerController>().PostWarningMoveSpeed = 6f; 

            // Set the door to inactive, allowing the user to move through it
            gameObject.SetActive(false); 
        }
        else // If the flood isn't finished
        {
            GameManager.OpenInteractableFeedback(MaybeStayHome); // Tell the user to maybe say home and to check back later
            ItemName = "Check back later"; // Change the interaction text of the item
        }
    }

    void Update()
    {

        // If the player previously tried to open the door and got the "Check back later" message...
        if (GameManager.FloodFinished && ItemName == "Check back later")
        {
            // Change the interaction text back to "Go outside".
            ItemName = "Go Outside";
        }
    }

    void Start()
    {
        ItemName = "Go Outside";
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
}

