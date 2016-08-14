using UnityEngine;
using System.Collections;

public class MapDisplay : MonoBehaviour
{
    public Renderer textRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer MeshRenderer;

    public void DrawTexture(Texture2D texture)
    {
        textRenderer.sharedMaterial.mainTexture = texture;
        textRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData, Texture2D texture)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        MeshRenderer.sharedMaterial.mainTexture = texture;
    }

}
