using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class addOfset : MonoBehaviour
{

    private void LateUpdate()
    {
        //  transform.position = new Vector3(transform.position.x, transform.position.y + 25, transform.position.z) ;
        transform.position = new Vector3(transform.position.x , 0, transform.position.y -25);
    }
}
