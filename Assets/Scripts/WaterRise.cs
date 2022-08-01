using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterRise : MonoBehaviour
{
    public float speed;
    public int maxHeight;
    public GameObject Water;

    void Update()
    {
        if (Water.transform.position.y < maxHeight) {
            Water.transform.position = Water.transform.position + new Vector3(0, speed / 100 * Time.deltaTime, 0);
        }
    }
}
