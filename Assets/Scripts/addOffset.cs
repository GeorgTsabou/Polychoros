using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class addOffset : MonoBehaviour
{
    public float offsetX =0;
    public float offsetY = -25;
    public float offsetZ =0;

    private void LateUpdate()
    {
        //  transform.position = new Vector3(transform.position.x, transform.position.y + 25, transform.position.z) ;
        transform.position = new Vector3(transform.position.x + offsetX , 0 + offsetY, transform.position.y + offsetZ);
    }
}
