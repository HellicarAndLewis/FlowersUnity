using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
public class ScanEffect : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float amount;
    [Range(0.0f, 2.0f)]
    public float nIntensity;
    [Range(0.0f, 1.0f)]
    public float sIntensity;
    [Range(0.0f, 200.0f)]
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

    public void OnAmount(float _val)
    {
        float m = 1.0f;
        float b = 0.0f;
        amount = m * _val + b;
    }

    public void OnNIntensity(float _val)
    {
        float m = 2.0f;
        float b = 0.0f;
        nIntensity = m * _val + b;
    }

    public void OnsIntensity(float _val)
    {
        float m = 1.0f;
        float b = 0.0f;
        sIntensity = m * _val + b;
    }

    public void OnsCount(float _val)
    {
        float m = 200.0f;
        float b = 0.0f;
        sCount = m * _val + b;
    }
}