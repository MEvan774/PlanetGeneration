using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityControl : MonoBehaviour
{
    [HideInInspector] public GravityOrbit gravity;
    Rigidbody rb;

    float rotationSpeed = 5;
    [HideInInspector] public Vector3 gravityUp;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (gravity)
        {
            //gravityUp = Vector3.zero;

            gravityUp = gravity.transform.up;
            //gravityUp = (transform.position - gravity.transform.position).normalized;

            //Vector3 localUp = transform.up;

            //Quaternion targetRotation = Quaternion.FromToRotation(localUp, gravityUp) * transform.rotation;

            Vector3 localUp = transform.up;

            Quaternion targetRotation = Quaternion.FromToRotation(localUp, gravityUp) * transform.rotation;
            rb.GetComponent<Rigidbody>().rotation = targetRotation;

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            rb.GetComponent<Rigidbody>().AddForce((-gravityUp * gravity.gravity) * rb.mass);

            //upTransform.up = Vector3.Lerp(transform.up, gravityUp, rotationSpeed * Time.deltaTime);

            rb.AddForce((-gravityUp * gravity.gravity) * rb.mass);
        }
    }
}
