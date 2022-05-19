using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour
{
    float mouseSens = 100f;

    Camera cam;
    Transform playerBody;

    float xRot;
    float yRot;

    float multiplier = 0.01f;
    public Vector2 lookAngleMinMax = new Vector2(-75, 80);

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //playerBody = GetComponent<Transform>();
        cam = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        CamInput();

        cam.transform.localRotation = Quaternion.Euler(xRot, 0, 0);
        transform.rotation = Quaternion.Euler(0, yRot, 0);
    }

    void CamInput()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        xRot = Mathf.Clamp(xRot, lookAngleMinMax.x, lookAngleMinMax.y);

        yRot += mouseX * mouseSens * multiplier;
        xRot -= mouseY * mouseSens * multiplier;
    }
}
