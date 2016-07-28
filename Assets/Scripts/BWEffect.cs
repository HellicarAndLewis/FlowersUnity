using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BWEffect : MonoBehaviour {

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
}
