using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrontDoor : Interactable
{
    GameManager GameManager;
    [SerializeField] GameObject MaybeStayHome;
    [SerializeField] GameObject PlayerController;

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
            MaybeStayHome.SetActive(true);
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
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        ItemName = "Go Outside";
    }
}

