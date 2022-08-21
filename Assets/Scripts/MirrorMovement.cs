using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorMovement : MonoBehaviour
{
    public float offsetX = 0;
    public float offsetY = 0;
    public float offsetZ = 0;

    public float ScreenWidthPixel = 38.4f;
    public float ScreenHeightPixel = 21.6f;

    private void LateUpdate()
    {
        //  transform.position = new Vector3(transform.position.x, transform.position.y + 25, transform.position.z) ;
        transform.position = new Vector3(ScreenWidthPixel - transform.position.x + offsetX, 0 + offsetY, ScreenHeightPixel - transform.position.y + offsetZ);
    }
}
