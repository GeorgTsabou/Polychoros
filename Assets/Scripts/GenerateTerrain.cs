using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateTerrain : MonoBehaviour
{
    public GameObject parent;

    [SerializeField]
    private GameObject blockPrefab;//use a unit cube (1x1x1 like unity's default cube)

    [SerializeField]
    private int chunkSize = 50;

    [SerializeField]
    private float noiseScale = .05f;

    [SerializeField, Range(0, 1)]
    private float threshold = .5f;

    [SerializeField]
    private Material material;

    [SerializeField]
    private bool sphere = false;

    private List<Mesh> meshes = new List<Mesh>();//used to avoid memory issues

    private void Generate()
    {
        float startTime = Time.realtimeSinceStartup;

        #region Create Mesh Data

        List<CombineInstance> blockData = new List<CombineInstance>();//this will contain the data for the final mesh
        MeshFilter blockMesh = Instantiate(blockPrefab, Vector3.zero, parent.transform.rotation /*identity*/, parent.transform).GetComponent<MeshFilter>();//create a unit cube and store the mesh from it

        //go through each block position
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {

                    float noiseValue = Perlin3D(x * noiseScale, y * noiseScale, z * noiseScale);//get value of the noise at given x, y, and z.
                    if (noiseValue >= threshold)
                    {//is noise value above the threshold for placing a block?

                        //ignore this block if it's a sphere and it's outside of the radius (ex: in the corner of the chunk, outside of the sphere)
                        //distance between the current point with the center point. if it's larger than the radius, then it's not inside the sphere.
                        float raduis = chunkSize / 2;
                        if (sphere && Vector3.Distance(new Vector3(x, y, z), Vector3.one * raduis) > raduis)
                            continue;

                        blockMesh.transform.position = new Vector3(x, y, z);//move the unit cube to the intended position
                        CombineInstance ci = new CombineInstance
                        {//copy the data off of the unit cube
                            mesh = blockMesh.sharedMesh,
                            transform = blockMesh.transform.localToWorldMatrix,
                        };
                        blockData.Add(ci);//add the data to the list
                    }

                }
            }
        }

        Destroy(blockMesh.gameObject);//original unit cube is no longer needed. we copied all the data we need to the block list.

        #endregion

        #region Separate Mesh Data

        //divide meshes into groups of 65536 vertices. Meshes can only have 65536 vertices so we need to divide them up into multiple block lists.

        List<List<CombineInstance>> blockDataLists = new List<List<CombineInstance>>();//we will store the meshes in a list of lists. each sub-list will contain the data for one mesh. same data as blockData, different format.
        int vertexCount = 0;
        blockDataLists.Add(new List<CombineInstance>());//initial list of mesh data
        for (int i = 0; i < blockData.Count; i++)
        {//go through each element in the previous list and add it to the new list.
            vertexCount += blockData[i].mesh.vertexCount;//keep track of total vertices
            if (vertexCount > 65536)
            {//if the list has reached it's capacity. if total vertex count is more then 65536, reset counter and start adding them to a new list.
                vertexCount = 0;
                blockDataLists.Add(new List<CombineInstance>());
                i--;
            }
            else
            {//if the list hasn't yet reached it's capacity. safe to add another block data to this list 
                blockDataLists.Last().Add(blockData[i]);//the newest list will always be the last one added
            }
        }

        #endregion

        #region Create Mesh

        //the creation of the final mesh from the data.

        Transform container = new GameObject("Meshys").transform;//create container object
        container.transform.parent = transform.parent;
        foreach (List<CombineInstance> data in blockDataLists)
        {//for each list (of block data) in the list (of other lists)
            GameObject g = new GameObject("Meshy");//create gameobject for the mesh
            g.transform.parent = container;//set parent to the container we just made

      

            MeshFilter mf = g.AddComponent<MeshFilter>();//add mesh component
            MeshRenderer mr = g.AddComponent<MeshRenderer>();//add mesh renderer component
            mr.material = material;//set material to avoid evil pinkness of missing texture
            mf.mesh.CombineMeshes(data.ToArray());//set mesh to the combination of all of the blocks in the list
            meshes.Add(mf.mesh);//keep track of mesh so we can destroy it when it's no longer needed
            g.AddComponent<MeshCollider>().sharedMesh = mf.sharedMesh;
            //setting colliders takes more time. disabled for testing.
            //Rigidbody rb = g.AddComponent<Rigidbody>();
            // rb.useGravity= false;
            

            
            
            
            g.transform.position += -g.GetComponent<Renderer>().bounds.center;

            //g.transform.position = new Vector3(2.27f, 2.34f, 2.23f);
        }
        container.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        container.transform.position = new Vector3(0.0f, 0.0f, 0.0f);

        #endregion

        Debug.Log("Loaded in " + (Time.realtimeSinceStartup - startTime) + " Seconds.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            threshold = Random.Range(0.52f, 0.55f);
            Destroy(GameObject.Find("Meshys"));//destroy parent gameobject as well as children.
            foreach (Mesh m in meshes)//meshes still exist even though they aren't in the scene anymore. destroy them so they don't take up memory.
                Destroy(m);
            Generate();
        }
    }

    //dunno how this works. copied it from somewhere.
    public static float Perlin3D(float x, float y, float z)
    {
        float ab = Mathf.PerlinNoise(x, y);
        float bc = Mathf.PerlinNoise(y, z);
        float ac = Mathf.PerlinNoise(x, z);

        float ba = Mathf.PerlinNoise(y, x);
        float cb = Mathf.PerlinNoise(z, y);
        float ca = Mathf.PerlinNoise(z, x);

        float abc = ab + bc + ac + ba + cb + ca;
        return abc / 6f;
    }

}