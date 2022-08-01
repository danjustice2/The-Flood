using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeJacket : Interactable
{
    GameManager GameManager;

    public override void Interact()
    {
        Debug.Log("LifeJacket: asking gamemanager to give life jacket");
        GameManager.GiveLifeJacket();
        FeedbackMessage();
        gameObject.SetActive(false);
    }

    void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        ItemName = "Life Jacket";
    }
}
