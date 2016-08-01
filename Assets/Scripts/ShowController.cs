using UnityEngine;
using System.Collections;

public enum ShowMode
{
    Nsdos, Blank, Terrain
}

public enum TerrainMode
{
	Intro=0, Dawn, Daytime, Dusk, Night
}

public class ShowController : AnimatedController
{
    public AnimatedController[] controllers;
    public float[] terrainSceneTimes = new float[5];
    public TerrainMode terrainMode;

    private float previousTime = 0;
    SceneFadeInOut sceneFade;
    


    // --------------------------------------------------------------------------------------------------------
    //
    void Awake()
    {
        terrainMode = TerrainMode.Intro;
        Preset(terrainMode, false);
        sceneFade = GetComponent<SceneFadeInOut>();
    }

    override protected void Update()
	{
        base.Update();

        foreach (var controller in controllers)
        {
            controller.PlayNormalised(normalisedTime);
        }
        UpdateTerrainMode();

        if (Input.GetKeyDown("1")) Preset(TerrainMode.Intro);
        if (Input.GetKeyDown("2")) Preset(TerrainMode.Dawn);
        if (Input.GetKeyDown("3")) Preset(TerrainMode.Daytime);
        if (Input.GetKeyDown("4")) Preset(TerrainMode.Dusk);
        if (Input.GetKeyDown("4")) Preset(TerrainMode.Night);

        if (Input.GetKeyDown("q")) GoToMode(ShowMode.Nsdos);
        if (Input.GetKeyDown("w")) GoToMode(ShowMode.Blank);
        if (Input.GetKeyDown("e")) GoToMode(ShowMode.Terrain);
    }

    void UpdateTerrainMode()
    {
        var nextMode = GetTerrainForTime(normalisedTime);
        if (terrainMode != nextMode)
        {
            terrainMode = nextMode;
            Preset(terrainMode, false);
        }
    }

    TerrainMode GetTerrainForTime(float time)
    {
        TerrainMode terrainMode = TerrainMode.Intro;
        if (time > terrainSceneTimes[terrainSceneTimes.Length-1]) terrainMode = TerrainMode.Night;
        else
        {
            for (int i = 0; i < terrainSceneTimes.Length - 1; i++)
            {
                var sceneTime = terrainSceneTimes[i];
                var nextSceneTime = terrainSceneTimes[i + 1];
                if (time >= sceneTime && time < nextSceneTime) terrainMode = (TerrainMode)i;
            }
        }
        
        return terrainMode;
    }

    // --------------------------------------------------------------------------------------------------------
    //
    void GoToMode(ShowMode mode)
    {
        if (mode == ShowMode.Nsdos)
        {
            sceneFade.EndScene("NSDOS");

        }
        else if (mode == ShowMode.Blank)
        {
            sceneFade.EndScene("");
        }
        else if (mode == ShowMode.Terrain)
        {
            sceneFade.EndScene("Terrain");
        }
    }

    // --------------------------------------------------------------------------------------------------------
    //
    public void Preset(TerrainMode mode, bool updateTime = true)
    {
        if (updateTime)
        {
            int index = (int)mode;
            var sceneTime = terrainSceneTimes[index];
            PlayNormalised(sceneTime);
        }

        terrainMode = mode;
        var terrains = FindObjectsOfType<TerrainBlendDeformer>();
        foreach (var terrain in terrains)
        {
            terrain.Preset(mode);
        }
    }
}
