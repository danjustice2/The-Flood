/*using UnityEngine;

public class Exposure : MonoBehaviour
{
    [SerializeField] GameObject playerCamera;
    bool isCovered;
    public float bodyTemperature = 250f;
    public GameObject RainManager;
    float rainIntensity;

    // Start is called before the first frame update
    void Start()
    {
        //RainManager.GetComponent<RainScript>().RainIntensity = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale > 0f) {
            insideCheck();
        };

        if (isCovered == false)
        {
            bodyTemperature = bodyTemperature - Time.deltaTime;
        }
        else
        {
            bodyTemperature = bodyTemperature - (Time.deltaTime / 2);
        }

    }

    void insideCheck()
    {
        Vector3 up = playerCamera.transform.TransformDirection(Vector3.up);

        if (Physics.Raycast(playerCamera.transform.position, up, 10) == false)
        { isCovered = false; }
        else { isCovered = true; }
    }
}*/
