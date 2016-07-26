using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneFadeInOut : MonoBehaviour
{
    public float fadeSpeed = 1.5f;

    private GUITexture texture;
    private string nextSceneName = "";
    private bool sceneStarting = true;
    private bool sceneEnding = false;

    // --------------------------------------------------------------------------------------------------------
    //
    void Awake()
    {
        texture = GetComponent<GUITexture>();
        texture.enabled = true;
        texture.pixelInset = new Rect(0f, 0f, Screen.width, Screen.height);
    }

    // --------------------------------------------------------------------------------------------------------
    //
    void Update()
    {
        if (sceneStarting)
            UpdateFadeIn();
        else if (sceneEnding)
            UpdateFadeOut();
    }

    // --------------------------------------------------------------------------------------------------------
    //
    void FadeToClear()
    {
        texture.color = Color.Lerp(texture.color, Color.clear, fadeSpeed * Time.deltaTime);
    }

    // --------------------------------------------------------------------------------------------------------
    //
    void FadeToBlack()
    {
        texture.color = Color.Lerp(texture.color, Color.black, fadeSpeed * Time.deltaTime);
    }

    // --------------------------------------------------------------------------------------------------------
    //
    void UpdateFadeIn()
    {
        FadeToClear();
        if (texture.color.a <= 0.05f)
        {
            texture.color = Color.clear;
            texture.enabled = false;
            sceneStarting = false;
        }
    }

    public void UpdateFadeOut()
    {
        FadeToBlack();
        if (texture.color.a >= 0.95f)
        {
            Debug.Log("Load next scene");
            if (nextSceneName != "")
                SceneManager.LoadScene(nextSceneName);
        }
    }

    // --------------------------------------------------------------------------------------------------------
    //
    public void EndScene(string nextSceneName)
    {
        this.nextSceneName = nextSceneName;
        texture.enabled = true;
        sceneStarting = false;
        sceneEnding = true;
    }
}