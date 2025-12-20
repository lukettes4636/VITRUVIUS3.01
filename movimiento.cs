using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movimiento : MonoBehaviour
{
    public int playerNumber = 1;
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    private CharacterController controller;
    private Vector3 playerVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal_p" + playerNumber);
        float moveZ = Input.GetAxisRaw("Vertical_p" + playerNumber);

        Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
