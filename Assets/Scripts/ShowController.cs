using UnityEngine;
using System.Collections;

public enum ShowMode
{
    Nsdos=0, Blank, Rock, Logo, World_1, World_2, Null
}

public enum TerrainMode
{
	Intro=0, Dawn, Daytime, Dusk, Night
}

public class ShowController : MonoBehaviour
{
    public SceneFadeInOut[] scenes;
    public GameObject commonWorld;

    public ShowMode showMode = ShowMode.World_1;
    private ShowMode queuedShowMode = ShowMode.World_1;
    
    public AnimatedController[] controllers;
    public float[] terrainSceneTimes = new float[5];
    
    
    
    // --------------------------------------------------------------------------------------------------------
    //
    void Awake()
    {

        Debug.Log("displays connected: " + Display.displays.Length);
        for (int i = 1; i < Display.displays.Length; i++)
        {
            Debug.Log("display " + i + " activated.");
            Display.displays[i].Activate();
        }

        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i].OnFadeOut += OnSceneFadeOut;
        }

        queuedShowMode = showMode;
        ToggleScenes();
    }

    void Update()
	{
        if (Input.GetKeyDown("q")) GoToMode(ShowMode.Nsdos);
        if (Input.GetKeyDown("w")) GoToMode(ShowMode.Blank);
        if (Input.GetKeyDown("e")) GoToMode(ShowMode.Rock);
        if (Input.GetKeyDown("r")) GoToMode(ShowMode.Logo);
        if (Input.GetKeyDown("t")) GoToMode(ShowMode.World_1);
        if (Input.GetKeyDown("y")) GoToMode(ShowMode.World_2);
    }
    
    // --------------------------------------------------------------------------------------------------------
    //
    public void GoToMode(ShowMode mode)
    {
        // close current mode, add new one to queue
        queuedShowMode = mode;
        int sceneIndex = (int)showMode;
        if (sceneIndex > -1 && sceneIndex < (int)ShowMode.Null)
        {
            scenes[sceneIndex].FadeOut();
        }
        
    }
    // --------------------------------------------------------------------------------------------------------
    //
    private void OnSceneFadeOut()
    {
        ToggleScenes();
    }

    // --------------------------------------------------------------------------------------------------------
    //
    public void ToggleScenes()
    {
        showMode = queuedShowMode;
        int sceneIndex = (int)showMode;
        if (sceneIndex > -1 && sceneIndex < (int)ShowMode.Null)
        {
            SetAllActive(false);
            scenes[sceneIndex].gameObject.SetActive(true);
            scenes[sceneIndex].FadeIn();
        }

        if (sceneIndex > 3)
        {
            commonWorld.SetActive(true);
        }
        else
        {
            commonWorld.SetActive(false);
        }
    }

    private void SetAllActive(bool active)
    {
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i].gameObject.SetActive(false);
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
    public void GoRock()
    {
        GoToMode(ShowMode.Rock);
    }
    public void GoLogo()
    {
        GoToMode(ShowMode.Logo);
    }
    public void GoWorld1()
    {
        GoToMode(ShowMode.World_1);
    }
    
}
