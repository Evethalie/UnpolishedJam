using Unity.VisualScripting;
using UnityEngine;

public class Button : MonoBehaviour
{
    public Door door;
    Animator animator;
    public bool isPressed;

    private int pressCount; 

    private void Awake()
    {
        animator = GetComponent<Animator>();
        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;  
    }

   
    private static bool IsActivator(Collider2D other)
    {
        
        if (other.CompareTag("Player") || other.CompareTag("PlayerPlatform")) return true;
        var root = other.transform.root;
        return root && (root.CompareTag("Player") || root.CompareTag("PlayerPlatform"));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsActivator(other)) return;
        pressCount++;
        SetPressed(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsActivator(other)) return;
        pressCount = Mathf.Max(0, pressCount - 1);
        if (pressCount == 0) SetPressed(false);
    }

    private void SetPressed(bool pressed)
    {
        if (isPressed == pressed) return;
        isPressed = pressed;
        animator?.SetBool("IsPressed", isPressed);
        if (pressed) door.Open();
        else StartCoroutine(door.Close());
    }
}
