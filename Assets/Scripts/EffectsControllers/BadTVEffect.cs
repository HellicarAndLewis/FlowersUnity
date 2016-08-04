using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
public class BadTVEffect : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float amount;
    [Range(0.01f, 10.0f)]
    public float distortion;
    [Range(0.0f, 1.0f)]
    public float distortion2;
    [Range(0.0f, 1.0f)]
    public float speed;
    [Range(0.0f, 20.0f)]
    public float rollSpeed;
    private Material material;
    // Use this for initialization
    void Awake()
    {
        material = new Material(Shader.Find("Hidden/BadTV"));
        distortion = 0.01f;
    }

    // Update is called once per frame
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        material.SetFloat("amount", amount);
        material.SetFloat("distortion", distortion);
        material.SetFloat("distortion2", distortion2);
        material.SetFloat("speed", speed);
        material.SetFloat("speed", rollSpeed);

        Graphics.Blit(source, destination, material);
    }

    public void OnAmount(float _val)
    {
        float m = 1.0f;
        float b = 0.0f;
        amount = m * _val + b;
    }

    public void OnDistortion(float _val)
    {
        float m = 10.0f;
        float b = 0.01f;
        distortion = m * _val + b;
    }

    public void OnDistortion2(float _val)
    {
        float m = 1.0f;
        float b = 0.0f;
        distortion2 = m * _val + b;
    }

    public void OnSpeed(float _val)
    {
        float m = 1.0f;
        float b = 0.0f;
        speed = m * _val + b;
    }

    public void OnRollSpeed(float _val)
    {
        float m = 20.0f;
        float b = 0.0f;
        rollSpeed = m * _val + b;
    }
}