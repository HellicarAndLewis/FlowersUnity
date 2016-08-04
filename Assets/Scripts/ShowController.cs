using UnityEngine;
using System.Collections;

public enum ShowMode
{
    Nsdos, Blank, Terrain, Null
}

public enum TerrainMode
{
	Intro=0, Dawn, Daytime, Dusk, Night
}

public class ShowController : AnimatedController
{
    public SceneFadeInOut nsdos;
    public SceneFadeInOut blank;
    public SceneFadeInOut terrain;

    public ShowMode showMode = ShowMode.Terrain;
    private ShowMode queuedShowMode = ShowMode.Terrain;
    public TerrainMode terrainMode;
    public bool pauseBetweenScenes = true;
    public bool resumePlayback = false;
    public AnimatedController[] controllers;
    public float[] terrainSceneTimes = new float[5];

    private float previousTime = 0;
    private float terrainTime = 0;
    private bool isPausedBetweenScenes = false;
    
    
    // --------------------------------------------------------------------------------------------------------
    //
    void Awake()
    {
        terrainMode = TerrainMode.Intro;
        Preset(terrainMode, false);

        Debug.Log("displays connected: " + Display.displays.Length);
        for (int i = 1; i < Display.displays.Length; i++)
        {
            Debug.Log("display " + i + " activated.");
            Display.displays[i].Activate();
        }
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
            if (isPlaying)
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
            else
            {
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
        // close current mode, add new one to queue
        queuedShowMode = mode;
        
        if (showMode == ShowMode.Nsdos)
        {
            nsdos.OnFadeOut += Nsdos_OnFadeOut;
            nsdos.FadeOut();
        }
        else if (showMode == ShowMode.Blank)
        {
            blank.OnFadeOut += Blank_OnFadeOut;
            blank.FadeOut();
        }
        else if (showMode == ShowMode.Terrain)
        {
            terrain.OnFadeOut += Terrain_OnFadeOut;
            terrain.FadeOut();
        }
        
    }

    private void Terrain_OnFadeOut()
    {
        terrain.OnFadeOut -= Nsdos_OnFadeOut;
        ToggleScenes();
    }

    private void Blank_OnFadeOut()
    {
        blank.OnFadeOut -= Nsdos_OnFadeOut;
        ToggleScenes();
    }

    private void Nsdos_OnFadeOut()
    {
        nsdos.OnFadeOut -= Nsdos_OnFadeOut;
        ToggleScenes();
    }

    public void ToggleScenes()
    {
        if (queuedShowMode == ShowMode.Nsdos)
        {
            nsdos.gameObject.SetActive(true);
            nsdos.FadeIn();
            blank.gameObject.SetActive(false);
            terrain.gameObject.SetActive(false);
        }
        else if (queuedShowMode == ShowMode.Blank)
        {
            nsdos.gameObject.SetActive(false);
            blank.gameObject.SetActive(true);
            blank.FadeIn();
            terrain.gameObject.SetActive(false);
        }
        else if (queuedShowMode == ShowMode.Terrain)
        {
            nsdos.gameObject.SetActive(false);
            blank.gameObject.SetActive(false);
            terrain.gameObject.SetActive(true);
            terrain.FadeIn();
        }

        showMode = queuedShowMode;
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
