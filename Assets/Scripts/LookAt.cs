using UnityEngine;
// This complete script can be attached to a camera to make it
// continuously point at another object.

public class LookAt : MonoBehaviour
{
    public Transform target;

    void Update()
    {
        // Rotate the camera every frame so it keeps looking at the target
        transform.LookAt(GameObject.FindGameObjectWithTag("trackedGO").transform)   ;

        // Same as above, but setting the worldUp parameter to Vector3.left in this example turns the camera on its side
        transform.LookAt(target, Vector3.left);
    }
}