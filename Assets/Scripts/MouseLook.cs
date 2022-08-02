using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/* 
This code was written by following a tutorial:
https://www.youtube.com/watch?v=w4IMYgpqgdQ
*/

public class MouseLook : MonoBehaviour
{

    private InputMaster controls;
    public float MouseSensitivity = 100f;
    private Vector2 mouseLook;
    private float xRotation = 0f;
    public Transform playerBody;

    private void Awake()
    {
        playerBody = transform.parent;

        controls = new InputMaster();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        Look();
    }

    private void Look()
    {
        mouseLook = controls.Player.Look.ReadValue<Vector2>();

        float mouseX = mouseLook.x * MouseSensitivity * Time.deltaTime;
        float mouseY = mouseLook.y * MouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90);

        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
}
