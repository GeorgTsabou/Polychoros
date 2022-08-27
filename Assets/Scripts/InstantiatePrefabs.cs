using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiatePrefabs : MonoBehaviour {


    //This creates a large block of objects.
    //Add script to an empty object. The creation of the this script will be in releation to where the empty block is placed.
    public GameObject parent;
    public GameObject prefab;
    public int cubeSize = 5;
    public int offset = 1;     //Distance between cube centers
    List<GameObject> cList;


    // Use this for initialization
    void Start () {
        cList = new List<GameObject>();
	    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))  {
            AddPrefab();
        }
        if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)) {
            RemovePrefab();
        }

        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus)) {

            DeleteAllPrefabs();
        }
		
	}

    public void AddPrefab( )
    {
        for (int z = 0; z < cubeSize * offset; z++)
        {
            for (int y = 0; y < cubeSize * offset; y++)
            {
                for (int x = 0; x < cubeSize * offset; x++)
                {

                    // Begin the instantiation where the empty object is. 
                    cList.Add((GameObject)Instantiate(prefab, new Vector3(transform.position.x + x, transform.position.y + y, transform.position.z + z), Quaternion.identity, parent.transform));
                    Debug.Log("Inserting ELement: " + cList.Count);
                    cList[cList.Count-1].GetComponentInChildren<Renderer>().material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

                    if (cList.Count > 90)
                    {
                        Debug.Log("Instance overflow. Deleting..: " + cList.Count);
                        Destroy(cList[0]);
                        cList.RemoveAt(0);

                    }

                    //Added this line so you can actually see how the cubes are being populated

                }
            }
        }
    }

    public void RemovePrefab()
    {
        while (cList.Count != 0)
        {
            Destroy(cList[cList.Count - 1]);
            cList.RemoveAt(cList.Count - 1);

        }
    }


    public void DeleteAllPrefabs()
    {
        if (cList.Count != 0)
        {
            Debug.Log("Deleting ELement: " + cList.Count);
            Destroy(cList[cList.Count - 1]);
            cList.RemoveAt(cList.Count - 1);
        }
        else
        {
           // Debug.Log("Element list is Empty!");

        }
    }
}
