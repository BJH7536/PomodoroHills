using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public TimerWork timeWork;

    public float maxGrow;
    public float currentGrow;


    void Start()
    {
        currentGrow = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!timeWork.IsStopped&&timeWork.IsWork&&!timeWork.RestCheck)
        {
            if (maxGrow > currentGrow)
            {
                currentGrow += Time.deltaTime;
                if (currentGrow > maxGrow) { currentGrow = maxGrow; }
                float scaleX = currentGrow / maxGrow *0.4f;
                transform.localScale = new Vector3 (scaleX, scaleX, scaleX);
            }
            
        }
    }
}
