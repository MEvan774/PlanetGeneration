using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityOrbit : MonoBehaviour
{

    public float gravity;

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<GravityControl>())
        {
            other.GetComponent<GravityControl>().gravity = GetComponent<GravityOrbit>();
        }
    }
}
