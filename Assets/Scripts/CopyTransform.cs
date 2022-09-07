using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyTransform : MonoBehaviour

{
    public GameObject GO;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GO = GameObject.FindGameObjectWithTag("trackedWall");
        gameObject.transform.position = GO.transform.localPosition;
    }
}
