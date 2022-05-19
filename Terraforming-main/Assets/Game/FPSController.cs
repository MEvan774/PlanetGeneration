using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : MonoBehaviour
{
    GravityControl gravity;

    float speed = 20;

    private Rigidbody rb;

    float moveX;
    float moveZ;
    Vector3 moveDir;
    float moveGroundMulti = 7;
    float moveAirMulti = 0.4f;

    float groundDrag = 6;
    float airDrag = 2;

    float jumpForce = 4000;
    [SerializeField] bool isGrounded;
    private float playerHeight;

    private void Start()
    {
        playerHeight = GetComponent<CapsuleCollider>().height;

        rb = GetComponent<Rigidbody>();
        gravity = GetComponent<GravityControl>();
    }

    private void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight / 2 + 0.1f);

        moveInput();
        ControlDrag();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    void ControlDrag()
    {
        if(isGrounded)
        rb.drag = groundDrag;
        else
            rb.drag = airDrag;
    }

    void moveInput()
    {
        moveX = Input.GetAxis("Horizontal");
        moveZ = Input.GetAxis("Vertical");

        moveDir = transform.right * moveX + transform.forward * moveZ;
    }

    private void FixedUpdate()
    {
        if(isGrounded)
            rb.AddForce(moveDir * speed * moveGroundMulti, ForceMode.Acceleration);
        else
            rb.AddForce(moveDir * speed * moveAirMulti, ForceMode.Acceleration);
    }
}
