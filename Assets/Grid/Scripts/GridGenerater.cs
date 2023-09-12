using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerater : MonoBehaviour
{
    [Range(1, 100)]
    public int horizonCount = 10;
    [Range(1, 100)]
    public int verticalCount = 10;

    public float height = 1f;
    public float width = 1f;

    private float horizonInterval
    {
        get
        {
            return height / horizonCount;
        }
    }
    private float verticalInterval
    {
        get
        {
            return width / verticalCount;
        }
    }

    public float thickness = 0.01f;

    private List<Vector3> positions = new List<Vector3>();

    private Mesh grid;
    private MeshFilter filter = null;
    private MeshRenderer meshRenderer = null;

    public Material gridMaterial;

    void Awake()
    {
        MeshRenderer t = GetComponent<MeshRenderer>();
        if( t != null )
            meshRenderer = t;
        else
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if( meshFilter != null )
            filter = meshFilter;
        else
        {
            filter = gameObject.AddComponent<MeshFilter>();
        }
        if( meshRenderer.sharedMaterial == null )
            meshRenderer.sharedMaterial = gridMaterial;
        filter.mesh = grid = new Mesh();
        grid.name = "lieGrid";
    }

    [Button(nameof(GenerateGrid))]
    public bool generate;
    public void GenerateGrid()
    {
        maxHeight = float.MinValue;
        minHeight = float.MaxValue;
        SetVertics();
        CreateMesh();
    }

    private void SetVertics()
    {
        positions.Clear();
        Vector3 pibot = transform.position;
        pibot.x -= width * 0.5f;
        pibot.z -=height * 0.5f;
        float posX;
        float posZ = 0;

        for( int i = 0; i < verticalCount; i++ )
        {
            posX = 0f;
            for( int j = 0; j < horizonCount; j++ )
            {
                Vector3 pos = pibot + new Vector3(posX, 0, posZ);
                CalcPosition( ref pos );
                positions.Add( pos );
                posX += horizonInterval;
            }
            posZ += verticalInterval;
        }
    }

    private float maxHeight = float.MinValue;
    private float minHeight = float.MaxValue;

    private float gap
    {
        get
        {
            return maxHeight - minHeight;
        }
    }

    private void CalcPosition( ref Vector3 pos )
    {
        if( filter == null )
            return;

        if( Physics.Raycast( pos + Vector3.up * 1000f, Vector3.down, out RaycastHit hit ) )
        {
            pos.y = hit.point.y;
        }

        if( pos.y > maxHeight )
            maxHeight = pos.y;
        if( pos.y < minHeight )
            minHeight = pos.y;

        return;
    }

    private void CreateMesh()
    {
        grid.Clear();
        for( int i = 0; i < positions.Count; i++ )
        {
            int x = i % horizonCount;
            int z = i / horizonCount;

            bool upDown;
            for( int j = 0; j < 4; j++ )
            {
                Vector3 point1 = positions[i] + diagonalDirection[j] * (thickness * 0.5f);
                int point2Idx = i;
                upDown = j % 2 == 1;
                int diagonal;
                if( j == 0 )
                {
                    if( x == horizonCount - 1 )
                        continue;
                    diagonal = 3;
                    point2Idx += 1;
                }
                else if( j == 1 )
                {
                    if( z == 0 )
                        continue;
                    diagonal = 0;
                    point2Idx -= horizonCount;
                }
                else if( j == 2 )
                {
                    if( x == 0 )
                        continue;

                    diagonal = 1;
                    point2Idx -= 1;
                }
                else
                {
                    if( z == verticalCount - 1 )
                        continue;

                    diagonal = 2;
                    point2Idx += horizonCount;
                }

                Vector3 liePoint2 = positions[point2Idx];
                Vector3 point2 = liePoint2;
                point2 += diagonalDirection[diagonal] * ( thickness * 0.5f );

                AddQuad( positions[i], point1, point2, liePoint2 );
                AddQuadUV( positions[i], point1, point2, liePoint2, upDown );
            }
        }
        grid.SetVertices( vertices );
        grid.SetTriangles( triangles, 0 );
        grid.SetUVs( 0, uvs );

        if( meshRenderer && meshRenderer.sharedMaterial )
        {
            meshRenderer.sharedMaterial.SetFloat("Height", gap );
        }
    }

    #region Calc Mesh Info
    private void AddQuad( Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4 )
    {
        int vertexIndex = vertices.Count;
        vertices.Add( v1 );
        vertices.Add( v2 );
        vertices.Add( v3 );
        vertices.Add( v4 );
        triangles.Add( vertexIndex );
        triangles.Add( vertexIndex + 1 );
        triangles.Add( vertexIndex + 2 );

        triangles.Add( vertexIndex + 2 );
        triangles.Add( vertexIndex + 3 );
        triangles.Add( vertexIndex );
    }
    private void AddQuadUV( Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, bool upDown )
    {
        float gap = Mathf.Abs(v2.y - v3.y);
        float speed = Mathf.Clamp(gap / maxLieSpeedHeights, 0f, maxLieSpeed);
        speed /= maxLieSpeed;

        float targetInterval = ( upDown ) ? verticalInterval : horizonInterval;
        float uvCalib = (targetInterval * 0.5f) / (thickness - targetInterval);

        if( v2.y == v3.y )
        {
            Vector2 u1 = new Vector2(0f - uvCalib, 0);
            Vector2 u2 = new Vector2(1 + uvCalib, 0);
            if( upDown )
            {
                if( v1.z > v4.z )
                {
                    uvs.Add( u1 );
                    uvs.Add( Vector2.zero );
                    uvs.Add( Vector2.right );
                    uvs.Add( u2 );
                }
                else
                {
                    uvs.Add( u2 );
                    uvs.Add( Vector2.right );
                    uvs.Add( Vector2.zero );
                    uvs.Add( u1 );
                }
            }
            else
            {
                if( v1.x > v4.x )
                {
                    uvs.Add( u1 );
                    uvs.Add( Vector2.zero );
                    uvs.Add( Vector2.right );
                    uvs.Add( u2 );
                }
                else
                {
                    uvs.Add( u2 );
                    uvs.Add( Vector2.right );
                    uvs.Add( Vector2.zero );
                    uvs.Add( u1 );
                }
            }
        }
        else if( v2.y > v3.y )
        {
            Vector2 u1 = new Vector2(0f - uvCalib, speed);
            Vector2 u2 = new Vector2(0f, speed);
            Vector2 u3 = new Vector2(1f, speed);
            Vector2 u4 = new Vector2(1 + uvCalib, speed);

            uvs.Add( u1 );
            uvs.Add( u2 );
            uvs.Add( u3 );
            uvs.Add( u4 );
        }
        else
        {
            Vector2 u1 = new Vector2(1 + uvCalib, speed);
            Vector2 u2 = new Vector2(1f, speed);
            Vector2 u3 = new Vector2(0f, speed);
            Vector2 u4 = new Vector2(0f - uvCalib, speed);

            uvs.Add( u1 );
            uvs.Add( u2 );
            uvs.Add( u3 );
            uvs.Add( u4 );
        }
    }

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();

    [Header("Lie Speed")]
    public float maxLieSpeedHeights = 5f;
    public float maxLieSpeed = 5f;
    #endregion

    Vector3[] diagonalDirection =
    {
        new Vector3( 1, 0, 1f ),
        new Vector3( 1, 0, -1f ),
        new Vector3( -1, 0, -1f ),
        new Vector3( -1f, 0, 1f ),
    };
    Vector3[] Direction =
    {
        new Vector3(1f,0f,0f),
        new Vector3(0f,0f,-1f),
        new Vector3(-1f,0f,0f),
        new Vector3(0f,0f,1f)
    };
}
