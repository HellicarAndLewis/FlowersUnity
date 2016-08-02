using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class PixelEffect : MonoBehaviour
{
    [Range (0.0001f, 0.1f)]
    public float width;
    [Range(0.0001f, 0.1f)]
    public float height;
    private Material material;
    // Use this for initialization
    void Awake()
    {
        material = new Material(Shader.Find("Hidden/Pixelation"));
    }

    // Update is called once per frame
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        material.SetFloat("_pixelWidth", width);
        material.SetFloat("_pixelHeight", height);
        Graphics.Blit(source, destination, material);
    }

    public void OnWidthChanged(float _val)
    {
        width = 0.01f + _val * 0.1f;
    }

    public void OnHeightChanged(float _val)
    {
        height = 0.01f + _val * 0.1f;
    }
}