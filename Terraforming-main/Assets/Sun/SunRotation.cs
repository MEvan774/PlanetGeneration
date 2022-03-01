using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunRotation : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] Vector3 RotationDir;

    void Update()
    {
        transform.RotateAround(transform.position, RotationDir, rotationSpeed * Time.deltaTime);
    }
}
