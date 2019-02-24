using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class terrainGenerator : MonoBehaviour {

    //detail and height restrictions on Midpoint Bisection method
    [Range(0.1f, 5f)]
    public float heightScale = 3.0f;
    [Range(0.1f, 5f)]
    public float detailScale = 3.0f;

    //plane components
    private Mesh slope;
    private Vector3[] vertices; 

    private int numVertice = 11;

    void Start()
    {
        slope = GetComponent<MeshFilter>().mesh;
        vertices = slope.vertices;
        if (tag != "top") GenerateTerrain();
    }

    
    void GenerateTerrain()
    {
        //run the recursive algorithm
        MidpointBisection(vertices, 0, numVertice-1); 
        slope.vertices = vertices;
        slope.RecalculateBounds();
        slope.RecalculateNormals();
    }

    //recursively generated natural looking mountain surface
    void MidpointBisection(Vector3[] relevantVertices, int start, int stop) 
    {
        int midpoint = (stop + start) / 2;
        if (midpoint != start && midpoint != stop)
        {
            //Displace the vector at midpoint
            float maxOffset = Mathf.Abs(relevantVertices[stop].x - relevantVertices[start].x) / heightScale;
            float offset = (float)Random.Range(-maxOffset-1, maxOffset+1);
            float length = Mathf.Abs((relevantVertices[midpoint].x - relevantVertices[0].x) / detailScale);
            //if we move it down, adjust the vertices below it down as well
            relevantVertices[midpoint].z += offset * length; 
            if(offset < 0)
            {
                for(int i = midpoint + numVertice; i<vertices.Length; i += numVertice)
                {
                    relevantVertices[i].z += offset * length;
                }
            }
            //Re-balancing and recursive call
            if (start < midpoint)
            {
                for (int i = midpoint - 1; i >= start; i--)
                {
                    relevantVertices[i].z += offset * length * (i - start) / (midpoint - start);
                    if (offset < 0)
                    {
                        for (int j = i; j < vertices.Length; j += numVertice)
                        {
                            relevantVertices[j].z += offset * length * (i - start) / (midpoint - start);
                        }
                    }
                }
                MidpointBisection(relevantVertices, start, midpoint);
            }
            if (midpoint < stop)
            {
                for (int i = midpoint + 1; i <= stop; i++)
                {
                    relevantVertices[i].z += offset * length * (stop - i) / (stop - midpoint);
                    if (offset < 0)
                    {
                        for (int j = i; j < vertices.Length; j += numVertice)
                        {
                            relevantVertices[j].z += offset * length * (stop - i) / (stop - midpoint);
                        }
                    }
                }
                MidpointBisection(relevantVertices, midpoint, stop);
            }
        }
    }

}
