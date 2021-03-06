﻿using UnityEngine;
using System.Collections;

//for Plane Object
public class MapDisplay : MonoBehaviour
{

    public Renderer textureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    //generate 1D Colormap from 2D Noisemap/texture
    public void DrawTexture(Texture2D texture)
    {
        textureRender.sharedMaterial.mainTexture = texture; //sharedmaterial: preview map inside editor without starting map
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);  //width and height of the texture
    }

    public void DrawMesh(MeshData meshData, Texture2D texture)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
    }

}
