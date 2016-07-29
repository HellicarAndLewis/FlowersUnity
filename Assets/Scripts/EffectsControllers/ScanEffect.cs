using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
public class ScanEffect : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float amount;
    [Range(0.0f, 0.5f)]
    public float nIntensity;
    [Range(0.0f, 0.1f)]
    public float sIntensity;
    [Range(0.0f, 100.0f)]
    public float sCount;
    private Material material;
    // Use this for initialization
    void Awake()
    {
        material = new Material(Shader.Find("Hidden/ScanLines"));
    }

    // Update is called once per frame
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        material.SetFloat("amount", amount);
        material.SetFloat("nIntensity", nIntensity);
        material.SetFloat("sIntensity", sIntensity);
        material.SetFloat("sCount", sCount);

        Graphics.Blit(source, destination, material);
    }
}