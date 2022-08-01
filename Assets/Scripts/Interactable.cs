using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public class Interactable : MonoBehaviour
{
    public float radius = 3f;

    bool isFocus = false;
    Transform player;

    bool hasInteracted = false;

    GameManager GameManager;

    [HideInInspector] public string ItemName = null;

    [SerializeField] GameObject message;

    public virtual void Interact()
    {
        // This is to be overwritten.
        Debug.LogError("No interaction set");
    }

    void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
    
    void Update()
    {
        if (isFocus && !hasInteracted)
        {
            float distance = Vector3.Distance(player.position, transform.position);

            if (distance <= radius)
            {
                hasInteracted = true;
            }
        }
    }

    public void FeedbackMessage()
    {
        /*if (message != null)
        {
            GameManager.OpenInteractableFeedback(message);
        }
        else
        {
            Debug.LogError("Message is null");
        }*/
        Debug.LogError("Deprecated FeedbackMessage function used");
    }

    public bool IsTargeted()
    {
        player = GameObject.Find("Player").GetComponent<Transform>();
        float distance = Vector3.Distance(player.position, gameObject.transform.position); 

        if (distance < radius)
        {
            return (true);
        }
        else
        {
            return (false);
        }
    }

    public void OnFocused (Transform playerTransform)
    {
        isFocus = true;
        player = playerTransform;
        hasInteracted = false;
    }

    public void OnDefocused()
    {
        isFocus = false;
        player = null;
        hasInteracted = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }



}
