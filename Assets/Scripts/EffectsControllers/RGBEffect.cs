﻿using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
public class RGBEffect : MonoBehaviour
{
    [Range(0.0f, 1.5f)]
    public float amount;
    [Range(0.0f, 2.0f*3.14159f)]
    public float angle;
    private Material material;
    // Use this for initialization
    void Awake()
    {
        material = new Material(Shader.Find("Hidden/RGBShift"));
    }

    // Update is called once per frame
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        material.SetFloat("amount", amount);
        material.SetFloat("angle", angle);
        Graphics.Blit(source, destination, material);
    }
}