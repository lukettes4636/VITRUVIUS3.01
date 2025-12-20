using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInventory))]
public class MovJoystick : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runSpeed = 10f;
    public float crouchSpeed = 2f;
    public float rotationSpeed = 5f;
    public float gravity = -9.81f;
    public float runDuration = 4f;
    public float runCooldown = 2f;
    public float standHeight = 2f;
    public float crouchHeight = 1f;
    public Vector3 standCenter = new Vector3(0, 1f, 0);
    public Vector3 crouchCenter = new Vector3(0, 0.5f, 0);
    
    [Header("Collection Settings")]
    public float collectionRange = 2f;
    public LayerMask collectableLayer = -1;
    public InputActionReference collectAction;

    private CharacterController controller;
    private Animator animator;
    private PlayerInventory inventory;
    private Vector2 moveInput;
    private float runTimer = 0f;
    private Vector3 verticalVelocity;
    private float cooldownTimer = 0f;
    private bool canRun = true;
    private bool isCrouching = false;
    private CollectableItem nearbyCollectable;
    private bool isInCollisionWithCollectable = false;
    private CollectableItem collisionCollectable;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        inventory = GetComponent<PlayerInventory>();
        controller.height = standHeight;
        controller.center = standCenter;
    }
    
    void OnEnable()
    {
        if (collectAction != null)
        {
            collectAction.action.Enable();
            collectAction.action.performed += OnCollectAction;
        }
    }
    
    void OnDisable()
    {
        if (collectAction != null)
        {
            collectAction.action.performed -= OnCollectAction;
            collectAction.action.Disable();
        }
    }

    public void OnCrouch()
    {
        isCrouching = !isCrouching;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnRun(InputValue value)
    {
        if (value.isPressed && canRun && !isCrouching)
        {
            runTimer = runDuration;
            canRun = false;
        }
    }
    
    public void OnCollect(InputValue value)
    {
        if (value.isPressed && nearbyCollectable != null)
        {
            TryCollectItem(nearbyCollectable);
        }
    }
    
    private void OnCollectAction(InputAction.CallbackContext context)
    {
        if (isInCollisionWithCollectable && collisionCollectable != null)
        {
            TryCollectItem(collisionCollectable);
        }
    }

    void Update()
    {
        CheckForNearbyCollectables();
        
        if (runTimer > 0)
        {
            runTimer -= Time.deltaTime;
            if (runTimer <= 0)
            {
                cooldownTimer = runCooldown;
            }
        }
        
        if (!canRun && runTimer <= 0)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                canRun = true;
            }
        }

        float currentSpeed;
        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else if (runTimer > 0)
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = moveSpeed;
        }

        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);

        if (controller.isGrounded)
        {
            verticalVelocity.y = -2f;
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }

        Vector3 finalMovement = (movement * currentSpeed) + new Vector3(0, verticalVelocity.y, 0);
        controller.Move(finalMovement * Time.deltaTime);

        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        animator.SetBool("IsCrouching", isCrouching);

        if (isCrouching)
        {
            controller.height = crouchHeight;
            controller.center = crouchCenter;
            animator.SetBool("IsRunning", false);
            animator.SetFloat("Speed", movement.magnitude);
        }
        else
        {
            controller.height = standHeight;
            controller.center = standCenter;
            animator.SetBool("IsRunning", runTimer > 0);
            animator.SetFloat("Speed", movement.magnitude);
        }
    }
    
    private void CheckForNearbyCollectables()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, collectionRange, collectableLayer);
        
        nearbyCollectable = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Collectable"))
            {
                CollectableItem item = col.GetComponent<CollectableItem>();
                if (item != null)
                {
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        nearbyCollectable = item;
                    }
                }
            }
        }
    }
    
    private void TryCollectItem(CollectableItem item)
    {
        
        
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectable"))
        {
            CollectableItem item = other.GetComponent<CollectableItem>();
            if (item != null)
            {
                isInCollisionWithCollectable = true;
                collisionCollectable = item;
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Collectable"))
        {
            isInCollisionWithCollectable = false;
            collisionCollectable = null;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectionRange);
    }
}



