using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public GameObject copyToObject;
    public Terrain copyTerrain;

    private TerrainData td;
    void Start()
    {
        td = copyTerrain.terrainData;
        testT = td.alphamapTextures;
        var bounds = copyTerrain.terrainData.bounds;
        var mf = copyToObject.GetComponent<MeshFilter>();
        var m = mf.mesh;

        List<Vector3> newVerts = new List<Vector3>();
        List<Color> colors = new List<Color>();
        List<Vector2> newUVs = new List<Vector2>();
        Vector3 scale = copyToObject.transform.localScale;

        Vector3 tSize = td.size;

        List<Texture2D> albedos = new List<Texture2D>();
        List<Texture2D> normals = new List<Texture2D>();

        var tLayers = td.terrainLayers;

        foreach( var vert in m.vertices )
        {
            var wPos = vert;
            wPos.x *= scale.x;
            wPos.y *= scale.y;
            wPos.z *= scale.z;

            wPos += copyToObject.transform.position;

            wPos.y = copyTerrain.SampleHeight(wPos);

            Vector3 deltaPos = wPos - copyTerrain.transform.position;
            deltaPos.y = 0;

            Vector2 uv = new Vector2(deltaPos.x / tSize.x, deltaPos.z / tSize.z);

            newVerts.Add(wPos);
            newUVs.Add(uv);
        }

        m.SetVertices(newVerts.ToArray());
        m.SetColors(colors);
        m.SetUVs(0, newUVs);
        m.RecalculateNormals();
        m.RecalculateTangents();
        m.RecalculateBounds();

        MeshRenderer mr = copyToObject.GetComponent<MeshRenderer>();
        if( mr == null )
            return;

        int defaultW = tLayers[0].diffuseTexture.width;
        int defaultH = tLayers[0].diffuseTexture.height;
        Texture2D diffuse = null;
        Texture2D normal = null;
        for( int i = 0; i < tLayers.Length; i++ )
        {
            diffuse = tLayers[i].diffuseTexture;
            if( diffuse == null )
            {
                diffuse = new Texture2D(defaultW, defaultH);
                var pix = diffuse.GetPixels();
                for( int j = 0; j < pix.Length; j++ )
                {
                    pix[j] = Color.white;

                }
                diffuse.SetPixels(pix);
            }
            albedos.Add(diffuse);
            normal = tLayers[i].normalMapTexture;
            if( tLayers[i].normalMapTexture == null )
            {
                normal = new Texture2D(defaultW, defaultH);
                var pix = normal.GetPixels();
                for( int j = 0; j < pix.Length; j++ )
                {
                    pix[j] = Color.black;
                    pix[j].a = 0;
                }
                normal.SetPixels(pix);
            }
            normals.Add(normal);
        }
        diffuse = new Texture2D(defaultW, defaultH);
        var p = diffuse.GetPixels();
        for( int j = 0; j < p.Length; j++ )
        {
            p[j] = Color.white;

        }
        diffuse.SetPixels(p);
        albedos.Add(diffuse);

        Texture2DArray diffuseArr = CreateTextureArray(albedos);
        Texture2DArray normalArr = CreateTextureArray(normals);

        Texture2DArray splats = CreateTextureArray(testT);

        Texture2D tilingMap = new Texture2D(albedos.Count, 1);
        for(int i = 0; i < albedos.Count - 1; i++ )
		{
            Vector2 tiling = tLayers[i].tileSize;
            float pow = 0;
            while( tiling.x > 1 || tiling.y > 1 )
			{
                tiling *= 0.1f;
                pow += 1f;
			}
            Color tilingHash = new Color(tiling.x, tiling.y, pow / 255f, 1f); 

            tilingMap.SetPixel(i, 0, tilingHash);
        }
        tilingMap.SetPixel(albedos.Count - 1, 0, Color.white);

        Material sM = mr.sharedMaterial;

        UnityEditor.AssetDatabase.CreateAsset(diffuseArr, "Assets/d.asset");
        UnityEditor.AssetDatabase.CreateAsset(normalArr, "Assets/n.asset");
        UnityEditor.AssetDatabase.CreateAsset(splats, "Assets/s.asset");
        UnityEditor.AssetDatabase.CreateAsset(tilingMap, "Assets/t.asset");

        sM.SetTexture("Diffuses", diffuseArr);
        sM.SetTexture("Normals", normalArr);
        sM.SetTexture("Splats", splats);
        sM.SetTexture("TilingMap", tilingMap);
        sM.SetFloat("DiffuseSize", albedos.Count - 1);
        mr.sharedMaterial = sM;
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
