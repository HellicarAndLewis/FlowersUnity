using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BWEffect : MonoBehaviour {

    [Range(-5.0f, 5.0f)]
    public float intensity;
    private Material material;
	// Use this for initialization
	void Awake () {
        material = new Material(Shader.Find("Hidden/BWDiffuse"));
	}
	
	// Update is called once per frame
	void OnRenderImage (RenderTexture source, RenderTexture destination) {
        if (intensity == 0) {
            Graphics.Blit(source, destination);
            return;
        }

        material.SetFloat("_bwBlend", intensity);
        Graphics.Blit(source, destination, material);
	}

    public void OnIntensity(float _val)
    {
        float m = 10.0f;
        float b = -5.0f;
        intensity = m * _val + b;
    }
}
