using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{

    private InputMaster controls;
    [SerializeField] float moveSpeed;
    private Vector3 velocity;
    private float gravity = -12f;
    private Vector2 move;
    [SerializeField] float StandardJumpHeight = 1.2f;
    float jumpHeight;
    bool JumpEnabled = true;
    private CharacterController controller;

    public Transform ground;
    public float distanceToGround = 0.4f;
    public LayerMask groundMask;
    private bool isGrounded;
    bool IsUnderWater;

    bool IsInWater;

    public Interactable focus;

    GameManager GameManager;
    Camera cam;
    [SerializeField] GameObject pressE;
    [SerializeField] GameObject ItemText;
    [SerializeField] float StartMoveSpeed = 3f;
    public float PostWarningMoveSpeed = 6f;
    bool FastSpeed = false;

    bool LifeJacket;


    private void Awake()
    {
        controls = new InputMaster();
        controller = GetComponent<CharacterController>();
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    void Update()
    {
        if (GameManager.Paused == false)
        {
            HasLifeJacket();
            Buoyancy();
            Grav();
            PlayerMovement();
            CheckInteractable();
        }
    }

    public void IncreaseMoveSpeed()
    {
        FastSpeed = true; // Increase move speed after flood warning
    }

    void HasLifeJacket()
    {
        // Make sure LifeJacket value from GameManager matches the local value.
        if(GameManager.LifeJacket && !LifeJacket) { LifeJacket = true; }
        else if(!GameManager.LifeJacket && LifeJacket) { LifeJacket = false; }
    }
    
    void Buoyancy()
    {
        IsUnderWater = GameManager.WaterCheck(0.5f);
        IsInWater = GameManager.IsInWater;

        if (IsUnderWater && LifeJacket)
        {
            gravity = 1f;
            jumpHeight = 0.6f;
            moveSpeed = 6f;
        }
        else if (IsUnderWater && !LifeJacket)
        {
            gravity = -6f;
            jumpHeight = 0.6f;
            moveSpeed = 3f;
        }
        else if (!IsUnderWater && !IsInWater)
        {
            if (FastSpeed) 
            {
                gravity = -12f;
                jumpHeight = StandardJumpHeight;
                moveSpeed = PostWarningMoveSpeed;
            }
            else
            {
                gravity = -12f;
                jumpHeight = StandardJumpHeight;
                moveSpeed = StartMoveSpeed;
            }
            
        }
        else if (!IsUnderWater && IsInWater)
        {
            gravity = -12f;
            jumpHeight = 0.8f;
            moveSpeed = 4f;
        }
    }

    void CheckInteractable()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null && !pressE.activeSelf)
            {
                bool IsValidTarget = interactable.IsTargeted();
                if (interactable != null && IsValidTarget && GameManager.Interactables)
                {
                    pressE.SetActive(true);
                    ItemText.GetComponent<TextMeshProUGUI>().text = interactable.ItemName;
                }
            }
            else if (interactable == null && pressE.activeSelf) { pressE.SetActive(false); ItemText.GetComponent<TextMeshProUGUI>().text = "null"; }
        }
    }

    void OnInteract()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null && GameManager.Interactables)
            {
                SetFocus(interactable);
                focus.Interact();
                // focus.transform.gameObject.SetActive(false);
            }
            else { RemoveFocus(); }
        }
    }

    void SetFocus (Interactable NewFocus)
    {
        if (NewFocus != focus)
        {
            if (focus != null)
            {
                focus.OnDefocused();
            }
            
            focus = NewFocus;
        }
        
        NewFocus.OnFocused(transform);
    }

    void RemoveFocus()
    {
        if (focus != null) 
        {
            focus = null;
            focus.OnDefocused();
        }
        
    }

    private void Grav()
    {
        //if (!IsUnderWater) 
        {
            isGrounded = Physics.CheckSphere(ground.position, distanceToGround, groundMask);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }

    private void PlayerMovement()
    {
        move = controls.Player.Movement.ReadValue<Vector2>();

        Vector3 movement = (move.y * transform.forward) + (move.x * transform.right);
        controller.Move(movement * moveSpeed * Time.deltaTime);
    }

    private void OnJump()
    {
        if (isGrounded && JumpEnabled || IsInWater && IsUnderWater)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
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
