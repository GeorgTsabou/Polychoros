using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantRotation : MonoBehaviour
{

    public float speedX;
    public float speedY;
    public float speedZ;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        transform.Rotate(speedZ * Time.deltaTime, speedZ * Time.deltaTime, speedZ * Time.deltaTime); //rotates 50 degrees per second around z axis
    }
}
