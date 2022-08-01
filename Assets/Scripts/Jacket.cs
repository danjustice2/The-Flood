using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jacket : Interactable
{
    GameManager GameManager;

    public override void Interact()
    {
        Debug.Log("Jacket: asking gamemanager to give life jacket");
        GameManager.GiveJacket();
        gameObject.SetActive(false);
    }

    void Start()
    {
        ItemName = "Jacket";
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
}
