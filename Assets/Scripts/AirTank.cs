using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirTank : Interactable
{
    GameManager GameManager;

    public override void Interact()
    {
        Debug.Log("AirTank: asking gamemanager to give air tank");
        GameManager.GiveAirTank();
        FeedbackMessage();
        gameObject.SetActive(false);
    }

    void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        ItemName = "Air Tank";
    }
}
