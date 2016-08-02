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
    public TerrainMode terrainMode;
    public bool pauseBetweenScenes = true;
    public bool resumePlayback = false;
    public AnimatedController[] controllers;
    public float[] terrainSceneTimes = new float[5];

    private float previousTime = 0;
    private float terrainTime = 0;
    private bool isPausedBetweenScenes = false;
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
        terrainTime = Mathf.Lerp(terrainTime, normalisedTime, 0.1f);
        foreach (var controller in controllers)
        {
            controller.PlayNormalised(terrainTime);
        }
        if (Input.GetKeyDown("r")) resumePlayback = true;
        UpdateTerrainMode();

        if (Input.GetKeyDown("1")) Preset(TerrainMode.Intro);
        if (Input.GetKeyDown("2")) Preset(TerrainMode.Dawn);
        if (Input.GetKeyDown("3")) Preset(TerrainMode.Daytime);
        if (Input.GetKeyDown("4")) Preset(TerrainMode.Dusk);
        if (Input.GetKeyDown("5")) Preset(TerrainMode.Night);

        if (Input.GetKeyDown("q")) GoToMode(ShowMode.Nsdos);
        if (Input.GetKeyDown("w")) GoToMode(ShowMode.Blank);
        if (Input.GetKeyDown("e")) GoToMode(ShowMode.Terrain);
    }

    public void Play()
    {
        if (!isPlaying && !isPausedBetweenScenes) isPlaying = true;
        else if (isPausedBetweenScenes)
        {
            resumePlayback = true;
        }
    }

    void UpdateTerrainMode()
    {
        var nextMode = GetTerrainForTime(normalisedTime);
        if (terrainMode != nextMode)
        {
            if (pauseBetweenScenes && !resumePlayback)
            {
                isPausedBetweenScenes = true;
                isPlaying = false;
                animator.speed = 0;
            }
            else
            {
                isPausedBetweenScenes = false;
                StartAnimation();
                resumePlayback = false;
                terrainMode = nextMode;
                Preset(terrainMode, false);
            }
            
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
    public void GoToMode(ShowMode mode)
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

    public void GoNSDOS()
    {
        GoToMode(ShowMode.Nsdos);
    }
    public void GoBlank()
    {
        GoToMode(ShowMode.Blank);
    }
    public void GoTerrain()
    {
        GoToMode(ShowMode.Terrain);
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
        var terrains = FindObjectsOfType<TerrainDeformer>();
        foreach (var terrain in terrains)
        {
            terrain.Preset(mode);
        }
    }
}
