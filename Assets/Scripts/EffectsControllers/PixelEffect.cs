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
        width = 0.0001f;
        height = 0.0001f;
    }

    // Update is called once per frame
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        material.SetFloat("_pixelWidth", width);
        material.SetFloat("_pixelHeight", height);
        Graphics.Blit(source, destination, material);
    }

    public void OnWidth(float _val)
    {
        float m = 0.1f;
        float b = 0.0001f;
        width = m * _val + b;
    }

    public void OnHeight(float _val)
    {
        float m = 0.1f;
        float b = 0.0001f;
        height = m * _val + b;
    }
}