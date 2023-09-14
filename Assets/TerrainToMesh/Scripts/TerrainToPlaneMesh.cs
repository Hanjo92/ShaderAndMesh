using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.ShaderGraph;
using UnityEngine.Video;

public class TerrainToPlaneMesh : MonoBehaviour
{
    public Terrain copyTerrain;
	[Range(1, 10)] public int chunkCount = 5;
	private TerrainData terrainData;

    [Button("Convert")]
    public bool convert = false;
    public void Convert()
    {
        if(Application.isPlaying == false)
        {
            Debug.LogWarning("Use on Play Mode");
            return;
        }

        terrainData = copyTerrain.terrainData;
        testT = terrainData.alphamapTextures;

        var meshParents = (chunkCount > 1) ? new GameObject("Mesh Parent").transform : null;
		var chunkStep = 1 / (float)chunkCount;
		for(int h = 0; h < chunkCount; h++)
        {
			for(int w = 0; w < chunkCount; w++)
            {
				var copyPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
				copyPlane.transform.SetParent(meshParents);
				copyPlane.name = "CreatedPlane";
				var meshFilter = copyPlane.GetComponent<MeshFilter>();

				var newVerts = new List<Vector3>();
				var colors = new List<Color>();
				var newUVs = new List<Vector2>();

				var terrainSize = terrainData.size;
				var mesh = meshFilter.mesh;
				var bound = mesh.bounds;

				foreach(var vert in mesh.vertices)
				{
					var vertCoord = vert - bound.center + new Vector3(5, 0, 5);
					vertCoord *= 0.1f;
					var uv = new Vector2(vertCoord.x * chunkStep + w * chunkStep, vertCoord.z * chunkStep + h * chunkStep);
					vertCoord.x *= terrainSize.x * chunkStep;
					vertCoord.y *= terrainSize.y;
					vertCoord.z *= terrainSize.z * chunkStep;
					vertCoord.x += terrainSize.x * chunkStep * w;
					vertCoord.z += terrainSize.z * chunkStep * h;

					vertCoord += copyTerrain.transform.position;
					vertCoord.y = copyTerrain.SampleHeight(vertCoord);
					vertCoord -= copyTerrain.transform.position;
					vertCoord.x -= terrainSize.x * chunkStep * w;
					vertCoord.z -= terrainSize.z * chunkStep * h;
					vertCoord.x *= chunkCount / terrainSize.x;
					vertCoord.y /= terrainSize.y;
					vertCoord.z *= chunkCount / terrainSize.z;
					vertCoord *= 10f;
					vertCoord += new Vector3(-5, 0, -5) + bound.center;

					newVerts.Add(vertCoord);
					newUVs.Add(uv);
				}
				if(meshParents)
				{
					copyPlane.transform.localScale = new Vector3(terrainSize.x * 0.1f * chunkStep, terrainSize.y * 0.1f, terrainSize.z * 0.1f * chunkStep);
					var chunkPosition = new Vector3(terrainSize.x * chunkStep * (w + 0.5f), 0, terrainSize.z * chunkStep * (h + 0.5f));
					copyPlane.transform.position = copyTerrain.transform.position + chunkPosition;
				}
				else
				{
					copyPlane.transform.localScale = terrainSize * 0.1f;
					copyPlane.transform.position = copyTerrain.transform.position + new Vector3(terrainSize.x * 0.5f, 0, terrainSize.z * 0.5f);
				}

				mesh.SetVertices(newVerts.ToArray());
				mesh.SetColors(colors);
				mesh.SetUVs(0, newUVs);
				mesh.RecalculateNormals();
				mesh.RecalculateTangents();
				mesh.RecalculateBounds();
				meshFilter.mesh = mesh;
			}
        }
        

		//var tLayers = terrainData.terrainLayers;
  //      if(tLayers == null || tLayers.Length == 0)
  //          return;

  //      var mr = copyPlane.GetComponent<MeshRenderer>();
  //      if( mr == null )
  //          return;
     
  //      Texture2D diffuse;
  //      Texture2D normal;
		//var albedos = new List<Texture2D>();
		//var normals = new List<Texture2D>();
  //      var defaultW = tLayers[0].diffuseTexture.width;
  //      var defaultH = tLayers[0].diffuseTexture.height;

		//for( int i = 0; i < tLayers.Length; i++ )
  //      {
  //          diffuse = tLayers[i].diffuseTexture;
  //          if( diffuse == null )
  //          {
  //              diffuse = new Texture2D(defaultW, defaultH);
  //              var pix = diffuse.GetPixels();
  //              for( int j = 0; j < pix.Length; j++ )
  //              {
  //                  pix[j] = Color.white;

  //              }
  //              diffuse.SetPixels(pix);
  //          }
  //          albedos.Add(diffuse);
  //          normal = tLayers[i].normalMapTexture;
  //          if( tLayers[i].normalMapTexture == null )
  //          {
  //              normal = new Texture2D(defaultW, defaultH);
  //              var pix = normal.GetPixels();
  //              for( int j = 0; j < pix.Length; j++ )
  //              {
  //                  pix[j] = Color.black;
  //                  pix[j].a = 0;
  //              }
  //              normal.SetPixels(pix);
  //          }
  //          normals.Add(normal);
  //      }
  //      diffuse = new Texture2D(defaultW, defaultH);
  //      var p = diffuse.GetPixels();
  //      for( int j = 0; j < p.Length; j++ )
  //      {
  //          p[j] = Color.white;

  //      }
  //      diffuse.SetPixels(p);
  //      albedos.Add(diffuse);

  //      var diffuseArr = CreateTextureArray(albedos);
  //      var normalArr = CreateTextureArray(normals);
  //      var splats = CreateTextureArray(testT);
  //      var tilingMap = new Texture2D(albedos.Count, 1);

  //      for(int i = 0; i < albedos.Count - 1; i++ )
		//{
  //          Vector2 tiling = tLayers[i].tileSize;
  //          float pow = 0;
  //          while( tiling.x > 1 || tiling.y > 1 )
		//	{
  //              tiling *= 0.1f;
  //              pow += 1f;
		//	}
  //          Color tilingHash = new Color(tiling.x, tiling.y, pow / 255f, 1f); 

  //          tilingMap.SetPixel(i, 0, tilingHash);
  //      }
  //      tilingMap.SetPixel(albedos.Count - 1, 0, Color.white);

  //      var sM = mr.sharedMaterial;

  //      UnityEditor.AssetDatabase.CreateAsset(diffuseArr, "Assets/d.asset");
  //      UnityEditor.AssetDatabase.CreateAsset(normalArr, "Assets/n.asset");
  //      UnityEditor.AssetDatabase.CreateAsset(splats, "Assets/s.asset");
  //      UnityEditor.AssetDatabase.CreateAsset(tilingMap, "Assets/t.asset");

  //      sM.SetTexture("Diffuses", diffuseArr);
  //      sM.SetTexture("Normals", normalArr);
  //      sM.SetTexture("Splats", splats);
  //      sM.SetTexture("TilingMap", tilingMap);
  //      sM.SetFloat("DiffuseSize", albedos.Count - 1);
  //      mr.sharedMaterial = sM;
    }

    public Texture2D[] testT;

    public Texture2DArray CreateTextureArray(List<Texture2D> textures)
	{
        if(textures.Count == 0)
            return null;

        Texture2D pibot = textures[0];

        Texture2DArray textureArray = new Texture2DArray(pibot.width, pibot.height, textures.Count, pibot.format, pibot.mipmapCount > 1);
        textureArray.anisoLevel = pibot.anisoLevel;
        textureArray.filterMode = pibot.filterMode;
        textureArray.wrapMode = pibot.wrapMode;

        for( int i = 0; i < textures.Count; i++ )
        {
            for( int m = 0; m < textures[i].mipmapCount; m++ )
            {
                Graphics.CopyTexture(textures[i], 0, m, textureArray, i, m);
            }
        }
        return textureArray;
    }
    public Texture2DArray CreateTextureArray(Texture2D[] textures)
    {
        return CreateTextureArray(new List<Texture2D>(textures));
    }
}
