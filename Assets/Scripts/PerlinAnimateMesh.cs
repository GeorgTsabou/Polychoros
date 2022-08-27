using UnityEngine;
using System.Collections;

public class PerlinAnimateMesh : MonoBehaviour
{
    public float perlinScale = 4.56f;
    public float waveSpeed = 1f;
    public float waveHeight = 2f;

    private Mesh mesh;
    private MeshCollider mCol;

    float lastconductivity;
    float Velocity = 0.0f;

    float conductivity = 1.0f;

    public MyMath mm;

    //public MagicHand magicHands;

    /*
    void Update()
    {
        AnimateMesh();
    }
    */

    public void LateUpdate()    //AnimateMesh( float conductivity)
    {
        //waveHeight = Mathf.Lerp(0, 0.5f, conductivity); //magicHands.Conductive;
        waveHeight= Mathf.SmoothDamp(lastconductivity, conductivity, ref Velocity, 0.3f);
       
        /// waveHeight = conductivity;

        if (!mesh){
            mesh = GetComponent<MeshFilter>().sharedMesh;
            mCol = (MeshCollider)GetComponent<MeshCollider>();
        }
        

        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            float pX = (vertices[i].x * perlinScale) + (Time.timeSinceLevelLoad * waveSpeed);
            float pZ = (vertices[i].z * perlinScale) + (Time.timeSinceLevelLoad * waveSpeed);

            vertices[i].y = (Mathf.PerlinNoise(pX, pZ) - 0.5f) * waveHeight;
        }

        mesh.vertices = vertices;

        GetComponent<MeshCollider>().enabled = false;
        GetComponent<MeshCollider>().enabled = true;

        lastconductivity = conductivity;
    }




}
