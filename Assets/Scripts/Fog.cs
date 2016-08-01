using UnityEngine;
using System.Collections;

/// <summary>
/// Configurable fog
/// </summary>
public class Fog : MonoBehaviour
{

    public Color colour = new Color(16f / 255f, 68f / 255f, 120f / 255f);
    public bool isEnabled = true;
    public bool forceUpdate = false;
    public FogMode mode = FogMode.Linear;
    public float density = 1f;
    public float endDistance = 200;
    public float startDistance = 0;
    
	// --------------------------------------------------------------------------------------------------------
	//
    void Start ()
    {
        Refresh();
    }

	// --------------------------------------------------------------------------------------------------------
	//
    void Update()
    {
        if (forceUpdate)
        {
            //forceUpdate = false;
            Refresh();
        }
    }

	// --------------------------------------------------------------------------------------------------------
	//
    void Refresh()
    {
        RenderSettings.fogColor = colour;
        RenderSettings.fog = isEnabled;
        RenderSettings.fogMode = mode;
        RenderSettings.fogDensity = density;
        RenderSettings.fogEndDistance = endDistance;
        RenderSettings.fogStartDistance = startDistance;
    }

}
