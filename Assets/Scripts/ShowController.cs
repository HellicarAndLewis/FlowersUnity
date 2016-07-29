using UnityEngine;
using System.Collections;

public enum ShowMode
{
    Nsdos, Blank, Terrain
}

public enum TerrainMode
{
	Nsdos, Blank, Dawn=0, Daytime, Dusk, Night
}

public class ShowController : MonoBehaviour
{

    SceneFadeInOut sceneFade;

    void Awake()
    {
        sceneFade = GetComponent<SceneFadeInOut>();
    }
    
	void Update()
	{
        if (Input.GetKeyDown("1")) Preset(TerrainMode.Dawn);
        if (Input.GetKeyDown("2")) Preset(TerrainMode.Daytime);
        if (Input.GetKeyDown("3")) Preset(TerrainMode.Dusk);
        if (Input.GetKeyDown("4")) Preset(TerrainMode.Night);

        if (Input.GetKeyDown("q")) GoToMode(ShowMode.Nsdos);
        if (Input.GetKeyDown("w")) GoToMode(ShowMode.Blank);
        if (Input.GetKeyDown("e")) GoToMode(ShowMode.Terrain);
    }

    void GoToMode(ShowMode mode)
    {
        if (mode == ShowMode.Nsdos)
        {
            //SceneManager.LoadScene("NSDOS");
            sceneFade.EndScene("NSDOS");

        }
        else if (mode == ShowMode.Blank)
        {
        }
        else if (mode == ShowMode.Terrain)
        {
            //SceneManager.LoadScene("Terrain");
            sceneFade.EndScene("Terrain");
        }
    }

    public void Preset(TerrainMode mode)
    {
        FindObjectOfType<LightController>().Preset(mode);
        var terrains = FindObjectsOfType<TerrainBlendDeformer>();
        foreach (var terrain in terrains)
        {
            terrain.Preset(mode);
        }
    }
}
