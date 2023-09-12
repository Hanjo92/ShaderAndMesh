using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( MeshFilter ), typeof( MeshRenderer ) )]
public class Plane : MonoBehaviour
{
    private Mesh mesh = null;
    private MeshFilter meshFilter = null;
    private MeshRenderer meshRenderer = null;
    
    [Range(1,200)]
    public int verticsCount = 10;
    public float size = 10f;
    private float interval { get { return size / verticsCount; } }

    public Material applyMaterial = null;

    private void Awake()
    {
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter.sharedMesh = mesh;
    }

    [Button("Generate")]
    public bool generate = false;
    public void Generate()
    {
        AddPoints();
        Triangulate();
        Apply();

    }
    private void Apply()
    {
        mesh.SetVertices( positions );
        mesh.SetTriangles( triangles, 0 );
        mesh.SetUVs( 0, meshUV0 );

        mesh.RecalculateNormals();
        if( applyMaterial == null )
            meshRenderer.sharedMaterial = new Material( Shader.Find( "Standard" ) );
        else
            meshRenderer.sharedMaterial = applyMaterial;
    }

    private List<Vector3> positions = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> meshUV0 = new List<Vector2>();
    private void AddPoints()
    {
        positions.Clear();
        meshUV0.Clear();
        Vector3 point = Vector3.zero;
        point.z = size * -0.5f;
        for( int v = 0; v <= verticsCount; ++v )
        {
            point.x = size * -0.5f;
            for( int h = 0; h <= verticsCount; ++h )
            {
                positions.Add( point );
                point.x += interval;
                meshUV0.Add( new Vector2( ( float )h / verticsCount, ( float )v / verticsCount ) );
            }
            point.z += interval;
        }
    }
    private void Triangulate()
    {
        triangles.Clear();
        int zCount = verticsCount + 1;
        for( int i = 0; i < verticsCount; i++ )
        {
            for( int j = 0; j < verticsCount; j++ )
            {
                int pibot = i * zCount + j;

                triangles.Add( pibot );
                triangles.Add( pibot + zCount );
                triangles.Add( pibot + 1 + zCount );
                triangles.Add( pibot );
                triangles.Add( pibot + 1 + zCount );
                triangles.Add( pibot + 1 );
            }
        }
    }
}
