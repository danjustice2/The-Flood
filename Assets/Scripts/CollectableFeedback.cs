using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            ShowTime -= Time.deltaTime;
        }
        else if (ShowTime <= 0)
        {
            ShowTime = StartTime;
            gameObject.SetActive(false);
        }
    }

}
