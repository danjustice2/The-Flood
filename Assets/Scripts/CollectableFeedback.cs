using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This code is written by me */

// This is included on item feedback windows, getting activated when the message is shown.
// We make sure the feedback window closes itself after a set amount of seconds

public class CollectableFeedback : MonoBehaviour
{
    float ShowTime;
    public float StartTime = 6f;

    void Start()
    {
        ShowTime = StartTime;
    }

    void Update()
    {
        if (ShowTime > 0)
        {
            ShowTime -= Time.deltaTime; // Count down
        }
        else if (ShowTime <= 0)
        {
            ShowTime = StartTime; // When we reach 0, reset the timer and set the window to inactive again.
            gameObject.SetActive(false);
        }
    }

}
