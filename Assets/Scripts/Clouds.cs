using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clouds : MonoBehaviour
{
    [SerializeField] float speed;

    void Update()
    {
        transform.Rotate(Vector3.up * speed);
    }
}
