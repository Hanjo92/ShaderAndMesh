using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PipeGenerator : MonoBehaviour
{
	[Header("Pipe Options")]
	public List<Transform> linePoints = new List<Transform>();
	private Vector3 startPosition
	{
		get
		{
			if( linePoints == null || linePoints.Count == 0)
				return Vector3.zero;

			return linePoints[0].position;
		}
	}
	private Vector3 endPosition
	{
		get
		{
			if( linePoints == null || linePoints.Count == 0 )
				return Vector3.one;
			
			return linePoints[linePoints.Count - 1].position;
		}
	}

	[Range(0,1f)]
	public float startRadius = 0.5f;
	[Range(0,1f)]
	public float endRadius = 0.5f;

	[Range(3, 26)]
	public int polyCount = 4;
	public int splatCount = 5;

	public bool hasCap = false;
	[Range(0,1f)]
	public float capUVHeight = 0.1f;
	private float capOffset
	{
		get
		{
			if( hasCap == false )
				return 0f;
			return capUVHeight * 0.5f;
		}
	}
	private float bodyUVRange
	{
		get
		{
			if( hasCap == false )
				return 1f;
			return 1f - capUVHeight;
		}
	}
	public bool bezierCalc = true;

	[Header("Target Mesh")]
	public GameObject pipe;
	private MeshRenderer pipeRenderer;
	public Material applyMaterial = null;
	private MeshFilter pipeFilter;
	private Mesh mesh;

	[Button( "Generate" )]
	public bool g;
	public void Generate()
	{
		if( pipe != null )
			DestroyImmediate( pipe );
		pipe = new GameObject( "Pipe" );
		pipe.transform.position = Vector3.Lerp( startPosition, endPosition, 0.5f );
		pipeRenderer = pipe.AddComponent<MeshRenderer>();
		pipeFilter = pipe.AddComponent<MeshFilter>();
		pipeFilter.sharedMesh = mesh = new Mesh();
		CalculateDirections();
		CalculateSplatPoints();
		Triangulate();
		Apply();
	}
	private void Apply()
	{
		mesh.SetVertices( vertics );
		mesh.SetTriangles( triangles, 0 );
		mesh.SetUVs( 0, meshUV0 );
		mesh.RecalculateNormals();
		if( applyMaterial == null )
			pipeRenderer.sharedMaterial = new Material( Shader.Find( "Standard" ) );
		else
			pipeRenderer.sharedMaterial = applyMaterial;
	}

	#region Create Mesh
	private List<Vector3> points = new List<Vector3>();
	private List<Vector3> vertics = new List<Vector3>();
	private List<int> triangles = new List<int>();
	private List<Vector2> meshUV0 = new List<Vector2>();
	private List<Vector3> directions = new List<Vector3>();
	private void CalculateDirections()
	{
		directions.Clear();
		Quaternion splatRotation = Quaternion.Euler( 0, 0, 360f / polyCount );
		Vector3 point = Vector3.up;
		for( int i = 0; i < polyCount; i++ )
		{
			directions.Add( point );
			point = splatRotation * point;
		}
		directions.Add( point );
	}
	private void CalculateSplatPoints()
	{
		points.Clear();
		Vector3 pipePos = Vector3.zero;
		if( pipe )
			pipePos = pipe.transform.position;

		if( bezierCalc )
		{
			for( int i = 0; i <= splatCount; i++ )
			{
				points.Add( Bezier.CalculateBezierPoint( ( float )i / splatCount, linePoints ) - pipePos );
			}
		}
		else
		{
			for( int i = 0; i <= splatCount; i++ )
			{
				points.Add( Vector3.Lerp( startPosition, endPosition, ( float )i / splatCount ) - pipePos );
			}
		}
	}

	private void Triangulate()
	{
		if( points.Count < 2 )
			return;
		vertics.Clear();
		meshUV0.Clear();
		triangles.Clear();

		TriangulateBody();
		if( hasCap )
		{
			TriangulateCap( true );
			TriangulateCap( false );
		}
	}
	private void TriangulateCap( bool isStart )
	{
		Vector3 center;
		Vector3 dir;
		float uv_v = capOffset;
		if( isStart )
		{
			center = points[0];
			dir = Vector3.Normalize( points[1] - points[0] );
		}
		else
		{
			center = points[points.Count - 1];
			dir = Vector3.Normalize( points[points.Count - 1] - points[points.Count - 2] );
			uv_v = 1f - capOffset;
		}
		float uv_u = (1f / polyCount);
		Quaternion polyRotation = Quaternion.LookRotation( dir );
		for( int j = 0; j < directions.Count - 1; j++ )
		{
			Vector3 v1 = center + ( isStart ? startRadius : endRadius )
				* ( polyRotation * directions[j] );

			Vector3 v2 = center + ( isStart ? startRadius : endRadius )
				* ( polyRotation * directions[j + 1] );
			
			Vector2 uv1 = new Vector2(uv_u * j, uv_v);
			Vector2 uv2 = new Vector2(uv_u * j, isStart ? 0f : 1f);
			Vector2 uv3 = new Vector2(uv_u * (j + 1), uv_v);
			if( isStart )
			{
				AddTriangle( center, v1, v2 );
				AddTriangleUV( uv2, uv1, uv3 );
			}
			else
			{
				AddTriangle( v1, center, v2 );
				AddTriangleUV( uv1, uv2, uv3 );
			}
		}
	}
	private void TriangulateBody()
	{
		float uv_u = 1f / polyCount;
		float uv_v = bodyUVRange / splatCount;
		for( int i = 0; i < splatCount; i++ )
		{
			Vector3 dir;
			Vector3 dirNext;
			if( i == 0 )
			{
				dir = Vector3.Normalize( points[1] - points[0] );
				dirNext = Vector3.Normalize( points[2] - points[0] );
			}
			else if( i == splatCount - 1 )
			{
				dir = Vector3.Normalize( points[i + 1] - points[i - 1] );
				dirNext = Vector3.Normalize( points[i + 1] - points[i] );
			}
			else
			{
				dir = Vector3.Normalize( points[i + 1] - points[i - 1] );
				dirNext = Vector3.Normalize( points[i + 2] - points[i] );
			}
			Quaternion pointRot = Quaternion.LookRotation( dir );
			Quaternion nextPointRot = Quaternion.LookRotation( dirNext );

			float uv_vCurrent = capOffset + (uv_v * i);
			float uv_vNext = capOffset + (uv_v * (i + 1));
			float radius1 = Mathf.Lerp( startRadius, endRadius, (float)i / splatCount );
			float radius2 = Mathf.Lerp( startRadius, endRadius, (float)(i + 1) / splatCount );

			for( int j = 0; j < directions.Count - 1; j++ )
			{
				Vector3 v1 = points[i] + radius1 * ( pointRot * directions[j] );
				Vector3 v2 = points[i] + radius1 * ( pointRot * directions[j + 1] );
				Vector3 v3 = points[i + 1] + radius2 * ( nextPointRot * directions[j] );
				Vector3 v4 = points[i + 1] + radius2 * ( nextPointRot * directions[j + 1] );

				Vector2 uv1 = new Vector2( uv_u * j, uv_vCurrent );
				Vector2 uv2 = new Vector2( uv_u * (j + 1), uv_vCurrent );
				Vector2 uv3 = new Vector2( uv_u * j, uv_vNext );
				Vector2 uv4 = new Vector2( uv_u * (j + 1), uv_vNext );

				AddQuad( v1, v3, v4, v2);
				AddQuadUV( uv1, uv3, uv4, uv2);
			}
		}
	}
	private void AddTriangle( Vector3 v1, Vector3 v2, Vector3 v3 )
	{
		int index = vertics.Count;
		vertics.Add( v1 );
		vertics.Add( v2 );
		vertics.Add( v3 );

		triangles.Add( index );
		triangles.Add( index + 2 );
		triangles.Add( index + 1 );
	}
	private void AddTriangleUV( Vector2 v1, Vector2 v2, Vector2 v3 )
	{
		meshUV0.Add( v1 );
		meshUV0.Add( v2 );
		meshUV0.Add( v3 );
	}
	private void AddQuad( Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4 )
	{
		int index = vertics.Count;
		vertics.Add( v1 );
		vertics.Add( v2 );
		vertics.Add( v3 );
		vertics.Add( v4 );

		triangles.Add( index );
		triangles.Add( index + 2 );
		triangles.Add( index + 1 );

		triangles.Add( index + 2 );
		triangles.Add( index );
		triangles.Add( index + 3 );
	}
	private void AddQuadUV( Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4 )
	{
		meshUV0.Add( v1 );
		meshUV0.Add( v2 );
		meshUV0.Add( v3 );
		meshUV0.Add( v4 );
	}
	#endregion
#if UNITY_EDITOR
	[Range(0,1)]
	public float gizmoSize = 0.1f;
	[Range(0, 30)]
	public int fontSize = 5;
	private void OnDrawGizmos()
	{
		GUIStyle style = new GUIStyle();
		style.fontSize = fontSize;

		if( bezierCalc )
		{
			for( int i = 0; i < linePoints.Count; i++ )
			{
				if( linePoints[i] == null )
					continue;
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere( linePoints[i].position, gizmoSize );
				UnityEditor.Handles.Label( linePoints[i].position, $"End Pos\n{linePoints[i].position}", style );
			}
		}
		else
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere( startPosition, gizmoSize );
			UnityEditor.Handles.Label( startPosition, $"Start Pos\n{startPosition}", style );

			Gizmos.color = Color.red;
			Gizmos.DrawSphere( endPosition, gizmoSize );
			UnityEditor.Handles.Label( endPosition, $"End Pos\n{endPosition}", style );
		}

		Gizmos.color = Color.gray;
		for( int i = 0; i < vertics.Count; i++ )
		{
			UnityEditor.Handles.Label( vertics[i], $"uv\n{meshUV0[i]}", style );
			Gizmos.DrawSphere( vertics[i], gizmoSize );
		}
	}
#endif
}
