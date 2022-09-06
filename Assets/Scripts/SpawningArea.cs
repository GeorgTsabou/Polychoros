using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns a prefab randomly throughout the volume of a Unity transform. Attach to a Unity cube to visually scale or rotate. For best results disable collider and renderer.
/// </summary>
public class SpawningArea : MonoBehaviour
{

    public GameObject ObjectToSpawn;
    public float RateOfSpawn = 1;

    private float nextSpawn = 0;

    List<GameObject> SpawnList;

    void Start()
    {
        SpawnList = new List<GameObject>();
    }


    // Update is called once per frame
    void Update()
    {

        if (Time.time > nextSpawn)
        {
            nextSpawn = Time.time + RateOfSpawn;

            // Random position within this transform
            Vector3 rndPosWithin;
            rndPosWithin = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            rndPosWithin = transform.TransformPoint(rndPosWithin * .5f);
            SpawnList.Add(Instantiate(ObjectToSpawn, rndPosWithin, transform.rotation));

            if (SpawnList.Count > 90)
            {
               // Debug.Log("Instance overflow. Deleting..: " + SpawnList.Count);
                Destroy(SpawnList[0]);
                SpawnList.RemoveAt(0);

            }
        }
    }

    public void SpawnRate(float rate) {
        RateOfSpawn = rate;
    }
}