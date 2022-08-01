using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrontDoor : Interactable
{
    [SerializeField] GameObject MaybeStayHome;
    [SerializeField] GameObject PlayerController;
    GameManager GameManager;

    public override void Interact()
    {
        if (GameManager.FloodFinished)
        {
            FeedbackMessage();
            gameObject.SetActive(false);
            PlayerController.GetComponent<PlayerController>().PostWarningMoveSpeed = 6f;
        }
        else
        {
            GameManager.OpenInteractableFeedback(MaybeStayHome);
            ItemName = "Check back later";
        }
    }

    void Update()
    {
        if (GameManager.FloodFinished && ItemName == "Check back later")
        {
            ItemName = "Go Outside";
        }
    }

    void Start()
    {
        ItemName = "Go Outside";
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
}

