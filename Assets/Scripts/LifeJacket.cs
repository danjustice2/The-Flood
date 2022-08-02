using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This code is written by me */

public class LifeJacket : Interactable
{
    GameManager GameManager;

    public override void Interact()
    {
        Debug.Log("LifeJacket: asking gamemanager to give life jacket");
        GameManager.GiveLifeJacket();
        gameObject.SetActive(false);
    }

    void Start()
    {
        ItemName = "Life Jacket";
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
}
